namespace Solnet.Pyth.Models
{
    /// <summary>
    /// Represents the status of the asset for a price account.
    /// </summary>
    public enum PriceStatus
    {
        /// <summary>
        /// Unknown status.
        /// </summary>
        Unknown,
        
        /// <summary>
        /// Asset is trading.
        /// </summary>
        Trading,
        
        /// <summary>
        /// Trading of the asset has been halted.
        /// </summary>
        Halted,
        
        /// <summary>
        /// Asset is in auction mode.
        /// </summary>
        Auction
    }
}