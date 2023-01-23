using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using SwapWallet.Models;
using SwapWallet.Services;
using Xamarin.Forms;

namespace SwapWallet.ViewModels
{
    public  class FlyoutHeaderViewModel : BaseViewModel
    {
        public ObservableCollection<VicAccount> Accounts
        {
            get => AuthenticationService.AuthenticatedUser.VicWallet.VicAccounts;
        }

        public VicAccount SelectedAccount
        {
            get => UserService.ActiveAccount;
            set
            {
                UserService.ActiveAccount = value;
                OnPropertyChanged();
            }
        }

        public FlyoutHeaderViewModel()
        {
            UserService.Emitter.AddObserver(UserServices.ActiveAccountChanged, OnActiveAccountChanged);
        }

        private void OnActiveAccountChanged(object obj)
        {
            OnPropertyChanged("SelectedAccount");
        }

    }
}
