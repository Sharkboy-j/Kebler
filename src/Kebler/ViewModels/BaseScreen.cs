using Caliburn.Micro;

namespace Kebler.ViewModels
{
    public class BaseScreen : Screen
    {
        private bool _isWorking;
        private string _title;

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        public bool IsWorking
        {
            get => _isWorking;
            set => Set(ref _isWorking, value);
        }
    }
}