using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using SwapWallet.Models;
using SwapWallet.Services;
using SwapWallet.Views;
using Xamarin.Forms;

namespace SwapWallet.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _password = "";
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {

                SetProperty(ref _selectedUser, value);
            }
        }

        public ObservableCollection<User> Users { get; set; }

        public Command Create => new Command(async () =>
        {
            UserService.Emitter.AddObserver(UserServices.UserAdded, OnUserAdded);
            var page = new UserCreationView();
            page.Disappearing += ((sender, args) => UserService.Emitter.RemoveObserver(UserServices.UserAdded, OnUserAdded));
            await Application.Current.MainPage.Navigation.PushAsync(new UserCreationView());
        });


        public Command Login => new Command(async () =>
        {
            if (SelectedUser == null) return;

            var success = AuthenticationService.Login(SelectedUser, _password).Result;
            if (!success)
                await Application.Current.MainPage.DisplayAlert("Oops", "Incorrect password.", "Accept");
            else
            {

                Application.Current.MainPage = new LoadingView();
            }

        });

        public LoginViewModel()
        {
            var users = Locator.Instance.Resolve<IFileService>().RetrieveAll<User>();
            Users = new ObservableCollection<User>(users);
            if (Users.Count>0)
                _selectedUser = Users[0];
        }
        
        private void OnUserAdded(object user)
        {
            Users.Insert(0, (User)user);
        }
    }
}
