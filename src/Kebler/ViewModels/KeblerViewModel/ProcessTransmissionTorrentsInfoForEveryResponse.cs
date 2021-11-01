using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Caliburn.Micro;
using Kebler.Models;
using Kebler.Models.Torrent;
using Kebler.Services;
using Kebler.Services.Converters;
using System.Windows;
using Microsoft.AppCenter.Crashes;

// ReSharper disable once CheckNamespace

namespace Kebler.ViewModels
{
    /// <summary>
    /// Process transmission torrents info for every response.
    /// </summary>
    public partial class KeblerViewModel
    {
        /// <summary>
        /// Parse Transmission server statistic.
        /// </summary>
        private void ParseStats()
        {
            try
            {
                Thread.CurrentThread.CurrentUICulture = LocalizationManager.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = LocalizationManager.CurrentCulture;

                IsConnectedStatusText = $"Transmission {_sessionInfo?.Version} (RPC:{_sessionInfo?.RpcVersion})     " +
                                        $"      {LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.Stats_Uploaded))} {Utils.GetSizeString(_stats.CumulativeStats.UploadedBytes)}" +
                                        $"      {LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.Stats_Downloaded))}  {Utils.GetSizeString(_stats.CumulativeStats.DownloadedBytes)}" +
                                        $"      {LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.Stats_ActiveTime))}  {TimeSpan.FromSeconds(_stats.CurrentStats.SecondsActive).ToPrettyFormat()}";

                var dSpeedText = BytesToUserFriendlySpeed.GetSizeString(_stats.DownloadSpeed);
                var uSpeedText = BytesToUserFriendlySpeed.GetSizeString(_stats.UploadSpeed);

                var dSpeed = string.IsNullOrEmpty(dSpeedText) ? "0 b/s" : dSpeedText;
                var uSpeed = string.IsNullOrEmpty(uSpeedText) ? "0 b/s" : uSpeedText;
                var altUp = _settings?.AlternativeSpeedEnabled == true
                    ? $" [{BytesToUserFriendlySpeed.GetSizeString(_settings.AlternativeSpeedUp * 1000)}]"
                    : string.Empty;
                var altD = _settings?.AlternativeSpeedEnabled == true
                    ? $" [{BytesToUserFriendlySpeed.GetSizeString(_settings.AlternativeSpeedDown * 1000)}]"
                    : string.Empty;

                DownloadSpeed = $"D: {dSpeed}{altD}";
                UploadSpeed = $"U: {uSpeed}{altUp}";
            }
            catch (Exception ex)
            {
                //#1665431308
                Log.Error(ex);
                Crashes.TrackError(ex);
            }
        }


        /// <summary>
        /// Parse transmission torrents data.
        /// </summary>
        /// <param name="data"></param>
        private void ProcessParsingTransmissionResponse(TransmissionTorrents data)
        {
            if (!IsConnected)
                return;

            _categoriesCount.Clear();

            _categoriesCount.Add(Enums.Categories.All, data.Torrents.Length);

            _categoriesCount.Add(Enums.Categories.Downloading, data.Torrents.Where(x => x.Status == 3 || x.Status == 4).ToArray().Length);
            var counts = data.Torrents.Where(x => x.Status == 4 || x.Status == 6 || x.Status == 2)
                .ToArray();
            counts = counts.Where(x => x.RateDownload > 1 || x.RateUpload > 1).ToArray();
            _categoriesCount.Add(Enums.Categories.Active, counts.Length);
            _categoriesCount.Add(Enums.Categories.Stopped, data.Torrents.Where(x => x.Status == 0 && string.IsNullOrEmpty(x.ErrorString))
                .ToArray().Length);
            _categoriesCount.Add(Enums.Categories.Error, data.Torrents.Where(x => !string.IsNullOrEmpty(x.ErrorString)).ToArray().Length);
            _categoriesCount.Add(Enums.Categories.Inactive, data.Torrents.Where(x => x.RateDownload <= 0 && x.RateUpload <= 0 && x.Status != 2)
                .ToArray().Length);
            _categoriesCount.Add(Enums.Categories.Ended, data.Torrents.Where(x => x.Status == 6).ToArray().Length);

            foreach (var cat in CategoriesList)
            {
                var val = _categoriesCount[cat.Cat].ToString();
                cat.Count = val;
            }
            

            lock (_syncTorrentList)
            {
                //1: 'check pending',
                //2: 'checking',
                //5: 'seed pending',
                //6: 'seeding',
                switch (_filterCategory)
                {
                    //3: 'download pending',
                    //4: 'downloading',
                    case Enums.Categories.Downloading:
                        data.Torrents = data.Torrents.Where(x => x.Status == 3 || x.Status == 4).ToArray();
                        break;

                    //4: 'downloading',
                    //6: 'seeding',
                    //2: 'checking',
                    case Enums.Categories.Active:
                        data.Torrents = data.Torrents.Where(x => x.Status == 4 || x.Status == 6 || x.Status == 2)
                            .ToArray();
                        data.Torrents = data.Torrents.Where(x => x.RateDownload > 1 || x.RateUpload > 1).ToArray();
                        break;

                    //0: 'stopped' and is error,
                    case Enums.Categories.Stopped:
                        data.Torrents = data.Torrents.Where(x => x.Status == 0 && string.IsNullOrEmpty(x.ErrorString))
                            .ToArray();
                        break;

                    case Enums.Categories.Error:
                        data.Torrents = data.Torrents.Where(x => !string.IsNullOrEmpty(x.ErrorString)).ToArray();
                        break;


                    //6: 'seeding',
                    //1: 'checking queue',
                    case Enums.Categories.Inactive:
                        //var array1 = data.Torrents.Where(x=> x.Status == 1).ToArray();
                        var array2 = data.Torrents.Where(x => x.RateDownload <= 0 && x.RateUpload <= 0 && x.Status != 2)
                            .ToArray();

                        //int array1OriginalLength = array1.Length;
                        //Array.Resize<TorrentInfo>(ref array1, array1OriginalLength + array2.Length);
                        //Array.Copy(array2, 0, array1, array1OriginalLength, array2.Length);
                        data.Torrents = array2;
                        break;

                    //6: 'seeding',
                    case Enums.Categories.Ended:
                        data.Torrents = data.Torrents.Where(x => x.Status == 6).ToArray();
                        break;
                }



                if (!string.IsNullOrEmpty(FilterText))
                {
                    //var txtfilter = FilterText;
                    if (FilterText.Contains("{p}:"))
                    {
                        var splited = FilterText.Split("{p}:");
                        var filterKey = splited[^1];
                        // txtfilter = txtfilter.Replace($"{{p}}:{filterKey}", string.Empty);

                        data.Torrents = data.Torrents
                            .Where(x => FolderCategory.NormalizePath(x.DownloadDir).Equals(filterKey)).ToArray();
                    }
                    else
                    {
                        data.Torrents = data.Torrents.Where(x => x.Name.ToLower().Contains(FilterText.ToLower()))
                            .ToArray();
                    }
                }

                for (var i = 0; i < data.Torrents.Length; i++) data.Torrents[i] = ValidateTorrent(data.Torrents[i]);

                //Debug.WriteLine("S" + DateTime.Now.ToString("HH:mm:ss:ffff"));

                UpdateOnUi(data.Torrents);

                //Debug.WriteLine("E" + DateTime.Now.ToString("HH:mm:ss:ffff"));

                UpdateCategories(allTorrents.Torrents.Select(x => new FolderCategory(x.DownloadDir)).ToList());


            }
        }



        /// <summary>
        /// Update torrents listView on UI thread.
        /// </summary>
        private void UpdateOnUi(TorrentInfo[] info)
        {
            var toRm = new List<TorrentInfo>(TorrentList.Except(info).ToList());

            if (toRm.Any())
                OnUIThread(() =>
                {
                    foreach (var item in toRm)
                    {
                        TorrentList.RemoveWithoutNotify(item);
                    }
                });

            foreach (var item in info)
            {
                var ind = TorrentList.IndexOf(item);
                if (ind == -1)
                {
                    TorrentList.Add(item);
                }
                else
                {
                    TorrentList[ind].Notify(item);
                }
            }
        }


        /// <summary>
        /// Update folder categories on UI thread.
        /// </summary>
        private void UpdateCategories(IEnumerable<FolderCategory> dirrectories)
        {
            var dirs = dirrectories.Distinct().ToList();

            var cats = new List<FolderCategory>();
            var toUpd = new List<FolderCategory>();

            foreach (var cat in dirs)
            {
                cat.Count = allTorrents.Torrents.Count(x =>
                    FolderCategory.NormalizePath(x.DownloadDir) == cat.FullPath);
                cat.Title = cat.FolderName;
                cats.Add(cat);

                if (Categories.Any(x => x.Equals(cat) && cat.Count != x.Count))
                {
                    toUpd.Add(cat);
                }
            }


            var toRm = Categories.Except(cats).ToList();
            var toAdd = cats.Except(Categories).ToList();

            if (toRm.Any())
            {
                Log.Info("Remove categories" + string.Join(", ", toRm));

                foreach (var itm in toRm) Application.Current.Dispatcher.Invoke(() => { Categories.Remove(itm); });
            }

            //update counter for existing
            foreach (var itm in toUpd)
            {
                Categories.First(x => x.Equals(itm)).Count = itm.Count;
            }

            if (toAdd.Any())
            {
                Log.Info("Add categories" + string.Join(", ", toAdd));
                foreach (var itm in toAdd)
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var ct = Categories.Count(x => x.FolderName == itm.FolderName);
                        if (ct <= 0)
                        {
                            Categories.Add(itm);
                        }
                        else
                        {
                            foreach (var cat in Categories)
                                if (cat.FolderName == itm.FolderName)
                                    cat.Title =
                                        $"{cat.FullPath} ({allTorrents.Torrents.Count(x => FolderCategory.NormalizePath(x.DownloadDir) == itm.FullPath)})";
                            itm.Title =
                                $"{itm.FullPath} ({allTorrents.Torrents.Count(x => FolderCategory.NormalizePath(x.DownloadDir) == itm.FullPath)})";
                            Categories.Add(itm);
                        }
                    });
            }


            if (_isAddWindOpened)
            {
                _eventAggregator.PublishOnUIThreadAsync(new Messages.DownlaodCategoriesChanged(Categories));
            }
        }


        /// <summary>
        /// Parse transmission server settings
        /// </summary>
        private void ParseTransmissionServerSettings()
        {
            if (_settings?.AlternativeSpeedEnabled != null)
                IsSlowModeEnabled = (bool)_settings.AlternativeSpeedEnabled;
        }

        /// TODO: remove maybe 
        ///  <summary>
        ///  fucking trash....
        ///  </summary>
        ///  <param name="torrInf"></param>
        /// <returns></returns>
        private TorrentInfo ValidateTorrent(TorrentInfo torrInf)
        {
            // if (torrInf.Status == 1 || torrInf.Status == 2)
            // {
            //     torrInf.PercentDone = torrInf.RecheckProgress;
            //     return torrInf;
            // }
            //
            // if (torrInf.Status == 0)
            //     return torrInf;
            //
            //
            // if (!skip && torrInf.TrackerStats.Length > 0 &&
            //     torrInf.TrackerStats.All(x => x.LastAnnounceSucceeded == false)) torrInf.Status = -1;

            return torrInf;
        }
    }
}