using Antlr4.Runtime;
using Fpx.Generated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fpx
{
    public class GatherErrorListener : BaseErrorListener
    {
        public List<string> Errors { get; }
        public bool AnyNonRecoverableErrors { get; set;  }

        public GatherErrorListener(List<string> _Errors)
        {
            this.Errors = _Errors;
        } // GatherErrorListener

        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, 
            int line, int charPositionInLine, string msg, RecognitionException e)
        {
            this.Errors.Add("line " + line + ":" + charPositionInLine + " " + msg);
            var fpxParser = (FpxParser)recognizer;
            //if (fpxParser.CurrentToken.Type == FpxLexer.STUB ||
            //    fpxParser.CurrentToken.Type == FpxLexer.HEADING)
            //{
            if (fpxParser.Context is FpxParser.HeadingContext ||
                fpxParser.Context is FpxParser.StubContext)
            {
                this.AnyNonRecoverableErrors = true;
            } // if
        } // SyntaxError
    } // class GatherErrorListener 
}
