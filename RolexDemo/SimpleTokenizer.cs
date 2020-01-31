//
//  This CSharp output file generated by Rolex
// Copyright (C) by codewitch honey crisis, 2020.
// Rolex uses the GPLEX engine
//  Gardens Point LEX (GPLEX) is Copyright (c) John Gough, QUT 2006-2014.
//  Output produced by Rolex is the property of the user.
//
//  Rolex Version:  1.2.2
//  Machine:  DESKTOP-EC2OMEU
//  DateTime: 1/31/2020 10:17:37 AM
//  UserName: honey
//  Rolex input file <C:\Users\honey\source\repos\Rolex\RolexDemo\Simple.rl - 1/31/2020 8:47:31 AM>
//  Rolex frame file <embedded resource>
//
//  Option settings: unicode, verbose, parser, minimize
//  Option settings: classes, compressMap, compressNext, persistBuffer, noEmbedBuffers
//  Fallback code page: Target machine default
//

//
// Revised backup code
// Version 1.2.1 of 24-June-2013
//
//

using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;


namespace RolexDemo
{
   
     internal sealed partial class SimpleTokenizer
    : IEnumerable<Token>
	{
		TextReader _reader = null;
		Stream _stream = null;
		string _codePage = null;
		IEnumerable<char> _string = null;
		string _filename = null;
     internal const int ErrorSymbol = -1;
		internal const int id = 0;
		internal const int @int = 1;
     internal SimpleTokenizer(TextReader reader)
		{
			_reader = reader;
		}

     internal SimpleTokenizer(IEnumerable<char> text)
		{
			_string = text;
		}
     internal SimpleTokenizer(Stream stream,string codePage)
		{
			_stream = stream;
			_codePage = codePage;
		}
     internal SimpleTokenizer(Stream stream)
		{
			_stream = stream;
		}
     private SimpleTokenizer() { }
     public static SimpleTokenizer Open(string filename,string codePage)
		{
			var tokenizer = new 
     SimpleTokenizer();
			tokenizer._filename = filename;
			tokenizer._codePage = codePage;
			return tokenizer;
		}
     public static SimpleTokenizer Open(string filename)
		{
			var tokenizer = new 
     SimpleTokenizer();
			tokenizer._filename = filename;
			return tokenizer;
		}
		public IEnumerator<Token> GetEnumerator()
		{
			if(null!=_reader)
			{
				var result = new 
     Enumerator(_reader);
				_reader = null;
				return result;
			}
			if(null!=_string)
			{
				return new 
     Enumerator(_string);
			}
			if(null!=_stream)
			{
     Enumerator result = null;
				if(null!=_codePage)
				{
					result = new 
     Enumerator(_stream, _codePage);
				} else
				{
					result = new 
     Enumerator(_stream);
				}
				_stream = null;
				return result;
			}
			if(null!=_filename)
			{
				if(null!=_codePage)
					return new 
     Enumerator(_filename, File.OpenRead(_filename), _codePage);
				else
					return new 
     Enumerator(_filename, File.OpenText(_filename));
			}
			throw new NotSupportedException("This type of input can be enumerated only once");
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	
        /// <summary>
        /// Summary Canonical example of GPLEX automaton
        /// </summary>
     
        // If the compiler can't find the scanner base class maybe you
        // need to run GPPG with the /gplex option, or GPLEX with /noparser

     internal sealed partial class Enumerator
          : IEnumerator<Token>
        {
            private const int _Disposed = -3;
		    private const int _Initial = -2;
		    private const int _EndOfInput = -1;
		    private const int _Enumerating = 0;
		    private int _state=_Initial;
		    private Token _current;
		    private IDisposable _ownedInput;

            private ScanBuff buffer;
            private int currentScOrd;  // start condition ordinal
        
            /// <summary>
            /// The input buffer for this scanner.
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public ScanBuff Buffer { get { return buffer; } }
        
      
 
        const int maxAccept = 6;
        const int initial = 7;
        const int eofNum = 0;
        const int goStart = -1;
        const int INITIAL = 0;

#region user code
#endregion user code

            int state;
            int currentStart = startState[0];
            int code;      // last code read
            int cCol;      // column number of code
            int lNum;      // current line number
            //
            // The following instance variables are used, among other
            // things, for constructing the yylloc location objects.
            //
            int tokPos;        // buffer position at start of token
            int tokCol;        // zero-based column number at start of token
            int tokLin;        // line number at start of token
            int tokEPos;       // buffer position at end of token
            int tokECol;       // column number at end of token
            int tokELin;       // line number at end of token
            string tokTxt;     // lazily constructed text of token
#if STACK          
            private Stack<int> scStack = new Stack<int>();
#endif // STACK

#region ScannerTables
    struct Table {
        public int min; public int rng; public int dflt;
        public sbyte[] nxt;
        public Table(int m, int x, int d, sbyte[] n) {
            min = m; rng = x; dflt = d; nxt = n;
        }
    };

    static int[] startState = new int[] {7, 0};

   static int[] anchorState = null;
#region CompressedCharacterMap
    //
    // There are 7 equivalence classes
    // There are 2 character sequence regions
    // There are 1 tables, 123 entries
    // There are 1 runs, 0 singletons
    // Decision tree depth is 1
    //
    static sbyte[] mapC0 = new sbyte[123] {
/*     '\0' */ 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 5, 6, 6, 6, 0, 0, 
/*   '\x10' */ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
/*   '\x20' */ 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 
/*      '0' */ 3, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 
/*      '@' */ 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 
/*      'P' */ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 
/*      '`' */ 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 
/*      'p' */ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

    static sbyte MapC(int code)
    { // '\0' <= code <= '\U0010FFFF'
      if (code < 123) // '\0' <= code <= 'z'
        return mapC0[code - 0];
      else // '{' <= code <= '\U0010FFFF'
        return (sbyte)0;
    }
#endregion

    static Table[] NxS = new Table[8] {
/* NxS[   0] */ new Table(0, 0, 0, null), // Shortest string ""
/* NxS[   1] */ new Table(0, 0, -1, null), // Shortest string ""
/* NxS[   2] */ // Shortest string "A"
      new Table(1, 3, -1, new sbyte[] {2, 2, 2}),
/* NxS[   3] */ // Shortest string "1"
      new Table(2, 2, -1, new sbyte[] {3, 3}),
/* NxS[   4] */ new Table(0, 0, -1, null), // Shortest string "0"
/* NxS[   5] */ // Shortest string "-"
      new Table(2, 1, -1, new sbyte[] {3}),
/* NxS[   6] */ new Table(0, 0, -1, null), // Shortest string "\n"
/* NxS[   7] */ // Shortest string ""
      new Table(0, 5, 6, new sbyte[] {1, 2, 3, 4, 5}),
    };

int NextState() {
    if (code == ScanBuff.EndOfFile)
        return eofNum;
    else
        unchecked {
            int rslt;
            int idx = MapC(code) - NxS[state].min;
            if (idx < 0) idx += 7;
            if ((uint)idx >= (uint)NxS[state].rng) rslt = NxS[state].dflt;
            else rslt = NxS[state].nxt[idx];
            return rslt;
        }
}

#endregion



            // ==============================================================
            // ==== Nested struct to support input switching in scanners ====
            // ==============================================================

		    struct BufferContext {
                internal ScanBuff buffSv;
			    internal int chrSv;
			    internal int cColSv;
			    internal int lNumSv;
		    }

#if BACKUP
            // ==============================================================
            // == Nested struct used for backup in automata that do backup ==
            // ==============================================================

            struct Context // class used for automaton backup.
            {
                public int bPos;
                public int rPos; // scanner.readPos saved value
                public int cCol;
                public int lNum; // Need this in case of backup over EOL.
                public int state;
                public int cChr;
            }
        
            private Context ctx = new Context();
#endif // BACKUP
            // ==============================================================
            // ===== Private methods to save and restore buffer contexts ====
            // ==============================================================

            /// <summary>
            /// This method creates a buffer context record from
            /// the current buffer object, together with some
            /// scanner state values. 
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            BufferContext MkBuffCtx()
		    {
			    BufferContext rslt;
			    rslt.buffSv = this.buffer;
			    rslt.chrSv = this.code;
			    rslt.cColSv = this.cCol;
			    rslt.lNumSv = this.lNum;
			    return rslt;
		    }

            /// <summary>
            /// This method restores the buffer value and allied
            /// scanner state from the given context record value.
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            void RestoreBuffCtx(BufferContext value)
		    {
			    this.buffer = value.buffSv;
			    this.code = value.chrSv;
			    this.cCol = value.cColSv;
			    this.lNum = value.lNumSv;
            } 
            // =================== End Nested classes =======================

         public Token Current {
			    get {
				    if(0>_state)
				    {
					    if (_Initial == _state)
						    throw new InvalidOperationException("The cursor is before the beginning of the enumeration");
					    if (_EndOfInput == _state)
						    throw new InvalidOperationException("The cursor is after the end of the enumeration");
					    _CheckDisposed();
				    }
				    return _current;
			    }
		    }
        object System.Collections.IEnumerator.Current { get { return Current; } }

		    void _CheckDisposed()
		    {
			    if (_Disposed == _state)
				    throw new ObjectDisposedException("Scanner");
		    }
		    void IDisposable.Dispose()
		    {
			    if (_Disposed != _state)
			    {
				    if (null != _ownedInput)
					    _ownedInput.Dispose();
				    _ownedInput = null;
				    _state = _Disposed;
			    }
		    }
        public bool MoveNext()
		    {
			    if(0>_state)
			    {
				    _CheckDisposed();
				    if (_EndOfInput == _state)
					    return false;
			    }
			    _state = _Enumerating;
			    int next;
			    do { next = Scan(); } while (next >= int.MaxValue);
			    if(-2==next)
			    {
				    _state = _EndOfInput;
				    return false;
			    }
			    _current.Line = tokLin;
			    _current.Column = tokCol+1;
			    _current.Position = tokPos;
			
                _current.SymbolId = next;
			    if (null==tokTxt)
				    tokTxt = buffer.GetString(tokPos, tokEPos);
			    _current.Value = tokTxt;
			    return true;
		    }
   
        void System.Collections.IEnumerator.Reset()
		    {
			    throw new NotSupportedException("This type of enumerator cannot be reset.");
		    }
            bool _TryReadUntil(int character, StringBuilder sb)
		    {
			
			    if (-1 == code) return false;
			    var chcmp = character.ToString();
			    var s = char.ConvertFromUtf32(code);
			    sb.Append(s);
			    if (code == character)
				    return true;
			    while (true)
			    {
				    GetCode();
				    if (-1 == code || code == character)
					    break;
				    s = char.ConvertFromUtf32(code);
				    sb.Append(s);
			    }
			    if (-1 != code)
			    {
				    s = char.ConvertFromUtf32(code);
				    sb.Append(s);
				    if (null == tokTxt)
					    tokTxt = sb.ToString();
				    else
					    tokTxt += sb.ToString();

				    return code == character;
			    }
			    return false;
		    }
		    // reads until the string is encountered, capturing it.
		    bool _TryReadUntilBlockEnd(string blockEnd)
		    {
			    string s = yytext;
			    var sb = new StringBuilder();
			    int ch = -1;
			    var isPair = false;
			    if (char.IsSurrogatePair(blockEnd, 0))
			    {
				    ch = char.ConvertToUtf32(blockEnd, 0);
				    isPair = true;
			    }
			    else
				    ch = blockEnd[0];
			    while (-1 != code && _TryReadUntil(ch, sb))
			    {
				    bool found = true;
				    int i = 1;
				    if (isPair)
					    ++i;
				    for (; found && i < blockEnd.Length; ++i)
				    {
					    GetCode();
					    int scmp = blockEnd[i];
					    if (char.IsSurrogatePair(blockEnd, i))
					    {
						    scmp = char.ConvertToUtf32(blockEnd, i);
						    ++i;
					    }
					    if (-1 == code || code != scmp)
						    found = false;
					    else if (-1 != code)
						    sb.Append(char.ConvertFromUtf32(code));
				    }
				    if (found)
				    {
					    // TODO: verify this
					    GetCode();
					    tokTxt = s + sb.ToString();
					    return true;
				    }
			    }
			    tokTxt = s + sb.ToString();
			    return false;
		    }
     internal Enumerator(TextReader reader)
            {
                SetSource(reader); 
            }
     internal Enumerator(IEnumerable<char> str)
            {
               IEnumerator<char> e = str.GetEnumerator();
                _ownedInput = e;
                SetSource(e); 
            }
     internal Enumerator(string filename,TextReader reader) : this(reader)
		    {
			    // we grab the filename from reader anyway
			    _ownedInput = reader;
		    }
     internal Enumerator(string filename, Stream stream, string codePage) : this(stream,codePage)
		    {
			    // we grab the filename from stream anyway
			    _ownedInput = stream;
		    }
     internal Enumerator(string filename, Stream stream) : this(stream)
		    {
			    // we grab the filename from stream anyway
			    _ownedInput = stream;
		    }
    
     internal Enumerator(Stream file) {
            SetSource(file, 0); // unicode option
        }

        public Enumerator(Stream file, string codepage) {
            SetSource(file, CodePageHandling.GetCodePage(codepage));
            }   

     internal Enumerator() { }

            private int readPos;

            void GetCode()
            {
                if (code == '\n')  // This needs to be fixed for other conventions
                                   // i.e. [\r\n\205\u2028\u2029]
                { 
                    cCol = -1;
                    ++lNum;
                } else if(code =='\r') {
                    cCol = -1;
                } else if(code=='\t') {
                   cCol = cCol + 3;
                }
                readPos = buffer.Pos;

                // Now read new codepoint.
                code = buffer.Read();
                if (code > ScanBuff.EndOfFile)
                {

                    if (code >= 0xD800 && code <= 0xDBFF)
                    {
                        int next = buffer.Read();
                        if (next < 0xDC00 || next > 0xDFFF)
                            code = ScanBuff.UnicodeReplacementChar;
                        else
                            code = (0x10000 + ((code & 0x3FF) << 10) + (next & 0x3FF));
                    }
                    ++cCol;
                }
            }

            void MarkToken()
            {
                tokPos = readPos;
                tokLin = lNum;
                tokCol = cCol;
            }
        
            void MarkEnd()
            {
                tokTxt = null;
                tokEPos = readPos;
                tokELin = lNum;
                tokECol = cCol;
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            int Peek()
            {
                int rslt, codeSv = code, cColSv = cCol, lNumSv = lNum, bPosSv = buffer.Pos;
                GetCode(); rslt = code;
                lNum = lNumSv; cCol = cColSv; code = codeSv; buffer.Pos = bPosSv;
                return rslt;
            }

            // ==============================================================
            // =====    Initialization of string-based input buffers     ====
            // ==============================================================

             /// <summary>
            /// Create and initialize a StringBuff buffer object for this scanner
            /// </summary>
            /// <param name="source">the input string</param>
            /// <param name="offset">starting offset in the string</param>
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public void SetSource(IEnumerator<char> source)
            {
                this.buffer = ScanBuff.GetBuffer(source);
                this.buffer.Pos = 0;
                this.lNum = 0;
                this.code = '\n'; // to initialize yyline, yycol and lineStart
                GetCode();
            }


       

            // =============== StreamBuffer Initialization ==================

            /// <summary>
            /// Create and initialize a StreamBuff buffer object for this scanner.
            /// StreamBuff is buffer for 8-bit byte files.
            /// </summary>
            /// <param name="source">the input byte stream</param>
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public void SetSource(Stream source)
            {
                this.buffer = ScanBuff.GetBuffer(source);
                this.lNum = 0;
                this.code = '\n'; // to initialize yyline, yycol and lineStart
                GetCode();
            }
            /// <summary>
            /// Create and initialize a TextBuff buffer object for this scanner.
            /// TextBuff is a buffer for encoded unicode files.
            /// </summary>
            /// <param name="source">the input text file</param>
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public void SetSource(TextReader source)
            {
                this.buffer = ScanBuff.GetBuffer(source);
                this.lNum = 0;
                this.code = '\n'; // to initialize yyline, yycol and lineStart
                GetCode();
            }
            // ================ TextBuffer Initialization ===================

            /// <summary>
            /// Create and initialize a TextBuff buffer object for this scanner.
            /// TextBuff is a buffer for encoded unicode files.
            /// </summary>
            /// <param name="source">the input text file</param>
            /// <param name="fallbackCodePage">Code page to use if file has
            /// no BOM. For 0, use machine default; for -1, 8-bit binary</param>
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public void SetSource(Stream source, int fallbackCodePage)
            {
                this.buffer = ScanBuff.GetBuffer(source, fallbackCodePage);
                this.lNum = 0;
                this.code = '\n'; // to initialize yyline, yycol and lineStart
                GetCode();
            }
        
            // ======== AbstractScanner<> Implementation =========

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "yylex")]
            [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "yylex")]
            public int yylex()
            {
                int next;
                do { next = Scan(); } while (next >= int.MaxValue);
                return next;
            }
        
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            int yypos { get { return tokPos; } }
        
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            int yyline { get { return tokLin; } }
        
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            int yycol { get { return tokCol; } }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "yytext")]
            [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "yytext")]
            public string yytext
            {
                get 
                {
                    if (tokTxt == null) 
                        tokTxt = buffer.GetString(tokPos, tokEPos);
                    return tokTxt;
                }
            }

            /// <summary>
            /// Discards all but the first "n" codepoints in the recognized pattern.
            /// Resets the buffer position so that only n codepoints have been consumed;
            /// yytext is also re-evaluated. 
            /// </summary>
            /// <param name="n">The number of codepoints to consume</param>
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            void yyless(int n)
            {
                buffer.Pos = tokPos;
                // Must read at least one char, so set before start.
                cCol = tokCol - 1; 
                GetCode();
                // Now ensure that line counting is correct.
                lNum = tokLin;
                // And count the rest of the text.
                for (int i = 0; i < n; ++i) GetCode();
                MarkEnd();
            }
       
            //
            //  It would be nice to count backward in the text
            //  but it does not seem possible to re-establish
            //  the correct column counts except by going forward.
            //
            /// <summary>
            /// Removes the last "n" code points from the pattern.
            /// </summary>
            /// <param name="n">The number to remove</param>
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            void _yytrunc(int n) { yyless(yyleng - n); }
        
            //
            // This is painful, but we no longer count
            // codepoints.  For the overwhelming majority 
            // of cases the single line code is fast, for
            // the others, well, at least it is all in the
            // buffer so no files are touched. Note that we
            // can't use (tokEPos - tokPos) because of the
            // possibility of surrogate pairs in the token.
            //
            /// <summary>
            /// The length of the pattern in codepoints (not the same as 
            /// string-length if the pattern contains any surrogate pairs).
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "yyleng")]
            [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "yyleng")]
            public int yyleng
            {
                get {
                    if (tokELin == tokLin)
                        return tokECol - tokCol;
                    else
                    {
                        int ch;
                        int count = 0;
                        int save = buffer.Pos;
                        buffer.Pos = tokPos;
                        do {
                            ch = buffer.Read();
                            if (!char.IsHighSurrogate((char)ch)) ++count;
                        } while (buffer.Pos < tokEPos && ch != ScanBuff.EndOfFile);
                        buffer.Pos = save;
                        return count;
                    }
                }
            }
        
            // ============ methods available in actions ==============

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal int YY_START {
                get { return currentScOrd; }
                set { currentScOrd = value; 
                      currentStart = startState[value]; 
                } 
            }
        
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal void BEGIN(int next) {
                currentScOrd = next;
                currentStart = startState[next];
            }

            // ============== The main tokenizer code =================

            int Scan() {
                    for (; ; ) {
                        int next;              // next state to enter
                        if(null!=anchorState) {
                            for (;;) {
                                // Discard characters that do not start any pattern.
                                // Must check the left anchor condition after *every* GetCode!
                                state = ((cCol == 0) ? anchorState[currentScOrd] : currentStart);
                                if ((next = NextState()) != goStart) break; // LOOP EXIT HERE...
                                GetCode();
                            }
                    
                        } else {
                            state = currentStart;
                            while ((next = NextState()) == goStart) {
                                // At this point, the current character has no
                                // transition from the current state.  We discard 
                                // the "no-match" char.   In traditional LEX such 
                                // characters are echoed to the console.
                                GetCode();
                            }
                        }

                        // At last, a valid transition ...    
                        MarkToken();
                        state = next;
                        GetCode();                    
#if BACKUP
                        bool contextSaved = false;
                        while ((next = NextState()) > eofNum) { // Exit for goStart AND for eofNum
                            if (state <= maxAccept && next > maxAccept) { // need to prepare backup data
                                // Store data for the *latest* accept state that was found.
                                SaveStateAndPos( ref ctx );
                                contextSaved = true;
                            }
                            state = next;
                            GetCode();
                        }
                        if (state > maxAccept && contextSaved)
                            RestoreStateAndPos( ref ctx );
#else  // BACKUP
                        while ((next = NextState()) > eofNum) { // Exit for goStart AND for eofNum
                             state = next;
                             GetCode();
                        }
#endif // BACKUP
                        if (state <= maxAccept) {
                            MarkEnd();
#region ActionSwitch
#pragma warning disable 162, 1522
    switch (state)
    {
        case eofNum:
            return -2;
            break;
        case 1: // Recognized .,	Shortest string ""
        case 5: // Recognized .,	Shortest string "-"
return -1;
            break;
        case 2: // Recognized '[A-Z_a-z][0-9A-Z_a-z]*',	Shortest string "A"
 return 0;
            break;
        case 3: // Recognized '0|\-?[1-9][0-9]*',	Shortest string "1"
        case 4: // Recognized '0|\-?[1-9][0-9]*',	Shortest string "0"
 return 1;
            break;
        case 6: // Recognized '[ \t\r\n\v\f]',	Shortest string "\n"

            break;
        default:
            break;
    }
#pragma warning restore 162, 1522
#endregion
                        }
                    }
            }

#if BACKUP
            void SaveStateAndPos(ref Context ctx) {
                ctx.bPos  = buffer.Pos;
                ctx.rPos  = readPos;
                ctx.cCol  = cCol;
                ctx.lNum  = lNum;
                ctx.state = state;
                ctx.cChr  = code;
            }

            void RestoreStateAndPos(ref Context ctx) {
                buffer.Pos = ctx.bPos;
                readPos = ctx.rPos;
                cCol  = ctx.cCol;
                lNum  = ctx.lNum;
                state = ctx.state;
                code  = ctx.cChr;
            }
#endif  // BACKUP


            // ============= End of the tokenizer code ================


#if STACK        
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal void yy_clear_stack() { scStack.Clear(); }
        
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal int yy_top_state() { return scStack.Peek(); }
        
            internal void yy_push_state(int state)
            {
                scStack.Push(currentScOrd);
                BEGIN(state);
            }
        
            internal void yy_pop_state()
            {
                // Protect against input errors that pop too far ...
                if (scStack.Count > 0) {
				    int newSc = scStack.Pop();
				    BEGIN(newSc);
                } // Otherwise leave stack unchanged.
            }
#endif // STACK

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal void ECHO() { Console.Out.Write(yytext); }
        
        } // end class $Scanner
    } // end class $Tokenizer

}
