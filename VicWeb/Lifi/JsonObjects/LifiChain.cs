using System.Collections.Generic;
using VicWeb.Interfaces;

namespace VicWeb.Lifi.JsonObjects
{
    public class LifiChainsRoot
    {
        public LifiChain[] chains { get; set; }
    }

    public class LifiChain : IChain
    {
        public string key { get; set; }
        public string name { get; set; }
        public string coin { get; set; }
        public int id { get; set; }
        public bool mainnet { get; set; }
        public string logoURI { get; set; }
        public string tokenlistUrl { get; set; }
        public Metamask metamask { get; set; }
        public List<string> faucetUrls { get; set; }
        public string multicallAddress { get; set; }

        public long Id => id;
        public string Name => name;
        public INativeCurrency NativeCurrency => metamask.nativeCurrency;
        public string LogoURI => logoURI;
        public string MulticallAddress => multicallAddress;
        public List<string> Explorers => metamask.blockExplorerUrls;
        public List<string> RpcUrls => metamask.rpcUrls;
        public string TokenListUrl => tokenlistUrl;
    }

    public class Metamask
    {
        public string chainId { get; set; }
        public List<string> blockExplorerUrls { get; set; }
        public string chainName { get; set; }
        public Nativecurrency nativeCurrency { get; set; }
        public List<string> rpcUrls { get; set; }
    }

    public class Nativecurrency : INativeCurrency
    {
        public string name { get; set; }
        public string symbol { get; set; }
        public int decimals { get; set; }
        public string Name => name;
        public string Symbol => symbol;
        public int Decimals => decimals;
    }

}
