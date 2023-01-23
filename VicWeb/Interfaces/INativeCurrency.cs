namespace VicWeb.Interfaces
{
    public interface INativeCurrency
    {
        string Name { get;}
        string Symbol { get; }
        int Decimals { get; }
    }
}
