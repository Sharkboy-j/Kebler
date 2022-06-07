using Caliburn.Micro;

namespace Kebler.UI.Models
{
    public class Bind<T> : BindableCollection<T>
    {
        /// <summary>
        /// Need cuz fucking BindableCollection on Notify reset selected item and index for ListView =\
        /// </summary>
        /// <param name="item"></param>
        public void RemoveWithoutNotify(T item)
        {
            if (PlatformProvider.Current.PropertyChangeNotificationsOnUIThread)
            {
                var index = IndexOf(item);
                RemoveAt(index);
            }
            else
            {
                var index = IndexOf(item);
                RemoveAt(index);
            }
        }

        /// <summary>
        /// need same for RemoveWithoutNotify
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public new void SetItem(int index, T item)
        {
            if (PlatformProvider.Current.PropertyChangeNotificationsOnUIThread)
            {
                OnUIThread(() => base.SetItemBase(index, item));
            }
            else
            {
                SetItemBase(index, item);
            }
        }
    }
}
