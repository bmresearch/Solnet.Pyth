using Microsoft.Extensions.Logging;
using Solnet.Pyth.Models;
using Solnet.Rpc;
using Solnet.Rpc.Core.Sockets;
using Solnet.Rpc.Types;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Solnet.Pyth
{
    /// <summary>
    /// Implements functionality to poll and stream data from Pyth.
    /// </summary>
    public class PythClient : BaseClient, IPythClient
    {
        /// <summary>
        /// The logger instance.
        /// </summary>
        private ILogger _logger;

        /// <summary>
        /// The list of <see cref="PriceDataAccount"/> subscriptions.
        /// </summary>
        private readonly IList<SubscriptionWrapper<PriceDataAccount>> _priceDataAccountSubscriptions;

        /// <summary>
        /// Initialize the Serum Client.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="rpcClient">The RPC client instance.</param>
        /// <param name="streamingRpcClient">The streaming RPC client.</param>
        /// <returns>The Serum Client.</returns>
        internal PythClient(ILogger logger = null, IRpcClient rpcClient = default,
            IStreamingRpcClient streamingRpcClient = default) : base(rpcClient, streamingRpcClient)
        {
            _logger = logger;
            _priceDataAccountSubscriptions = new List<SubscriptionWrapper<PriceDataAccount>>();
        }

        #region Streaming JSON RPC

        /// <inheritdoc cref="SubscribePriceDataAccountAsync(Action{Subscription, PriceDataAccount, ulong}, string, Commitment)"/>
        public async Task<Subscription> SubscribePriceDataAccountAsync(
            Action<Subscription, PriceDataAccount, ulong> action, string priceAccountAddress,
            Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState sub = await SubscribeAccount<PriceDataAccount>(priceAccountAddress,
                (state, accountInfo, priceAccount) =>
                {
                    SubscriptionWrapper<PriceDataAccount> priceAccountSub =
                        _priceDataAccountSubscriptions.FirstOrDefault(
                            s => s.Address.Key == priceAccountAddress);
                    if (priceAccountSub == null) return;
                    priceAccountSub.SubscriptionState = state;
                    action(priceAccountSub, priceAccount, accountInfo.Context.Slot);
                }, commitment);

            SubscriptionWrapper<PriceDataAccount> subOpenOrders = new()
            {
                SubscriptionState = sub, Address = new PublicKey(priceAccountAddress)
            };
            _priceDataAccountSubscriptions.Add(subOpenOrders);
            return subOpenOrders;
        }

        /// <inheritdoc cref="SubscribePriceDataAccount(Action{Subscription, PriceDataAccount, ulong}, string, Commitment)"/>
        public Subscription SubscribePriceDataAccount(Action<Subscription, PriceDataAccount, ulong> action,
            string priceAccountAddress, Commitment commitment = Commitment.Finalized) =>
            SubscribePriceDataAccountAsync(action, priceAccountAddress, commitment).Result;

        /// <inheritdoc cref="UnsubscribePriceDataAccountAsync(string)"/>
        public Task UnsubscribePriceDataAccountAsync(string priceAccountAddress)
        {
            SubscriptionWrapper<PriceDataAccount> subscriptionWrapper = null;

            foreach (SubscriptionWrapper<PriceDataAccount> sub in _priceDataAccountSubscriptions)
            {
                if (sub.Address.Key != priceAccountAddress) continue;

                subscriptionWrapper = sub;
                _priceDataAccountSubscriptions.Remove(sub);
                break;
            }

            return subscriptionWrapper == null
                ? null
                : StreamingRpcClient.UnsubscribeAsync(subscriptionWrapper.SubscriptionState);
        }

        /// <inheritdoc cref="UnsubscribePriceDataAccount(string)"/>
        public void UnsubscribePriceDataAccount(string priceAccountAddress) =>
            UnsubscribePriceDataAccountAsync(priceAccountAddress).Wait();

        #endregion

        #region JSON RPC Requests

        /// <inheritdoc cref="GetMappingAccountAsync(string, Commitment)"/>
        public async Task<AccountResultWrapper<MappingAccount>> GetMappingAccountAsync(string account,
            Commitment commitment = Commitment.Finalized)
            => await GetAccount<MappingAccount>(account, commitment);

        /// <inheritdoc cref="GetMappingAccount(string, Commitment)"/>
        public AccountResultWrapper<MappingAccount> GetMappingAccount(string account,
            Commitment commitment = Commitment.Finalized) => GetMappingAccountAsync(account, commitment).Result;

        /// <inheritdoc cref="GetProductAccountAsync(string, Commitment)"/>
        public async Task<AccountResultWrapper<ProductAccount>> GetProductAccountAsync(string account,
            Commitment commitment = Commitment.Finalized)
            => await GetAccount<ProductAccount>(account, commitment);

        /// <inheritdoc cref="GetProductAccount(string, Commitment)"/>
        public AccountResultWrapper<ProductAccount> GetProductAccount(string account,
            Commitment commitment = Commitment.Finalized) => GetProductAccountAsync(account, commitment).Result;

        /// <inheritdoc cref="GetProductAccountsAsync(MappingAccount, Commitment)"/>
        public async Task<MultipleAccountsResultWrapper<List<ProductAccount>>> GetProductAccountsAsync(
            MappingAccount account, Commitment commitment = Commitment.Finalized) =>
            await GetMultipleAccounts<ProductAccount>(account.ProductAccountKeys.Select(x => x.Key).ToList(),
                commitment);

        /// <inheritdoc cref="GetProductAccounts(MappingAccount, Commitment)"/>
        public MultipleAccountsResultWrapper<List<ProductAccount>> GetProductAccounts(MappingAccount account,
            Commitment commitment = Commitment.Finalized) => GetProductAccountsAsync(account, commitment).Result;

        /// <inheritdoc cref="GetPriceDataAccountAsync(string, Commitment)"/>
        public async Task<AccountResultWrapper<PriceDataAccount>> GetPriceDataAccountAsync(string account,
            Commitment commitment = Commitment.Finalized) => await GetAccount<PriceDataAccount>(account, commitment);

        /// <inheritdoc cref="GetPriceDataAccount(string, Commitment)"/>
        public AccountResultWrapper<PriceDataAccount> GetPriceDataAccount(string account,
            Commitment commitment = Commitment.Finalized) => GetPriceDataAccountAsync(account, commitment).Result;

        /// <inheritdoc cref="GetPriceDataAccountsAsync(IEnumerable{ProductAccount}, Commitment)"/>
        public async Task<MultipleAccountsResultWrapper<List<PriceDataAccount>>> GetPriceDataAccountsAsync(
            IEnumerable<ProductAccount> productAccounts, Commitment commitment = Commitment.Finalized) =>
            await GetMultipleAccounts<PriceDataAccount>(
                productAccounts.Select(x => x.PriceAccount.Key).ToList(), commitment);

        /// <inheritdoc cref="GetPriceDataAccounts(IEnumerable{ProductAccount}, Commitment)"/>
        public MultipleAccountsResultWrapper<List<PriceDataAccount>> GetPriceDataAccounts(
            IEnumerable<ProductAccount> productAccounts, Commitment commitment = Commitment.Finalized)
            => GetPriceDataAccountsAsync(productAccounts, commitment).Result;

        #endregion
    }
}