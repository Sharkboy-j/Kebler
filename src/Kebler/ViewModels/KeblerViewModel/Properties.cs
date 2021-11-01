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
using Caliburn.Micro;

namespace Kebler.ViewModels
{
    /// <summary>
    /// Properties for Kebler view.
    /// </summary>
    public partial class KeblerViewModel
    {
        private BindableCollection<StatusCategory> _categoriesList = new();


        /// <summary>
        /// Torrent type categories.
        /// </summary>
        public BindableCollection<StatusCategory> CategoriesList
        {
            get => _categoriesList;
            set
            {
                var dd = Set(ref _categoriesList, value);
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

    public partial class KeblerViewModel
    {
        private Dictionary<Enums.Categories, int> _categoriesCount = new();
    }
}
