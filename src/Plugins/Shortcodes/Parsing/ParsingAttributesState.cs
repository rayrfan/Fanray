using Shortcodes.Services;
using System.Linq;

namespace Shortcodes.Parsing
{
    public class ParsingAttributesState : ParserState
    {
        public ParsingAttributesState(ShortcodeService shortcodeParser) : base(shortcodeParser) {}

        public override void Parse()
        {
            var beginNamePosition = _textParser.Position;

            _textParser.MoveTo(new[] { SPACE_CHAR, EQUAL_CHAR, TAG_END_CHAR });

            var attributeName = _textParser.Extract(beginNamePosition, _textParser.Position);

            AddAttribute(attributeName);

            _textParser.MovePastWhitespace();

            if (_textParser.Peek() == EQUAL_CHAR) // attribute has a value
            {
                _textParser.MoveAhead();
                _textParser.MovePastWhitespace();

                var peek = _textParser.Peek();
                char enclosingChar = SPACE_CHAR;
                bool isInQuotation = false;

                if (QuoteChars.Contains(peek))
                {
                    isInQuotation = true;
                    enclosingChar = peek;

                    _textParser.MoveAhead();
                }

                var beginValuePosition = _textParser.Position;

                _textParser.MoveTo(isInQuotation ? 
                    new[] { SINGLE_QUOTE_CHAR, DOUBLE_QUOTE_CHAR, SINGLE_QUOTE_CHAR2a, SINGLE_QUOTE_CHAR2b, DOUBLE_QUOTE_CHAR2a, DOUBLE_QUOTE_CHAR2b} : 
                    new[] { SPACE_CHAR, TAG_END_CHAR, CLOSING_SLASH_CHAR });

                var attributeValue = _textParser.Extract(beginValuePosition, _textParser.Position);
                
                _shortcodeParser.CurrentShortcode.Attributes[attributeName] = attributeValue;

                _textParser.MovePast(new[] { SINGLE_QUOTE_CHAR, DOUBLE_QUOTE_CHAR, SINGLE_QUOTE_CHAR2a, SINGLE_QUOTE_CHAR2b, DOUBLE_QUOTE_CHAR2a, DOUBLE_QUOTE_CHAR2b, enclosingChar, CLOSING_SLASH_CHAR });
            }
            
            _textParser.MovePastWhitespace();

            if (IsAttributeNameChar(_textParser.Peek()))
            {
                SetState(new ParsingAttributesState(_shortcodeParser));
            }
            else
            {
                _textParser.MovePast(new[] { CLOSING_SLASH_CHAR, TAG_END_CHAR });
                _shortcodeParser.CurrentShortcode.EndPosition = _textParser.Position;

                StoreCurrentShortcode();

                SetState(new LookingForTagState(_shortcodeParser));  
            }
        }

        private void AddAttribute(string attributeName)
        {
            if (!_shortcodeParser.CurrentShortcode.Attributes.ContainsKey(attributeName))
            {
                _shortcodeParser.CurrentShortcode.Attributes.Add(attributeName, string.Empty);
            }
        }
    }
}