using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VicWeb.Misc
{
    public static class MiscExt
    {
        public static void AddRange<T>(this ObservableCollection<T> self, IEnumerable<T> list)
        {
            foreach (var item in list)
                self.Add(item);
        }
    }
}
