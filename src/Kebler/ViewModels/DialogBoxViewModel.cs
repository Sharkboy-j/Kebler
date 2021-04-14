using Kebler.Models;
using Kebler.Models.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Kebler.ViewModels
{
    public class DialogBoxViewModel : BoxViewModel
    {
        private string _dialogTextBoxText;
        private bool _showTextBox, _showPasswordBox, _showCombo;
        private IDialogBox _view;
        private object _value;
        private int _selectedIndex;
        private IEnumerable<object> _values;

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


        public DialogBoxViewModel(string message, IEnumerable<string> values, string val = "",  Enums.MessageBoxDilogButtons buttons = Enums.MessageBoxDilogButtons.OkCancel)
        {
            ShowPasswordBox = false;
            ShowTextBox = false;
            Message = message;
            LogoVisibility = true;
            Values = values;
            Value = val;
            ShowCombo = true;
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

        public bool ShowCombo
        {
            get => _showCombo;
            set => Set(ref _showCombo, value);
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

        public override void OkYes()
        {
            if (_showPasswordBox)
            {
                Value = _view.PWD.Password;
                _view.PWD.Clear();
            }
            else if(ShowTextBox)
            {
                Value = _dialogTextBoxText;
            }

            base.OkYes();
        }
    }
}