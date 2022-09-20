using System.Threading.Tasks;
using Caliburn.Micro;
using Kebler.Core.Models;
using Kebler.TransmissionTorrentClient.Models;

namespace Kebler.ViewModels
{
    public class MessageBoxViewModel : BoxViewModel
    {
        public MessageBoxViewModel(string message, string title = null,
            Enums.MessageBoxDilogButtons buttons = Enums.MessageBoxDilogButtons.Ok, bool showLogo = false)
        {
            MinWidth = 350;
            Message = message;
            LogoVisibility = showLogo;
            ShowButtons(buttons);
        }


        public static async Task<bool?> ShowDialog(string msg, IWindowManager manager = null, string titile = "",
            Enums.MessageBoxDilogButtons buttons = Enums.MessageBoxDilogButtons.Ok)
        {
            var mgr = manager ?? new WindowManager();
            var vm = new MessageBoxViewModel(msg, titile, buttons, true);
            var resp =  await mgr.ShowDialogAsync(vm);
            return vm.Result;
        }
    }
}