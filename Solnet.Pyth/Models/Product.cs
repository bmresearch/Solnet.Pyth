namespace Solnet.Pyth.Models
{
    /// <summary>
    /// Represents a Pyth product.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// The base of the product.
        /// <remarks>Example: SOL</remarks>
        /// </summary>
        public string Base;

        /// <summary>
        /// The symbol of the product.
        /// <remarks>Example: SOL/USD</remarks>
        /// </summary>
        public string Symbol;

        /// <summary>
        /// The type of the asset.
        /// <remarks>Example: Crypto</remarks>
        /// </summary>
        public string AssetType;

        /// <summary>
        /// The quote currency of the product.
        /// <remarks>Example: USD</remarks>
        /// </summary>
        public string QuoteCurrency;

        /// <summary>
        /// The tenor of the product.
        /// <remarks>Example: Spot</remarks>
        /// </summary>
        public string Tenor;
        
        /// <summary>
        /// The description of the product.
        /// <remarks>Example: NETFLIX INC</remarks>
        /// </summary>
        public string Description;

        /// <summary>
        /// The country of the product.
        /// <remarks>Example: US</remarks>
        /// </summary>
        public string Country;
    }
}