using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Nethereum.HdWallet;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;
using Newtonsoft.Json;
using VicWeb.Misc;

namespace SwapWallet.Models
{
    public class VicWallet
    {
        private Wallet _hdWallet;

        public void Initialize(Wallet wallet)
        {
            _hdWallet = wallet;
        }

        public void Unintialize()
        {
            _hdWallet = null;
        }


        public ObservableCollection<VicAccount> VicAccounts { get; } = new ObservableCollection<VicAccount>();

        public string GetWords()
        {
            Insist.IsNotNull(_hdWallet, "Accessing wallet that hasn't been unlocked. This shouldn't happen.");
            return string.Join(" ", _hdWallet.Words);
        }

        private int GetNextIndex()
        {
            return NextIndex++;
        }

        [JsonProperty]
        private int NextIndex { get; set; } = 0;

        public string GetPrivateKey(VicAccount account)
        {
            Insist.IsNotNull(_hdWallet, "Accessing wallet that hasn't been unlocked. This shouldn't happen.");

            var words = string.Join(" ", _hdWallet.Words);
            if (!StringCipher.TryDecrypt(account.Cipher, words, out var pk))
            {
                Insist.Fail("This shouldn't happen.");
            }

            return pk;
        }

        public VicAccount GenerateNewAccount()
        {

            Insist.IsNotNull(_hdWallet, "Accessing wallet that hasn't been unlocked. This shouldn't happen.");

            var index = GetNextIndex();
            var name = $"Account {index}";
            var cipher = StringCipher.Encrypt(_hdWallet.GetPrivateKey(index).ToHex(), GetWords());
            var publicAddress = _hdWallet.GetAddresses()[index];

            var account = new VicAccount(name, cipher, publicAddress, false);
            VicAccounts.Add(account);
            return account;

        }

        public VicAccount ImportAccount(string privateKey)
        {
            var index = VicAccounts.Count(o => o.IsImport);
            while (VicAccounts.Any(o => o.Name == $"Account {index} -import"))
                index++;
            var name = $"Account {index} -import";
            var publicAddress = Web3.GetAddressFromPrivateKey(privateKey);
            var cipher = StringCipher.Encrypt(privateKey, GetWords());
            var account = new VicAccount(name, cipher, publicAddress, true);
            VicAccounts.Add(account);
            return account;
        }

    }
}
