using VicWeb.Interfaces;

namespace VicWeb.Lifi.JsonObjects
{

    public class LifiTools
    {
        public Bridge[] bridges { get; set; }
        public Exchange[] exchanges { get; set; }
        public class Bridge
        {
            public string key { get; set; }
            public string name { get; set; }
            public string logoURI { get; set; }
        }

        public class Exchange : IExchange
        {
            public string key { get; set; }
            public string name { get; set; }
            public string logoURI { get; set; }

            public string Name => name;

            public string LogoURI => logoURI;
        }
    }

}
