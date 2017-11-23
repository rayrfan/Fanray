namespace Fan.Shortcodes.Parsing
{
    public class LookingForTagState : ParserState
    {
        public LookingForTagState(ShortcodeService shortcodeParser) : base(shortcodeParser) { }

        public override void Parse()
        {
            _textParser.MoveTo(TAG_BEGIN_CHAR);

            if (_textParser.IsEndOfText)
            {
                StoreCurrentShortcode();
                return;
            }

            // another begin tag char, escaped content ("[[")
            if (_textParser.Peek(1) == TAG_BEGIN_CHAR)
            {
                HandleEscapedContent();
                return;
            }
            
            var tagBeginPosition = _textParser.Position;

            _textParser.MoveAhead();
            _textParser.MovePastWhitespace();

            var peek = _textParser.Peek();

            // is it an end tag: [/shortcode]
            if (peek == CLOSING_SLASH_CHAR) // tag is a closing tag
            {
                SetState(new ParsingCloseTagState(_shortcodeParser)
                {
                    TagBeginPosition = tagBeginPosition
                });
            }
            // new tag found
            else if (IsAttributeNameChar(peek))
            {
                // before going into a new tag, save any current ones to processed list.
                StoreCurrentShortcode();

                // we found a new tag, change to parsing tag state
                SetState(new ParsingOpenTagState(_shortcodeParser)
                {
                    TagBeginPosition = tagBeginPosition
                });
            }
        }

        private void HandleEscapedContent()
        {
            var beginEscapedPosition = _textParser.Position;

            _textParser.MovePast(new[] { TAG_BEGIN_CHAR });

            var beginTagPosition = _textParser.Position;

            _textParser.MoveTo(new[] { SPACE_CHAR, CLOSING_SLASH_CHAR, TAG_END_CHAR });

            var tagName = _textParser.Extract(beginTagPosition, _textParser.Position);

            MovePastEscapedTag();

            if (_shortcodeParser.Exists(tagName) && _textParser.Peek() == TAG_END_CHAR)
            {
                _textParser.MoveAhead();

                _shortcodeParser.ParseInstructions.Add(new ShortcodeParseInfo
                {
                    BeginPosition = beginEscapedPosition,
                    EndPosition = _textParser.Position,
                    PreCompiledContent = _textParser.Extract(beginEscapedPosition + 1, _textParser.Position - 1)
                });
            }
            else
            {
                // we didn't find a matching square bracket for escaped content.
                // reset position to where it was.
                _textParser.ResetTo(beginEscapedPosition);
                _textParser.MovePast(new[] { TAG_BEGIN_CHAR });
            }
        }

        private void MovePastEscapedTag()
        {
            if (_textParser.IsEndOfText)
                return;

            _textParser.MoveTo(TAG_END_CHAR);

            // didnt find matching escaped end chars, call recursive
            if (_textParser.Peek(1) != TAG_END_CHAR)
            {
                _textParser.MoveAhead();

                MovePastEscapedTag();
            }
            else
            {
                _textParser.MoveAhead();
            }
        }
    }
}