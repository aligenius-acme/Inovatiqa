using System.Collections.Generic;

namespace Inovatiqa.Services.Messages.Interfaces
{
    public partial interface ITokenizerService
    {
        string Replace(string template, IEnumerable<Token> tokens, bool htmlEncode);
    }
}
