using Fan.Helpers;
using System;
using System.Linq;

namespace Fan.Shortcodes.Parsing
{
    public abstract class ParserState
    {
        protected ShortcodeService _shortcodeParser;
        protected TextParser _textParser;

        protected const char SPACE_CHAR = ' ';
        protected const char TAG_BEGIN_CHAR = '[';
        protected const char TAG_END_CHAR = ']';
        protected const char CLOSING_SLASH_CHAR = '/';
        protected const char EQUAL_CHAR = '=';
        protected const char SINGLE_QUOTE_CHAR = '\'';
        protected const char DOUBLE_QUOTE_CHAR = '\"';
        // olw uses these quote chars by default
        protected const char SINGLE_QUOTE_CHAR2a = '‘';
        protected const char SINGLE_QUOTE_CHAR2b = '’';
        protected const char DOUBLE_QUOTE_CHAR2a = '“';
        protected const char DOUBLE_QUOTE_CHAR2b = '”';
        

        protected static readonly char[] SpecialChars =
        {
            TAG_BEGIN_CHAR, TAG_END_CHAR, CLOSING_SLASH_CHAR, EQUAL_CHAR, SINGLE_QUOTE_CHAR, DOUBLE_QUOTE_CHAR,
            SINGLE_QUOTE_CHAR2a, SINGLE_QUOTE_CHAR2b, DOUBLE_QUOTE_CHAR2a, DOUBLE_QUOTE_CHAR2b
        };

        protected static readonly char[] QuoteChars =
        {
            SINGLE_QUOTE_CHAR, DOUBLE_QUOTE_CHAR, SINGLE_QUOTE_CHAR2a, SINGLE_QUOTE_CHAR2b, DOUBLE_QUOTE_CHAR2a, DOUBLE_QUOTE_CHAR2b
        };

        protected ParserState(ShortcodeService shortcodeParser)
        {
            _shortcodeParser = shortcodeParser;
            _textParser = shortcodeParser.TextParser;
        }

        public abstract void Parse();

        public void SetState(ParserState state)
        {
            _shortcodeParser.ParserState = state;
        }

        protected void StoreCurrentShortcode()
        {
            if (_shortcodeParser.CurrentShortcode != null)
            {
                _shortcodeParser.ParseInstructions.Add(_shortcodeParser.CurrentShortcode);
                _shortcodeParser.CurrentShortcode = null;
            }
        }

        protected bool IsAttributeNameChar(char @char)
        {
            if (@char == SPACE_CHAR || SpecialChars.Contains(@char))
                return false;

            return Char.IsLetterOrDigit(@char) || Char.IsSymbol(@char);
        }
    }

}
