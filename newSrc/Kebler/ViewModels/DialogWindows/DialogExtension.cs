using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using Kebler.Domain.Interfaces;
using Kebler.Domain.Models;

namespace Kebler.ViewModels.DialogWindows
{
    public static class DialogExtension
    {
        public static Task<bool?> ShowMessageBoxDialog(this IWindowManager manager, in string msg, in string titile = "",
            in MessageBoxDilogButtons buttons = MessageBoxDilogButtons.Ok)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            return manager.ShowDialogAsync(new MessageBoxViewModel(msg, titile, buttons, true));
        }

        public static async Task<(string, bool?)> AskPasswordDialog(this IWindowManager manager, string message)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            var provider = IoC.Get<ICustomLocalizationProvider>();
            var dialog = new DialogBoxViewModel(message, string.Empty, true, provider);

            var dialogResult = await manager.ShowDialogAsync(dialog);

            return ((string)dialog.Value, dialogResult);
        }
    }
}
