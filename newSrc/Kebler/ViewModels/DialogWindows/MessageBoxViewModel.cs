using Kebler.Domain.Models;

namespace Kebler.ViewModels.DialogWindows
{
    public class MessageBoxViewModel : BoxViewModel
    {
        public MessageBoxViewModel(in string message, in string title = null,
            in MessageBoxDilogButtons buttons = MessageBoxDilogButtons.Ok, in bool showLogo = false)
        {
            Message = message;
            LogoVisibility = showLogo;
            ShowButtons(buttons);
        }
    }
}