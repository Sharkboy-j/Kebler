using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kebler.ViewModels
{
    public partial class KeblerViewModel : BaseScreen
    {
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
                System.Diagnostics.FileVersionInfo fileVersionInfo =
                    System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                return $"{nameof(Kebler)} {fileVersionInfo.FileVersion} Beta x64";
#endif
            }
        }
    }
}
