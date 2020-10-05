using System.Collections.Generic;

namespace Kebler.ViewModels
{
    public class RemoveListDialogViewModel : BaseScreen
    {
        public IEnumerable<string> _names;

        public RemoveListDialogViewModel(string title, IEnumerable<string> names)
        {
            Title = title;
            Names = names;
        }

        public IEnumerable<string> Names
        {
            get => _names;
            set => Set(ref _names, value);
        }

        public void No()
        {
            TryCloseAsync(false);
        }

        public void Yes()
        {
            TryCloseAsync(true);
        }
    }
}