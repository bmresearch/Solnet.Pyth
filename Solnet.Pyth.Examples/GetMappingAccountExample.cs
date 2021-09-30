using Solnet.Pyth.Models;
using Solnet.Rpc;
using Solnet.Wallet;
using System;

namespace Solnet.Pyth.Examples
{
    public class GetMappingAccountExample : IRunnableExample
    {
        private static readonly IRpcClient RpcClient = Solnet.Rpc.ClientFactory.GetClient(Cluster.MainNet);

        private static readonly IStreamingRpcClient StreamingRpcClient =
            Solnet.Rpc.ClientFactory.GetStreamingClient(Cluster.MainNet);

        private readonly IPythClient _pythClient;

        public GetMappingAccountExample()
        {
            _pythClient = ClientFactory.GetClient(RpcClient, StreamingRpcClient);
        }

        public void Run()
        {
            AccountResultWrapper<MappingAccount> mappingAccount =
                _pythClient.GetMappingAccount(Constants.MappingAccount);

            foreach (PublicKey productAccountKey in mappingAccount.ParsedResult.ProductAccountKeys)
            {
                Console.WriteLine($"ProductAccount: {productAccountKey}");
            }

            Console.ReadKey();
        }
    }
}