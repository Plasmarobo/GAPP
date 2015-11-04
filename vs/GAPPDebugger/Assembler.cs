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

        public Assembler()
        {
        }

        public bool Assemble(String filename)
        {
            AntlrFileStream inputStream = new AntlrFileStream(filename);
            GBASMLexer lexer = new GBASMLexer(inputStream);
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);
            GBASMParser parser = new GBASMParser(tokenStream);
            parser.BuildParseTree = true;
            Antlr4.Runtime.Tree.IParseTree tree = parser.eval();
            Antlr4.Runtime.Tree.ParseTreeWalker.Default.Walk(this, tree);
            return true;
        }

        public void EnterEval(GBASMParser.EvalContext context)
        {
            //Do nothing
        }

        public void ExitEval(GBASMParser.EvalContext context)
        {
            //EOF must have been reached
        }

        public void EnterExp(GBASMParser.ExpContext context)
        {
            throw new NotImplementedException();
        }

        public void ExitExp(GBASMParser.ExpContext context)
        {
            throw new NotImplementedException();
        }

        public void EnterOp(GBASMParser.OpContext context)
        {
            throw new NotImplementedException();
        }

        public void ExitOp(GBASMParser.OpContext context)
        {
            throw new NotImplementedException();
        }

        public void EnterJumpflag(GBASMParser.JumpflagContext context)
        {
            throw new NotImplementedException();
        }

        public void ExitJumpflag(GBASMParser.JumpflagContext context)
        {
            throw new NotImplementedException();
        }

        public void EnterComplexop(GBASMParser.ComplexopContext context)
        {
            throw new NotImplementedException();
        }

        public void ExitComplexop(GBASMParser.ComplexopContext context)
        {
            throw new NotImplementedException();
        }

        public void EnterCmd(GBASMParser.CmdContext context)
        {
            throw new NotImplementedException();
        }

        public void ExitCmd(GBASMParser.CmdContext context)
        {
            throw new NotImplementedException();
        }

        public void EnterRegop(GBASMParser.RegopContext context)
        {
            throw new NotImplementedException();
        }

        public void ExitRegop(GBASMParser.RegopContext context)
        {
            throw new NotImplementedException();
        }

        public void EnterFlagop(GBASMParser.FlagopContext context)
        {
            throw new NotImplementedException();
        }

        public void ExitFlagop(GBASMParser.FlagopContext context)
        {
            throw new NotImplementedException();
        }

        public void EnterComplex(GBASMParser.ComplexContext context)
        {
            throw new NotImplementedException();
        }

        public void ExitComplex(GBASMParser.ComplexContext context)
        {
            throw new NotImplementedException();
        }

        public void EnterLoc(GBASMParser.LocContext context)
        {
            throw new NotImplementedException();
        }

        public void ExitLoc(GBASMParser.LocContext context)
        {
            throw new NotImplementedException();
        }

        public void EnterMemory(GBASMParser.MemoryContext context)
        {
            throw new NotImplementedException();
        }

        public void ExitMemory(GBASMParser.MemoryContext context)
        {
            throw new NotImplementedException();
        }

        public void EnterOffset(GBASMParser.OffsetContext context)
        {
            throw new NotImplementedException();
        }

        public void ExitOffset(GBASMParser.OffsetContext context)
        {
            throw new NotImplementedException();
        }

        public void EnterRegister(GBASMParser.RegisterContext context)
        {
            throw new NotImplementedException();
        }

        public void ExitRegister(GBASMParser.RegisterContext context)
        {
            throw new NotImplementedException();
        }

        public void EnterWideregister(GBASMParser.WideregisterContext context)
        {
            throw new NotImplementedException();
        }

        public void ExitWideregister(GBASMParser.WideregisterContext context)
        {
            throw new NotImplementedException();
        }

        public void EnterFlag(GBASMParser.FlagContext context)
        {
            throw new NotImplementedException();
        }

        public void ExitFlag(GBASMParser.FlagContext context)
        {
            throw new NotImplementedException();
        }

        public void EnterValue(GBASMParser.ValueContext context)
        {
            throw new NotImplementedException();
        }

        public void ExitValue(GBASMParser.ValueContext context)
        {
            throw new NotImplementedException();
        }

        public void EnterNegvalue(GBASMParser.NegvalueContext context)
        {
            throw new NotImplementedException();
        }

        public void ExitNegvalue(GBASMParser.NegvalueContext context)
        {
            throw new NotImplementedException();
        }

        public void EnterEveryRule(ParserRuleContext ctx)
        {
            throw new NotImplementedException();
        }

        public void ExitEveryRule(ParserRuleContext ctx)
        {
            throw new NotImplementedException();
        }

        public void VisitErrorNode(Antlr4.Runtime.Tree.IErrorNode node)
        {
            throw new NotImplementedException();
        }

        public void VisitTerminal(Antlr4.Runtime.Tree.ITerminalNode node)
        {
            throw new NotImplementedException();
        }
    }
}
