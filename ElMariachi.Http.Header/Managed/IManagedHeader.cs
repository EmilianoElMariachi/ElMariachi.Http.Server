namespace ElMariachi.Http.Header.Managed
{
    /// <summary>
    /// Base interface for all managed headers
    /// </summary>
    public interface IManagedHeader : IHeader
    {
        /// <summary>
        /// Removes this header
        /// </summary>
        void Unset();
    }
}
