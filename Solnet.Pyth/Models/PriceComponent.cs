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
    }
}