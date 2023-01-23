using Nethereum.Util;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SwapWallet.ViewModels;
using VicWeb;
using VicWeb.Interfaces;
using VicWeb.Misc;
using Xamarin.Forms;
using SwapWallet.Services;
using SwapWallet.Models;

namespace Swapper.ViewModels.Popups
{
    public class CoinPickerViewModel : BaseViewModel
    {
        private ObservableCollection<IToken> _visibleTokens = new ObservableCollection<IToken>();
        public ObservableCollection<IToken> VisibleTokenList
        {
            get { return _visibleTokens; }
            set { SetProperty(ref _visibleTokens, value); }
        }

        private List<IToken> _allChainTokens;

        private bool _canImport;

        public bool CanImport
        {
            get => _canImport;
            set => SetProperty(ref _canImport, value);
        }

        private long _chainId;
        public long ChainId
        {
            get => _chainId;
            set => SetProperty(ref _chainId, value, onChanged: OnChainSet);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value, onChanged: OnSearchTextChanged);
        }

        private IToken _searchResult;
        public IToken SearchResult { get => _searchResult; set => SetProperty(ref _searchResult, value); }

        private IToken _selectedToken;
        public IToken SelectedToken
        {
            get => _selectedToken;
            set
            {

                if (value == null)
                    return;
                _allChainTokens = null;
                _dismiss?.Invoke(value);
            }
        }

        public Action<object> _dismiss;
        public void SetDismiss(Action<object> action)
        {
            _dismiss = action;
        }

        public Command ImportToken => new Command(() =>
        {
            if (!UserService.History.Imported.TryGetValue(ChainId, out var list))
            {
                list = new List<string>();
                UserService.History.Imported.Add(ChainId, list);
            }

            list.Add(VisibleTokenList[0].Address);
            FileService.Persist(AuthenticationService.AuthenticatedUser);
        });

        public Command ImportPopular => new Command(async () =>
        {
            
            IsBusy = true;
            
            var list = await TokenList.BuildFromUrlAsync(UserService.FromChain.TokenListUrl, "LifiDefault");
            UserService.ImportTokenListAsync(ChainId, list);
            FileService.Persist(AuthenticationService.AuthenticatedUser);
            OnChainSet();
            IsBusy = false;
            
        });

        private IMetaService _metaService;
        public CoinPickerViewModel()
        {
            _metaService = Locator.Instance.Resolve<IMetaService>();

        }

        private async void OnChainSet()
        {
            _allChainTokens = new List<IToken>();



            var tokenLists = UserService.GetImportedListAddresses(ChainId);
            if (tokenLists != null)
            {
                var tokenListsTokens = await Locator.Instance.Resolve<IMetaService>().LookUpTokensAsync(ChainId, tokenLists);
                _allChainTokens.AddRange(tokenListsTokens);
            }

            var assetHistory = FileService.Retrieve<AssetHistory>(UserService.ActiveAccount.PublicAddress);

            if (assetHistory != null)
            {
                if (assetHistory.Balances.ContainsKey(ChainId))
                {
                    var ownedAddresses = assetHistory.Balances[ChainId].Keys;

                    var owend = await _metaService.LookUpTokensAsync(ChainId, ownedAddresses);
                    foreach (var t in owend)
                    {
                        var match = _allChainTokens.FirstOrDefault(o => o.Address == t.Address);

                        if (match != null)
                            _allChainTokens.Remove(match);

                        _allChainTokens.Insert(0, t);

                    }
                }

            }


            if (UserService.History.Imported.TryGetValue(ChainId, out var imported))
            {
                var imports = await _metaService.LookUpTokensAsync(ChainId, imported);
                foreach (var t in imports)
                {
                    var match = _allChainTokens.FirstOrDefault(o => o.Address == t.Address);

                    if (match != null)
                        _allChainTokens.Remove(match);

                    _allChainTokens.Insert(0, t);

                }
            }
            
            VisibleTokenList.Clear();
            VisibleTokenList.AddRange(_allChainTokens);

        }


        private async void OnSearchTextChanged()
        {
            VisibleTokenList.Clear();
            CanImport = false;
            SearchResult = null;
            if (AddressUtil.Current.IsValidAddressLength(_searchText))
            {
                IsBusy = true;
                try
                {
                    var result = await _metaService.LookUpTokenAsync(_chainId, _searchText);
                    VisibleTokenList.Clear();
                    VisibleTokenList.Add(result);
                    SearchResult = result;
                    CanImport = !_allChainTokens.Any(o=>o.Address == result.Address);
                    return;
                }
                catch (Exception e)
                {
                }
                finally
                {
                    IsBusy = false;
                }

            }

            {

                foreach (var token in _allChainTokens.Where(o => o.Symbol.ToUpper().StartsWith(_searchText.ToUpper()) || o.Name.ToUpper().StartsWith(_searchText.ToUpper())))
                {
                    VisibleTokenList.Add(token);
                }
            }

        }
    }
}
