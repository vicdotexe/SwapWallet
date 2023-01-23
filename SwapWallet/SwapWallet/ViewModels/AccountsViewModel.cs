using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SwapWallet.Models;
using SwapWallet.Services;
using VicWeb.Misc;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using static NBitcoin.Scripting.OutputDescriptor;

namespace SwapWallet.ViewModels
{
    public class AccountsViewModel : BaseViewModel
    {
        public ObservableCollection<VicAccount> Accounts
        {
            get => AuthenticationService.AuthenticatedUser.VicWallet.VicAccounts;
        }

        private VicAccount _selectedAccount;
        public VicAccount SelectedAccount
        {
            get => _selectedAccount;
            set => SetProperty(ref _selectedAccount, value);
        }

        public Command GetPrivateKey => new Command(async () =>
        {
            var result = await Shell.Current.DisplayPromptAsync("Caution",
                "Enter password to retrieve private your key.", "Ok", "Cancel");
            if (!string.IsNullOrWhiteSpace(result))
            {
                var success = StringCipher.TryDecrypt(AuthenticationService.AuthenticatedUser.Cipher, result,
                    out var words);

                if (success)
                {
                    StringCipher.TryDecrypt(SelectedAccount.Cipher, words, out var pk);
                    await Clipboard.SetTextAsync(pk);
                    await Application.Current.MainPage.DisplayAlert("Copied","Private key copied to keyboard.", "Accept");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error","Incorrect password.","Accept");
                }

            }
        });

        public Command Edit => new Command(async () =>
        {
            var result = await Shell.Current.DisplayPromptAsync("Edit", "Change Account Name", "Accept", "Cancel");
            if (string.IsNullOrWhiteSpace(result) || Accounts.Any(o => o.Name == result))
            {
                await Shell.Current.DisplayAlert("Error", "Name must be unique to other accounts, and not empty.", "Accept");
                return;
            }
            var name = result;
            if (SelectedAccount.IsImport)
                name += " (imported)";
            SelectedAccount.Name = name;
            Locator.Instance.Resolve<IFileService>().Persist(AuthenticationService.AuthenticatedUser);
        });

        public Command GenerateNew => new Command(() =>
        {
            UserService.GenerateNewAccount();
        });

        public Command Import => new Command(async() =>
        {
            var result = await Shell.Current.DisplayPromptAsync("Import Account", "Enter private key:", "Accept", "Cancel");
            if (!string.IsNullOrWhiteSpace(result))
            {
                UserService.ImportAccount(result);
            }
            
        });
        public AccountsViewModel()
        {
        }

    }
}
