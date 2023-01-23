using System;
using System.Threading.Tasks;
using SwapWallet.Services;
using SwapWallet.ViewModels;
using SwapWallet.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SwapWallet
{
    public partial class App : Application
    {
        public static App Instance;
        public static Size WindowSize { get; set; }
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NzA4OTgzQDMyMzAyZTMyMmUzMEcxckhUYU0vTk5OdkFxWVNKeThaUkRnb0pDNDdnVmZ4L1M2eEo3b3hSNHM9");
            Instance = this;
            Locator.Instance.Build();
            InitializeComponent();
            MainPage = new NavigationPage(new LoginView());
        }

        protected override void OnStart()
        {

        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
        public bool DoBack
        {
            get
            {

                if (Shell.Current.Navigation.NavigationStack.Count > 1)
                    return true;
                return false;

            }
        }
    }
}
