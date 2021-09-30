using Solnet.Pyth.Models;
using Solnet.Rpc.Core.Sockets;
using Solnet.Rpc.Types;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Solnet.Pyth
{
    /// <summary>
    /// Specifies functionality for the Pyth Client.
    /// </summary>
    public interface IPythClient
    {
        /// <summary>
        /// Gets the given <see cref="MappingAccount"/>. This is an asynchronous operation.
        /// </summary>
        /// <param name="account">The <see cref="MappingAccount"/> <see cref="PublicKey"/>.</param>
        /// <param name="commitment">The confirmation commitment parameter for the RPC call.</param>
        /// <returns>The list of <see cref="MappingAccount"/>s or null in case an error occurred.</returns>
        Task<AccountResultWrapper<MappingAccount>> GetMappingAccountAsync(string account,
            Commitment commitment = Commitment.Finalized);

        /// <summary>
        /// Gets the given <see cref="ProductAccount"/>.
        /// </summary>
        /// <param name="account">The <see cref="ProductAccount"/> <see cref="PublicKey"/>.</param>
        /// <param name="commitment">The confirmation commitment parameter for the RPC call.</param>
        /// <returns>The list of <see cref="ProductAccount"/>s.</returns>
        AccountResultWrapper<MappingAccount> GetMappingAccount(string account,
            Commitment commitment = Commitment.Finalized);

        /// <summary>
        /// Gets the given <see cref="ProductAccount"/>. This is an asynchronous operation.
        /// </summary>
        /// <param name="account">The <see cref="ProductAccount"/> <see cref="PublicKey"/>.</param>
        /// <param name="commitment">The confirmation commitment parameter for the RPC call.</param>
        /// <returns>The list of <see cref="ProductAccount"/>s or null in case an error occurred.</returns>
        Task<AccountResultWrapper<ProductAccount>> GetProductAccountAsync(string account,
            Commitment commitment = Commitment.Finalized);

        /// <summary>
        /// Gets the given <see cref="ProductAccount"/>.
        /// </summary>
        /// <param name="account">The <see cref="ProductAccount"/> <see cref="PublicKey"/>.</param>
        /// <param name="commitment">The confirmation commitment parameter for the RPC call.</param>
        /// <returns>The list of <see cref="ProductAccount"/>s.</returns>
        AccountResultWrapper<ProductAccount> GetProductAccount(string account,
            Commitment commitment = Commitment.Finalized);

        /// <summary>
        /// Gets a list of <see cref="ProductAccount"/>s from the given <see cref="MappingAccount"/>. This is an asynchronous operation.
        /// </summary>
        /// <param name="mappingAccount">The <see cref="ProductAccount"/> <see cref="PublicKey"/>.</param>
        /// <param name="commitment">The confirmation commitment parameter for the RPC call.</param>
        /// <returns>The list of <see cref="ProductAccount"/>s or null in case an error occurred.</returns>
        Task<MultipleAccountsResultWrapper<List<ProductAccount>>> GetProductAccountsAsync(MappingAccount mappingAccount,
            Commitment commitment = Commitment.Finalized);

        /// <summary>
        /// Gets a list of <see cref="ProductAccount"/>s from the given <see cref="MappingAccount"/>.
        /// </summary>
        /// <param name="mappingAccount">The <see cref="ProductAccount"/> <see cref="PublicKey"/>.</param>
        /// <param name="commitment">The confirmation commitment parameter for the RPC call.</param>
        /// <returns>The list of <see cref="ProductAccount"/>s.</returns>
        MultipleAccountsResultWrapper<List<ProductAccount>> GetProductAccounts(MappingAccount mappingAccount,
            Commitment commitment = Commitment.Finalized);

        /// <summary>
        /// Gets the given <see cref="PriceDataAccount"/>. This is an asynchronous operation.
        /// </summary>
        /// <param name="account">The <see cref="PriceDataAccount"/> <see cref="PublicKey"/>.</param>
        /// <param name="commitment">The confirmation commitment parameter for the RPC call.</param>
        /// <returns>The list of <see cref="PriceDataAccount"/>s or null in case an error occurred.</returns>
        Task<AccountResultWrapper<PriceDataAccount>> GetPriceDataAccountAsync(string account,
            Commitment commitment = Commitment.Finalized);

        /// <summary>
        /// Gets the given <see cref="PriceDataAccount"/>.
        /// </summary>
        /// <param name="account">The <see cref="PriceDataAccount"/> <see cref="PublicKey"/>.</param>
        /// <param name="commitment">The confirmation commitment parameter for the RPC call.</param>
        /// <returns>The list of <see cref="PriceDataAccount"/>s.</returns>
        AccountResultWrapper<PriceDataAccount> GetPriceDataAccount(string account,
            Commitment commitment = Commitment.Finalized);

        /// <summary>
        /// Gets a list of <see cref="PriceDataAccount"/>s from the given list of <see cref="ProductAccount"/>.
        /// This is an asynchronous operation.
        /// </summary>
        /// <param name="productAccounts">The <see cref="PriceDataAccount"/> <see cref="PublicKey"/>.</param>
        /// <param name="commitment">The confirmation commitment parameter for the RPC call.</param>
        /// <returns>The list of <see cref="PriceDataAccount"/>s or null in case an error occurred.</returns>
        Task<MultipleAccountsResultWrapper<List<PriceDataAccount>>> GetPriceDataAccountsAsync(
            IEnumerable<ProductAccount> productAccounts,
            Commitment commitment = Commitment.Finalized);

        /// <summary>
        /// Gets a list of <see cref="PriceDataAccount"/>s from the given list of <see cref="ProductAccount"/>.
        /// </summary>
        /// <param name="productAccounts">The <see cref="PriceDataAccount"/> <see cref="PublicKey"/>.</param>
        /// <param name="commitment">The confirmation commitment parameter for the RPC call.</param>
        /// <returns>The list of <see cref="PriceDataAccount"/>s.</returns>
        MultipleAccountsResultWrapper<List<PriceDataAccount>> GetPriceDataAccounts(IEnumerable<ProductAccount> productAccounts,
            Commitment commitment = Commitment.Finalized);

        /// <summary>
        /// Subscribe to a live feed of a <see cref="PriceDataAccount"/>, the given action is called on every notification received. This is an asynchronous operation.
        /// </summary>
        /// <param name="action">An action which receives the <see cref="Subscription"/>, the decoded <see cref="PriceDataAccount"/> and the corresponding block slot.</param>
        /// <param name="priceAccountAddress">The <see cref="PublicKey"/> of the <see cref="PriceDataAccount"/>.</param>
        /// <param name="commitment">The commitment parameter for the Rpc Client.</param>
        /// <returns>The <see cref="Subscription"/> object containing the account's <see cref="PublicKey"/> and the <see cref="SubscriptionState"/>.</returns>
        Task<Subscription> SubscribePriceDataAccountAsync(Action<Subscription, PriceDataAccount, ulong> action,
            string priceAccountAddress, Commitment commitment = Commitment.Finalized);

        /// <summary>
        /// Subscribe to a live feed of a <see cref="PriceDataAccount"/>, the given action is called on every notification received. 
        /// </summary>
        /// <param name="action">An action which receives the <see cref="Subscription"/>, the decoded <see cref="PriceDataAccount"/> and the corresponding block slot.</param>
        /// <param name="priceAccountAddress">The <see cref="PublicKey"/> of the <see cref="PriceDataAccount"/>.</param>
        /// <param name="commitment">The commitment parameter for the Rpc Client.</param>
        /// <returns>The <see cref="Subscription"/> object containing the account's <see cref="PublicKey"/> and the <see cref="SubscriptionState"/>.</returns>
        Subscription SubscribePriceDataAccount(Action<Subscription, PriceDataAccount, ulong> action,
            string priceAccountAddress, Commitment commitment = Commitment.Finalized);

        /// <summary>
        /// Unsubscribe to a live feed of a <see cref="PriceDataAccount"/>. This is an asynchronous operation.
        /// </summary>
        /// <param name="priceAccountAddress">The <see cref="PublicKey"/> of the <see cref="PriceDataAccount"/>.</param>
        Task UnsubscribePriceDataAccountAsync(string priceAccountAddress);

        /// <summary>
        /// Unsubscribe to a live feed of a <see cref="PriceDataAccount"/>.
        /// </summary>
        /// <param name="priceAccountAddress">The <see cref="PublicKey"/> of the <see cref="PriceDataAccount"/>.</param>
        void UnsubscribePriceDataAccount(string priceAccountAddress);
    }
}