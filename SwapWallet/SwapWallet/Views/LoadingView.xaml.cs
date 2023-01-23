using SwapWallet.Services;
using SwapWallet.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SwapWallet.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingView : ContentPage
    {
        public LoadingView()
        {
            InitializeComponent();

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Task.Run(async () =>
            {
                await Locator.Instance.Resolve<IMetaService>().InitializeAsync();
                var userService = Locator.Instance.Resolve<IUserService>();

                userService.FromChain = Locator.Instance.Resolve<IMetaService>().GetChains()
                    .First(o => o.Id == userService.History.LastFromChain);
                Dispatcher.BeginInvokeOnMainThread(()=>
                {
                    Application.Current.MainPage = new AppShell();
                });
            });
        }
    }
}