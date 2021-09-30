using Solnet.Programs;
using Solnet.Programs.Utilities;
using Solnet.Wallet;
using System;
using System.Collections.Generic;

namespace Solnet.Pyth.Models
{
    /// <summary>
    /// Represents a price data account on Pyth.
    /// </summary>
    public class PriceDataAccount : Account
    {
        /// <summary>
        /// Represents the data layout of the <see cref="PriceDataAccount"/>.
        /// </summary>
        internal class ExtraLayout
        {
            /// <summary>
            /// The offset at which the PriceType value starts.
            /// </summary>
            internal const int PriceTypeOffset = 16;

            /// <summary>
            /// The offset at which the Exponent value starts.
            /// </summary>
            internal const int ExponentOffset = 20;

            /// <summary>
            /// The offset at which the NumComponentPrices value starts.
            /// </summary>
            internal const int NumComponentPricesOffset = 24;

            /// <summary>
            /// The offset at which the NumQuoters value starts.
            /// </summary>
            internal const int NumQuotersOffset = 28;

            /// <summary>
            /// The offset at which the LastSlot value starts.
            /// </summary>
            internal const int LastSlotOffset = 32;

            /// <summary>
            /// The offset at which the ValidSlot value starts.
            /// </summary>
            internal const int ValidSlotOffset = 40;

            /// <summary>
            /// The offset at which the Twap value starts.
            /// </summary>
            internal const int TwapOffset = 48;

            /// <summary>
            /// The offset at which the Twac value starts.
            /// </summary>
            internal const int TwacOffset = 72;

            /// <summary>
            /// The offset at which the Drv1Component value starts.
            /// </summary>
            internal const int Drv1ComponentOffset = 96;

            /// <summary>
            /// The offset at which the Drv2Component value starts.
            /// </summary>
            internal const int Drv2ComponentOffset = 104;

            /// <summary>
            /// The offset at which the ProductAccount public key value starts.
            /// </summary>
            internal const int ProductAccountKeyOffset = 112;

            /// <summary>
            /// The offset at which the NextPriceAccount public key value starts.
            /// </summary>
            internal const int NextPriceAccountKeyOffset = 144;

            /// <summary>
            /// The offset at which the PreviousSlot public key value starts.
            /// </summary>
            internal const int PreviousSlotOffset = 176;

            /// <summary>
            /// The offset at which the PreviousPriceComponent public key value starts.
            /// </summary>
            internal const int PreviousPriceComponentOffset = 184;

            /// <summary>
            /// The offset at which the PreviousPriceConfidence public key value starts.
            /// </summary>
            internal const int PreviousPriceConfidenceOffset = 192;

            /// <summary>
            /// The offset at which the Drv3Component public key value starts.
            /// </summary>
            internal const int Drv3ComponentOffset = 200;

            /// <summary>
            /// The offset at which the AggregatePriceInfo public key value starts.
            /// </summary>
            internal const int AggregatePriceInfoOffset = 208;

            /// <summary>
            /// The offset at which the AggregatePriceInfo public key value starts.
            /// </summary>
            internal const int PriceComponentsOffset = 240;
        }

        /// <summary>
        /// The price or calculation type.
        /// </summary>
        public DeriveType PriceType;

        /// <summary>
        /// The exponent used to calculate the values from their components.
        /// </summary>
        public int Exponent;

        /// <summary>
        /// The number of price components.
        /// </summary>
        public uint NumPriceComponents;

        /// <summary>
        /// The number of quoters for the price.
        /// </summary>
        public uint NumQuoters;

        /// <summary>
        /// The current slot.
        /// </summary>
        public ulong LastSlot;

        /// <summary>
        /// The slot until which this price data is valid.
        /// </summary>
        public ulong ValidSlot;

        /// <summary>
        /// The time-weighted average price component.
        /// </summary>
        public Ema Twap;

        /// <summary>
        /// The time-weighted average confidence intervals.
        /// </summary>
        public Ema Twac;

        #region Future Derived Values

        /// <summary>
        /// Future derived value component.
        /// </summary>
        public long Drv1Component;

        /// <summary>
        /// Future derived value component.
        /// </summary>
        public long Drv2Component;

        /// <summary>
        /// Future derived value component.
        /// </summary>
        public long Drv3Component;

        /// <summary>
        /// Future derived value component.
        /// </summary>
        public double Drv1;

        /// <summary>
        /// Future derived value component.
        /// </summary>
        public double Drv2;

        /// <summary>
        /// Future derived value component.
        /// </summary>
        public double Drv3;

        #endregion

        /// <summary>
        /// The product account associated with this price data.
        /// </summary>
        public PublicKey ProductAccount;

        /// <summary>
        /// The next price account.
        /// </summary>
        public PublicKey NextPriceAccount;

        /// <summary>
        /// The previous valid slot.
        /// </summary>
        public ulong PreviousSlot;

        /// <summary>
        /// The raw aggregate price of the previous update.
        /// </summary>
        public long PreviousPriceComponent;

        /// <summary>
        /// The aggregate price of the previous update.
        /// </summary>
        public double PreviousPrice;

        /// <summary>
        /// The raw aggregate confidence interval of the previous update.
        /// </summary>
        public ulong PreviousConfidenceComponent;

        /// <summary>
        /// The aggregate confidence interval of the previous update.
        /// </summary>
        public double PreviousConfidence;

        /// <summary>
        /// The aggregate price info.
        /// </summary>
        public PriceInfo AggregatePriceInfo;

        /// <summary>
        /// The price components of this price data account.
        /// </summary>
        public List<PriceComponent> PriceComponents;

        /// <summary>
        /// Attempt to deserialize an account data into a <see cref="PriceDataAccount"/>.
        /// </summary>
        /// <param name="data">The account data as a span of bytes.</param>
        /// <returns>The <see cref="PriceDataAccount"/>.</returns>
        public static PriceDataAccount Deserialize(byte[] data)
        {
            ReadOnlySpan<byte> span = data.AsSpan();

            int exponent = span.GetS32(ExtraLayout.ExponentOffset);
            double multiplier = Math.Pow(10, exponent);
            uint numPriceComponents = span.GetU32(ExtraLayout.NumComponentPricesOffset);

            List<PriceComponent> priceComponents = new((int)numPriceComponents);
            ReadOnlySpan<byte> priceComponentsBytes = span[ExtraLayout.PriceComponentsOffset..];
            int idx = 0;

            while (idx < priceComponentsBytes.Length)
            {
                PublicKey publisher = priceComponentsBytes.GetPubKey(idx);
                if (publisher.Key == SystemProgram.ProgramIdKey) break;
                idx += PublicKey.PublicKeyLength;

                PriceInfo aggregate =
                    PriceInfo.Deserialize(priceComponentsBytes.Slice(idx, PriceInfo.Layout.Length), exponent);
                idx += PriceInfo.Layout.Length;

                PriceInfo latest =
                    PriceInfo.Deserialize(priceComponentsBytes.Slice(idx, PriceInfo.Layout.Length), exponent);
                idx += PriceInfo.Layout.Length;

                priceComponents.Add(new PriceComponent
                {
                    Aggregate = aggregate, Latest = latest, Publisher = publisher,
                });
            }

            long previousPriceComponent = span.GetS64(ExtraLayout.PreviousPriceComponentOffset);
            ulong previousConfidenceComponent = span.GetU64(ExtraLayout.PreviousPriceConfidenceOffset);
            long drv1 = span.GetS64(ExtraLayout.PreviousPriceComponentOffset);
            long drv2 = span.GetS64(ExtraLayout.PreviousPriceComponentOffset);
            long drv3 = span.GetS64(ExtraLayout.PreviousPriceComponentOffset);

            return new PriceDataAccount
            {
                MagicNumber = span.GetU32(Layout.MagicNumberOffset),
                Version = span.GetU32(Layout.VersionOffset),
                Size = span.GetU32(Layout.SizeOffset),
                Type = (AccountType)Enum.Parse(typeof(AccountType), span.GetU32(Layout.TypeOffset).ToString()),
                PriceType =
                    (DeriveType)Enum.Parse(typeof(DeriveType), span.GetU32(ExtraLayout.PriceTypeOffset).ToString()),
                Exponent = exponent,
                NumPriceComponents = numPriceComponents,
                NumQuoters = span.GetU32(ExtraLayout.NumQuotersOffset),
                LastSlot = span.GetU64(ExtraLayout.LastSlotOffset),
                ValidSlot = span.GetU64(ExtraLayout.ValidSlotOffset),
                Twap = Ema.Deserialize(span.Slice(ExtraLayout.TwapOffset, Ema.Layout.Length), multiplier),
                Twac = Ema.Deserialize(span.Slice(ExtraLayout.TwacOffset, Ema.Layout.Length), multiplier),
                Drv1Component = drv1,
                Drv2Component = drv2,
                Drv3Component = drv3,
                Drv1 = drv1 * multiplier,
                Drv2 = drv2 * multiplier,
                Drv3 = drv3 * multiplier,
                ProductAccount = span.GetPubKey(ExtraLayout.ProductAccountKeyOffset),
                NextPriceAccount = span.GetPubKey(ExtraLayout.NextPriceAccountKeyOffset),
                PreviousSlot = span.GetU64(ExtraLayout.PreviousSlotOffset),
                PreviousPriceComponent = previousPriceComponent,
                PreviousPrice = previousPriceComponent * multiplier,
                PreviousConfidenceComponent = previousConfidenceComponent,
                PreviousConfidence = (long)previousConfidenceComponent * multiplier,
                AggregatePriceInfo =
                    PriceInfo.Deserialize(span.Slice(ExtraLayout.AggregatePriceInfoOffset, PriceInfo.Layout.Length),
                        multiplier),
                PriceComponents = priceComponents
            };
        }
    }
}