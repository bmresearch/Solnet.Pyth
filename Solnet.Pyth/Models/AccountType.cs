namespace Solnet.Pyth.Models
{
    /// <summary>
    /// The type of Pyth account.
    /// </summary>
    public enum AccountType
    {
        /// <summary>
        /// An unknown <see cref="Account"/>.
        /// </summary>
        Unknown,
        
        /// <summary>
        /// A <see cref="MappingAccount"/>.
        /// </summary>
        Mapping,
        
        /// <summary>
        /// A <see cref="ProductAccount"/>.
        /// </summary>
        Product,
        
        /// <summary>
        /// A <see cref="PriceDataAccount"/>.
        /// </summary>
        Price,
        
        /// <summary>
        /// A test account.
        /// </summary>
        Test,
    }
}