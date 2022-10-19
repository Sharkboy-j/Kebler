using Caliburn.Micro;
using Kebler.Models.Interfaces;
using Kebler.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kebler.Core.Models;
using Kebler.TransmissionTorrentClient.Models;

namespace Kebler.ViewModels
{
    public class DialogBoxViewModel : BoxViewModel
    {
        private string _dialogTextBoxText, _dialogCheckBoxText;
        private bool _showTextBox, _showPasswordBox, _showCombo, _showCheckBox, _dialogCheckBoxValue;
        private IDialogBox _view;
        private object _value;
        private int _selectedIndex;
        private IEnumerable<object> _values;
        private string _emptyText;

        public DialogBoxViewModel(string message, string boxHint, bool isPassword,
            Enums.MessageBoxDilogButtons buttons = Enums.MessageBoxDilogButtons.OkCancel, string emptyText = null)
        {
            ShowPasswordBox = isPassword;
            ShowTextBox = !ShowPasswordBox;
            Message = message;
            LogoVisibility = true;
            _emptyText = emptyText;
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


        //public DialogBoxViewModel(string message, IEnumerable<string> values, string val = "", Enums.MessageBoxDilogButtons buttons = Enums.MessageBoxDilogButtons.OkCancel)
        //{
        //    ShowPasswordBox = false;
        //    ShowTextBox = false;
        //    Message = message;
        //    LogoVisibility = true;
        //    Values = values;
        //    Value = val;
        //    ShowCombo = true;
        //    ShowCheckBox = false;
        //    ShowButtons(buttons);
        //}

        public DialogBoxViewModel(
            string message,
            IEnumerable<string> values,
            string val = "",
            Enums.MessageBoxDilogButtons buttons = Enums.MessageBoxDilogButtons.OkCancel,
            string checkBoxText = "")
        {
            ShowPasswordBox = false;
            ShowTextBox = false;
            Message = message;
            LogoVisibility = true;
            Values = values;
            Value = val;
            ShowCombo = true;
            ShowCheckBox = string.IsNullOrEmpty(checkBoxText) ? false : true;
            DialogCheckBoxText = checkBoxText;
            DialogCheckBoxValue = true;
            ShowButtons(buttons);
        }

        public bool ShowTextBox
        {
            get => _showTextBox;
            set => Set(ref _showTextBox, value);
        }  
        
        public bool DialogCheckBoxValue
        {
            get => _dialogCheckBoxValue;
            set => Set(ref _dialogCheckBoxValue, value);
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
        
        public string DialogCheckBoxText
        {
            get => _dialogCheckBoxText;
            set => Set(ref _dialogCheckBoxText, value);
        }

        public bool ShowCombo
        {
            get => _showCombo;
            set => Set(ref _showCombo, value);
        }

        public bool ShowCheckBox
        {
            get => _showCheckBox;
            set => Set(ref _showCheckBox, value);
        }

        public IEnumerable<object> Values
        {
            get => _values;
            set => Set(ref _values, value);
        }

        public object Value
        {
            get => _value;
            set => Set(ref _value, value);
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => Set(ref _selectedIndex, value);
        }



        protected override void OnViewAttached(object view, object context)
        {
            _view = view as IDialogBox;
            if (ShowPasswordBox)
                _view?.PWD.Focus();
            else if (_values?.Count() == 0)
                _view?.TBX.Focus();
            else
                _view?.CBX.Focus();

            base.OnViewAttached(view, context);
        }

        public override async void OkYes()
        {
            async Task ShowError()
            {
                if (_emptyText is not null)
                    await MessageBoxViewModel.ShowDialog(_emptyText, new WindowManager(), LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.Error)));
            }

            if (_showPasswordBox)
            {
                if (string.IsNullOrEmpty(_view.PWD.Password))
                {
                    await ShowError();
                    return;
                }

                Value = _view.PWD.Password;
                _view.PWD.Clear();
            }
            else if (ShowTextBox)
            {
                if (string.IsNullOrEmpty(_dialogTextBoxText))
                {
                    await ShowError();
                    return;
                }

                Value = _dialogTextBoxText;
            }

            base.OkYes();
        }
    }
}