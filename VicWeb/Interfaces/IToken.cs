namespace VicWeb.Interfaces
{
    public interface IToken
    {
        string Name { get; }
        string Symbol { get;  }
        string Address { get;  }
        long ChainId { get;  }
        int Decimals { get;  }
        string LogoURI { get;  }
    }
}
