using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Solnet.Programs.Models;
using Solnet.Pyth.Models;
using Solnet.Rpc;
using Solnet.Wallet;
using System;

namespace Solnet.Pyth.Examples
{
    public class GetMappingAccountExample : IRunnableExample
    {
        private readonly IRpcClient _rpcClient;
        private readonly IStreamingRpcClient _streamingRpcClient;
        private readonly ILogger _logger;
        private readonly IPythClient _pythClient;

        public GetMappingAccountExample()
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

            foreach (PublicKey productAccountKey in mappingAccount.ParsedResult.ProductAccountKeys)
            {
                Console.WriteLine($"ProductAccount: {productAccountKey}");
            }

            Console.ReadKey();
        }
    }
}