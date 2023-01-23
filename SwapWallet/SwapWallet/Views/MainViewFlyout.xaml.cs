using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SwapWallet.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainViewFlyout : ContentPage
    {
        public ListView ListView;

        public MainViewFlyout()
        {
            InitializeComponent();

            BindingContext = new MainViewFlyoutViewModel();
            ListView = MenuItemsListView;
        }

        private class MainViewFlyoutViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<MainViewFlyoutMenuItem> MenuItems { get; set; }

            public MainViewFlyoutViewModel()
            {
                MenuItems = new ObservableCollection<MainViewFlyoutMenuItem>(new[]
                {
                    new MainViewFlyoutMenuItem { Id = 0, Title = "Page 1" },
                    new MainViewFlyoutMenuItem { Id = 1, Title = "Page 2" },
                    new MainViewFlyoutMenuItem { Id = 2, Title = "Page 3" },
                    new MainViewFlyoutMenuItem { Id = 3, Title = "Page 4" },
                    new MainViewFlyoutMenuItem { Id = 4, Title = "Page 5" },
                });
            }

            #region INotifyPropertyChanged Implementation
            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged == null)
                    return;

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
    }
}