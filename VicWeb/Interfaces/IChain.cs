using System.Collections.Generic;

namespace VicWeb.Interfaces
{
    public interface IChain
    {
        long Id { get; }
        string Name { get; }
        INativeCurrency NativeCurrency { get;}
        string LogoURI { get; }
        string MulticallAddress { get; }
        List<string> Explorers { get; }
        List<string> RpcUrls { get; }
        string TokenListUrl { get;  }
    }
}
