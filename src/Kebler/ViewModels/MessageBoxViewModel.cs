using System.Threading.Tasks;
using Caliburn.Micro;
using static Kebler.Models.Enums;

namespace Kebler.ViewModels
{
    public class MessageBoxViewModel : BoxViewModel
    {
        public MessageBoxViewModel(string message, string title = "",
            MessageBoxDilogButtons buttons = MessageBoxDilogButtons.Ok, bool showLogo = false)
        {
            MinWidth = 350;

            Message = message;

            LogoVisibility = showLogo;

            ShowButtons(buttons);
        }


        public static Task<bool?> ShowDialog(string msg, IWindowManager manager = null, string titile = "",
            MessageBoxDilogButtons buttons = MessageBoxDilogButtons.Ok)
        {
            var mgr = manager ?? new WindowManager();
            return mgr.ShowDialogAsync(new MessageBoxViewModel(msg, titile, buttons, true));
        }
    }
}