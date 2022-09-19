using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Humanizer;
using Kebler.Domain.Interfaces;
using Kebler.Domain.Interfaces.Torrents;
using Kebler.Domain.Models;
using Kebler.Domain.Models.Events;
using Kebler.Localisation;
using Kebler.Services;
using Kebler.UI;
using Kebler.UI.Converters;
using Kebler.UI.Models;
using Kebler.ViewModels.DialogWindows;

namespace Kebler.ViewModels
{
    public partial class KeblerViewModel
    {
        private List<Server> _servers;
        private Task _checkerTask;
        private Task _collectDataCycleTask;
        private DateTimeOffset _longActionTimeStart;
        private bool _isLongTaskRunning, _requested, _isAddWindOpened;

        private CancellationTokenSource _cancelTokenSource = new();
        private IStatistic _sessionInfo;
        private IEnumerable<ITorrent> _torrentCache;
        private BytesToUserFriendlyString _bytesToUserFriendlyString = new();
        private ITorrentClientSettings _settings;
        private readonly Dictionary<Categories, int> _categoriesCount = new();
        private Categories _filterCategory;
        private readonly object _syncTorrentList = new();

        private IEnumerable<Server> ServersList => _servers ??= GetServers();



        #region Properties
        private string _filterText;
        public string FilterText
        {
            get => _filterText ?? string.Empty;
            set => Set(ref _filterText, value);
        }

        private Bind<ITorrent> _torrentList = new();
        public Bind<ITorrent> TorrentList
        {
            get => _torrentList;
            set => Set(ref _torrentList, value);
        }

        private BindableCollection<FolderCategory> _folderCategory = new();
        public BindableCollection<FolderCategory> Categories
        {
            get => _folderCategory;
            set => Set(ref _folderCategory, value);
        }

        private BindableCollection<StatusCategory> _categoriesList = new();
        public BindableCollection<StatusCategory> CategoriesList
        {
            get => _categoriesList;
            set => Set(ref _categoriesList, value);
        }

        private string _downloadSpeed;
        public string DownloadSpeed
        {
            get => _downloadSpeed ?? string.Empty;
            set => Set(ref _downloadSpeed, value);
        }

        private string _uploadSpeed;
        public string UploadSpeed
        {
            get => _uploadSpeed ?? string.Empty;
            set => Set(ref _uploadSpeed, value);
        }

        private bool _isConnecting;
        public bool IsConnecting
        {
            get => _isConnecting;
            set => Set(ref _isConnecting, value);
        }

        private string _isConnectedStatusText;
        public string IsConnectedStatusText
        {
            get => _isConnectedStatusText ?? string.Empty;
            set => Set(ref _isConnectedStatusText, value);
        }

        private bool _isErrorOccuredWhileConnecting;
        public bool IsErrorOccuredWhileConnecting
        {
            get => _isErrorOccuredWhileConnecting;
            set => Set(ref _isErrorOccuredWhileConnecting, value);
        }

        private bool _isDoingStuff;
        public bool IsDoingStuff
        {
            get => _isDoingStuff;
            set => Set(ref _isDoingStuff, value);
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set => Set(ref _isConnected, value);
        }

        private IServer _selectedServer;
        public IServer SelectedServer
        {
            get => _selectedServer;
            set => Set(ref _selectedServer, value);
        }

        private IServer _connectedServer;
        public IServer ConnectedServer
        {
            get => _connectedServer;
            set
            {
                Set(ref _connectedServer, value);
                _eventAggregator.PublishOnUIThreadAsync(new ConnectedServerChanged(value));
            }
        }
        #endregion

        /// <summary>
        /// Find server and init connection to him.
        /// </summary>
        private void GetServerAndInitConnection()
        {
            if (IsConnected) return;

            _servers = GetServers();


            if (!ServersList.Any())
                return;


            if (_checkerTask is null)
            {
                _checkerTask = Task.Run(GetLongChecker);
            }

            SelectedServer ??= ServersList.First();
            TryConnect();
        }

        /// <summary>
        /// Get _serversMenu from storageRepository
        /// </summary>
        private static List<Server> GetServers()
        {
            var bdServers = StorageRepository.GetServersList();
            return bdServers.FindAll().ToList();
        }

        private async Task GetLongChecker()
        {
            while (true)
            {
                try
                {
                    if (Application.Current != null && Application.Current.Dispatcher.HasShutdownStarted)
                        throw new TaskCanceledException("Dispatcher.HasShutdownStarted  = true");

                    var date = DateTimeOffset.Now;
                    var time = date - _longActionTimeStart;
                    if (time.TotalSeconds > 2 && _isLongTaskRunning) IsDoingStuff = true;
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                catch
                {
                    // ignored
                }

                await Task.Delay(1000);
            }
        }


        /// <summary>
        /// Ask user for password using UI thread.
        /// </summary>
        /// <returns>Password</returns>
        /// <exception cref="TaskCanceledException"></exception>
        private async Task<string> GetPassword()
        {
            string passwordResult = null;
            await Execute.OnUIThreadAsync(async () =>

            {
                (passwordResult, _) = await _manager.AskPasswordDialog(nameof(Strings.DialogBox_EnterPWD));
            });

            return passwordResult ?? string.Empty;
        }

        /// <summary>
        /// Try connect to selected server.
        /// </summary>
        private async void TryConnect()
        {
            _logger.Info("Try initialize connection");
            IsErrorOccuredWhileConnecting = false;
            IsConnectedStatusText = _localizationProvider.GetLocalizedValue(nameof(Strings.MW_ConnectingText));

            if (SelectedServer != null)
            {
                try
                {
                    IsConnecting = true;
                    string password = null;
                    if (SelectedServer.AskForPassword)
                    {
                        _logger.Info("Manual ask password");
                        password = await GetPassword();


                        if (string.IsNullOrEmpty(password))
                        {
                            IsErrorOccuredWhileConnecting = true;
                            _logger.Info("Manual ask password return empty string");
                            return;
                        }
                    }

                    try
                    {
                        password ??= SecureStorage.DecryptStringAndUnSecure(SelectedServer.Password);

                        _worker.Init(SelectedServer, password);

                        //_transmissionClient = new TransmissionClient(
                        //    url: SelectedServer.FullUriPath,
                        //    login: SelectedServer.UserName,
                        //    password: SelectedServer.AskForPassword ?
                        //         password
                        //        : SecureStorage.DecryptStringAndUnSecure(SelectedServer.Password));
                        _logger.Info("TransmissionClient object created");

                        _collectDataCycleTask = CreateCollectDataCycleTask();
                    }
                    catch (UriFormatException uriEx)
                    {
                        //var msg = $"URI:'{SelectedServer.FullUriPath}' cant be parsed";
                        _logger.Error(uriEx);
                        //Crashes.TrackError(new Exception(msg));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        //Crashes.TrackError(ex);

                        IsConnectedStatusText = ex.Message;
                        IsConnected = false;
                        IsErrorOccuredWhileConnecting = true;
                    }
                }
                catch (TaskCanceledException)
                {
                    IsConnectedStatusText = "Canceled";
                }
                finally
                {
                    IsConnecting = false;
                }
            }

        }

        /// <summary>
        /// Start recursive polling the server.
        /// </summary>
        private async Task CreateCollectDataCycleTask()
        {
            _cancelTokenSource = new CancellationTokenSource();
            try
            {
                var (result, exception) = await _worker.CheckConnectionAsync(SelectedServer);

                if (result)
                {

                    IsConnected = true;
                    _logger.Info($"Connected");
                    ConnectedServer = SelectedServer;
                    var stopwatch = new Stopwatch();
                    if (App.Instance.TorrentsToAdd.Count > 0)
                        OpenPaseedWithArgsFiles();

                    while (IsConnected && !_cancelTokenSource.Token.IsCancellationRequested)
                    {
                        try
                        {
                            if (Application.Current?.Dispatcher != null &&
                                Application.Current.Dispatcher.HasShutdownStarted)
                                throw new TaskCanceledException();
                            stopwatch.Start();

                            _sessionInfo = await _worker.GetSessionStatisticAsync(ConnectedServer, _cancelTokenSource.Token);

                            _torrentCache = (await _worker.TorrentsGetAsync(ConnectedServer, _cancelTokenSource.Token)).ToList();

                            _settings = await _worker.GetTorrentClientSettingsAsync(ConnectedServer, _cancelTokenSource.Token);

                            ////ParseTransmissionServerSettings();
                            //ParseStats();
                            //ProcessParsingTransmissionResponse(_torrentCache.ToList());

                            stopwatch.Stop();
                            _logger.Trace(stopwatch);
                            stopwatch.Reset();
                        }
                        finally
                        {
                            await Task.Factory.StartNew(ProcessDataTask, _cancelTokenSource.Token);
                        }

                        await Task.Delay(ConfigService.DefaultSettingsInstanse.UpdateTime, _cancelTokenSource.Token);
                    }
                }
                else
                {
                    IsErrorOccuredWhileConnecting = true;
                    throw exception;
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (WebException ex)
            {
                string msg;
                switch (ex.Status)
                {
                    case WebExceptionStatus.NameResolutionFailure:
                        msg = $"{_localizationProvider.GetLocalizedValue(nameof(Strings.EX_Host))} '{SelectedServer.FullUriPath}'";
                        break;
                    case WebExceptionStatus.UnknownError:
                        msg = ex.Message;
                        break;
                    default:
                        msg = $"{ex.Status} {Environment.NewLine} {ex.Message}";
                        break;
                }

                _logger.Error(ex.Message);
                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    _manager.ShowMessageBoxDialog(msg);
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                //Crashes.TrackError(ex);
            }
            finally
            {
                _cancelTokenSource.Cancel();
                ConnectedServer = null;
                IsConnecting = false;
                //TorrentList = new Bind<TorrentInfo>();
                IsConnected = false;
                Categories?.Clear();
                IsConnectedStatusText = DownloadSpeed = UploadSpeed = string.Empty;
                _logger.Info("Disconnected from server");
                if (_requested)
                    await _eventAggregator.PublishOnBackgroundThreadAsync(new ReconnectAllowed(),
                        CancellationToken.None);
            }
        }

        private Task ProcessDataTask()
        {
            ///ParseTransmissionServerSettings();
            ParseStats();

            if (!IsConnected)
                return Task.CompletedTask;

            var preProcessingData = _torrentCache?.ToList() ?? new List<ITorrent>();


            //update counter on categories

            _categoriesCount.Clear();

            _categoriesCount.Add(Kebler.Domain.Models.Categories.All, preProcessingData.Count());

            _categoriesCount.Add(Kebler.Domain.Models.Categories.Downloading, preProcessingData.Where(x => x.Status == 3 || x.Status == 4).ToArray().Length);
            var counts = preProcessingData.Where(x => x.Status == 4 || x.Status == 6 || x.Status == 2)
                .ToArray();
            counts = counts.Where(x => x.RateDownload > 1 || x.RateUpload > 1).ToArray();
            _categoriesCount.Add(Kebler.Domain.Models.Categories.Active, counts.Length);
            _categoriesCount.Add(Kebler.Domain.Models.Categories.Stopped, preProcessingData.Where(x => x.Status == 0 && string.IsNullOrEmpty(x.ErrorString))
                .ToArray().Length);
            _categoriesCount.Add(Kebler.Domain.Models.Categories.Error, preProcessingData.Where(x => !string.IsNullOrEmpty(x.ErrorString)).ToArray().Length);
            _categoriesCount.Add(Kebler.Domain.Models.Categories.Inactive, preProcessingData.Where(x => x.RateDownload <= 0 && x.RateUpload <= 0 && x.Status != 2)
                .ToArray().Length);
            _categoriesCount.Add(Kebler.Domain.Models.Categories.Ended, preProcessingData.Where(x => x.Status == 6).ToArray().Length);

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
                    case Kebler.Domain.Models.Categories.Downloading:
                        preProcessingData = preProcessingData.Where(x => x.Status == 3 || x.Status == 4).ToList();
                        break;

                    //4: 'downloading',
                    //6: 'seeding',
                    //2: 'checking',
                    case Kebler.Domain.Models.Categories.Active:
                        preProcessingData = preProcessingData.Where(x => x.Status == 4 || x.Status == 6 || x.Status == 2)
                            .ToList();
                        preProcessingData = preProcessingData.Where(x => x.RateDownload > 1 || x.RateUpload > 1).ToList();
                        break;

                    //0: 'stopped' and is error,
                    case Kebler.Domain.Models.Categories.Stopped:
                        preProcessingData = preProcessingData.Where(x => x.Status == 0 && string.IsNullOrEmpty(x.ErrorString))
                            .ToList();
                        break;

                    case Kebler.Domain.Models.Categories.Error:
                        preProcessingData = preProcessingData.Where(x => !string.IsNullOrEmpty(x.ErrorString)).ToList();
                        break;


                    //6: 'seeding',
                    //1: 'checking queue',
                    case Kebler.Domain.Models.Categories.Inactive:
                        //var array1 = data.Torrents.Where(x=> x.Status == 1).ToArray();
                        preProcessingData = preProcessingData.Where(x => x.RateDownload <= 0 && x.RateUpload <= 0 && x.Status != 2)
                            .ToList();

                        //int array1OriginalLength = array1.Length;
                        //Array.Resize<TorrentInfo>(ref array1, array1OriginalLength + array2.Length);
                        //Array.Copy(array2, 0, array1, array1OriginalLength, array2.Length);
                        break;

                    //6: 'seeding',
                    case Kebler.Domain.Models.Categories.Ended:
                        preProcessingData = preProcessingData.Where(x => x.Status == 6).ToList();
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

                        preProcessingData = preProcessingData.Where(x => FolderCategory.NormalizePath(x.DownloadDir).Equals(filterKey)).ToList();
                    }
                    else
                    {
                        preProcessingData = preProcessingData.Where(x => x.Name.ToLower().Contains(FilterText.ToLower())).ToList();
                    }
                }

                //for (var i = 0; i < data.Count(); i++)
                //    data[i] = ValidateTorrent(data.Torrents[i]);

                //Debug.WriteLine("S" + DateTime.Now.ToString("HH:mm:ss:ffff"));

                UpdateOnUi(preProcessingData);

                //Debug.WriteLine("E" + DateTime.Now.ToString("HH:mm:ss:ffff"));

                UpdateCategories(preProcessingData);


            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Update torrents listView on UI thread.
        /// </summary>
        private void UpdateOnUi(IList<ITorrent> info)
        {
            var toRm = new List<ITorrent>(TorrentList.Except(info).ToList());

            if (toRm.Any())
                foreach (var item in toRm)
                {
                    TorrentList.RemoveWithoutNotify(item);
                }

            foreach (var item in info)
            {
                var ind = TorrentList.IndexOf(item);
                if (ind == -1)
                {
                    TorrentList.Add(item);
                }
                else
                {
                    TorrentList[ind].UpdateData(item);
                }
            }
        }

        /// <summary>
        /// Update folder categories on UI thread.
        /// </summary>
        private void UpdateCategories(IList<ITorrent> preProcessingData)
        {

            var dirrectories = preProcessingData.Select(x => new FolderCategory(x.DownloadDir)).ToList();
            var dirs = dirrectories.Distinct().ToList();

            var cats = new List<FolderCategory>();
            var toUpd = new List<FolderCategory>();

            foreach (var cat in dirs)
            {
                cat.Count = preProcessingData.Count(x =>
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
                _logger.Info("Remove categories" + string.Join(", ", toRm));

                foreach (var itm in toRm) Application.Current.Dispatcher.Invoke(() => { Categories.Remove(itm); });
            }

            //update counter for existing
            foreach (var itm in toUpd)
            {
                Categories.First(x => x.Equals(itm)).Count = itm.Count;
            }

            if (toAdd.Any())
            {
                _logger.Info("Add categories" + string.Join(", ", toAdd));
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
                                        $"{cat.FullPath} ({preProcessingData.Count(x => FolderCategory.NormalizePath(x.DownloadDir) == itm.FullPath)})";
                            itm.Title =
                                $"{itm.FullPath} ({preProcessingData.Count(x => FolderCategory.NormalizePath(x.DownloadDir) == itm.FullPath)})";
                            Categories.Add(itm);
                        }
                    });
            }


            if (_isAddWindOpened)
            {
                _eventAggregator.PublishOnUIThreadAsync(new DownlaodCategoriesChanged(Categories));
            }
        }

        public void OpenPaseedWithArgsFiles()
        {
            if (App.Instance.TorrentsToAdd.Count > 0)
            {
                Task.Factory.StartNew(() =>
                {
                    Execute.OnUIThread(OpenTorrent);
                }, _cancelTokenSource.Token);
            }
        }

        private void ParseStats()
        {
            try
            {
                Thread.CurrentThread.CurrentUICulture = _localizationManager.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = _localizationManager.CurrentCulture;

                //IsConnectedStatusText = $"Transmission {_sessionInfo?.Version} (RPC:{_sessionInfo?.RpcVersion})     " +
                //                        $"      {_localizationProvider.GetLocalizedValue(nameof(Strings.Stats_Uploaded))} {Utils.GetSizeString(_stats.CumulativeStats.UploadedBytes)}" +
                //                        $"      {_localizationProvider.GetLocalizedValue(nameof(Strings.Stats_Downloaded))}  {Utils.GetSizeString(_stats.CumulativeStats.DownloadedBytes)}" +
                //                        $"      {_localizationProvider.GetLocalizedValue(nameof(Strings.Stats_ActiveTime))}  {TimeSpan.FromSeconds(_stats.CurrentStats.SecondsActive).ToPrettyFormat()}";

                if (_sessionInfo != null)
                {
                    IsConnectedStatusText = ConnectedServer.ClientType switch
                    {
                        ClientType.Transmission =>
                            $"{nameof(ClientType.Transmission)} {_sessionInfo?.Version} (RPC:{_sessionInfo.RpcVersion})         " +
                            $"      {_localizationProvider.GetLocalizedValue(nameof(Strings.Stats_Uploaded))} {_bytesToUserFriendlyString.Convert(_sessionInfo.UploadedBytes)}" +
                            $"      {_localizationProvider.GetLocalizedValue(nameof(Strings.Stats_Downloaded))}  {_bytesToUserFriendlyString.Convert(_sessionInfo.DownloadedBytes)}" +
                            $"      {_localizationProvider.GetLocalizedValue(nameof(Strings.Stats_ActiveTime))}  {TimeSpan.FromSeconds(_sessionInfo.Uptime).Humanize(5, culture: _localizationManager.CurrentCulture)}",

                        ClientType.QBittorrent =>
                            $"{nameof(ClientType.QBittorrent)} {_sessionInfo?.Version}         " +
                            $"{_localizationProvider.GetLocalizedValue(nameof(Strings.Stats_Uploaded))} {_bytesToUserFriendlyString.Convert(_sessionInfo.UploadedBytes)} " +
                            $"{_localizationProvider.GetLocalizedValue(nameof(Strings.Stats_Downloaded))} {_bytesToUserFriendlyString.Convert(_sessionInfo.DownloadedBytes)}",
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    var dSpeedText = BytesToUserFriendlySpeed.GetSizeString(_sessionInfo.DownloadSpeed);
                    var uSpeedText = BytesToUserFriendlySpeed.GetSizeString(_sessionInfo.UploadSpeed);

                    var dSpeed = string.IsNullOrEmpty(dSpeedText) ? "0 b/s" : dSpeedText;
                    var uSpeed = string.IsNullOrEmpty(uSpeedText) ? "0 b/s" : uSpeedText;
                    var altUp = _settings?.AlternativeSpeedEnabled == true ? $" [{BytesToUserFriendlySpeed.GetSizeString(_settings.AlternativeSpeedUp * 1000)}]" : string.Empty;
                    var altD = _settings?.AlternativeSpeedEnabled == true ? $" [{BytesToUserFriendlySpeed.GetSizeString(_settings.AlternativeSpeedDown * 1000)}]" : string.Empty;

                    DownloadSpeed = $"D: {dSpeed}{altD}";
                    UploadSpeed = $"U: {uSpeed}{altUp}";
                }

            }
            catch (Exception ex)
            {
                //#1665431308
                _logger.Error(ex);
                //Crashes.TrackError(ex);
            }
        }

        private async void OpenTorrent()
        {
            State = WindowState.Normal;
            Application.Current.MainWindow?.Activate();
            await OpenTorrent(App.Instance.TorrentsToAdd);
            App.Instance.TorrentsToAdd.Clear();
        }

        public void Retry()
        {

        }
    }
}
