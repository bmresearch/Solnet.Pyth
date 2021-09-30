using Solnet.Programs.Utilities;
using System;

namespace Solnet.Pyth.Models
{
    /// <summary>
    /// Represents information about price on Pyth.
    /// </summary>
    public class PriceInfo
    {
        /// <summary>
        /// Represents the data layout of the <see cref="PriceInfo"/>.
        /// </summary>
        internal static class Layout
        {
            /// <summary>
            /// The length of the <see cref="PriceInfo"/> account data.
            /// </summary>
            internal const int Length = 32;

            /// <summary>
            /// The offset at which the PriceComponent value starts.
            /// </summary>
            internal const int PriceComponentOffset = 0;
        
            /// <summary>
            /// The offset at which the ConfidenceComponent value starts.
            /// </summary>
            internal const int ConfidenceComponentOffset = 8;
        
            /// <summary>
            /// The offset at which the Status value starts.
            /// </summary>
            internal const int StatusOffset = 16;

            /// <summary>
            /// The offset at which the CorporateAction value starts.
            /// </summary>
            internal const int CorporateActionOffset = 20;

            /// <summary>
            /// The offset at which the PublishSlot value starts.
            /// </summary>
            internal const int PublishSlotOffset = 24;
        }
        
        /// <summary>
        /// The raw aggregate price.
        /// </summary>
        public long PriceComponent;

        /// <summary>
        /// The aggregate price after calculated according to it's exponent.
        /// </summary>
        public double Price;

        /// <summary>
        /// The raw aggregate confidence.
        /// </summary>
        public ulong ConfidenceComponent;

        /// <summary>
        /// The aggregate confidence after calculated according to it's exponent.
        /// </summary>
        public double Confidence;
        
        /// <summary>
        /// The aggregate status.
        /// </summary>
        public PriceStatus Status;

        /// <summary>
        /// The aggregate corporate action.
        /// </summary>
        public uint CorporateAction;
        
        /// <summary>
        /// The aggregate publish slot.
        /// </summary>
        public ulong PublishSlot;

        /// <summary>
        /// Attempt to deserialize an account data into a <see cref="PriceInfo"/>.
        /// </summary>
        /// <param name="data">The account data as a span of bytes.</param>
        /// <param name="multiplier">The multiplier to be used to calculate underlying price and confidence.</param>
        /// <returns>The <see cref="PriceInfo"/>.</returns>
        public static PriceInfo Deserialize(ReadOnlySpan<byte> data, double multiplier)
        {
            if (data.Length != Layout.Length) throw new Exception("data length is invalid");

            long priceComponent = data.GetS64(Layout.PriceComponentOffset);
            ulong confidenceComponent = data.GetU64(Layout.ConfidenceComponentOffset);

            return new PriceInfo
            {
                PriceComponent = priceComponent,
                Price =  priceComponent * multiplier,
                ConfidenceComponent = confidenceComponent,
                Confidence = confidenceComponent * multiplier,
                Status = (PriceStatus) Enum.Parse(typeof(PriceStatus), data.GetU32(Layout.StatusOffset).ToString()),
                CorporateAction = data.GetU32(Layout.CorporateActionOffset),
                PublishSlot = data.GetU64(Layout.PublishSlotOffset)
            };
        }
    }
}