using SwapWallet.ViewModels;
using System.Net.Http;
using SwapWallet.Droid.PlatformSpecific;
using Xamarin.Android.Net;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidHttpMessageHandlerProvider))]
namespace SwapWallet.Droid.PlatformSpecific
{
    public class AndroidHttpMessageHandlerProvider : INativeHttpMessageHandlerProvider
    {
        public HttpMessageHandler Get()
        {
            return new AndroidClientHandler();
        }
    }
}