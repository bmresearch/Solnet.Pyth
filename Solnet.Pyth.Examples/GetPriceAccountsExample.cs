using Solnet.Pyth.Models;
using Solnet.Rpc;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Solnet.Pyth.Examples
{
    public class GetPriceAccountsExample : IRunnableExample
    {
        private static readonly IRpcClient RpcClient = Solnet.Rpc.ClientFactory.GetClient(Cluster.MainNet);

        private static readonly IStreamingRpcClient StreamingRpcClient =
            Solnet.Rpc.ClientFactory.GetStreamingClient(Cluster.MainNet);

        private readonly IPythClient _pythClient;

        public GetPriceAccountsExample()
        {
            _pythClient = ClientFactory.GetClient(RpcClient, StreamingRpcClient);
        }

        public void Run()
        {
            AccountResultWrapper<MappingAccount> mappingAccount =
                _pythClient.GetMappingAccount(Constants.MappingAccount);

            MultipleAccountsResultWrapper<List<ProductAccount>> productAccounts =
                _pythClient.GetProductAccounts(mappingAccount.ParsedResult);

            for (int i = 0; i < productAccounts.OriginalRequest.Result.Value.Count - 1; i++)
            {
                Console.WriteLine($"ProductAccount: {mappingAccount.ParsedResult.ProductAccountKeys[i]}");
                Console.WriteLine($"\tPriceAccount: {productAccounts.ParsedResult[i].PriceAccount}");

                AccountResultWrapper<PriceDataAccount> priceAccount =
                    _pythClient.GetPriceDataAccount(productAccounts.ParsedResult[i].PriceAccount);

                Console.WriteLine($"\tSymbol: {productAccounts.ParsedResult[i].Product.Symbol}\n" +
                                  $"\tPrice: {priceAccount.ParsedResult.PreviousPrice}\n" +
                                  $"\tConfidence: {priceAccount.ParsedResult.PreviousConfidence}");

                Task.Delay(250).Wait();
            }

            Console.ReadKey();
        }
    }
}