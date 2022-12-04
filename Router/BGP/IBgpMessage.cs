namespace Router
{
    public interface IBgpMessage
    {
        /// <summary>
        /// Gets the BGP message as a sequence of bytes
        /// </summary>
        /// <returns>a byte array</returns>
        public byte[] GetBytes();
    }
}