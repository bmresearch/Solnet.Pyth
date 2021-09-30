using Solnet.Programs.Utilities;
using Solnet.Wallet;
using System;
using System.Collections.Generic;

namespace Solnet.Pyth.Models
{
    /// <summary>
    /// Represents a mapping account on Pyth.
    /// </summary>
    public class MappingAccount : Account
    {
        /// <summary>
        /// Represents the data layout of the <see cref="MappingAccount"/>.
        /// </summary>
        internal static class ExtraLayout
        {
            /// <summary>
            /// The offset at which the NumProducts value starts.
            /// </summary>
            internal const int NumProductsOffset = 16;

            /// <summary>
            /// The offset at which the NextMappingAccount value starts.
            /// <remarks>This is a public key.</remarks>
            /// </summary>
            internal const int NextMappingAccountOffset = 24;

            /// <summary>
            /// The offset at which the ProductAccountKeys value starts. 
            /// <remarks>This is a list of public keys, it's length is defined by the NumProducts value..</remarks>
            /// </summary>
            internal const int ProductAccountKeysOffset = 56;
        }
        
        /// <summary>
        /// The number of available products.
        /// </summary>
        public uint NumProducts;

        /// <summary>
        /// The public key of the next mapping account.
        /// </summary>
        public PublicKey NextMappingAccount;

        /// <summary>
        /// A list of product account public keys.
        /// </summary>
        public List<PublicKey> ProductAccountKeys;
        
        /// <summary>
        /// Attempt to deserialize an account data into a <see cref="MappingAccount"/>.
        /// </summary>
        /// <param name="data">The account data as a span of bytes.</param>
        /// <returns>The <see cref="MappingAccount"/>.</returns>
        public static MappingAccount Deserialize(byte[] data)
        {
            ReadOnlySpan<byte> span = data.AsSpan();
            
            uint numProducts = span.GetU32(ExtraLayout.NumProductsOffset);
            List<PublicKey> productAccounts = new((int) numProducts);
            ReadOnlySpan<byte> productAccountsBytes = span[ExtraLayout.ProductAccountKeysOffset..];

            for (int i = 0; i < numProducts; i++)
            {
                productAccounts.Add(productAccountsBytes.GetPubKey(i * PublicKey.PublicKeyLength));

            }
            
            return new MappingAccount
            {
                MagicNumber = span.GetU32(Layout.MagicNumberOffset),
                Version = span.GetU32(Layout.VersionOffset),
                Size = span.GetU32(Layout.SizeOffset),
                Type = (AccountType) Enum.Parse(typeof(AccountType), span.GetU32(Layout.TypeOffset).ToString()),
                NumProducts = numProducts,
                NextMappingAccount = span.GetPubKey(ExtraLayout.NextMappingAccountOffset),
                ProductAccountKeys = productAccounts
            };
        }
    }
}