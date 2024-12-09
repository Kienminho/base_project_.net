using System.Globalization;

namespace Common.Authorization.Utils
{
    public class KeyExistsException : Exception
    {
        public KeyExistsException() : base() { }
        public KeyExistsException(string message) : base(message) { }
        public KeyExistsException(string message, params object[] args) : base(String.Format(CultureInfo.CurrentCulture, message, args)) { }
    }
}
