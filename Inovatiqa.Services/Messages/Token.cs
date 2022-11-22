namespace Inovatiqa.Services.Messages
{
    public sealed class Token
    {
        #region Ctor

        public Token(string key, object value) : this(key, value, false)
        {
        }

        public Token(string key, object value, bool neverHtmlEncoded)
        {
            Key = key;
            Value = value;
            NeverHtmlEncoded = neverHtmlEncoded;
        }

        #endregion

        #region Properties

        public string Key { get; }

        public object Value { get; }

        public bool NeverHtmlEncoded { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"{Key}: {Value}";
        }

        #endregion
    }
}
