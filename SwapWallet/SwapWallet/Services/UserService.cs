using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SwapWallet.Models;
using SwapWallet.ViewModels;
using VicWeb.Interfaces;
using VicWeb.Misc;

namespace SwapWallet.Services
{
    public interface IUserService : INotifyPropertyChanged
    {
        VicAccount ActiveAccount { get; set; }
        IChain FromChain { get; set; }
        UserHistory History { get; }
        Emitter<UserServices,object> Emitter { get; }
        void ImportAccount(string privateKey);
        void ImportTokenListAsync(long chainId, TokenList tokenList);
        List<string> GetImportedListAddresses(long chainId);
        void GenerateNewAccount();
    }

    public enum UserServices
    {
        UserAdded,
        ActiveAccountChanged,
        AccountsAltered,
        FromChainChanged
    }

    public class UserService : IUserService
    {
        private Emitter<UserServices, object> _emitter = new Emitter<UserServices, object>();
        public  Emitter<UserServices, object> Emitter => _emitter;
        private User ActiveUser => Locator.Instance.Resolve<IAuthenticationService>().AuthenticatedUser;

        private VicAccount _activeAccount;
        public VicAccount ActiveAccount
        {
            get => _activeAccount ?? SetActiveAccount(ActiveUser.VicWallet.VicAccounts[0]);
            set
            {
                _activeAccount = value;
                if (_activeAccount != null)
                {
                    ActiveUser.History.LastAccount = _activeAccount.Id.ToString();
                    Persist();
                }

                Emitter.Emit(UserServices.ActiveAccountChanged, value);
            }
        }

        public void ImportAccount(string privateKey)
        {
            var account = ActiveUser.VicWallet.ImportAccount(privateKey);
            Persist();
        }

        public void GenerateNewAccount()
        {
            var account = ActiveUser.VicWallet.GenerateNewAccount();
            Persist();
        }

        private VicAccount SetActiveAccount(VicAccount account)
        {
            ActiveAccount = account;
            return account;
        }

        public void ImportTokenListAsync(long chainId, TokenList tokenList)
        {   
            if (!History.ActiveLists.TryGetValue(chainId, out var dict))
                History.ActiveLists.Add(chainId, new Dictionary<string, List<string>>());
            History.ActiveLists[chainId].Add(tokenList.name,tokenList.tokens.Select(o=>o.address).ToList());
        }

        public List<string> GetImportedListAddresses(long chainId)
        {
            List<string> addresses = new List<string>();
            if (History.ActiveLists.TryGetValue(chainId, out var dict))
            {
                return dict.Values.SelectMany(o=>o).ToList();
            }

            return null;
        }

        public UserHistory History => ActiveUser.History;

        private IChain _fromChain;
        public IChain FromChain
        {
            get => _fromChain;
            set
            {
                if (value == null)
                    return;
                History.LastFromChain = value.Id;
                Persist();
                _fromChain = value;
                Emitter.Emit(UserServices.FromChainChanged, value);
            }
        }
        

        private void Persist()
        {
            Locator.Instance.Resolve<IFileService>().Persist(ActiveUser);
        }


        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
#endregion
    }
}
