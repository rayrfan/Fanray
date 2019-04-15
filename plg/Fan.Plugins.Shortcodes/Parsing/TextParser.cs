using System;

namespace Fan.Plugins.Shortcodes.Parsing
{
    /// <summary>
    /// A parsing approach that scans the input text character by character.
    /// </summary>
    /// <remarks>
    /// http://www.blackbeltcoder.com/Articles/strings/a-text-parsing-helper-class
    /// </remarks>
    public class TextParser
    {
        private string _text;
        private int _position;

        public string Text { get { return _text; } }
        public string ProcessedText { get { return Extract(0, _position); } }
        public int Position { get { return _position; } }
        public Char CurrentChar { get { return _text[_position]; } }
        public int Length { get { return _text.Length; } }
        public int Remaining { get { return _text.Length - _position; } }
        public static char NullChar = (char)0;

        public TextParser()
        {
            Reset(null);
        }

        public TextParser(string text)
        {
            Reset(text);
        }

        /// <summary>
        /// Resets the current position to the start of the current document
        /// </summary>
        public void Reset()
        {
            _position = 0;
        }

        /// <summary>
        /// Resets the current position to a specified position in the current document
        /// </summary>
        public void ResetTo(int position)
        {
            _position = position;

            if (_position > _text.Length)
                _position = _text.Length;
        }

        /// <summary>
        /// Sets the current document and resets the current position to the start of it
        /// </summary>
        /// <param name="html"></param>
        public void Reset(string text)
        {
            _text = text ?? String.Empty;
            _position = 0;
        }

        /// <summary>
        /// Indicates if the current position is at the end of the current document
        /// </summary>
        public bool IsEndOfText
        {
            get { return (_position >= _text.Length); }
        }

        /// <summary>
        /// Returns the character at the current position, or a null character if we're
        /// at the end of the document
        /// </summary>
        /// <returns>The character at the current position</returns>
        public char Peek()
        {
            return Peek(0);
        }

        /// <summary>
        /// Returns the character at the specified number of characters beyond the current
        /// position, or a null character if the specified position is at the end of the
        /// document
        /// </summary>
        /// <param name="ahead">The number of characters beyond the current position</param>
        /// <returns>The character at the specified position</returns>
        public char Peek(int ahead)
        {
            int pos = (_position + ahead);
            if (pos < _text.Length)
                return _text[pos];
            return NullChar;
        }

        /// <summary>
        /// Extracts a substring from the specified position to the end of the text
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public string Extract(int start)
        {
            return Extract(start, _text.Length);
        }

        /// <summary>
        /// Extracts a substring from the specified range of the current text
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public string Extract(int start, int end)
        {
            return _text.Substring(start, end - start);
        }

        /// <summary>
        /// Moves the current position ahead one character
        /// </summary>
        public void MoveAhead()
        {
            MoveAhead(1);
        }

        /// <summary>
        /// Moves the current position ahead the specified number of characters
        /// </summary>
        /// <param name="ahead">The number of characters to move ahead</param>
        public void MoveAhead(int ahead)
        {
            _position = Math.Min(_position + ahead, _text.Length);
        }

        /// <summary>
        /// Moves to the next occurrence of the specified string
        /// </summary>
        /// <param name="text">String to find</param>
        /// <param name="ignoreCase">Indicates if case-insensitive comparisons
        /// are used</param>
        public void MoveTo(string text, bool ignoreCase = false)
        {
            _position = _text.IndexOf(text, _position, ignoreCase ?
                StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

            if (_position < 0)
                _position = _text.Length;
        }

        /// <summary>
        /// Moves to the next occurrence of the specified character
        /// </summary>
        /// <param name="c">Character to find</param>
        public void MoveTo(char c)
        {
            _position = _text.IndexOf(c, _position);
            if (_position < 0)
                _position = _text.Length;
        }

        /// <summary>
        /// Moves to the next occurrence of any one of the specified
        /// characters
        /// </summary>
        /// <param name="chars">Array of characters to find</param>
        public void MoveTo(char[] chars)
        {
            _position = _text.IndexOfAny(chars, _position);
            if (_position < 0)
                _position = _text.Length;
        }

        /// <summary>
        /// Moves to the next occurrence of any character that is not one
        /// of the specified characters
        /// </summary>
        /// <param name="chars">Array of characters to move past</param>
        public void MovePast(char[] chars)
        {
            while (IsInArray(Peek(), chars))
                MoveAhead();
        }

        /// <summary>
        /// Determines if the specified character exists in the specified
        /// character array.
        /// </summary>
        /// <param name="c">Character to find</param>
        /// <param name="chars">Character array to search</param>
        /// <returns></returns>
        protected bool IsInArray(char c, char[] chars)
        {
            foreach (char ch in chars)
            {
                if (c == ch)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Moves the current position to the first character that is part of a newline
        /// </summary>
        public void MoveToEndOfLine()
        {
            char c = Peek();
            while (c != '\r' && c != '\n' && !IsEndOfText)
            {
                MoveAhead();
                c = Peek();
            }
        }

        /// <summary>
        /// Moves the current position to the next character that is not whitespace
        /// </summary>
        public void MovePastWhitespace()
        {
            while (Char.IsWhiteSpace(Peek()))
                MoveAhead();
        }
    }
}
