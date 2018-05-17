using Antlr4.Runtime;
using Fpx.Generated;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fpx
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                StreamReader input = new StreamReader(args[0]);
//                AntlrInputStream inputStream = new AntlrInputStream(input);
                UnbufferedCharStream inputStream = new UnbufferedCharStream(input);
                FpxLexer fpxLexer = new FpxLexer(inputStream);
                fpxLexer.TokenFactory = new CommonTokenFactory(true);
//                CommonTokenStream commonTokenStream = new CommonTokenStream(fpxLexer);
                UnbufferedTokenStream commonTokenStream = new UnbufferedTokenStream(fpxLexer);
                FpxParser fpxParser = new FpxParser(commonTokenStream);
//                fpxParser.ErrorHandler = new BailErrorStrategy();
//                fpxParser.BuildParseTree = false;
                List<string> Errors = new List<string>();
                GatherErrorListener gErrorListener = new GatherErrorListener(Errors);
                fpxParser.RemoveErrorListeners();
                fpxParser.AddErrorListener(gErrorListener);

                FpxParser.PxfileContext pxFileContext = fpxParser.pxfile();
                FpxVisitor visitor = new FpxVisitor(Errors);
                
                if (!gErrorListener.AnyNonRecoverableErrors)
                    Console.WriteLine(visitor.Visit(pxFileContext));
                DisplayErrors(Errors);
            } // try
            catch (Exception _e)
            {
                Console.WriteLine("Error: " + _e);
                Console.WriteLine("Error: " + _e.StackTrace);
            } // catch
        } // Main

        private static void DisplayErrors(List<string> _errors)
        {
            foreach (var e in _errors)
            {
                Console.WriteLine(e);
            } // foreach
        }
    } // class Program
} // namespace Fpx
