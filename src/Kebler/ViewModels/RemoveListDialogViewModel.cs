using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kebler.ViewModels
{
    public class RemoveListDialogViewModel : BaseScreen
    {
        public RemoveListDialogViewModel(string title, IEnumerable<string> names)
        {
            Title = title;
            Names = names;
        }

        public void No()
        {
            TryCloseAsync(false);
        }

        public void Yes()
        {
            TryCloseAsync(true);
        }

        public IEnumerable<string> _names;

        public IEnumerable<string> Names
        {
            get => _names;
            set => Set(ref _names, value);
        }

    }
}
