using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Signer;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Swapper.Views.Popups;
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
    public class LifiSwapViewModel : BaseViewModel
    {
        private IMetaService _metaService;
        private ILifiService _lifiService;

        public List<IExchange> SelectedExchanges
        {
            get;
            set;
        } = new List<IExchange>();

        public ObservableCollection<IExchange> Exchanges { get; set; } = new ObservableCollection<IExchange>();

        private IChain _fromChain;
        public IChain FromChain
        {
            get => _fromChain;
            set
            {
                UserService.FromChain = value;
                OnFromChainChanged(value);
            }
        }

        private IChain _toChain;
        public IChain ToChain
        {
            get => _toChain;
            set => SetProperty(ref _toChain, value);
        }


        private IToken _fromToken;
        public IToken FromToken
        {
            get => _fromToken;
            set 
            {
                SetProperty(ref _fromToken, value, onChanged: OnInputChanged); UpdateTokenPrices();
            }
        }

        private decimal _fromTokenUsdPrice;
        public decimal FromTokenUsdPrice
        {
            get => _fromTokenUsdPrice;
            set=> SetProperty(ref _fromTokenUsdPrice, value);
        }

        private IToken _toToken;
        public IToken ToToken
        {
            get => _toToken;
            set
            {
                SetProperty(ref _toToken, value, onChanged: OnInputChanged);
                UpdateTokenPrices();
            }
        }

        private decimal _toTokenUsdPrice;
        public decimal ToTokenUsdPrice
        {
            get => _toTokenUsdPrice;
            set => SetProperty(ref _toTokenUsdPrice, value);
        }

        private LifiQuote _quote;
        public LifiQuote Quote
        {
            get => _quote;
            set { SetProperty(ref _quote, value); }
        }
        public decimal Slippage { get; set; }

        private Account _selectedAccount;

        private decimal _fromAmount;
        public decimal FromAmount        
        {
            get => _fromAmount;
            set => SetProperty(ref _fromAmount, value, onChanged: OnInputChanged);
        }

        private decimal _toAmount;
        public decimal ToAmount
        {
            get => _toAmount;
            set => SetProperty(ref _toAmount, value);
        }

        private bool _gettingQuote;

        public bool GettingQuote
        {
            get => _gettingQuote;
            set => SetProperty(ref _gettingQuote, value);
        }

        private bool _quoteValid;
        public bool QuoteValid
        {
            get => _quoteValid;
            set => SetProperty(ref _quoteValid, value);
        }

        public Command FromTokenPressed => new Command(async () =>
        {
            var token = await Application.Current.MainPage.ShowPopupAsync(new CoinPicker(FromChain.Id));
            if (token != null)
                FromToken = (IToken)token;

        });
        public Command ToTokenPressed => new Command(async() =>
        {
            var token = await Application.Current.MainPage.ShowPopupAsync(new CoinPicker(FromChain.Id));
            if (token != null)
                ToToken = (IToken)token;
        });

        public Command FromChainPressed => new Command(async() =>
        {
            var chain = await Shell.Current.ShowPopupAsync(new ChainPicker());
            if (chain != null)
                FromChain = (IChain)chain;
        });
        public Command ToolsPressed => new Command(() => { });

        private DateTime _lastRequestTime;


        public LifiSwapViewModel()
        {
            _metaService = Locator.Instance.Resolve<IMetaService>();
            _lifiService = Locator.Instance.Resolve<ILifiService>();
            UserService.Emitter.AddObserver(UserServices.FromChainChanged, OnFromChainChanged);
            _fromChain = UserService.FromChain;
            if (_fromChain != null)
            {
                _web3 = Locator.Instance.Resolve<IWeb3Provider>().Get(_fromChain);
            }
            var tools = _metaService.GetLifiTools();
            Exchanges.AddRange(tools.exchanges);
            SelectedExchanges.AddRange(Exchanges);
            StartQuotingAsync();
        }
        Web3 _web3;

        private void OnFromChainChanged(object obj)
        {
            Task.Run(async () =>
            {
                while (IsBusy)
                    await Task.Delay(250);
                IsBusy = true;
                _fromChain = UserService.FromChain;
                OnPropertyChanged("FromChain");
                FromToken = null;
                ToToken = null;
                ResetQuoteData();
                _web3 = Locator.Instance.Resolve<IWeb3Provider>().Get(_fromChain);
                IsBusy = false;
            });

        }


        private bool IsRequestValid()
        {
            return _fromChain !=null  && FromToken !=null && ToToken !=null && FromAmount > 0;
        }

        private LifiQuoteRequest _lastRequest;

        private bool _canSend;
        public bool CanSend {  get => _canSend; set => SetProperty(ref _canSend, value); }

        private bool _needsApproved;
        public bool NeedsApproved { get => _needsApproved; set => SetProperty(ref _needsApproved, value); }

        private void OnInputChanged()
        {
            CanSend = false;
            QuoteValid = IsRequestValid();
            if (!IsRequestValid())
            {
                _lastRequest = null;
                Quote = null;
                return;
            }

            _lastRequest = new LifiQuoteRequest()
            {
                FromChain = _fromChain.Id.ToString(),
                ToChain = ToChain != null ? ToChain.Id.ToString() : _fromChain.Id.ToString(),
                FromToken = FromToken.Address,
                ToToken = ToToken.Address,
                FromAddress = UserService.ActiveAccount.PublicAddress,
                FromAmount = Web3.Convert.ToWei(FromAmount, FromToken.Decimals).ToString(),
                Slippage = Slippage.ToString(),
                AllowExchanges = SelectedExchanges?.Cast<LifiTools.Exchange>().ToList()
            };
            _lastRequestTime = DateTime.Now;
            if (_forceRequestTask!= null && _forceRequestTask.IsCompleted)
            {
                _forceRequestTask = Task.Run(async () => await ForceRequest());
            }
            else
            {
                _forceRequestTask = Task.Run(async () => await ForceRequest());
            }
            
        }

        private Task _forceRequestTask;
        private bool _forceRequest;
        private async Task ForceRequest()
        {
            while (_lastRequestTime.AddSeconds(1) > DateTime.Now)
                await Task.Delay(100);
            _forceRequest = true;
        }

        private DateTime _lastQuoteTime;

        private int _secondsUntilUpdate;
        public int SecondsUntilUpdate
        {
            get => _secondsUntilUpdate;
            set
            {
                SetProperty(ref _secondsUntilUpdate, value);
            }
        }

        private async void UpdateTokenPrices()
        {
            Task fromTask = Task.CompletedTask, toTask = Task.CompletedTask;
            if (FromToken != null)
                fromTask = _lifiService.GetTokenAsync(FromToken.ChainId, FromToken.Address);
           
            if (ToToken != null)
                toTask = _lifiService.GetTokenAsync(ToToken.ChainId, ToToken.Address);
            
            await Task.WhenAll(fromTask, toTask);
            if (FromToken != null)
                FromTokenUsdPrice = decimal.Parse(((Task<LifiToken>)fromTask).Result.priceUSD);
            else
                FromTokenUsdPrice = 0;

            if (ToToken != null)
                ToTokenUsdPrice = decimal.Parse(((Task<LifiToken>)toTask).Result.priceUSD);
            else
                ToTokenUsdPrice = 0;
        }
        
        private async void StartQuotingAsync()
        {
           
            _lastQuoteTime = DateTime.Now;
            while (true)
            {
                SecondsUntilUpdate = (_lastQuoteTime.AddSeconds(20) - DateTime.Now).Seconds;
                if (_lastQuoteTime.AddSeconds(20) > DateTime.Now && !_forceRequest)
                {
                    await Task.Delay(500);
                    continue;
                }

                UpdateTokenPrices();
                _forceRequest = false;


                if (_lastRequest != null)
                {
                    try
                    {
                        GettingQuote = true;
                        LifiQuote quote = await _lifiService.GetQuoteAsync(_lastRequest);
                        Quote = quote;
                        GettingQuote = false;
                        if (quote != null)
                        {
                            ExtractQuoteData(quote);
                            CanSend = true;
                            var allowance = await _web3.Eth.ERC20.GetContractService(quote.action.fromToken.address).AllowanceQueryAsync("0x72BfdF110A10f5B0D54BB011247033a5a47B4515", "0x10ed43c718714eb63d5aa57b78b54704e256024e");// quote.estimate.approvalAddress);
                            if (allowance == 0)
                                NeedsApproved = true;
                            else
                            {

                            }
                        }
                        else
                        {
                            ResetQuoteData();
                        }
                    }
                    catch(Exception e)
                    {
                        ResetQuoteData();
                    }

                }
                else
                {
                    ResetQuoteData();
                }
                
                _lastQuoteTime = DateTime.Now;
            }
        }

        private void ExtractQuoteData(LifiQuote quote)
        {
            try
            {
                ToAmount = Web3.Convert.FromWei(BigInteger.Parse(quote.estimate.toAmount), ToToken.Decimals);

                string toolPath = "";
                foreach (var step in quote.includedSteps)
                {
                    toolPath += $"{step.tool} > ( -";
                    if (step.estimate.data?.sources != null)
                    {
                        var total = step.estimate.data.sources.Sum(o => decimal.Parse(o.proportion));

                        foreach (var s in step.estimate.data.sources)
                        {
                            var val = decimal.Parse(s.proportion);
                            if (val > 0)
                                toolPath += $" {s.name}({Math.Round((val/total)*100)}%) -";
                        }
                    }

                    toolPath += ")";
                }
                ToolPath = toolPath;
            }
            catch (Exception e)
            {
                ToolPath = "Error extracting quote";
            }

        }

        private string _toolPath;
        public string ToolPath
        {
            get => _toolPath;
            set => SetProperty(ref _toolPath, value);
        }

        private void ResetQuoteData()
        {
            ToAmount = 0;
            ToolPath = "";
        }

    }
}