using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Solnet.Programs.Models;
using Solnet.Pyth.Models;
using Solnet.Rpc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Solnet.Pyth.Examples
{
    public class GetProductAccountsExample : IRunnableExample
    {
        private readonly IRpcClient _rpcClient;
        private readonly IStreamingRpcClient _streamingRpcClient;
        private readonly ILogger _logger;
        private readonly IPythClient _pythClient;

        public GetProductAccountsExample()
        {
            _logger = LoggerFactory.Create(x =>
            {
                x.AddSimpleConsole(o =>
                {
                    o.UseUtcTimestamp = true;
                    o.IncludeScopes = true;
                    o.ColorBehavior = LoggerColorBehavior.Enabled;
                    o.TimestampFormat = "HH:mm:ss ";
                })
                .SetMinimumLevel(LogLevel.Debug);
            }).CreateLogger<IRpcClient>();

            // the clients
            _rpcClient = Rpc.ClientFactory.GetClient(Cluster.MainNet, _logger);
            _streamingRpcClient = Rpc.ClientFactory.GetStreamingClient(Cluster.MainNet, _logger);
            _pythClient = ClientFactory.GetClient(_rpcClient, _streamingRpcClient);
        }

        public void Run()
        {
            AccountResultWrapper<MappingAccount> mappingAccount =
                _pythClient.GetMappingAccount(Constants.MappingAccount);

            /*  Optionally perform a single request to get the product account
            foreach (PublicKey productAccountKey in mappingAccount.ParsedResult.ProductAccountKeys)
            {
                var productAccount = pythClient.GetProductAccount(productAccountKey);
            }
            */

            MultipleAccountsResultWrapper<List<ProductAccount>> productAccounts =
                _pythClient.GetProductAccounts(mappingAccount.ParsedResult);

            for (int i = 0; i < productAccounts.OriginalRequest.Result.Value.Count - 1; i++)
            {
                Console.WriteLine($"ProductAccount: {mappingAccount.ParsedResult.ProductAccountKeys[i]}");
                Console.WriteLine($"\tPriceAccount: {productAccounts.ParsedResult[i].PriceAccount}");

                foreach ((string key, string value) in productAccounts.ParsedResult[i].ProductAttributes)
                {
                    Console.WriteLine($"\tKey: {key} Value: {value}");
                }
                
            }

            Console.ReadKey();
        }
    }
}