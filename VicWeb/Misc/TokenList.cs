using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VicWeb.Interfaces;

namespace VicWeb.Misc
{
    public class TokenList
    {
        public string name { get; set; }
        public DateTime timestamp { get; set; }
        public Version version { get; set; }
        public string logoURI { get; set; }
        public string[] keywords { get; set; }
        public TokenListToken[] tokens { get; set; }

        public static async Task<TokenList> BuildFromUrlAsync(string url, string fallbackName)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    //client.BaseAddress = new Uri(builder.ToString());
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync(new Uri(url));
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        return BuildFromJson(json, fallbackName);
                    } 
                }
            }
            catch(Exception ex)
            {

            }
            return null;
        }
        public static TokenList BuildFromJson(string json, string fallBackListName)
        {
            var tList = new TokenList();

            var tokens = new List<TokenListToken>();
            
            try
            {
                var obj = JObject.Parse(json);
                foreach (var jt in obj)
                {
                    switch (jt.Key)
                    {
                        case "name":
                            tList.name = jt.Value.Value<string>();
                            break;
                        case "tokens":
                            var list = obj.SelectToken("tokens");
                            var test = list.First().First();
                            foreach (var t in list)
                            {
                                var test2 = t.ToObject<TokenListToken>();
                                tokens.Add(test2);
                            }
                            break;

                    }
                }
                tList.tokens = tokens.ToArray();
            }
            catch(Exception e)
            {
                var obj = JArray.Parse(json);
                try
                {
                    foreach (var t in obj)
                    {
                       
                        var token = t.ToObject<TokenListToken>();
                        tokens.Add(token);
                    }
                    tList.name = fallBackListName;
                    tList.tokens = tokens.ToArray();
                }
                catch(Exception e2)
                {

                }
            }
            return tList;
        }
    }

    public class Version
    {
        public int major { get; set; }
        public int minor { get; set; }
        public int patch { get; set; }
    }

    public class TokenListToken : IToken
    {
        public string name { get; set; }
        public string symbol { get; set; }
        public string address { get; set; }
        public long chainId { get; set; }
        public int decimals { get; set; }
        public string logoURI { get; set; }

        string IToken.Name => name;

        string IToken.Symbol => symbol;

        string IToken.Address => address;

        long IToken.ChainId => chainId;

        int IToken.Decimals => decimals;

        string IToken.LogoURI => logoURI;
    }

}
