using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using SwapWallet.Services;
using SwapWallet.ViewModels;

namespace SwapWallet.Models
{

    public class VicAccount : INotifyPropertyChanged
    {
        public VicAccount() { }
        public VicAccount(string name, string cipher, string publicAddress, bool isImport)
        {
            Name = name;
            Cipher = cipher;
            PublicAddress = publicAddress;
            IsImport = isImport;
            Id = Guid.NewGuid();
        }

        [JsonProperty]
        public Guid Id { get; private set; }

        private string _name;
        [JsonProperty]
        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }
        [JsonProperty]
        public string Cipher { get; private set; }
        [JsonProperty]
        public string PublicAddress { get; private set; }
        [JsonProperty]
        public bool IsImport { get; private set; }

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

    public static class VicAccountExt
    {
        public static AssetHistory GetAssetHistory(this VicAccount self)
        {
            return Locator.Instance.Resolve<IFileService>().Retrieve<AssetHistory>(self.PublicAddress) ?? new AssetHistory(){PublicAddress=self.PublicAddress};
        }
    }

}
