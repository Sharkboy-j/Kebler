using Kebler.Models;
using Kebler.Models.Interfaces;

namespace Kebler.ViewModels
{
    public class DialogBoxViewModel : BoxViewModel
    {
        private string _dialogTextBoxText;
        private bool _showTextBox, _showPasswordBox;
        private IDialogBox _view;
        public string Value;

        public DialogBoxViewModel(string message, string boxHint, bool isPassword,
            Enums.MessageBoxDilogButtons buttons = Enums.MessageBoxDilogButtons.OkCancel)
        {
            ShowPasswordBox = isPassword;
            ShowTextBox = !ShowPasswordBox;
            Message = message;
            LogoVisibility = true;
            if (!isPassword)
            {
                ShowTextBox = true;
                DialogTextBoxText = boxHint;
            }
            else
            {
                ShowPasswordBox = true;
            }

            ShowButtons(buttons);
        }

        public bool ShowTextBox
        {
            get => _showTextBox;
            set => Set(ref _showTextBox, value);
        }

        public bool ShowPasswordBox
        {
            get => _showPasswordBox;
            set => Set(ref _showPasswordBox, value);
        }

        public string DialogTextBoxText
        {
            get => _dialogTextBoxText;
            set => Set(ref _dialogTextBoxText, value);
        }


        protected override void OnViewAttached(object view, object context)
        {
            _view = view as IDialogBox;
            if (ShowPasswordBox)
                _view?.PWD.Focus();
            else
                _view?.TBX.Focus();

            base.OnViewAttached(view, context);
        }

        public override void OkYes()
        {
            if (_showPasswordBox)
            {
                Value = _view.PWD.Password;
                _view.PWD.Clear();
            }
            else
            {
                Value = _dialogTextBoxText;
            }

            base.OkYes();
        }
    }
}