using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VicWeb.Lifi.JsonObjects;

namespace VicWeb.Lifi
{
    public interface ILifiService
    {
        Task<LifiChain[]> GetChainsAsync(bool testnet=false);
        Task<List<LifiToken>> GetTokensAsync(bool testnet = false, params long?[] chainId);
        Task<LifiToken> GetTokenAsync(long chainId, string tokenAddress, bool testnet = false);
        Task<LifiQuote> GetQuoteAsync(LifiQuoteRequest request, bool testnet = false);
        Task<LifiTools> GetToolsAsync();
        Task<List<LifiToken>> GetTokens(long chainId, IEnumerable<string> requested);
        void InitializeLimiter();
    }

    public class LifiService : ILifiService
    {
        
        private const string _base = "https://li.quest/v1/";
        private const string _testnet = "https://staging.li.quest/v1/";
        public Queue<Task> _taskQueue;
        private int _limitLeft = 30;
        private bool _limiterInitialized;

        public LifiService()
        {
            InitializeLimiter();
        }
        public async void InitializeLimiter()
        {
            DateTime time = DateTime.Now.AddSeconds(60);
            _limiterInitialized = true;
            while (true)
            {
                if (DateTime.Now > time)
                {
                    _limitLeft = 30;
                    time = DateTime.Now.AddSeconds(60);
                }
                else
                    await Task.Delay(250);
            }
            
        }

        private async Task AwaitOrIncrement()
        {
            if (!_limiterInitialized)
                return;

            while (_limitLeft <= 0)
            {
                await Task.Delay(250);
            }

            TickLimit();
        }

        private void TickLimit()
        {
            _limitLeft--;
        }

        public async Task<string> GetAsync(string route, bool testnet = false,params (string,string)[] parameters)
        {
            await AwaitOrIncrement();

            string prms = $"?{parameters[0].Item1}={parameters[0].Item2}";
            for (int i = 1; i< parameters.Length; i++)
            {
                prms += $"&{parameters[i].Item1}={parameters[i].Item2}";
            }

            string result = null;
            using (var client = new HttpClient())
            {
                try
                {
                    result = await client.GetStringAsync(testnet ? _testnet : _base + route +prms);
                }
                
                catch(Exception e)
                {

                }
            }
            return result;
        }
        
        public async Task<LifiChain[]> GetChainsAsync(bool testnet = false)
        {
            await AwaitOrIncrement();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(testnet ? "https://staging.li.quest" : "https://li.quest");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("/v1/chains");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var list = JsonConvert.DeserializeObject<LifiChainsRoot>(content).chains;
                    return list;
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<List<LifiToken>> GetTokensAsync(bool testnet = false, params long?[] chainId)
        {
            await AwaitOrIncrement();
            //ToDo: Implement specific chains

            var list = new List<LifiToken>();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(testnet ? _testnet : _base);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    var test = "tokens";
                    if (chainId != null && chainId.Length>0)
                    {
                        test += "/?chains=" + chainId[0];
                        for (int i = 1; i < chainId.Length; i++)
                            test += $"&chains={chainId[i]}";
                    }

                    HttpResponseMessage response = await client.GetAsync(test);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var obj = JObject.Parse(content);
                        foreach (var r in obj)
                            foreach (var c in r.Value)
                                foreach (var t in c.Values())
                                list.Add(t.ToObject<LifiToken>());
                    }
                }
            }
            catch (Exception e)
            {
                    
            }

            return list;
        }

        public async Task<LifiToken> GetTokenAsync(long chainId, string tokenAddress, bool testnet = false)
        {

            
            var result = await GetAsync("token", testnet,("chain", chainId.ToString()), ("token", tokenAddress));
            if (result == null)
            {
                result = await GetAsync("token", testnet, ("chain", chainId.ToString()), ("token", "0x0000000000000000000000000000000000000000"));
            }
            return JsonConvert.DeserializeObject<LifiToken>(result);
        }


        public async Task<LifiTools> GetToolsAsync()
        {
            await AwaitOrIncrement();
            try
            {
                using (var client = new HttpClient())
                {
                    //client.BaseAddress = new Uri(builder.ToString());
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = await client.GetAsync(new Uri("https://li.quest/v1/tools"));
                    if (response.IsSuccessStatusCode && response.Content != null)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var obj = JObject.Parse(content);
                        var tools = obj.ToObject<LifiTools>();
                        return tools;
                    }
                }
            }
            catch(Exception e)
            { 
            }

            return null;
        }

        public async Task<LifiQuote> GetQuoteAsync(LifiQuoteRequest request, bool testnet = false)
        {
            await AwaitOrIncrement();
            var builder = new UriBuilder((testnet ? _testnet : _base)+"quote");
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["fromChain"] = request.FromChain;
            query["toChain"] = request.ToChain;
            query["fromToken"] = request.FromToken;
            query["toToken"] = request.ToToken;
            query["fromAddress"] = request.FromAddress;
            query["fromAmount"] = request.FromAmount;
            query["slippage"] = request.Slippage;

            var path = query.ToString();
            if (request.AllowExchanges != null)
            {
                foreach (var ex in request.AllowExchanges)
                    path+=$"&allowExchanges={ex.key}";
            }
            builder.Query = path;
                
            try
            {
                using (var client = new HttpClient())
                {
                    //client.BaseAddress = new Uri(builder.ToString());
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync(new Uri(builder.ToString()));
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var obj = JObject.Parse(content);
                        var quote = obj.ToObject<LifiQuote>();
                        return quote;
                    }

                    return null;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }


        public async Task<List<LifiToken>> GetTokens(long chainId, IEnumerable<string> requested)
        {
            
            var tasks = new List<Task<LifiToken>>();
            foreach (var token in requested)
            {
                tasks.Add(GetTokenAsync(chainId, token));
            }
            await Task.WhenAll(tasks);
            return tasks.Select(o => o.Result).ToList();
        }



    }

}
