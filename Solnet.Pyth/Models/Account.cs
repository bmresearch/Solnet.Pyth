namespace Solnet.Pyth.Models
{
    /// <summary>
    /// Represents a base account in Pyth.
    /// </summary>
    public abstract class Account
    {
        /// <summary>
        /// Represents the data layout of the <see cref="Account"/>.
        /// </summary>
        internal static class Layout
        {
            /// <summary>
            /// The offset at which the MagicNumber value starts.
            /// </summary>
            internal const int MagicNumberOffset = 0;
        
            /// <summary>
            /// The offset at which the Version value starts.
            /// </summary>
            internal const int VersionOffset = 4;

            /// <summary>
            /// The offset at which the Type value starts.
            /// </summary>
            internal const int TypeOffset = 8;

            /// <summary>
            /// The offset at which the Size value starts.
            /// </summary>
            internal const int SizeOffset = 12;
        }
        
        /// <summary>
        /// Magic.
        /// </summary>
        public uint MagicNumber;

        /// <summary>
        /// The current version.
        /// </summary>
        public uint Version;

        /// <summary>
        /// The type of the mapping account.
        /// </summary>
        public AccountType Type;

        /// <summary>
        /// The size of the account data.
        /// </summary>
        public uint Size;
    }
}