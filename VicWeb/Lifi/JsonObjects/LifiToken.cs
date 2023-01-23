using VicWeb.Interfaces;

namespace VicWeb.Lifi.JsonObjects
{

    public class LifiToken : IToken
    {
        public string address { get; set; }
        public int chainId { get; set; }
        public string symbol { get; set; }
        public int decimals { get; set; }
        public string name { get; set; }
        public string coinKey { get; set; }
        public string priceUSD { get; set; }
        public string logoURI { get; set; }

        public string Name => name;
        public string Symbol => symbol;
        public string Address => address;
        public long ChainId => chainId;
        public int Decimals => decimals;
        public string LogoURI => logoURI;
    }

}
