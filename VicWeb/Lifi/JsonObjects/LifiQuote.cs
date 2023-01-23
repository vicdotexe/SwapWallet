using System.Collections.Generic;

namespace VicWeb.Lifi.JsonObjects
{

    public class LifiQuote
    {
        public string id { get; set; }
        public string type { get; set; }
        public string tool { get; set; }
        public ToolDetails toolDetails { get; set; }
        public Action action { get; set; }
        public Estimate estimate { get; set; }
        public List<IncludedStep> includedSteps { get; set; }
        public TransactionRequest transactionRequest { get; set; }

        public class Action
        {
            public long fromChainId { get; set; }
            public long toChainId { get; set; }
            public FromToken fromToken { get; set; }
            public ToToken toToken { get; set; }
            public decimal fromAmount { get; set; }
            public decimal slippage { get; set; }
            public string fromAddress { get; set; }
            public string toAddress { get; set; }
        }

        public class FeeCost
        {
            public string name { get; set; }
            public string percentage { get; set; }
            public Token token { get; set; }
            public string amount { get; set; }
        }

        public class TransactionRequest
        {
            public string from { get; set; }
            public string to { get; set; }
            public long chainId { get; set; }
            public string data { get; set; }
            public string value { get; set; }
            public string gasPrice { get; set; }
            public string gasLimit { get; set; }
        }

        public class Data
        {
            public long chainId { get; set; }
            public string price { get; set; }
            public string guaranteedPrice { get; set; }
            public string estimatedPriceImpact { get; set; }
            public string to { get; set; }
            public string data { get; set; }
            public string value { get; set; }
            public string gas { get; set; }
            public string estimatedGas { get; set; }
            public string gasPrice { get; set; }
            public string protocolFee { get; set; }
            public string minimumProtocolFee { get; set; }
            public string buyTokenAddress { get; set; }
            public string sellTokenAddress { get; set; }
            public string buyAmount { get; set; }
            public string sellAmount { get; set; }
            public List<Source> sources { get; set; }
            public List<Order> orders { get; set; }
            public string allowanceTarget { get; set; }
            public string decodedUniqueId { get; set; }
            public string sellTokenToEthRate { get; set; }
            public string buyTokenToEthRate { get; set; }
            public string expectedSlippage { get; set; }
            public List<string> path { get; set; }
            public string routerAddress { get; set; }
        }
        public class Source
        {
            public string name { get; set; }
            public string proportion { get; set; }
        }

        public class ToolDetails
        {
            public string key { get; set; }
            public string name { get; set; }
            public string logoURI { get; set; }
        }

        public class Token
        {
            public string address { get; set; }
            public int decimals { get; set; }
            public string symbol { get; set; }
            public long chainId { get; set; }
            public string coinKey { get; set; }
            public string name { get; set; }
            public string logoURI { get; set; }
            public decimal priceUSD { get; set; }
        }

        public class FromToken
        {
            public string address { get; set; }
            public string symbol { get; set; }
            public int decimals { get; set; }
            public long chainId { get; set; }
            public string name { get; set; }
            public string coinKey { get; set; }
            public decimal priceUSD { get; set; }
            public string logoURI { get; set; }
        }

        public class ToToken
        {
            public string address { get; set; }
            public int decimals { get; set; }
            public string symbol { get; set; }
            public long chainId { get; set; }
            public string coinKey { get; set; }
            public string name { get; set; }
            public string logoURI { get; set; }
            public decimal priceUSD { get; set; }
        }

        public class Order
        {
            public string  type { get; set; }
            public string source { get; set; }
            public string makerToken { get; set; }
            public string takerToken { get; set; }
            public string makerAmount { get; set; }
            public string takerAmount { get; set; }
            public FillData fillData { get; set; }
            public Fill fill { get; set; }
            public string sourcePathId { get; set; }
        }

        public class Estimate
        {
            public string fromAmount { get; set; }
            public string toAmount { get; set; }
            public string toAmountMin { get; set; }
            public string approvalAddress { get; set; }
            public string executionDuration { get; set; }
            public List<FeeCost> feeCosts { get; set; }
            public List<GasCost> gasCosts { get; set; }
            public Data data { get; set; }
            public string fromAmountUSD { get; set; }
            public string toAmountUSD { get; set; }
        }

        public class Fill
        {
            public string input { get; set; }
            public string output { get; set; }
            public string adjustedOutput { get; set; }
            public string gas { get; set; }
        }

        public class FillData
        {
            public List<string> tokenAddressPath { get; set; }
            public string router { get; set; }
        }



        public class GasCost
        {
            public string type { get; set; }
            public string price { get; set; }
            public string estimate { get; set; }
            public string limit { get; set; }
            public string amount { get; set; }
            public decimal amountUSD { get; set; }
            public Token token { get; set; }
        }

        public class IncludedStep
        {
            public string id { get; set; }
            public string type { get; set; }
            public string tool { get; set; }
            public ToolDetails toolDetails { get; set; }
            public Action action { get; set; }
            public Estimate estimate { get; set; }
            public TransactionRequest transactionRequest { get; set; }
        }
    }










}