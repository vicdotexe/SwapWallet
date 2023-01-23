using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using NBitcoin.RPC;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using SwapWallet.ViewModels;
using VicWeb.Interfaces;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SwapWallet.Services
{
    public interface IWeb3Provider
    {
        Web3 Get(IChain chain);
    }
    public class Web3Provider : IWeb3Provider

    {
    private Web3 _cached;
    private HttpMessageHandler handler;

    public Web3Provider()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            handler = DependencyService.Get<INativeHttpMessageHandlerProvider>().Get();
        }
        else
        {
            handler = new HttpClientHandler();
        }

    }

    public Web3 Get(IChain chain)
    {
        if (_lastChainId != chain.Id)
            _cached = new Web3(new RpcClient(new Uri(chain.RpcUrls[0]), new HttpClient(handler)));

        _lastChainId = chain.Id;
        return _cached;
    }

    private long _lastChainId;
    }
}
