using Kebler.Services;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable once CheckNamespace

namespace Kebler.ViewModels
{
    /// <summary>
    /// Torrent Grid right click menu or top bar actions.
    /// </summary>
    public partial class KeblerViewModel
    { 
        /// <summary>
        /// Rename torrent.
        /// </summary>
        public async void Rename()
        {
            if (selectedIDs.Length > 1)
            {
                await MessageBoxViewModel.ShowDialog(LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.MSG_OnlyOneTorrent)), manager);
                return;
            }

            if (selectedIDs.Length < 1)
            {
                await MessageBoxViewModel.ShowDialog(LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.MSG_SelectOneTorrent)), manager);
                return;
            }

            if (!IsConnected || SelectedTorrent is null) return;
            
            var sel = SelectedTorrent;
            var dialog = new DialogBoxViewModel(LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.MSG_InterNewName)), sel.Name, false);

            var newName = dialog.Value.ToString();
            
            if (await manager.ShowDialogAsync(dialog) is not true || _transmissionClient is null || newName is null)
                return;
            
            var resp = await _transmissionClient.TorrentRenamePathAsync(sel.Id, sel.Name, newName,
                _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);
        }
    }
}