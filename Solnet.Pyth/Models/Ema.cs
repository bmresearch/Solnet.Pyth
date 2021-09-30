using Solnet.Programs.Utilities;
using System;

namespace Solnet.Pyth.Models
{
    /// <summary>
    /// Represents an EMA on a Pyth <see cref="PriceDataAccount"/>.
    /// </summary>
    public class Ema
    {
        /// <summary>
        /// Represents the layout of the <see cref="Ema"/>.
        /// </summary>
        internal static class Layout
        {
            /// <summary>
            /// The length of the <see cref="Ema"/> account data.
            /// </summary>
            internal const int Length = 24;

            /// <summary>
            /// The offset at which the ValueComponent value begins.
            /// </summary>
            internal const int ValueComponentOffset = 0;

            /// <summary>
            /// The offset at which the Numerator value begins.
            /// </summary>
            internal const int NumeratorOffset = 8;

            /// <summary>
            /// The offset at which the Denominator value begins.
            /// </summary>
            internal const int DenominatorOffset = 16;
        }

        /// <summary>
        /// The raw value component.
        /// </summary>
        public long ValueComponent;
        
        /// <summary>
        /// The value after calculated using the relevant exponent.
        /// </summary>
        public double Value;

        /// <summary>
        /// The numerator.
        /// </summary>
        public long Numerator;

        /// <summary>
        /// The denominator.
        /// </summary>
        public long Denominator;
        
        /// <summary>
        /// Attempt to deserialize an account data into a <see cref="Ema"/>.
        /// </summary>
        /// <param name="data">The account data as a span of bytes.</param>
        /// <param name="multiplier">The multiplier to be used to calculate underlying price and confidence.</param>
        /// <returns>The <see cref="Ema"/>.</returns>
        public static Ema Deserialize(ReadOnlySpan<byte> data, double multiplier)
        {
            if (data.Length != Layout.Length) throw new Exception("data length is invalid");

            long valueComponent = data.GetS64(Layout.ValueComponentOffset);

            return new Ema
            {
                ValueComponent = valueComponent,
                Value = valueComponent * multiplier,
                Numerator = data.GetS64(Layout.NumeratorOffset),
                Denominator = data.GetS64(Layout.DenominatorOffset),
            };
        }
    }
}