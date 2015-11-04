using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;

namespace GAPPDebugger
{
    class Assembler : IGBASMListener
    {
        private GAPPDebugger.Controls.AssemblerProgress window;
        private List<Byte> rom;
        private AntlrFileStream inputStream;
        private GBASMLexer lexer;
        private CommonTokenStream tokenStream;
        private GBASMParser parser;

        public Assembler()
        {
            window = new GAPPDebugger.Controls.AssemblerProgress();
            
        }

        public void PrintLine(String line)
        {
            window.ConsoleWin.Text += line + "\n";
        }

        public List<Byte> Assemble(String filename)
        {
            window.Show();
            window.Focus();
            rom = new List<byte>();
            inputStream = new AntlrFileStream(filename);
            lexer = new GBASMLexer(inputStream);
            tokenStream = new CommonTokenStream(lexer);
            parser = new GBASMParser(tokenStream);
            
            parser.BuildParseTree = true;
            Antlr4.Runtime.Tree.IParseTree tree = parser.eval();
            Antlr4.Runtime.Tree.ParseTreeWalker.Default.Walk(this, tree);
            return rom;
        }


        public void EnterEval(GBASMParser.EvalContext context)
        {
            PrintLine("Starting");
        }

        public void ExitEval(GBASMParser.EvalContext context)
        {
            PrintLine("Finished");
        }

        public void EnterExp(GBASMParser.ExpContext context)
        {
            //throw new NotImplementedException();
        }

        public void ExitExp(GBASMParser.ExpContext context)
        {
            //throw new NotImplementedException();
        }

        public void EnterOp(GBASMParser.OpContext context)
        {
            //throw new NotImplementedException();
        }

        public void ExitOp(GBASMParser.OpContext context)
        {
            //throw new NotImplementedException();
        }

        public void EnterMonad(GBASMParser.MonadContext context)
        {
            PrintLine("Monad Detected");
            PrintLine(context.GetText());
            //throw new NotImplementedException();
        }

        public void ExitMonad(GBASMParser.MonadContext context)
        {
            //throw new NotImplementedException();
        }

        public void EnterBiad(GBASMParser.BiadContext context)
        {
            PrintLine("Biad Detected");
            PrintLine(context.GetText());
            //throw new NotImplementedException();
        }

        public void ExitBiad(GBASMParser.BiadContext context)
        {
            //throw new NotImplementedException();
        }

        public void EnterTriad(GBASMParser.TriadContext context)
        {
            PrintLine("Triad Detected");
            PrintLine(context.GetText());
            //throw new NotImplementedException();
        }

        public void ExitTriad(GBASMParser.TriadContext context)
        {
            //throw new NotImplementedException();
        }

        public void EnterArg(GBASMParser.ArgContext context)
        {
            PrintLine("Argument");
            //throw new NotImplementedException();
        }

        public void ExitArg(GBASMParser.ArgContext context)
        {
            //throw new NotImplementedException();
        }

        public void EnterMemory(GBASMParser.MemoryContext context)
        {
            PrintLine("Memory Location");
            //throw new NotImplementedException();
        }

        public void ExitMemory(GBASMParser.MemoryContext context)
        {
            //throw new NotImplementedException();
        }

        public void EnterOffset(GBASMParser.OffsetContext context)
        {
            PrintLine("Offset");
            //throw new NotImplementedException();
        }

        public void ExitOffset(GBASMParser.OffsetContext context)
        {
            //throw new NotImplementedException();
        }

        public void EnterRegister(GBASMParser.RegisterContext context)
        {
            PrintLine("Register");
        }

        public void ExitRegister(GBASMParser.RegisterContext context)
        {
            //throw new NotImplementedException();
        }

        public void EnterFlag(GBASMParser.FlagContext context)
        {
            PrintLine("Flag");
            //throw new NotImplementedException();
        }

        public void ExitFlag(GBASMParser.FlagContext context)
        {
            //throw new NotImplementedException();
        }

        public void EnterValue(GBASMParser.ValueContext context)
        {
            PrintLine("Numeric Value");
            PrintLine(context.GetText());
            //throw new NotImplementedException();
        }

        public void ExitValue(GBASMParser.ValueContext context)
        {
            //throw new NotImplementedException();
        }

        public void EnterNegvalue(GBASMParser.NegvalueContext context)
        {
            PrintLine("Signed Value");
            PrintLine(context.GetText());
            //throw new NotImplementedException();
        }

        public void ExitNegvalue(GBASMParser.NegvalueContext context)
        {
            //throw new NotImplementedException();
        }

        public void EnterEveryRule(ParserRuleContext ctx)
        {
            //throw new NotImplementedException();
        }

        public void ExitEveryRule(ParserRuleContext ctx)
        {
            //throw new NotImplementedException();
        }

        public void VisitErrorNode(Antlr4.Runtime.Tree.IErrorNode node)
        {
            //throw new NotImplementedException();
        }

        public void VisitTerminal(Antlr4.Runtime.Tree.ITerminalNode node)
        {
            //PrintLine("No more rules");
            //throw new NotImplementedException();
        }
    }
}
