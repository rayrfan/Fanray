using System;

namespace Fan.Shortcodes.Parsing
{
    public class ParsingCloseTagState : ParserState
    {
        public ParsingCloseTagState(ShortcodeService shortcodeParser)
            : base(shortcodeParser)
        {
        }

        /// <summary>
        /// Gets or sets position where the tag starts.
        /// </summary>
        public int TagBeginPosition { get; set; }

        public override void Parse()
        {
            _textParser.MovePast(new[] { CLOSING_SLASH_CHAR });

            var beginTagPosition = _textParser.Position;

            _textParser.MoveTo(new[] { SPACE_CHAR, CLOSING_SLASH_CHAR, TAG_END_CHAR });

            var tagName = _textParser.Extract(beginTagPosition, _textParser.Position);

            if (_shortcodeParser.Exists(tagName))
            {
                StoreCurrentShortcode();

                for (int i = _shortcodeParser.ParseInstructions.Count - 1; i >= 0; i--)
                {
                    var shortcodeParseInfo = _shortcodeParser.ParseInstructions[i];

                    if (shortcodeParseInfo.Tag != null && shortcodeParseInfo.Tag.Equals(tagName, StringComparison.InvariantCultureIgnoreCase) && !shortcodeParseInfo.IsClosed)
                    {
                        shortcodeParseInfo.Content = _textParser.Extract(shortcodeParseInfo.EndPosition, TagBeginPosition);
                        shortcodeParseInfo.EndPosition = _textParser.Position + 1;
                        shortcodeParseInfo.IsClosed = true;

                        break;
                    }
                    else
                    {
                        // remove shortcodes between start and closing tag of shortcode.
                        _shortcodeParser.ParseInstructions.RemoveAt(i);
                    }
                }
            }

            SetState(new LookingForTagState(_shortcodeParser));
        }
    }
}