using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kebler.ViewModels
{
    public class BaseScreen : Screen
    {
        private string _title;
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        private bool _isWorking;
        public bool IsWorking
        {
            get => _isWorking;
            set => Set(ref _isWorking, value);
        }
    }
}
