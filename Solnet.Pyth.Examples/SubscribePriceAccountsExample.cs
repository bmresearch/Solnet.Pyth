using Solnet.Pyth.Models;
using Solnet.Rpc;
using Solnet.Rpc.Types;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Solnet.Pyth.Examples
{
    public class SubscribePriceAccountsExample : IRunnableExample
    {
        private static readonly IRpcClient RpcClient = Solnet.Rpc.ClientFactory.GetClient(Cluster.MainNet);

        private static readonly IStreamingRpcClient StreamingRpcClient =
            Solnet.Rpc.ClientFactory.GetStreamingClient(Cluster.MainNet);

        private static readonly List<string> Pairs = new()
        {
            "BTC",
            "SOL",
            "SRM",
            "LTC",
            "ETH",
            "DOGE",
            "COPE",
            "FTT",
            "LUNA",
            "MNGO",
            "RAY",
            "SBR",
            "BNB",
            "BCH",
            "HXRO",
            "MER",
            "AAPL",
            "AMC",
            "AMZN",
            "GOOG",
            "NFLX",
            "SPY",
            "TSLA"
        };

        private readonly IPythClient _pythClient;

        private List<Subscription> _subscriptions;

        public SubscribePriceAccountsExample()
        {
            StreamingRpcClient.ConnectAsync().Wait();
            _pythClient = ClientFactory.GetClient(RpcClient, StreamingRpcClient);
            _subscriptions = new List<Subscription>();
        }

        public void Run()
        {
            AccountResultWrapper<MappingAccount> mappingAccount =
                _pythClient.GetMappingAccount(Constants.MappingAccount);
            MultipleAccountsResultWrapper<List<ProductAccount>> productAccounts =
                _pythClient.GetProductAccounts(mappingAccount.ParsedResult);

            foreach (ProductAccount productAccount in productAccounts.ParsedResult.Where(productAccount =>
                Pairs.Any(s => productAccount.Product.Symbol.Contains(s))))
            {
                _subscriptions.Add(_pythClient.SubscribePriceDataAccount((subscription, priceDataAccount, slot) =>
                {
                    Console.WriteLine($"{productAccount.Product.Symbol}\t-\t" +
                                      $"{priceDataAccount.PreviousPrice:C2} " +
                                      $"±{priceDataAccount.PreviousConfidence:N6}\t");
                }, productAccount.PriceAccount, Commitment.Confirmed));
            }

            Console.ReadKey();
        }
    }
}