using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SwapWallet.Views;
using Xamarin.Forms;

namespace SwapWallet
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();

        }

        private async void MenuItem_OnClicked(object sender, EventArgs e)
        {
            Application.Current.MainPage = new LoginView();
        }
    }
}
