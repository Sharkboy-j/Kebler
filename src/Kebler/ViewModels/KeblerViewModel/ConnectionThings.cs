using System;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Kebler.Core.Models;
using Kebler.Models;
using Kebler.Services;
using Kebler.TransmissionTorrentClient;
using Kebler.TransmissionTorrentClient.Models;
using Microsoft.AppCenter.Crashes;
// ReSharper disable once CheckNamespace


namespace Kebler.ViewModels
{
    /// <summary>
    /// Connection things.
    /// </summary>
    public partial class KeblerViewModel
    {

        /// <summary>
        /// Retry connect to server.
        /// </summary>
        public void Retry()
        {
            IsErrorOccuredWhileConnecting = false;
            _servers = GetServers();
            GetServerAndInitConnection();
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
                var dialog = new DialogBoxViewModel(LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.DialogBox_EnterPWD)), string.Empty, true);

                await manager.ShowDialogAsync(dialog);

                passwordResult = dialog.Value.ToString();
            });

            return passwordResult ?? string.Empty;
        }


        /// <summary>
        /// Get servers from storageRepository
        /// </summary>
        private List<Server> GetServers()
        {
            var bdServers = StorageRepository.GetServersList();
            return bdServers.FindAll().ToList();
        }


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
                _checkerTask = GetLongChecker();
                _checkerTask.Start();
            }

            SelectedServer = SelectedServer ?? ServersList.First();
            TryConnect();
        }


        /// <summary>
        /// Try connect to selected server.
        /// </summary>
        private async void TryConnect()
        {
            Log.Info("Try initialize connection");
            IsErrorOccuredWhileConnecting = false;
            IsConnectedStatusText = LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.MW_ConnectingText));

            if (SelectedServer != null)
            {
                try
                {
                    IsConnecting = true;
                    var password = string.Empty;
                    if (SelectedServer.AskForPassword)
                    {
                        Log.Info("Manual ask password");
                        password = await GetPassword();


                        if (string.IsNullOrEmpty(password))
                        {
                            IsErrorOccuredWhileConnecting = true;
                            Log.Info("Manual ask password return empty string");
                            return;
                        }
                    }

                    try
                    {
                        _transmissionClient = new TransmissionClient(
                            url: SelectedServer.FullUriPath,
                            login: SelectedServer.UserName,
                            password: SelectedServer.AskForPassword ?
                                 password
                                : SecureStorage.DecryptStringAndUnSecure(SelectedServer.Password));
                        Log.Info("TransmissionClient object created");

                        StartCycle();
                    }
                    catch (UriFormatException uriEx)
                    {
                        var msg = $"URI:'{SelectedServer.FullUriPath}' cant be parsed";
                        Log.Error(uriEx);
                        Crashes.TrackError(new Exception(msg));
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                        Crashes.TrackError(ex);

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
        private void StartCycle()
        {
            _cancelTokenSource = new CancellationTokenSource();
            var token = _cancelTokenSource.Token;
            _whileCycleTask = new Task(async () =>
            {
                if (_transmissionClient is null)
                    return;

                try
                {
                    var info = await Get(
                        task: _transmissionClient.GetSessionInformationAsync(_cancelTokenSource.Token),
                        statusText: LocalizationProvider.GetLocalizedValue(
                            nameof(Resources.Strings.MW_StatusText_Session)));

                    if (IsResponseStatusOk(info.Response))
                    {
                        _sessionInfo = info.Value;
                        IsConnected = true;
                        Log.Info($"Connected {_sessionInfo.Version}");
                        ConnectedServer = SelectedServer;
                        var st = new Stopwatch();
                        if (App.Instance.TorrentsToAdd.Count > 0)
                            OpenPaseedWithArgsFiles();

                        while (IsConnected && !token.IsCancellationRequested)
                        {
                            if (Application.Current?.Dispatcher != null &&
                                Application.Current.Dispatcher.HasShutdownStarted)
                                throw new TaskCanceledException();
                            st.Start();

                            if (_transmissionClient != null)
                            {
                                _stats = (await Get(
                                    _transmissionClient.GetSessionStatisticAsync(_cancelTokenSource.Token),
                                    LocalizationProvider.GetLocalizedValue(
                                        nameof(Resources.Strings.MW_StatusText_Stats)))).Value;

                                allTorrents =
                                    (await Get(
                                        _transmissionClient.TorrentGetAsync(
                                            State == WindowState.Minimized ? TorrentFields.WORK_HIDDEN : TorrentFields.WORK,
                                            _cancelTokenSource.Token),
                                        LocalizationProvider.GetLocalizedValue(
                                            nameof(Resources.Strings.MW_StatusText_Torrents)))).Value;
                                _settings = (await Get(
                                    _transmissionClient.GetSessionSettingsAsync(_cancelTokenSource.Token),
                                    LocalizationProvider.GetLocalizedValue(
                                        nameof(Resources.Strings.MW_StatusText_Settings)))).Value;

                                if (allTorrents != null)
                                {
                                    ParseTransmissionServerSettings();
                                    ParseStats();

                                    if (allTorrents?.Clone() is TransmissionTorrents data)
                                        ProcessParsingTransmissionResponse(data);
                                }

                            }
                            else
                            {
                                break;
                            }

                            st.Stop();
                            Log.Trace(st);
                            st.Reset();

                            await Task.Delay(ConfigService.Instanse.UpdateTime, token);
                        }
                    }
                    else
                    {
                        IsErrorOccuredWhileConnecting = true;
                        if (info.Response.WebException != null)
                            throw info.Response.WebException;

                        if (info.Response.CustomException != null)
                            throw info.Response.CustomException;

                        if (info.Response.HttpWebResponse != null)
                            throw new Exception(info.Response.HttpWebResponse.StatusCode.ToString());
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
                            msg =
                                $"{LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.EX_Host))} '{SelectedServer.FullUriPath}'";
                            break;
                        case WebExceptionStatus.UnknownError:
                            msg = ex.Message;
                            break;
                        default:
                            msg = $"{ex.Status} {Environment.NewLine} {ex?.Message}";
                            break;
                    }

                    Log.Error(ex.Message);
                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        MessageBoxViewModel.ShowDialog(msg, manager, string.Empty);
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    Crashes.TrackError(ex);
                }
                finally
                {
                    ConnectedServer = null;
                    IsConnecting = false;
                    TorrentList = new Bind<TorrentInfo>();
                    IsConnected = false;
                    Categories?.Clear();
                    IsConnectedStatusText = DownloadSpeed = UploadSpeed = string.Empty;
                    Log.Info("Disconnected from server");
                    if (requested)
                        await _eventAggregator.PublishOnBackgroundThreadAsync(new Messages.ReconnectAllowed(),
                            CancellationToken.None);
                }
            }, token);

            _whileCycleTask.Start();
        }

        private bool IsResponseStatusOk(ITransmissionResponse resp)
        {
            if (resp.WebException != null)
            {
                //Application.Current.Dispatcher.Invoke(() =>
                //{
                //    var msg = resp.WebException.Status switch
                //    {
                //        WebExceptionStatus.NameResolutionFailure =>
                //            $"{LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.EX_Host))} '{SelectedServer.FullUriPath}'",
                //        _ => $"{resp.WebException.Status} {Environment.NewLine} {resp.WebException?.Message}"
                //    };

                //    MessageBoxViewModel.ShowDialog(msg, manager, string.Empty);
                //});
                return false;
            }

            return true;
        }
    }
}