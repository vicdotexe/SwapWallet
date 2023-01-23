using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwapWallet.Services;
using SwapWallet.ViewModels;
using VicWeb.Interfaces;
using VicWeb.Lifi;
using VicWeb.Lifi.JsonObjects;
using VicWeb.Misc;

namespace SwapWallet.Services
{
    public interface IMetaService
    {
        Task InitializeAsync();
        void ImportToken(IToken token);
        void ImportChain(IChain chain);
        void ImportTokens(IEnumerable<IToken> tokens);
        void ImportChains(IEnumerable<IChain> chains);

        List<IChain> GetChains();
        List<IToken> GetKnownTokens(long chainId);

        Task<IToken> LookUpTokenAsync(long chainId, string address);
        Task<List<IToken>> LookUpTokensAsync(long chainId, IEnumerable<string> addresses);
        LifiTools GetLifiTools();
    }


    public class MetaService : IMetaService
    {
        public static readonly string NullAddress = "0x0000000000000000000000000000000000000000";
        private MetaBase _meta;
        
        private IFileService _fileService;
        private ILifiService _lifiService;

        public async Task InitializeAsync()
        {
            _fileService = Locator.Instance.Resolve<IFileService>();
            _lifiService = Locator.Instance.Resolve<ILifiService>();

            _meta = _fileService.RetrieveAll<MetaBase>().FirstOrDefault();
            try
            {
                if (_meta == null)
                _meta = new MetaBase();

                _fileService.IsSuppressed = true;
                await ImportLifiChains();
                await ImportLifiTokens();
                await ImportLifiTools();
                _fileService.IsSuppressed = false;
                _fileService.Persist(_meta);
            }
            catch(Exception ex)
            {

            }
        }
        

        public void ImportToken(IToken token)
        {
            _meta.ImportToken(token);
            _fileService.Persist(_meta);

        }

        public void ImportTokens(IEnumerable<IToken> tokens)
        {
            foreach (var token in tokens)
                _meta.ImportToken(token);
            _fileService.Persist(_meta);
        }

        public void ImportChain(IChain chain)
        {
            _meta.ImportChain(chain);
            _fileService.Persist(_meta);
        }

        public void ImportChains(IEnumerable<IChain> chains)
        {
            foreach (var chain in chains)
                _meta.ImportChain(chain);
            _fileService.Persist(_meta);
        }

        private async Task ImportLifiTools()
        {
            _meta.LifiTools = await _lifiService.GetToolsAsync();
        }

        private async Task ImportLifiChains()
        {
            var chains = await _lifiService.GetChainsAsync();
            foreach (var chain in chains)
            {
                _meta.ImportChain(MetaChain.CreateFrom(chain));
            }

            _fileService.Persist(_meta);
        }

        public LifiTools GetLifiTools()
        {
            return _meta.LifiTools;
        }

        private async Task ImportLifiTokens()
        {
            var tokens = await _lifiService.GetTokensAsync();
            ImportTokens(tokens);
        }

        public async Task<IToken> LookUpTokenAsync(long chainId, string address)
        {
            var token = _meta.GetToken(chainId, address);

            if (token != null) 
                return token;

            token = await _lifiService.GetTokenAsync(chainId, address);
            ImportToken(token);

            return token;
        }

        public async Task<List<IToken>> LookUpTokensAsync(long chainId, IEnumerable<string> addresses)
        {
            List<IToken> tokens = new List<IToken>();

            foreach (var address in addresses)
            {
                try
                {
                    var token = _meta.GetToken(chainId, address);

                    if (token == null)
                    {
                        token = await _lifiService.GetTokenAsync(chainId, address);
                        ImportToken(token);
                    }

                    tokens.Add(token);
                }
                catch (Exception e)
                {

                }
            }

            return tokens;
        }

        public List<IToken> GetKnownTokens(long chainId)
        {
            if (_meta.KnownTokens.TryGetValue(chainId, out var tokens))
            {
                return tokens.Values.Cast<IToken>().ToList();
            }

            return new List<IToken>();
        }

        public IChain GetChain(long chainId)
        {
            return _meta.KnownChains[chainId];
        }

        public string GetTokenLogo(long chainId, string address)
        {
            string path = null;
            var token = _meta.GetToken(chainId, address);
            if (token != null)
                path = token.LogoURI;

            return string.IsNullOrWhiteSpace(path) ? null : path;
        }

        public List<IChain> GetChains() => _meta.KnownChains.Values.Cast<IChain>().ToList();

    }

    public class MetaBase : IFile
    {
        string IFile.FileName => "MetaBase";
        public Dictionary<long, Dictionary<string,MetaToken>> KnownTokens { get; set; } = new Dictionary<long, Dictionary<string, MetaToken>>();
        public Dictionary<long, MetaChain> KnownChains { get; set; } = new Dictionary<long, MetaChain>();
        public LifiTools LifiTools { get; set;  } = new LifiTools();

        public IToken GetToken(long chainId, string address)
        {
            if (KnownTokens.TryGetValue(chainId, out var tokens))
            {
                return tokens.TryGetValue(address.ToLower(), out var token) ? token : null;
            }
            return null;
        }

        public IChain GetChain(IChain chain)
        {
            return KnownChains.TryGetValue(chain.Id, out var result) ? result : null;
        }

        public void ImportChain(IChain chain)
        {
            if (!KnownChains.TryGetValue(chain.Id, out var result))
                KnownChains.Add(chain.Id, MetaChain.CreateFrom(chain));
        }

        public void ImportToken(IToken token)
        {
            var address = token.Address.ToLower();

            if (KnownTokens.TryGetValue(token.ChainId, out var tokens))
            {
                if (!tokens.ContainsKey(address))
                    tokens.Add(address, MetaToken.CreateFrom(token));
            }
            else
            {
                KnownTokens.Add(token.ChainId, new Dictionary<string, MetaToken>());
                KnownTokens[token.ChainId].Add(address, MetaToken.CreateFrom(token));
            }
           
        }
        
    }

    public class MetaToken : IToken
    {
        public string Address { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public int Decimals { get; set; }
        public long ChainId { get; set; }
        public string LogoURI { get; set; }


        public static MetaToken CreateFrom(IToken token)
        {
            var metaToken = new MetaToken()
            {
                Address = token.Address,
                ChainId = token.ChainId,
                Decimals = token.Decimals,
                LogoURI = token.LogoURI,
                Name = token.Name,
                Symbol = token.Symbol
            };
            return metaToken;
        }
    }

    public class MetaChain : IChain
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public MetaNativeCurrency NativeCurrency { get; set; }
        public string LogoURI { get; set; }
        public string MulticallAddress { get; set; }
        public List<string> Explorers { get; set; }
        public List<string> RpcUrls { get; set; }
        public string TokenListUrl { get; set; }

        INativeCurrency IChain.NativeCurrency => NativeCurrency;

        public static MetaChain CreateFrom(IChain chain)
        {
            var c = new MetaChain()
            {
                Explorers = chain.Explorers,
                RpcUrls = chain.RpcUrls,
                TokenListUrl = chain.TokenListUrl,
                Id = chain.Id,
                LogoURI = chain.LogoURI,
                MulticallAddress = chain.MulticallAddress,
                Name = chain.Name,
                NativeCurrency = MetaNativeCurrency.CreateFrom(chain.NativeCurrency),
            };
            return c;
        }
    }

    public class MetaNativeCurrency : INativeCurrency
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        public int Decimals { get; set; }

        public static MetaNativeCurrency CreateFrom(INativeCurrency currency)
        {
            var nc = new MetaNativeCurrency()
            {
                Name = currency.Name,
                Symbol = currency.Symbol,
                Decimals = currency.Decimals
            };
            return nc;
        }
    }
}
