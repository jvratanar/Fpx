using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fpx
{
    public class ThrowingErrorListener : BaseErrorListener, IAntlrErrorListener<int>
    {
        public static readonly ThrowingErrorListener INSTANCE = new ThrowingErrorListener();

        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, 
            int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new ParseCanceledException("line " + line + ":" + charPositionInLine + " " + msg);
        } // SyntaxError


        public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, 
            int charPositionInLine, string msg, RecognitionException e)
        {
            throw new ArgumentException("Invalid Expression: {0}", msg, e);
        }
    } // class ThrowingErrorListener
} // namespace Fpx
