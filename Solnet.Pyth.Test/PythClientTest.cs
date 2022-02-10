using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Solnet.Programs;
using Solnet.Programs.Models;
using Solnet.Pyth.Models;
using Solnet.Rpc;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Core.Sockets;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;
using Solnet.Rpc.Types;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Solnet.Pyth.Test
{
    [TestClass]
    public class PythClientTest
    {
        private static readonly string MainNetUrl = "https://api.mainnet-beta.solana.com/";
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        private WebSocketState _wsState;
        private Mock<SubscriptionState> _subscriptionStateMock;
        private Action<SubscriptionState, ResponseValue<AccountInfo>> _action;

        private Mock<IStreamingRpcClient> MultipleNotificationsStreamingClientTestSetup<T>(
            out Action<Subscription, T, ulong> action, Action<T> resultCaptureCallback,
            string network, Commitment commitment = Commitment.Finalized)
        {
            Mock<Action<Subscription, T, ulong>> actionMock = new();
            actionMock
                .Setup(_ => _(It.IsAny<Subscription>(), It.IsAny<T>(), It.IsAny<ulong>()))
                .Callback<Subscription, T, ulong>((sub, notification, slot) =>
                {
                    resultCaptureCallback(notification);
                });
            action = actionMock.Object;

            _subscriptionStateMock = new Mock<SubscriptionState>(MockBehavior.Strict);
            Mock<IStreamingRpcClient> streamingRpcMock = new(MockBehavior.Strict);
            streamingRpcMock
                .Setup(s => s.NodeAddress)
                .Returns(new Uri(network))
                .Verifiable();
            streamingRpcMock
                .Setup(s => s.State)
                .Returns(() => _wsState)
                .Verifiable();

            streamingRpcMock
                .Setup(s => s.ConnectAsync())
                .Callback(() =>
                {
                    _wsState = WebSocketState.Open;
                })
                .Returns(() => null)
                .Verifiable();

            streamingRpcMock
                .Setup(s => s.DisconnectAsync())
                .Callback(() =>
                {
                    _wsState = WebSocketState.Closed;
                })
                .Returns(() => null)
                .Verifiable();

            streamingRpcMock
                .Setup(s => s.SubscribeAccountInfoAsync(
                    It.IsAny<string>(),
                    It.IsAny<Action<SubscriptionState, ResponseValue<AccountInfo>>>(),
                    It.Is<Commitment>(c => c == commitment)))
                .Callback<string, Action<SubscriptionState, ResponseValue<AccountInfo>>, Commitment>(
                    (_, notificationAction, _) =>
                    {
                        _action = notificationAction;
                    })
                .ReturnsAsync(() => _subscriptionStateMock.Object)
                .Verifiable();
            return streamingRpcMock;
        }

        /// <summary>
        /// Setup the JSON RPC test with the request and response data.
        /// </summary>
        /// <param name="responseContent">The response content.</param>
        /// <param name="address">The address parameter for <c>GetAccountInfo</c>.</param>
        /// <param name="commitment">The commitment parameter for the <c>GetAccountInfo</c>.</param>
        /// <param name="network">The network address for the <c>GetAccountInfo</c> request.</param>
        private static Mock<IRpcClient> SetupGetAccountInfo(string responseContent, string address, string network,
            Commitment commitment = Commitment.Finalized)
        {
            var rpcMock = new Mock<IRpcClient>(MockBehavior.Strict) { };
            rpcMock
                .Setup(s => s.NodeAddress)
                .Returns(new Uri(network))
                .Verifiable();
            rpcMock
                .Setup(s => s.GetAccountInfoAsync(
                        It.Is<string>(s1 => s1 == address),
                        It.Is<Commitment>(c => c == commitment),
                        It.IsAny<BinaryEncoding>()))
                .ReturnsAsync(() =>
                {
                    var res = new RequestResult<ResponseValue<AccountInfo>>(
                        new HttpResponseMessage(HttpStatusCode.OK),
                        JsonSerializer.Deserialize<ResponseValue<AccountInfo>>(responseContent, JsonSerializerOptions))
                    {
                        WasRequestSuccessfullyHandled = true
                    };

                    return res;
                })
                .Verifiable();
            return rpcMock;
        }

        /// <summary>
        /// Setup the JSON RPC test with the request and response data.
        /// </summary>
        /// <param name="responseContent">The response content.</param>
        /// <param name="addresses">The address for <c>GetMultipleAccountsAsync</c>.</param>
        /// <param name="commitment">The commitment parameter for the <c>GetMultipleAccountsAsync</c>.</param>
        /// <param name="network">The network address for the <c>GetMultipleAccountsAsync</c> request.</param>
        private static Mock<IRpcClient> SetupGetMultipleAccounts(string responseContent, List<PublicKey> addresses, string network,
            Commitment commitment = Commitment.Finalized)
        {
            var rpcMock = new Mock<IRpcClient>(MockBehavior.Strict) { };
            rpcMock
                .Setup(s => s.NodeAddress)
                .Returns(new Uri(network))
                .Verifiable();
            rpcMock.Setup(s => s.GetMultipleAccountsAsync(
               It.Is<List<string>>(s => addresses.TrueForAll(x => s.Contains(x))),
               It.Is<Commitment>(c => c == commitment)))
            .ReturnsAsync(() =>
            {
                var res = new RequestResult<ResponseValue<List<AccountInfo>>>(
                        new HttpResponseMessage(HttpStatusCode.OK),
                        JsonSerializer.Deserialize<ResponseValue<List<AccountInfo>>>(responseContent, JsonSerializerOptions))
                {
                    WasRequestSuccessfullyHandled = true
                };
                return res;
            })
            .Verifiable();
            return rpcMock;
        }

        /// <summary>
        /// Setup the JSON RPC test with the request and response data.
        /// </summary>
        /// <param name="responseContent">The response content.</param>
        /// <param name="addresses">The addresses for <c>GetMultipleAccountsAsync</c>.</param>
        /// <param name="commitment">The commitment parameter for the <c>GetMultipleAccountsAsync</c>.</param>
        private static Mock<IRpcClient> SetupGetMultipleAccounts(Mock<IRpcClient> rpcMock, string responseContent, List<PublicKey> addresses,
            Commitment commitment = Commitment.Finalized)
        {
            rpcMock.Setup(s => s.GetMultipleAccountsAsync(
               It.Is<List<string>>(s => addresses.TrueForAll(x => s.Contains(x))),
               It.Is<Commitment>(c => c == commitment)))
            .ReturnsAsync(() =>
            {
                var res = new RequestResult<ResponseValue<List<AccountInfo>>>(
                        new HttpResponseMessage(HttpStatusCode.OK),
                        JsonSerializer.Deserialize<ResponseValue<List<AccountInfo>>>(responseContent, JsonSerializerOptions))
                {
                    WasRequestSuccessfullyHandled = true
                };
                return res;
            })
            .Verifiable();
            return rpcMock;
        }

        private async Task<byte[]> LoadData(string path)
        {
            var accountData = await File.ReadAllTextAsync(path);

            return Convert.FromBase64String(accountData);
        }

        private async Task<MappingAccount> LoadMappingAccount(string path)
        {
            return MappingAccount.Deserialize(await LoadData(path));
        }

        private async Task<ProductAccount> LoadProductAccount(string path)
        {
            return ProductAccount.Deserialize(await LoadData(path));
        }

        [TestInitialize]
        public void Setup()
        {
            _wsState = WebSocketState.None;
        }

        [TestMethod]
        public void GetMappingAccount()
        {
            string response = File.ReadAllText("Resources/GetMappingAccountAccountInfo.json");
            var rpc = SetupGetAccountInfo(response, Constants.MappingAccount, MainNetUrl);

            var sut = ClientFactory.GetClient(rpc.Object);

            var res = sut.GetMappingAccount(Constants.MappingAccount);

            Assert.IsNotNull(res);
            Assert.AreEqual(AccountType.Mapping, res.ParsedResult.Type);
            Assert.AreEqual(2712847316u, res.ParsedResult.MagicNumber);
            Assert.AreEqual(2u, res.ParsedResult.Version);
            Assert.AreEqual(1912u, res.ParsedResult.Size);
            Assert.AreEqual(SystemProgram.ProgramIdKey, res.ParsedResult.NextMappingAccount);
            Assert.AreEqual(58u, res.ParsedResult.NumProducts);
            Assert.AreEqual(58, res.ParsedResult.ProductAccountKeys.Count);
            Assert.IsTrue(res.ParsedResult.ProductAccountKeys.Contains(new("5uKdRzB3FzdmwyCHrqSGq4u2URja617jqtKkM71BVrkw")));
        }

        [TestMethod]
        public void GetProductAccount()
        {
            string response = File.ReadAllText("Resources/GetProductAccountAccountInfo.json");
            var rpc = SetupGetAccountInfo(response, "5uKdRzB3FzdmwyCHrqSGq4u2URja617jqtKkM71BVrkw", MainNetUrl);

            var sut = ClientFactory.GetClient(rpc.Object);

            var res = sut.GetProductAccount("5uKdRzB3FzdmwyCHrqSGq4u2URja617jqtKkM71BVrkw");

            Assert.IsNotNull(res);
            Assert.AreEqual(AccountType.Product, res.ParsedResult.Type);
            Assert.AreEqual(2712847316u, res.ParsedResult.MagicNumber);
            Assert.AreEqual(2u, res.ParsedResult.Version);
            Assert.AreEqual(158u, res.ParsedResult.Size);
            Assert.AreEqual("5ALDzwcRJfSyGdGyhP3kP628aqBNHZzLuVww7o9kdspe", res.ParsedResult.PriceAccount);
            Assert.AreEqual(6, res.ParsedResult.ProductAttributes.Count);
            Assert.AreEqual("Crypto", res.ParsedResult.Product.AssetType);
            Assert.AreEqual("BCH", res.ParsedResult.Product.Base);
            Assert.AreEqual(string.Empty, res.ParsedResult.Product.Country);
            Assert.AreEqual("BCH/USD", res.ParsedResult.Product.Description);
            Assert.AreEqual("USD", res.ParsedResult.Product.QuoteCurrency);
            Assert.AreEqual("Crypto.BCH/USD", res.ParsedResult.Product.Symbol);
            Assert.AreEqual(string.Empty, res.ParsedResult.Product.Tenor);
        }

        [TestMethod]
        public void GetPriceDataAccount()
        {
            string response = File.ReadAllText("Resources/PriceDataAccountAccountInfo.json");
            var rpc = SetupGetAccountInfo(response, "5ALDzwcRJfSyGdGyhP3kP628aqBNHZzLuVww7o9kdspe", MainNetUrl);

            var sut = ClientFactory.GetClient(rpc.Object);

            var res = sut.GetPriceDataAccount("5ALDzwcRJfSyGdGyhP3kP628aqBNHZzLuVww7o9kdspe");

            Assert.IsNotNull(res);
            Assert.AreEqual(AccountType.Price, res.ParsedResult.Type);
            Assert.AreEqual(0.449595d, res.ParsedResult.AggregatePriceInfo.Confidence);
            Assert.AreEqual(44959500ul, res.ParsedResult.AggregatePriceInfo.ConfidenceComponent);
            Assert.AreEqual(CorporateAction.NoCorporateAction, res.ParsedResult.AggregatePriceInfo.CorporateAction);
            Assert.AreEqual(353.4737d, res.ParsedResult.AggregatePriceInfo.Price);
            Assert.AreEqual(35347370000, res.ParsedResult.AggregatePriceInfo.PriceComponent);
            Assert.AreEqual(120232495ul, res.ParsedResult.AggregatePriceInfo.PublishSlot);
            Assert.AreEqual(PriceStatus.Trading, res.ParsedResult.AggregatePriceInfo.Status);
            Assert.AreEqual(353.48636d, res.ParsedResult.Drv1);
            Assert.AreEqual(35348636000, res.ParsedResult.Drv1Component);
            Assert.AreEqual(353.48636d, res.ParsedResult.Drv2);
            Assert.AreEqual(35348636000, res.ParsedResult.Drv2Component);
            Assert.AreEqual(353.48636d, res.ParsedResult.Drv3);
            Assert.AreEqual(35348636000, res.ParsedResult.Drv3Component);
            Assert.AreEqual(-8, res.ParsedResult.Exponent);
            Assert.AreEqual(120232495ul, res.ParsedResult.LastSlot);
            Assert.AreEqual(120232494ul, res.ParsedResult.ValidSlot);
            Assert.AreEqual(2712847316u, res.ParsedResult.MagicNumber);
            Assert.AreEqual(14u, res.ParsedResult.NumPriceComponents);
            Assert.AreEqual(14, res.ParsedResult.PriceComponents.Count);
            Assert.AreEqual(13u, res.ParsedResult.NumQuoters);
            Assert.AreEqual(0.471935d, res.ParsedResult.PreviousConfidence);
            Assert.AreEqual(47193500ul, res.ParsedResult.PreviousConfidenceComponent);
            Assert.AreEqual(353.48636d, res.ParsedResult.PreviousPrice);
            Assert.AreEqual(35348636000, res.ParsedResult.PreviousPriceComponent);
            Assert.AreEqual(DeriveType.TWAP, res.ParsedResult.PriceType);
            Assert.AreEqual(DeriveType.TWAP, res.ParsedResult.PriceType);
            Assert.AreEqual(2u, res.ParsedResult.Version);
            Assert.AreEqual(1584u, res.ParsedResult.Size);
            Assert.AreEqual("5uKdRzB3FzdmwyCHrqSGq4u2URja617jqtKkM71BVrkw", res.ParsedResult.ProductAccount);
            Assert.AreEqual(SystemProgram.ProgramIdKey, res.ParsedResult.NextPriceAccount);
        }

        [TestMethod]
        public void GetProductAccounts()
        {
            var mappingAccount = LoadMappingAccount("Resources/MappingAccountInfo.txt").Result;

            string response = File.ReadAllText("Resources/GetProductAccountsMultipleAccountInfos.json");
            var rpc = SetupGetMultipleAccounts(response, mappingAccount.ProductAccountKeys, MainNetUrl);

            var sut = ClientFactory.GetClient(rpc.Object);

            var res = sut.GetProductAccounts(mappingAccount);

            Assert.IsNotNull(res);
            foreach(var acc in res.ParsedResult)
            {
                Assert.AreEqual(AccountType.Product, acc.Type);
            }
        }

        [TestMethod]
        public void GetPriceDataAccounts()
        {
            var mappingAccount = LoadMappingAccount("Resources/MappingAccountInfo.txt").Result;

            string productAccountsResponse = File.ReadAllText("Resources/GetProductAccountsMultipleAccountInfos.json");
            var rpc = SetupGetMultipleAccounts(productAccountsResponse, mappingAccount.ProductAccountKeys, MainNetUrl);

            var sut = ClientFactory.GetClient(rpc.Object);

            var productAccounts = sut.GetProductAccounts(mappingAccount);

            string response = File.ReadAllText("Resources/GetPriceDataAccountsMultipleAccountInfos.json");
            SetupGetMultipleAccounts(rpc, response, productAccounts.ParsedResult.Select(x => x.PriceAccount).ToList());

            var res = sut.GetPriceDataAccounts(productAccounts.ParsedResult);

            Assert.IsNotNull(res);
            foreach (var acc in res.ParsedResult)
            {
                Assert.AreEqual(AccountType.Price, acc.Type);
            }
        }

        [TestMethod]
        public void Connect()
        {
            Mock<IStreamingRpcClient> streamingRpcMock = new(MockBehavior.Strict);
            streamingRpcMock
                .Setup(s => s.NodeAddress)
                .Returns(new Uri(MainNetUrl))
                .Verifiable();
            streamingRpcMock
                .Setup(s => s.State)
                .Returns(() => _wsState)
                .Verifiable();

            streamingRpcMock
                .Setup(s => s.ConnectAsync())
                .Callback(() =>
                {
                    _wsState = WebSocketState.Open;
                })
                .Returns(() => null)
                .Verifiable();

            var rpcClient = Rpc.ClientFactory.GetClient(Cluster.MainNet);

            PythClient sut = new(rpcClient: rpcClient, streamingRpcClient: streamingRpcMock.Object);

            sut.ConnectAsync();

            Assert.AreEqual(WebSocketState.Open, _wsState);

            sut.ConnectAsync();

            Assert.AreEqual(WebSocketState.Open, _wsState);
        }

        [TestMethod]
        public void Disconnect()
        {
            Mock<IStreamingRpcClient> streamingRpcMock = new(MockBehavior.Strict);
            streamingRpcMock
                .Setup(s => s.NodeAddress)
                .Returns(new Uri(MainNetUrl))
                .Verifiable();
            streamingRpcMock
                .Setup(s => s.State)
                .Returns(() => _wsState)
                .Verifiable();

            streamingRpcMock
                .Setup(s => s.ConnectAsync())
                .Callback(() =>
                {
                    _wsState = WebSocketState.Open;
                })
                .Returns(() => null)
                .Verifiable();

            streamingRpcMock
                .Setup(s => s.DisconnectAsync())
                .Callback(() =>
                {
                    _wsState = WebSocketState.Closed;
                })
                .Returns(() => null)
                .Verifiable();

            var rpcClient = Rpc.ClientFactory.GetClient(Cluster.MainNet);

            PythClient sut = new(rpcClient: rpcClient, streamingRpcClient: streamingRpcMock.Object);
            sut.ConnectAsync();
            sut.DisconnectAsync();

            Assert.AreEqual(WebSocketState.Closed, _wsState);
        }

        [TestMethod]
        public void DisconnectNull()
        {
            Mock<IStreamingRpcClient> streamingRpcMock = new(MockBehavior.Strict);
            streamingRpcMock
                .Setup(s => s.NodeAddress)
                .Returns(new Uri(MainNetUrl))
                .Verifiable();
            streamingRpcMock
                .Setup(s => s.State)
                .Returns(() => _wsState)
                .Verifiable();

            streamingRpcMock
                .Setup(s => s.ConnectAsync())
                .Callback(() =>
                {
                    _wsState = WebSocketState.Open;
                })
                .Returns(() => null)
                .Verifiable();

            streamingRpcMock
                .Setup(s => s.DisconnectAsync())
                .Callback(() =>
                {
                    _wsState = WebSocketState.Closed;
                })
                .Returns(() => null)
                .Verifiable();

            var rpcClient = Rpc.ClientFactory.GetClient(Cluster.MainNet);

            PythClient sut = new(rpcClient: rpcClient, streamingRpcClient: streamingRpcMock.Object);
            sut.DisconnectAsync();

            Assert.AreEqual(WebSocketState.None, _wsState);
        }

        [TestMethod]
        public void SubscribePriceDataAccount()
        {
            string firstAccountInfoNotification =
                File.ReadAllText("Resources/PriceDataAccountAccountInfo.json");
            PriceDataAccount resultNotification = null;
            Mock<IStreamingRpcClient> streamingRpcMock = MultipleNotificationsStreamingClientTestSetup(
                out Action<Subscription, PriceDataAccount, ulong> action,
                (x) =>
                {
                    resultNotification = x;
                },
                "https://api.mainnet-beta.solana.com");

            var rpcClient = Rpc.ClientFactory.GetClient(Cluster.MainNet);

            PythClient sut = new(rpcClient: rpcClient, streamingRpcClient: streamingRpcMock.Object);
            Assert.IsNotNull(sut.StreamingRpcClient);
            Assert.AreEqual(MainNetUrl, sut.NodeAddress.ToString());
            sut.ConnectAsync();

            Assert.AreEqual(WebSocketState.Open, sut.State);

            Subscription sub = sut.SubscribePriceDataAccount(action, "31cKs646dt1YkA3zPyxZ7rUAkxTBz279w4XEobFXcAKP");

            ResponseValue<AccountInfo> notificationContent =
                JsonSerializer.Deserialize<ResponseValue<AccountInfo>>(firstAccountInfoNotification,
                    JsonSerializerOptions);
            _action(_subscriptionStateMock.Object, notificationContent);
            Assert.IsNotNull(sub);
            Assert.IsNotNull(resultNotification);
            Assert.AreEqual(AccountType.Price, resultNotification.Type);
            Assert.AreEqual(0.449595d, resultNotification.AggregatePriceInfo.Confidence);
            Assert.AreEqual(44959500ul, resultNotification.AggregatePriceInfo.ConfidenceComponent);
            Assert.AreEqual(CorporateAction.NoCorporateAction, resultNotification.AggregatePriceInfo.CorporateAction);
            Assert.AreEqual(353.4737d, resultNotification.AggregatePriceInfo.Price);
            Assert.AreEqual(35347370000, resultNotification.AggregatePriceInfo.PriceComponent);
            Assert.AreEqual(120232495ul, resultNotification.AggregatePriceInfo.PublishSlot);
            Assert.AreEqual(PriceStatus.Trading, resultNotification.AggregatePriceInfo.Status);
            Assert.AreEqual(353.48636d, resultNotification.Drv1);
            Assert.AreEqual(35348636000, resultNotification.Drv1Component);
            Assert.AreEqual(353.48636d, resultNotification.Drv2);
            Assert.AreEqual(35348636000, resultNotification.Drv2Component);
            Assert.AreEqual(353.48636d, resultNotification.Drv3);
            Assert.AreEqual(35348636000, resultNotification.Drv3Component);
            Assert.AreEqual(-8, resultNotification.Exponent);
            Assert.AreEqual(120232495ul, resultNotification.LastSlot);
            Assert.AreEqual(120232494ul, resultNotification.ValidSlot);
            Assert.AreEqual(2712847316u, resultNotification.MagicNumber);
            Assert.AreEqual(14u, resultNotification.NumPriceComponents);
            Assert.AreEqual(14, resultNotification.PriceComponents.Count);
            Assert.AreEqual(13u, resultNotification.NumQuoters);
            Assert.AreEqual(0.471935d, resultNotification.PreviousConfidence);
            Assert.AreEqual(47193500ul, resultNotification.PreviousConfidenceComponent);
            Assert.AreEqual(353.48636d, resultNotification.PreviousPrice);
            Assert.AreEqual(35348636000, resultNotification.PreviousPriceComponent);
            Assert.AreEqual(DeriveType.TWAP, resultNotification.PriceType);
            Assert.AreEqual(DeriveType.TWAP, resultNotification.PriceType);
            Assert.AreEqual(2u, resultNotification.Version);
            Assert.AreEqual(1584u, resultNotification.Size);
            Assert.AreEqual("5uKdRzB3FzdmwyCHrqSGq4u2URja617jqtKkM71BVrkw", resultNotification.ProductAccount);
            Assert.AreEqual(SystemProgram.ProgramIdKey, resultNotification.NextPriceAccount);
        }

        [TestMethod]
        public void UnsubscribePriceDataAccount()
        {
            string firstAccountInfoNotification =
                File.ReadAllText("Resources/PriceDataAccountAccountInfo.json");
            PriceDataAccount resultNotification = null;
            Mock<IStreamingRpcClient> streamingRpcMock = MultipleNotificationsStreamingClientTestSetup(
                out Action<Subscription, PriceDataAccount, ulong> action,
                (x) =>
                {
                    resultNotification = x;
                },
                "https://api.mainnet-beta.solana.com");

            var rpcClient = Rpc.ClientFactory.GetClient(Cluster.MainNet);

            PythClient sut = new(rpcClient: rpcClient, streamingRpcClient: streamingRpcMock.Object);
            Assert.IsNotNull(sut.StreamingRpcClient);
            Assert.AreEqual(MainNetUrl, sut.NodeAddress.ToString());
            sut.ConnectAsync();

            Assert.AreEqual(WebSocketState.Open, sut.State);

            Subscription sub = sut.SubscribePriceDataAccount(action, "5ALDzwcRJfSyGdGyhP3kP628aqBNHZzLuVww7o9kdspe");

            ResponseValue<AccountInfo> notificationContent =
                JsonSerializer.Deserialize<ResponseValue<AccountInfo>>(firstAccountInfoNotification,
                    JsonSerializerOptions);
            _action(_subscriptionStateMock.Object, notificationContent);
            Assert.IsNotNull(sub);
            Assert.IsNotNull(resultNotification);
            Assert.AreEqual(AccountType.Price, resultNotification.Type);
            Assert.AreEqual(0.449595d, resultNotification.AggregatePriceInfo.Confidence);
            Assert.AreEqual(44959500ul, resultNotification.AggregatePriceInfo.ConfidenceComponent);
            Assert.AreEqual(CorporateAction.NoCorporateAction, resultNotification.AggregatePriceInfo.CorporateAction);
            Assert.AreEqual(353.4737d, resultNotification.AggregatePriceInfo.Price);
            Assert.AreEqual(35347370000, resultNotification.AggregatePriceInfo.PriceComponent);
            Assert.AreEqual(120232495ul, resultNotification.AggregatePriceInfo.PublishSlot);
            Assert.AreEqual(PriceStatus.Trading, resultNotification.AggregatePriceInfo.Status);
            Assert.AreEqual(353.48636d, resultNotification.Drv1);
            Assert.AreEqual(35348636000, resultNotification.Drv1Component);
            Assert.AreEqual(353.48636d, resultNotification.Drv2);
            Assert.AreEqual(35348636000, resultNotification.Drv2Component);
            Assert.AreEqual(353.48636d, resultNotification.Drv3);
            Assert.AreEqual(35348636000, resultNotification.Drv3Component);
            Assert.AreEqual(-8, resultNotification.Exponent);
            Assert.AreEqual(120232495ul, resultNotification.LastSlot);
            Assert.AreEqual(120232494ul, resultNotification.ValidSlot);
            Assert.AreEqual(2712847316u, resultNotification.MagicNumber);
            Assert.AreEqual(14u, resultNotification.NumPriceComponents);
            Assert.AreEqual(14, resultNotification.PriceComponents.Count);
            Assert.AreEqual(13u, resultNotification.NumQuoters);
            Assert.AreEqual(0.471935d, resultNotification.PreviousConfidence);
            Assert.AreEqual(47193500ul, resultNotification.PreviousConfidenceComponent);
            Assert.AreEqual(353.48636d, resultNotification.PreviousPrice);
            Assert.AreEqual(35348636000, resultNotification.PreviousPriceComponent);
            Assert.AreEqual(DeriveType.TWAP, resultNotification.PriceType);
            Assert.AreEqual(DeriveType.TWAP, resultNotification.PriceType);
            Assert.AreEqual(2u, resultNotification.Version);
            Assert.AreEqual(1584u, resultNotification.Size);
            Assert.AreEqual("5uKdRzB3FzdmwyCHrqSGq4u2URja617jqtKkM71BVrkw", resultNotification.ProductAccount);
            Assert.AreEqual(SystemProgram.ProgramIdKey, resultNotification.NextPriceAccount);

            streamingRpcMock
                .Setup(s => s.UnsubscribeAsync(It.IsAny<SubscriptionState>()))
                .Callback<SubscriptionState>(state =>
                {
                    Assert.AreEqual(sub.SubscriptionState, state);
                })
                .Returns(() => null)
                .Verifiable();

            sut.UnsubscribePriceDataAccount("5ALDzwcRJfSyGdGyhP3kP628aqBNHZzLuVww7o9kdspe");

            streamingRpcMock.Verify(
                s => s.UnsubscribeAsync(
                    It.Is<SubscriptionState>(ss => ss.Channel == sub.SubscriptionState.Channel)), Times.Once);
        }
    }
}