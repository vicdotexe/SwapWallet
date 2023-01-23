using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using SwapWallet.Models;
using SwapWallet.Services;
using SwapWallet.ViewModels;
using SwapWallet.Views.Popups;
using VicWeb;
using VicWeb.Interfaces;
using VicWeb.Lifi;
using VicWeb.Lifi.JsonObjects;
using VicWeb.Misc;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;

namespace SwapWallet.ViewModels
{
    public interface INativeHttpMessageHandlerProvider
    {
        HttpMessageHandler Get();
    }
    public class AssetsViewModel : BaseViewModel
    {
        private IMetaService _metaService;
        private ILifiService _lifiService;
        private IWeb3Provider _web3Provider;

        private IChain _chain;
        public IChain Chain
        {
            get => UserService.FromChain;
            set
            {
                UserService.FromChain = value;
                OnPropertyChanged();
            }
        }

        private List<IToken> _tokens;

        public ObservableCollection<TokenBalance> TokenBalances { get; set; } =
            new ObservableCollection<TokenBalance>();
        private LifiToken _weth;

        public Command Import => new Command(() =>
        {
            //Shell.Current.Navigation.PushAsync(new TokenImportPage(BlackBoard.AccountManager.FromChain.Id));
        });

        public Command PickChain => new Command(async () =>
        {
            var chain = await Shell.Current.ShowPopupAsync(new ChainPicker());
            if (chain != null)
                Chain = (IChain)chain;
        });

        public TokenBalance Selected
        {
            set
            {
                if (value == null)
                    return;
                //Shell.Current.Navigation.PushAsync(new AssetDetailsPage(value.Token));
            } 
        }
        public Command Check => new Command(CheckBalances);

        public AssetsViewModel()
        {
            _metaService = Locator.Instance.Resolve<IMetaService>();
            _lifiService = Locator.Instance.Resolve<ILifiService>();
            _web3Provider = Locator.Instance.Resolve<IWeb3Provider>();
            UserService.Emitter.AddObserver(UserServices.FromChainChanged, OnFromChainChanged);
            UserService.Emitter.AddObserver(UserServices.ActiveAccountChanged, OnFromChainChanged);
            PollPricesAsync();
        }

        private DateTime nextUpdate;
        private bool forceUpdate;

        private async void PollPricesAsync()
        {
            nextUpdate = DateTime.Now;
            while (true)
            {
                try
                {
                    if ((forceUpdate || DateTime.Now >= nextUpdate) && TokenBalances.Count > 0 )
                    {
                        await PollPrices();
                        forceUpdate = false;
                        nextUpdate = DateTime.Now.AddSeconds(30);
                    }
                    else
                        await Task.Delay(1000);

                }
                catch (Exception ex)
                {

                }
            }
        }

        private async Task PollPrices()
        {
            if (TokenBalances?.Count() <1)
                return;

            var lifiTokens = await _lifiService.GetTokens(_chain.Id, TokenBalances.Select(o => o.Token.Address));
            foreach (var tb in TokenBalances)
            {
                var lf = lifiTokens.FirstOrDefault(o => o.address == tb.Token.Address);
                tb.PriceUsd = decimal.Parse(lf.priceUSD);
            }
        }

        private void OnFromChainChanged(object obj)
        {
            _chain = Chain;
            OnPropertyChanged("Chain");
            if (_chain != null)
                OnChainChanged();
        }


        private Web3 _web3 => _web3Provider.Get(_chain);
        private async void OnChainChanged()
        {

            while (IsBusy)
                await Task.Delay(250);

            IsBusy = true;

            TokenBalances.Clear();

            HexBigInteger nativebalance = new HexBigInteger(10000000000000000000);
            for (int i = 0; i < _chain.RpcUrls.Count; i++)
            {
                try
                {
                    _web3.TransactionManager.UseLegacyAsDefault = true;
                    _weth = await _lifiService.GetTokenAsync(_chain.Id, MetaService.NullAddress);
                    i = _chain.RpcUrls.Count;
                    nativebalance =
                        await _web3.Eth.GetBalance.SendRequestAsync(UserService.ActiveAccount.PublicAddress);
                }
                catch (Exception ex)
                {
                    i++;
                }
            }




            var history = UserService.ActiveAccount.GetAssetHistory();
            
            if (history.GetBalances(Chain.Id) == null)
            {
                _tokens = _metaService.GetKnownTokens(Chain.Id);

                var allWithBalance = await GetAllTokenBalances(_tokens, _web3,
                    UserService.ActiveAccount.PublicAddress,
                    _chain.MulticallAddress, true);

                foreach (var balance in allWithBalance)
                    history.UpdateBalance(balance.Token, balance.Balance);
                
                var nToken = _tokens[0];
                var value = Web3.Convert.FromWei(nativebalance, _chain.NativeCurrency.Decimals);
                var nBalance = new TokenBalance() { Balance = value, Token = nToken };
                history.UpdateBalance(nToken, value);
                FileService.Persist(history);

                TokenBalances.Add(nBalance);
                TokenBalances.AddRange(allWithBalance);
            }
            else
            {
                var addresses = history.Balances[_chain.Id].Keys.ToList();
                _tokens = await _metaService.LookUpTokensAsync(_chain.Id, addresses);
                CheckBalances();
            }

            forceUpdate = true;
            IsBusy = false;
        }

        public async Task<List<TokenBalance>> GetAllTokenBalances(List<IToken> tokens, Web3 web3, string owner, string multicallAddress, bool ignoreZeroBalance = false, int batchLimit = 500)
        {
            var list = new List<TokenBalance>();
            var balances = await _web3.Eth.ERC20.GetAllTokenBalancesUsingMultiCallAsync(owner, tokens.Select(o => o.Address), batchLimit, multicallAddress);
            for (int i = 0; i < tokens.Count; i++)
            {
                var b = Web3.Convert.FromWei(balances[i].Balance, tokens[i].Decimals);
                if (b > 0)
                {
                    var balanceResult = new TokenBalance()
                    {
                        Balance = b,
                        Token = tokens[i],
                        //PriceUsd = decimal.Parse(tokens[i].PriceUSD)
                    };

                    if (balanceResult.Balance == 0 &&
                        ignoreZeroBalance) // && ((balanceResult.Balance * decimal.Parse(tokens[i].priceUSD)) > 0))
                        continue;
                    
                    if (!UserService.History.IgnoreList.Contains(balanceResult.Token.Address))
                        list.Add(balanceResult);
                }
            }
            return list;
        }

        public async void CheckBalances()
        {
            try
            {
                _weth = await _lifiService.GetTokenAsync(_chain.Id, MetaService.NullAddress);
                var balanceTask = await _web3.Eth.GetBalance.SendRequestAsync(UserService.ActiveAccount.PublicAddress);

                var balances = await GetAllTokenBalances(_tokens, _web3,
                    UserService.ActiveAccount.PublicAddress,
                    _chain.MulticallAddress);
                if (!TokenBalances.Any(o => o.Token.Address == MetaService.NullAddress))
                    TokenBalances.Add(new TokenBalance()
                        { Token = _tokens.First(o => o.Address == MetaService.NullAddress) });

                foreach (var balance in balances)
                {
                    var match = TokenBalances.FirstOrDefault(o => o.Token.Address == balance.Token.Address);
                    if (match != null)
                        match.Balance = balance.Balance;
                    else
                        TokenBalances.Add(balance);
                }

                var value = Web3.Convert.FromWei(balanceTask.Value, _chain.NativeCurrency.Decimals);
                TokenBalances.FirstOrDefault(o => o.Token.Address == MetaService.NullAddress).Balance = value;

            }
            catch (Exception ex)
            {

            }

        }
    }



    public class TokenBalance : INotifyPropertyChanged
    {
        public IToken Token { get; set; }
        public decimal Balance { get; set; }
        private decimal _priceUsd;

        public decimal PriceUsd
        {
            get => _priceUsd;
            set
            {
                SetField(ref _priceUsd, value);
                OnPropertyChanged("BalanceUsd");
            }
        }

        public decimal BalanceUsd => Balance * PriceUsd;

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
    }
}
