using Solnet.Programs.Utilities;
using Solnet.Wallet;
using System;

namespace Solnet.Pyth.Models
{
    /// <summary>
    /// Represents a Pyth price component.
    /// </summary>
    public class PriceComponent
    {
        /// <summary>
        /// Represents the data layout of the <see cref="PriceComponent"/>.
        /// </summary>
        internal static class Layout
        {
            /// <summary>
            /// The length of the <see cref="PriceComponent"/> account data.
            /// </summary>
            internal const int Length = 96;

            /// <summary>
            /// The offset at which the publisher's <see cref="PublicKey"/> begins.
            /// </summary>
            internal const int PublisherOffset = 0;

            /// <summary>
            /// The offset at which the aggregate <see cref="PriceInfo"/> begins.
            /// </summary>
            internal const int AggregateOffset = 32;

            /// <summary>
            /// The offset at which the latest <see cref="PriceInfo"/> begins.
            /// </summary>
            internal const int LatestOffset = 64;
        }
        
        /// <summary>
        /// The public key of this price component's publisher.
        /// </summary>
        public PublicKey Publisher;
        
        /// <summary>
        /// The aggregate price info of this price component.
        /// </summary>
        public PriceInfo Aggregate;

        /// <summary>
        /// The latest price info of this price component.
        /// </summary>
        public PriceInfo Latest;
        
        /// <summary>
        /// Attempt to deserialize an account data into a <see cref="PriceComponent"/>.
        /// </summary>
        /// <param name="data">The account data as a span of bytes.</param>
        /// <param name="multiplier">The multiplier to be used to calculate underlying price and confidence.</param>
        /// <returns>The <see cref="PriceComponent"/>.</returns>
        public static PriceComponent Deserialize(ReadOnlySpan<byte> data, double multiplier)
        {
            if (data.Length != Layout.Length) throw new Exception("data length is invalid");

            return new PriceComponent
            {
                Publisher = data.GetPubKey(Layout.PublisherOffset),
                Aggregate = PriceInfo.Deserialize(data.Slice(Layout.AggregateOffset, PriceInfo.Layout.Length), multiplier),
                Latest = PriceInfo.Deserialize(data.Slice(Layout.LatestOffset, PriceInfo.Layout.Length), multiplier),
            };
        }
    }
}