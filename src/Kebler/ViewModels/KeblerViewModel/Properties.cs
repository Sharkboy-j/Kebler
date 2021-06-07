using Kebler.Models;
using Kebler.Resources;
using Kebler.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kebler.ViewModels
{
    /// <summary>
    /// Properties for Kebler view.
    /// </summary>
    public partial class KeblerViewModel
    {


        /// <summary>
        /// Torrent type categories.
        /// </summary>
        public IReadOnlyCollection<StatusCategory> CategoriesList
        {
            get
            {
                var lst = new List<StatusCategory>()
                {
                    new StatusCategory
                    {
                        Title = LocalizationProvider.GetLocalizedValue(nameof(Strings.Cat_AllTorrents)),
                        Cat = Enums.Categories.All
                    },
                    new StatusCategory
                    {
                        Title = LocalizationProvider.GetLocalizedValue(nameof(Strings.Cat_Downloading)),
                        Cat = Enums.Categories.Downloading
                    },
                    new StatusCategory
                    {
                        Title = LocalizationProvider.GetLocalizedValue(nameof(Strings.Cat_Active)),
                        Cat = Enums.Categories.Active
                    },
                    new StatusCategory
                    {
                        Title = LocalizationProvider.GetLocalizedValue(nameof(Strings.Cat_InActive)),
                        Cat = Enums.Categories.Inactive
                    },
                    new StatusCategory
                    {
                        Title = LocalizationProvider.GetLocalizedValue(nameof(Strings.Cat_Ended)),
                        Cat = Enums.Categories.Ended
                    },
                    new StatusCategory
                    {
                        Title = LocalizationProvider.GetLocalizedValue(nameof(Strings.Cat_Stopped)),
                        Cat = Enums.Categories.Stopped
                    },
                    new StatusCategory
                    {
                        Title = LocalizationProvider.GetLocalizedValue(nameof(Strings.Cat_Error)),
                        Cat = Enums.Categories.Error
                    }
                };

                return new ReadOnlyCollection<StatusCategory>(lst);
            }
        }

        /// <summary>
        /// Kebler window Title with version.
        /// </summary>
        public static string HeaderTitle
        {
            get
            {
#if DEBUG
                return "Kebler [DEBUG]";
#elif PORTABLE
                return "Kebler [Portable]";
#else
                var assembly = Assembly.GetExecutingAssembly();
                System.Diagnostics.FileVersionInfo fileVersionInfo =
                    System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                return $"{nameof(Kebler)} {fileVersionInfo.FileVersion} Beta";
#endif
            }
        }
    }
}
