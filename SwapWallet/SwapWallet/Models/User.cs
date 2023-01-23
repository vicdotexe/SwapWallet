using System;
using System.Collections.Generic;
using NBitcoin;
using Nethereum.HdWallet;
using SwapWallet.Services;
using VicWeb.Misc;

namespace SwapWallet.Models
{

    public class User : IFile
    {
        string IFile.FileName => Id.ToString();
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Cipher { get; set; }
        public VicWallet VicWallet { get; set; } = new VicWallet();

        public bool TryInitialize(string password)
        {
            var success = StringCipher.TryDecrypt(Cipher, password, out var words);
            if (success)
            {
                var wallet = new Wallet(words, null);
                VicWallet.Initialize(wallet);
            }
            return success;
        }

        public void Uninitialize()
        {
            VicWallet.Unintialize();
        }

        public static User GenerateUser(Mnemonic mnemonic, string password, string name)
        {
            var wallet = new Wallet(mnemonic.ToString(), null);
            var wordsCipher = StringCipher.Encrypt(mnemonic.ToString(), password);
            var user = new User()
            {
                Cipher = wordsCipher,
                Name = name,
            };
            return user;
        }

        public UserHistory History { get; set; } = new UserHistory();
    }
}
