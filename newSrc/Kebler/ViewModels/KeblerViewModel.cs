using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Kebler.ViewModels
{
    public partial class KeblerViewModel : BaseScreen
    {
        private readonly IWindowManager _manager;

        public KeblerViewModel(IWindowManager manager)
        {
            _manager = manager;
        }


        /// <summary>
        /// Kebler window Title with version.
        /// </summary>
        public new string Title
        {
            get
            {
#if DEBUG
                return "Kebler [DEBUG]";
#elif PORTABLE
                return "Kebler [Portable]";
#elif RELEASE 
 var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var fileVersionInfo =
                    System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                return $"{nameof(Kebler)} {fileVersionInfo.FileVersion} Beta x64";
#endif
            }
        }


        #region CaliburnEvents
        public async Task ShowConnectionManager()
        {
            var vm = IoC.Get<ConnectionManagerViewModel>();
            await _manager.ShowDialogAsync(vm);
        }

        #endregion
    }
}
