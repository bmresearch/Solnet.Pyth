namespace Solnet.Pyth.Models
{
    /// <summary>
    /// The derived component type.
    /// </summary>
    public enum DeriveType
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown,
        
        /// <summary>
        /// Time weighted average price.
        /// </summary>
        TWAP,
        
        /// <summary>
        /// Volatility.
        /// </summary>
        Volatility,
    }
}