using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Kebler.Models;
using Kebler.Models.Torrent;
using Kebler.Models.Torrent.Attributes;
using Kebler.Models.Torrent.Common;
using Kebler.Models.Torrent.Entity;
using Kebler.Models.Torrent.Response;
using Kebler.Services;
using Kebler.Services.Converters;
using Kebler.TransmissionCore;
using Newtonsoft.Json;
using MessageBox = Kebler.Dialogs.MessageBox;

namespace Kebler.UI.Windows
{
    public partial class KeblerWindow
    {

        #region REQUEST
        public void UpdateServers()
        {
            _dbServers = StorageRepository.GetServersList();
            _servers = _dbServers.FindAll().ToList();
        }

        public void InitConnection()
        {
            if (IsConnected) return;

            if (ServersList.Count == 0)
            {
                ShowConnectionManager();
            }
            else
            {
                if (_checkerTask == null)
                {
                    _checkerTask = GetLongChecker();
                    _checkerTask.Start();
                }

                SelectedServer ??= ServersList.FirstOrDefault();
                TryConnect();

            }
        }

        private async void TryConnect()
        {
            if (SelectedServer == null) return;

            Log.Info("Try initialize connection");
            IsErrorOccuredWhileConnecting = false;
            IsConnectedStatusText = Kebler.Resources.Windows.MW_ConnectingText;
            try
            {
                IsConnecting = true;
                var pass = string.Empty;
                if (SelectedServer.AskForPassword)
                {
                    Log.Info("Manual ask password");

                    pass = await GetPassword();
                    if (string.IsNullOrEmpty(pass))
                    {
                        IsErrorOccuredWhileConnecting = true;
                        return;
                    }
                }

                try
                {
                    _transmissionClient = new TransmissionClient(SelectedServer.FullUriPath, null, SelectedServer.UserName,
                        SelectedServer.AskForPassword ? pass : SecureStorage.DecryptStringAndUnSecure(SelectedServer.Password));
                    Log.Info("Client created");
                    ConnectedServer = SelectedServer;
                    StartCycle();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                    IsConnectedStatusText = ex.Message;
                    IsConnected = false;
                    IsErrorOccuredWhileConnecting = true;
                }
            }
            finally
            {
                IsConnecting = false;
            }
        }

        private Task GetLongChecker()
        {
            return new Task(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (Dispatcher.HasShutdownStarted)
                        {
                            throw new TaskCanceledException("Dispatcher.HasShutdownStarted  = true");
                        }

                        var date = DateTimeOffset.Now;
                        var time = (date - _longActionTimeStart);
                        if (time.TotalSeconds > 2 && _isLongTaskRunning)
                        {
                            IsDoingStuff = true;
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                    catch
                    {

                    }
                    await Task.Delay(1000);
                }
            });
        }

        void StartCycle()
        {
            _cancelTokenSource = new CancellationTokenSource();
            var token = _cancelTokenSource.Token;
            MoreInfoControl.FileTreeViewControl.OnFileStatusUpdate += FileTreeViewControl_OnFileStatusUpdate;
            _whileCycleTask = new Task(async () =>
            {
                try
                {
                    var info = await Get(_transmissionClient.GetSessionInformationAsync(_cancelTokenSource.Token), Kebler.Resources.Windows.MW_StatusText_Session);

                    if (CheckResponse(info.Response))
                    {
                        _sessionInfo = info.Value;
                        IsConnected = true;
                        Log.Info($"Connected {_sessionInfo.Version}");

                        while (IsConnected && !token.IsCancellationRequested)
                        {

                            if (Application.Current?.Dispatcher != null &&
                                Application.Current.Dispatcher.HasShutdownStarted)
                                throw new TaskCanceledException();


                            _stats = (await Get(_transmissionClient.GetSessionStatisticAsync(_cancelTokenSource.Token), Kebler.Resources.Windows.MW_StatusText_Stats)).Value;

                            allTorrents = (await Get(_transmissionClient.TorrentGetAsync(WorkingParams, _cancelTokenSource.Token), Kebler.Resources.Windows.MW_StatusText_Torrents)).Value;

                            ParseSettings();
                            ParseStats();

                            if (allTorrents.Clone() is TransmissionTorrents data)
                            {
                                ProcessParsingTransmissionResponse(data);
                            }

                            await Task.Delay(5000, token);
                        }
                    }
                    else
                    {
                        if (info.Response.WebException != null)
                            throw info.Response.WebException;

                        if (info.Response.CustomException != null)
                            throw info.Response.CustomException;

                        throw new Exception(info.Response.HttpWebResponse.StatusCode.ToString());
                    }
                }
                catch (TaskCanceledException)
                {

                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
                finally
                {
                    if (IsConnected)
                    {
                        await _transmissionClient.CloseSessionAsync(_cancelTokenSource.Token);
                    }

                    allTorrents = null;
                    TorrentList = new System.Collections.ObjectModel.ObservableCollection<TorrentInfo>();
                    IsConnected = false;
                    IsConnectedStatusText = DownloadSpeed = UploadSpeed = string.Empty;
                    Log.Info("Disconnected from server");
                }
            }, token);

            _whileCycleTask.Start();
        }




        private bool CheckResponse(TransmissionResponse resp)
        {

            if (resp.WebException != null)
            {
                Dispatcher.Invoke(() =>
                {
                    var msg = resp.WebException.Status switch
                    {
                        System.Net.WebExceptionStatus.NameResolutionFailure => $"{Kebler.Resources.Dialogs.EX_Host} '{SelectedServer.Host}'",
                        _ => $"{resp.WebException.Status} {Environment.NewLine} {resp.WebException?.Message}"
                    };

                    var dialog = new MessageBox(msg, Kebler.Resources.Dialogs.Error, Enums.MessageBoxDilogButtons.Ok, true) { Owner = this };
                    dialog.ShowDialog();
                });
                return false;
            }

            if (resp.CustomException != null)
            {

            }
            return true;
        }



        public void Disconnect()
        {
            if (!IsConnected) return;

            IsConnected = false;
            _cancelTokenSource.Cancel();
            _whileCycleTask.Wait();
            _whileCycleTask.Dispose();
            _whileCycleTask = null;
        }

        public async void ReconnectToNewServer()
        {
            Log.Info("Try connect to another server");
            Disconnect();
            await Task.Delay(1500);
            InitConnection();
        }


        private async Task<TransmissionResponse<T>> Get<T>(Task<TransmissionResponse<T>> t, string status)
        {
            try
            {
                _isLongTaskRunning = true;
                _longActionTimeStart = DateTimeOffset.Now;
                LongStatusText = status;
                var resp = await t;
                resp.ParseTransmissionReponse(Log);
                return resp;
            }
            finally
            {
                IsDoingStuff = false;
                LongStatusText = string.Empty;
                _isLongTaskRunning = false;
            }
        }
        #endregion





        #region parsing

        public void ParseSettings()
        {

            if (_settings?.AlternativeSpeedEnabled != null)
                IsSlowModeEnabled = (bool)_settings.AlternativeSpeedEnabled;
        }

        public void ParseStats()
        {
            Thread.CurrentThread.CurrentUICulture = LocalizationManager.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = LocalizationManager.CurrentCulture;

            IsConnectedStatusText = $"Transmission {_sessionInfo?.Version} (RPC:{_sessionInfo?.RpcVersion})     " +
                                    $"      {Kebler.Resources.Windows.Stats_Uploaded} {Utils.GetSizeString(_stats.CumulativeStats.UploadedBytes)}" +
                                    $"      {Kebler.Resources.Windows.Stats_Downloaded}  {Utils.GetSizeString(_stats.CumulativeStats.DownloadedBytes)}" +
                                    $"      {Kebler.Resources.Windows.Stats_ActiveTime}  {TimeSpan.FromSeconds(_stats.CurrentStats.SecondsActive).ToPrettyFormat()}";

            var dSpeedText = BytesToUserFriendlySpeed.GetSizeString(_stats.DownloadSpeed);
            var uSpeedText = BytesToUserFriendlySpeed.GetSizeString(_stats.UploadSpeed);

            var dSpeed = string.IsNullOrEmpty(dSpeedText) ? "0 b/s" : dSpeedText;
            var uSpeed = string.IsNullOrEmpty(uSpeedText) ? "0 b/s" : uSpeedText;

            DownloadSpeed = $"D: {dSpeed}";
            UploadSpeed = $"U: {uSpeed}";
        }

        private void UpdateCategories(List<FolderCategory> dirrectories)
        {
            if (dirrectories == null) return;
            var dirs = dirrectories.Distinct().ToList();

            var cats = new List<FolderCategory>();

            foreach (var cat in dirs)
            {

                cat.Title += $"{cat.FolderName} ({allTorrents.Torrents.Count(x => FolderCategory.NormalizePath(x.DownloadDir) == cat.FullPath)})";
                cats.Add(cat);
            }


            var toRm = Categories.Except(cats).ToList();
            var toAdd = cats.Except(Categories).ToList();

            if (toRm.Any())
            {
                Log.Info($"Remove categories" + string.Join(", ", toRm));

                foreach (var itm in toRm)
                {
                    Dispatcher.Invoke(() => { Categories.Remove(itm); });
                }
            }


            Dispatcher.Invoke(() =>
            {
                foreach (var itm in Categories)
                {
                    itm.Title += $"{itm.FolderName} ({allTorrents.Torrents.Count(x => FolderCategory.NormalizePath(x.DownloadDir) == itm.FullPath)})";
                }
            });

            if (toAdd.Any())
            {
                Log.Info($"Add categories" + string.Join(", ", toAdd));
                foreach (var itm in toAdd)
                {

                    Dispatcher.Invoke(() => { Categories.Add(itm); });
                }
            }


        }

        private void UpdateTorrentData(int oldIndex, TorrentInfo newData)
        {
            var myType = typeof(TorrentInfo);
            var props = myType.GetProperties().Where(p => p.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length == 0);
            props = props.Where(p => p.GetCustomAttributes(typeof(SetIgnoreAttribute), true).Length == 0);

            var trnt = ValidateTorrent(newData);
            foreach (var item in props)
            {

                var name = item.GetCustomAttribute(typeof(JsonPropertyAttribute));



                if (WorkingParams.Contains(((JsonPropertyAttribute)name)?.PropertyName))
                {
                    var newVal = trnt.Get(item.Name);
                    TorrentList[oldIndex].Set(item.Name, newVal);
                }
            }
        }

        private void ProcessParsingTransmissionResponse(TransmissionTorrents data)
        {
            if (!IsConnected)
                return;

            lock (_syncTorrentList)
            {
                //var torrents = new List<TorrentInfo>(TorrentList);
                ////1: 'check pending',
                ////2: 'checking',

                ////5: 'seed pending',
                ////6: 'seeding',
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
                        data.Torrents = data.Torrents.Where(x => x.Status == 4 || x.Status == 6 || x.Status == 2).ToArray();
                        data.Torrents = data.Torrents.Where(x => x.RateDownload > 1 || x.RateUpload > 1).ToArray();
                        break;

                    //0: 'stopped' and is error,
                    case Enums.Categories.Stopped:
                        data.Torrents = data.Torrents.Where(x => x.Status == 0 && string.IsNullOrEmpty(x.ErrorString)).ToArray();
                        break;

                    case Enums.Categories.Error:
                        data.Torrents = data.Torrents.Where(x => !string.IsNullOrEmpty(x.ErrorString)).ToArray();
                        break;


                    //6: 'seeding',
                    //1: 'checking queue',
                    case Enums.Categories.Inactive:
                        //var array1 = data.Torrents.Where(x=> x.Status == 1).ToArray();
                        var array2 = data.Torrents.Where(x => x.RateDownload <= 0 && x.RateUpload <= 0 && x.Status != 2).ToArray();

                        //int array1OriginalLength = array1.Length;
                        //Array.Resize<TorrentInfo>(ref array1, array1OriginalLength + array2.Length);
                        //Array.Copy(array2, 0, array1, array1OriginalLength, array2.Length);
                        data.Torrents = array2;
                        break;

                    //6: 'seeding',
                    case Enums.Categories.Ended:
                        data.Torrents = data.Torrents.Where(x => x.Status == 6).ToArray();
                        break;
                    case Enums.Categories.All:
                    default:
                        break;
                }

                if (!string.IsNullOrEmpty(FilterText) && FilterText.Contains("{p}:"))
                {
                    var splited = FilterText.Split("{p}:");
                    var filterKey = splited[^1];
                    data.Torrents = data.Torrents.Where(x => FolderCategory.NormalizePath(x.DownloadDir).Equals(filterKey)).ToArray();
                }


                //add or remove torrents to grid
                try
                {

                    var toRm = TorrentList.Except(data.Torrents).ToArray();
                    var toAdd = data.Torrents.Except(TorrentList).ToArray();

                    //remove
                    if (toRm.Length > 0)
                    {
                        Debug.WriteLine($"toRm {toRm.Length}");
                        foreach (var item in toRm)
                        {
                            Dispatcher.Invoke(() => { TorrentList.Remove(item); });
                            Log.Info($"Removed torrent from UI Grid'{item}'");
                        }
                    }




                    foreach (var item in TorrentList)
                    {
                        UpdateTorrentData(TorrentList.IndexOf(item), data.Torrents.First(x => x.Id == item.Id));
                    }

                    //add
                    if (toAdd.Length > 0)
                    {
                        Debug.WriteLine($"toAdd {toAdd.Length}");
                        foreach (var item in toAdd)
                        {
                            var dt = ValidateTorrent(item, true);
                            Dispatcher.Invoke(() => { TorrentList.Add(dt); });
                            Log.Info($"Added torrent to UI Grid '{dt}'");
                        }
                    }



                    toAdd = null;
                    toRm = null;


                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }


                UpdateCategories(allTorrents.Torrents.Select(x => new FolderCategory(x.DownloadDir)).ToList());



                //Dispatcher.Invoke(() =>
                //{
                //    var column = TorrentsDataGrid.Columns.Last();

                //    // Clear current sort descriptions
                //    TorrentsDataGrid.Items.SortDescriptions.Clear();

                //    // Add the new sort description
                //    TorrentsDataGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, ListSortDirection.Descending));

                //    // Apply sort
                //    foreach (var col in TorrentsDataGrid.Columns)
                //    {
                //        col.SortDirection = null;
                //    }
                //    column.SortDirection = ListSortDirection.Ascending;

                //    // Refresh items to display sort
                //    TorrentsDataGrid.Items.Refresh();
                //});


            }
        }

        public TorrentInfo ValidateTorrent(TorrentInfo torrInf, bool skip = false)
        {
            if (torrInf.Status == 1 || torrInf.Status == 2)
            {
                torrInf.PercentDone = torrInf.RecheckProgress;
                return torrInf;
            }

            if (torrInf.Status == 0)
                return torrInf;

            if (!skip)
                if (torrInf.TrackerStats.All(x => x.LastAnnounceSucceeded == false))
                {
                    torrInf.Status = -1;
                }
            return torrInf;

        }
        #endregion
    }
}