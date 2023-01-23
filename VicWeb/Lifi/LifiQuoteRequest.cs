using System.Collections.Generic;
using VicWeb.Lifi.JsonObjects;

namespace VicWeb.Lifi
{
    public class LifiQuoteRequest
    {
        public string FromChain { get; set; }
        public string ToChain { get; set; }
        public string FromToken { get; set; }
        public string ToToken { get; set; }
        public string FromAddress { get; set; }
        public string FromAmount { get; set; }
        public string Slippage { get; set; }

        public List<LifiTools.Exchange> AllowExchanges { get; set; }
    }
}
