using Kebler.Models;
using System.Collections.Generic;
using Caliburn.Micro;

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
        public BindableCollection<StatusCategory> CategoriesList
        {
            get => _categoriesList;
            set=> Set(ref _categoriesList, value);
        }

        /// <summary>
        /// Preference to show/hide categories count.
        /// </summary>
        public bool ShowCategoriesCount
        {
            get => _showCategoriesCount;
            set => Set(ref _showCategoriesCount, value);
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
#elif RELEASE 
 var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                System.Diagnostics.FileVersionInfo fileVersionInfo =
                    System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                return $"{nameof(Kebler)} {fileVersionInfo.FileVersion} Beta x64";
#endif
            }
        }
    }

    public partial class KeblerViewModel
    {
        private BindableCollection<StatusCategory> _categoriesList = new();
        private bool _showCategoriesCount = true;
        private readonly Dictionary<Enums.Categories, int> _categoriesCount = new();
    }
}
