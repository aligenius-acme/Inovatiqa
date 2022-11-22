using Inovatiqa.Core;
using Inovatiqa.Services.Messages.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Text.RegularExpressions;

namespace Inovatiqa.Services.Messages
{
    public partial class TokenizerService : ITokenizerService
    {
        #region Fields

        #endregion

        #region Ctor

        public TokenizerService()
        {
        }

        #endregion

        #region Utilities

        protected string Replace(string original, string pattern, string replacement)
        {
            var stringComparison = InovatiqaDefaults.CaseInvariantReplacement ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (stringComparison == StringComparison.Ordinal)
                return original.Replace(pattern, replacement);

            var count = 0;
            var position0 = 0;
            int position1;

            var inc = original.Length / pattern.Length * (replacement.Length - pattern.Length);
            var chars = new char[original.Length + Math.Max(0, inc)];
            while ((position1 = original.IndexOf(pattern, position0, stringComparison)) != -1)
            {
                for (var i = position0; i < position1; ++i)
                    chars[count++] = original[i];
                for (var i = 0; i < replacement.Length; ++i)
                    chars[count++] = replacement[i];
                position0 = position1 + pattern.Length;
            }

            if (position0 == 0)
                return original;

            for (var i = position0; i < original.Length; ++i)
                chars[count++] = original[i];

            return new string(chars, 0, count);
        }

        protected string ReplaceTokens(string template, IEnumerable<Token> tokens, bool htmlEncode = false, bool stringWithQuotes = false)
        {
            foreach (var token in tokens)
            {
                var tokenValue = token.Value ?? string.Empty;

                if (stringWithQuotes && tokenValue is string)
                    tokenValue = $"\"{tokenValue}\"";
                else
                {
                    if (htmlEncode && !token.NeverHtmlEncoded)
                        tokenValue = WebUtility.HtmlEncode(tokenValue.ToString());
                }

                template = Replace(template, $@"%{token.Key}%", tokenValue.ToString());
            }

            return template;
        }

        protected string ReplaceConditionalStatements(string template, IEnumerable<Token> tokens)
        {
            var regexFullConditionalSatement = new Regex(@"(?:(?'Group' %if)|(?'Condition-Group' endif%)|(?! (%if|endif%)).)*(?(Group)(?!))",
                RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var regexCondition = new Regex(@"\s*\((?:(?'Group' \()|(?'-Group' \))|[^()])*(?(Group)(?!))\)\s*",
                RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.IgnoreCase);

            var conditionalStatements = regexFullConditionalSatement.Matches(template)
                .SelectMany(match => match.Groups["Condition"].Captures.Select(capture => new
                {
                    capture.Index,
                    FullStatement = capture.Value,
                    Condition = regexCondition.Match(capture.Value).Value
                })).ToList();

            if (!conditionalStatements.Any())
                return template;

            foreach (var statement in conditionalStatements.OrderBy(statement => statement.Index))
            {
                var conditionIsMet = false;
                if (!string.IsNullOrEmpty(statement.Condition))
                {
                    try
                    {
                        var conditionString = ReplaceTokens(statement.Condition, tokens, stringWithQuotes: true);
                        conditionIsMet = new[] { statement }.AsQueryable().Where(conditionString).Any();
                    }
                    catch
                    {
                    }
                }

                template = template.Replace(conditionIsMet ? statement.Condition : statement.FullStatement, string.Empty);
            }

            template = template.Replace("%if", string.Empty).Replace("endif%", string.Empty);

            return template;
        }

        #endregion

        #region Methods

        public string Replace(string template, IEnumerable<Token> tokens, bool htmlEncode)
        {
            if (string.IsNullOrWhiteSpace(template))
                throw new ArgumentNullException(nameof(template));

            if (tokens == null)
                throw new ArgumentNullException(nameof(tokens));

            template = ReplaceConditionalStatements(template, tokens);

            template = ReplaceTokens(template, tokens, htmlEncode);

            return template;
        }

        #endregion
    }
}