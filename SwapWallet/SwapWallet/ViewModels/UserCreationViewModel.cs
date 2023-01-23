using System.Collections.Generic;
using System.Linq;
using NBitcoin;
using SwapWallet.Models;
using SwapWallet.Services;
using Xamarin.Forms;

namespace SwapWallet.ViewModels
{
    public class UserCreationViewModel : BaseViewModel
    {
        private string _name;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        private List<string> _words;

        public List<string> Words
        {
            get => _words;
            set => SetProperty(ref _words, value);
        }

        private Mnemonic _mnemonic;
        public Command NewWords => new Command(() =>
        {
            _mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);
            Words = _mnemonic.Words.ToList();
        });

        private string _password;

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public Command Confirm => new Command(() =>
        {
            if (string.IsNullOrEmpty(_password) || string.IsNullOrEmpty(_name))
            {
                Application.Current.MainPage.DisplayAlert("Oops", "Name and password cannot be blank", "Accept");
                return;
            }

            var user = User.GenerateUser(_mnemonic, _password, _name);
            user.TryInitialize(_password);
            user.VicWallet.GenerateNewAccount();
            user.VicWallet.Unintialize();
            user.History.LastAccount = user.VicWallet.VicAccounts[0].PublicAddress;
            FileService.Persist(user);
            UserService.Emitter.Emit(UserServices.UserAdded, user);
            Application.Current.MainPage.Navigation.PopAsync();
        });

        public Command Cancel => new Command(() =>
        {
            Application.Current.MainPage.Navigation.PopAsync();
        });

        public UserCreationViewModel()
        {
            NewWords.Execute(null);
        }
    }
}
