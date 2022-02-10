using Solnet.Programs.Utilities;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Text;

namespace Solnet.Pyth.Models
{
    /// <summary>
    /// Represents a product account on Pyth.
    /// </summary>
    public class ProductAccount : Account
    {
        /// <summary>
        /// Represents the data layout of the <see cref="ProductAccount"/>.
        /// </summary>
        internal static class ExtraLayout
        {
            /// <summary>
            /// The offset at which the PriceAccount value starts.
            /// <remarks>This value is a public key.</remarks>
            /// </summary>
            internal const int PriceAccountOffset = 16;
        
            /// <summary>
            /// The offset at which the ProductAttributes value starts.
            /// <remarks>This value is a dictionary where both keys and values are strings.</remarks>
            /// </summary>
            internal const int ProductAttributesOffset = 48;
        }
        
        /// <summary>
        /// The public key of this product's price account.
        /// </summary>
        public PublicKey PriceAccount;

        /// <summary>
        /// A dictionary of attributes associated with this product.
        /// </summary>
        public Dictionary<string, string> ProductAttributes;

        /// <summary>
        /// The product information.
        /// </summary>
        public Product Product;
        
        /// <summary>
        /// Attempt to deserialize an account data into a <see cref="ProductAccount"/>.
        /// </summary>
        /// <param name="data">The account data as a span of bytes.</param>
        /// <returns>The <see cref="ProductAccount"/>.</returns>
        public static ProductAccount Deserialize(byte[] data)
        {
            ReadOnlySpan<byte> span = data.AsSpan();

            Dictionary<string, string> productAttributes = new();
            ReadOnlySpan<byte> productAttributesBytes = span[ExtraLayout.ProductAttributesOffset..];
            int idx = 0;
            while (idx < productAttributesBytes.Length)
            {
                int keyLength = productAttributesBytes[idx];
                idx++;
                if (keyLength == 0) continue;

                ReadOnlySpan<byte> key = productAttributesBytes.Slice(idx, keyLength);
                idx += keyLength;

                int valueLength = productAttributesBytes[idx];
                idx++;
                if (valueLength == 0) continue;

                ReadOnlySpan<byte> value = productAttributesBytes.Slice(idx, valueLength);
                idx += valueLength;

                string keyString = Encoding.UTF8.GetString(key);
                string valueString = Encoding.UTF8.GetString(value);
                productAttributes.Add(keyString, valueString);
            }

            bool hasType = productAttributes.TryGetValue("asset_type", out string assetType);
            bool hasBase = productAttributes.TryGetValue("base", out string baseSymbol);
            bool hasSymbol = productAttributes.TryGetValue("symbol", out string symbol);
            bool hasTenor = productAttributes.TryGetValue("tenor", out string tenor);
            bool hasQuote = productAttributes.TryGetValue("quote_currency", out string quoteCurrency);
            bool hasCountry = productAttributes.TryGetValue("country", out string country);
            bool hasDescription = productAttributes.TryGetValue("description", out string description);

            Product product = new ()
            {
                Base = hasBase ? baseSymbol : string.Empty,
                AssetType = hasType ? assetType : string.Empty,
                Symbol = hasSymbol ? symbol : string.Empty,
                Tenor = hasTenor ? tenor : string.Empty,
                QuoteCurrency = hasQuote ? quoteCurrency : string.Empty,
                Country = hasCountry ? country : string.Empty,
                Description = hasDescription ? description : string.Empty,
            };
            
            return new ProductAccount
            {
                MagicNumber = span.GetU32(Layout.MagicNumberOffset),
                Version = span.GetU32(Layout.VersionOffset),
                Size = span.GetU32(Layout.SizeOffset),
                Type = (AccountType) Enum.Parse(typeof(AccountType), span.GetU32(Layout.TypeOffset).ToString()),
                PriceAccount = span.GetPubKey(ExtraLayout.PriceAccountOffset),
                ProductAttributes = productAttributes,
                Product = product
            };
        }
    }
}