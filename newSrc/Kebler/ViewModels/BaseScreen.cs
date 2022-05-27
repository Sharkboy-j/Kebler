using Caliburn.Micro;

namespace Kebler.ViewModels
{
    public class BaseScreen : Screen
    {
        public BaseScreen()
        {
            ShowHeaderLine = true;
        }

        private bool _isWorking;
        private bool _showHeaderLine;
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
        
        public bool ShowHeaderLine
        {
            get => _showHeaderLine;
            set => Set(ref _showHeaderLine, value);
        }


    }
}
