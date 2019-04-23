using Shortcodes.Services;

namespace Shortcodes.Parsing
{
    public class ParsingOpenTagState : ParserState
    {
        public ParsingOpenTagState(ShortcodeService shortcodeParser)
            : base(shortcodeParser)
        {
        }

        /// <summary>
        /// Gets or sets position where the tag starts.
        /// </summary>
        public int TagBeginPosition { get; set; }

        public override void Parse()
        {
            var tagNameBeginPosition = _textParser.Position;

            _textParser.MoveTo(new[] { SPACE_CHAR, CLOSING_SLASH_CHAR, TAG_END_CHAR });

            var tagName = _textParser.Extract(tagNameBeginPosition, _textParser.Position);

            if (_shortcodeParser.Exists(tagName))
            {
                _shortcodeParser.CurrentShortcode = new ShortcodeParseInfo
                {
                    Tag = tagName,
                    BeginPosition = TagBeginPosition,
                    EndPosition = _textParser.Position + 1
                };

                _textParser.MovePastWhitespace();

                var peek = _textParser.Peek();

                if (peek == TAG_END_CHAR) // end char for tag
                {
                    SetState(new LookingForTagState(_shortcodeParser));
                }
                else if (peek == CLOSING_SLASH_CHAR) // closing tag
                {
                    _textParser.MovePast(new[] { TAG_BEGIN_CHAR, CLOSING_SLASH_CHAR });

                    _shortcodeParser.CurrentShortcode.IsClosed = true;
                    _shortcodeParser.CurrentShortcode.EndPosition = _textParser.Position + 1;

                    StoreCurrentShortcode();

                    SetState(new LookingForTagState(_shortcodeParser));
                }
                else if (IsAttributeNameChar(peek)) // is start of an attribute
                {
                    SetState(new ParsingAttributesState(_shortcodeParser));
                }
                else
                {
                    // invalid tag, reset currentShortcode and start looking for new tag.
                    _shortcodeParser.CurrentShortcode = null;
                    SetState(new LookingForTagState(_shortcodeParser));
                }
            }
            else
            {
                _textParser.MoveTo(TAG_END_CHAR);
                SetState(new LookingForTagState(_shortcodeParser));
            }
        }
    }
}