using Solnet.Pyth.Models;
using Solnet.Rpc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Solnet.Pyth.Examples
{
    public class GetProductAccountsExample : IRunnableExample
    {
        private static readonly IRpcClient RpcClient = Solnet.Rpc.ClientFactory.GetClient(Cluster.MainNet);

        private static readonly IStreamingRpcClient StreamingRpcClient =
            Solnet.Rpc.ClientFactory.GetStreamingClient(Cluster.MainNet);

        private readonly IPythClient _pythClient;

        public GetProductAccountsExample()
        {
            _pythClient = ClientFactory.GetClient(RpcClient, StreamingRpcClient);
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

                /*
                foreach ((string key, string value) in productAccounts.ParsedResult[i].ProductAttributes)
                {
                    Console.WriteLine($"\tKey: {key} Value: {value}");
                }
                */

                Task.Delay(250).Wait();
            }

            Console.ReadKey();
        }
    }
}