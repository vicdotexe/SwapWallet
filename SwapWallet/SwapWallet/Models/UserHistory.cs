using System.Collections.Generic;
using Newtonsoft.Json;
using SwapWallet.Services;
using VicWeb.Interfaces;

namespace SwapWallet.Models
{


    public class UserHistory
    {
        public string LastAccount { get; set; }
        public long LastFromChain { get; set; } = 56;
        public List<string> IgnoreList { get; set; } = new List<string>();
        public Dictionary<long, List<string>> Imported { get; set; } = new Dictionary<long, List<string>>();

        public Dictionary<long, Dictionary<string, List<string>>> ActiveLists =
            new Dictionary<long, Dictionary<string, List<string>>>();


    }

    public class AssetHistory : IFile
    {
        public string PublicAddress { get; set; }
        public Dictionary<long, Dictionary<string, decimal>> Balances { get; set; } = new Dictionary<long, Dictionary<string, decimal>>();
        

        public Dictionary<string, decimal> GetBalances(long chainId)
        {
            Balances.TryGetValue(chainId, out var balances);
            return balances;
        }

        public void UpdateBalance(IToken token, decimal balance)
        {
            if (!Balances.TryGetValue(token.ChainId, out var balances))
            {
                balances = new Dictionary<string, decimal>();
                Balances.Add(token.ChainId, balances);
            }

            balances[token.Address] = balance;

        }

        public string FileName => PublicAddress;
    }
}
