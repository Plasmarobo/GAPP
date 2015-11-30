using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System.Globalization;

namespace GBASMAssembler
{
    public delegate void OnProgressUpdatedHandler(int progress, int max);
    public delegate void OnMessagePrintedHandler(String message);
    public delegate void OnAssemblyComplete(List<Byte> rom);
    public delegate void OnVerboseUpdate(String message);
    
    public class Assembler : IGBASMListener
    {
        public event OnProgressUpdatedHandler ProgressUpdated;
        public event OnMessagePrintedHandler MessagePrinted;
        public event OnAssemblyComplete AssemblyComplete;
        public event OnVerboseUpdate VerboseUpdate;

        private List<Byte> rom;
        private List<String> lineToBytes;
        private List<String> lines;
        private AntlrInputStream inputStream;
        private GBASMLexer lexer;
        private CommonTokenStream tokenStream;
        private GBASMParser parser;
        private bool isSection;
        private int db_stack;
        private Dictionary<String, int> jumpAddressIndex;
        private Dictionary<String, List<int>> unProcessedJumpLabels;
        private int rom_ptr;

        protected Instruction currentInst;
        protected enum ArgFSM
        {
            COMPLETE = 0,
            ARG,
            DST,
            SRC,
        }

        protected ArgFSM argFSM;

        protected void DecArg()
        {
            if (argFSM == ArgFSM.SRC)
            {
                argFSM = ArgFSM.DST;
            }
            else
            {
                argFSM = ArgFSM.COMPLETE;
            }
        }

        protected void SetArgLoc(Locations loc)
        {
            switch (argFSM)
            {
                case ArgFSM.SRC:
                case ArgFSM.ARG:
                    if (currentInst.src.loc == Locations.NONE)
                    {
                        currentInst.src.loc = loc;
                    }
                    break;
                case ArgFSM.DST:
                    if (currentInst.dst.loc == Locations.NONE)
                    {
                        currentInst.dst.loc = loc;
                    }
                    break;

                case ArgFSM.COMPLETE:
                default:
                    //Error
                    break;

            }
        }

        protected void SetArgVal(Int16 val)
        {
            switch (argFSM)
            {
                case ArgFSM.SRC:
                case ArgFSM.ARG:
                    currentInst.src.val = val;
                    break;
                case ArgFSM.DST:
                    currentInst.dst.val = val;
                    break;

                case ArgFSM.COMPLETE:
                default:
                    //Error
                    break;

            }
        }

        protected LocationInfo GetCurrentArg()
        {
            switch (argFSM)
            {
                case ArgFSM.SRC:
                case ArgFSM.ARG:
                    return currentInst.src;
                case ArgFSM.DST:
                    return currentInst.dst;

                case ArgFSM.COMPLETE:
                default:
                    return currentInst.src;

            }
        }

        private String BuildByteString()
        {
            String str = "";
            for (int i = rom_ptr; i < rom.Count; ++i)
            {
                str = str + "0x" + rom[i].ToString("X2") + ((i == (rom.Count - 1)) ? "" : " ");
            }
            return str;
        }

        public Assembler()
        {
          
        }

        public void PrintLine(String line)
        {
            if (MessagePrinted != null)
            {
                MessagePrinted(line);
            }
        }

        public List<Byte> Assemble()
        {
            
            rom = new List<byte>();
            lines = new List<String>();
            lineToBytes = new List<String>();
            lexer = new GBASMLexer(inputStream);
            tokenStream = new CommonTokenStream(lexer);
            parser = new GBASMParser(tokenStream);
            argFSM = ArgFSM.COMPLETE;
            db_stack = 0;
            isSection = false;
            jumpAddressIndex = new Dictionary<string,int>();
            unProcessedJumpLabels = new Dictionary<string, List<int>>(); ;
            parser.BuildParseTree = true;
            Antlr4.Runtime.Tree.IParseTree tree = parser.eval();
            Antlr4.Runtime.Tree.ParseTreeWalker.Default.Walk(this, tree);
            return rom;
        }

        public List<Byte> AssembleString(String data)
        {

            inputStream = new AntlrInputStream(data);

            return Assemble();
        }

        public List<Byte> AssembleFile(String filename)
        {
            inputStream = new AntlrFileStream(filename);

            return Assemble();
        }


        public void EnterEval(GBASMParser.EvalContext context)
        {
            PrintLine("Starting");
        }

        public void ExitEval(GBASMParser.EvalContext context)
        {
            PrintLine("Finished");
            if (AssemblyComplete != null)
            {
                AssemblyComplete(rom);
            }
        }

        public void EnterExp(GBASMParser.ExpContext context)
        {
            PrintLine("Start Exp");
        }

        public void ExitExp(GBASMParser.ExpContext context)
        {
            PrintLine("End Exp");
        }

        public void EnterOp(GBASMParser.OpContext context)
        {
            PrintLine("Start op");
            rom_ptr = rom.Count;
            if (context.Stop != null)
            {
                lines.Add(inputStream.GetText(new Interval((int)context.Start.StartIndex, (int)context.Stop.StopIndex)));
            }
            else
            {
                lines.Add(inputStream.GetText(new Interval((int)context.Start.StartIndex, (int)context.Start.StopIndex)));
            }
            currentInst = new Instruction();
        }

        public void ExitOp(GBASMParser.OpContext context)
        {
            PrintLine("End Op");
            currentInst.AppendTo(rom);
            lineToBytes.Add(BuildByteString());
        }

        public void EnterMonad(GBASMParser.MonadContext context)
        {
            PrintLine("Monad Detected");
            argFSM = ArgFSM.COMPLETE;
            currentInst.op = Instruction.MapOp(context.GetText());
            currentInst.src.loc = Locations.NONE;
            currentInst.dst.loc = Locations.NONE;
        }

        public void ExitMonad(GBASMParser.MonadContext context)
        {
            //throw new NotImplementedException();
        }

        public void EnterBiad(GBASMParser.BiadContext context)
        {
            PrintLine("Biad Detected");
            argFSM = ArgFSM.ARG;
            currentInst.op = Instruction.MapOp(context.GetText());
        }

        public void ExitBiad(GBASMParser.BiadContext context)
        {
            //throw new NotImplementedException();
        }

        public void EnterTriad(GBASMParser.TriadContext context)
        {
            PrintLine("Triad Detected");
            argFSM = ArgFSM.SRC;
            currentInst.op = Instruction.MapOp(context.GetText());
        }

        public void ExitTriad(GBASMParser.TriadContext context)
        {
            //throw new NotImplementedException();
        }

        public void EnterArg(GBASMParser.ArgContext context)
        {
            //Register, value, negvalue, flag, offset, LABEL, memory
            PrintLine("Argument");
            //Should try: Memory -> Offset -> Register -> Flag -> Value -> Label
        }

        public void ExitArg(GBASMParser.ArgContext context)
        {
            DecArg();
        }

        public void EnterMemory(GBASMParser.MemoryContext context)
        {
            PrintLine("Memory");
            //SetArgLoc(Locations.MEM);
            LocationInfo l = GetCurrentArg();
            l.isMem = true;
            l.isWide = true;
        }

        public void ExitMemory(GBASMParser.MemoryContext context)
        {
        }

        public void EnterOffset(GBASMParser.OffsetContext context)
        {
            PrintLine("Offset");
            SetArgLoc(Locations.OFFSET);
            LocationInfo l = GetCurrentArg();
            l.isOffset = true;
        }

        public void ExitOffset(GBASMParser.OffsetContext context)
        {

        }

        public void EnterRegister(GBASMParser.RegisterContext context)
        {
            PrintLine("Register");
            LocationInfo l = GetCurrentArg();
            l.isReg = true;
            switch (context.GetText())
            {
                case "A":
                    SetArgLoc(Locations.A);
                    break;
                case "B":
                    SetArgLoc(Locations.B);
                    break;
                case "C":
                    SetArgLoc(Locations.C);
                    break;
                case "D":
                    SetArgLoc(Locations.D);
                    break;
                case "E":
                    SetArgLoc(Locations.E);
                    break;
                case "F":
                    SetArgLoc(Locations.F);
                    break;
                case "H":
                    SetArgLoc(Locations.H);
                    break;
                case "L":
                    SetArgLoc(Locations.L);
                    break;
                case "AF":
                    SetArgLoc(Locations.AF);
                    break;
                case "BC":
                    SetArgLoc(Locations.BC);
                    break;
                case "DE":
                    SetArgLoc(Locations.DE);
                    break;
                case "HL":
                    SetArgLoc(Locations.HL);
                    break;
                case "SP":
                    SetArgLoc(Locations.SP);
                    break;
                case "HL+":
                case "HLI":
                    currentInst.op = Instructions.LDI;
                    SetArgLoc(Locations.HL);
                    break;
                case "HL-":
                case "HLD":
                    currentInst.op = Instructions.LDD;
                    SetArgLoc(Locations.HL);
                    break;
                default:
                    break;
            }

        }

        public void ExitRegister(GBASMParser.RegisterContext context)
        {

        }

        public void EnterFlag(GBASMParser.FlagContext context)
        {
            PrintLine("Flag");
            LocationInfo l = GetCurrentArg();
            l.isFlag = true;
            switch (context.GetText())
            {
                case "Z":
                    SetArgLoc(Locations.Z);
                    break;
                case "C":
                    SetArgLoc(Locations.C);
                    break;
                case "NZ":
                    SetArgLoc(Locations.NZ);
                    break;
                case "NC":
                    SetArgLoc(Locations.NC);
                    break;
                default:
                    break;
            }
        }

        public void ExitFlag(GBASMParser.FlagContext context)
        {

        }

        protected Int16 ParseNum(String s)
        {
            Int16 value;
            if(s.Length > 0 && s[0] == '$')
            {
                value = Int16.Parse(s.Substring(1), NumberStyles.HexNumber);
            }
            else if (s.Length > 2 && s[1] == 'x')
            {
                value = Int16.Parse(s.Substring(2), NumberStyles.HexNumber);
            }
            else if (s.Length > 1 && s.Last() == 'h' || s.Last() == 'H')
            {
                value = Int16.Parse(s.Substring(0, s.Length-1), NumberStyles.HexNumber);
            }
            else
            {
                value = Int16.Parse(s);
            }
            return value;
        }

        public void EnterValue(GBASMParser.ValueContext context)
        {
            
            PrintLine("Numeric Value");
            
            String s = context.GetText();
           
            PrintLine(s);
            Int16 value = ParseNum(s);
            if(!isSection)
            {
                if (db_stack > 0)
                {
                    rom.Add((Byte)value);
                }
                else
                {
                    if (Math.Abs(value) > 255)
                    {
                        SetArgLoc(Locations.WIDE_IMM);
                    }
                    else
                    {
                        SetArgLoc(Locations.IMM);
                    }
                    SetArgVal(value);
                }
            }
            else
            {
                for(int i = rom.Count; i < value; ++i)
                {
                    rom.Add(0x00);
                }
            }
        }

        public void ExitValue(GBASMParser.ValueContext context)
        {

        }

        public void EnterNegvalue(GBASMParser.NegvalueContext context)
        {
            PrintLine("Signed Value");
            

            Int16 value = (Int16)(-ParseNum(context.GetText().Substring(1)));
            if (db_stack > 0)
            {
                rom.Add((Byte)value);
            }
            else
            {
                if (Math.Abs(value) > 127)
                {
                    SetArgLoc(Locations.WIDE_IMM);
                }
                else
                {
                    SetArgLoc(Locations.IMM);
                }
                SetArgVal(value);
            }
        }

        public void ExitNegvalue(GBASMParser.NegvalueContext context)
        {

        }

       

        public void EnterEveryRule(ParserRuleContext ctx)
        {
            
            if (VerboseUpdate != null)
            {
                VerboseUpdate(ctx.GetText());
            }
        }

        public void ExitEveryRule(ParserRuleContext ctx)
        {
            if (ProgressUpdated != null)
            {
                ProgressUpdated(ctx.Start.StartIndex, inputStream.Size);
            }
        }

        public void VisitErrorNode(Antlr4.Runtime.Tree.IErrorNode node)
        {
            PrintLine("ERROR!");
            PrintLine(node.GetText());
        }

        public void VisitTerminal(Antlr4.Runtime.Tree.ITerminalNode node)
        {
        }

        public int GetLength()
        {
            return lines.Count;
        }

        public String GetLine(int i)
        {
            return lines[i];
        }

        public String GetByteString(int i)
        {
            return lineToBytes[i];
        }


        public void EnterSys(GBASMParser.SysContext context)
        {
            PrintLine("Starting Sys");
            rom_ptr = rom.Count;
            lines.Add(inputStream.GetText(new Interval((int)context.Start.StartIndex, (int)context.Stop.StopIndex)));
        }

        public void ExitSys(GBASMParser.SysContext context)
        {
            PrintLine("Ending Sys");
            lineToBytes.Add(BuildByteString());
        }

        public void EnterInclude(GBASMParser.IncludeContext context)
        {
            PrintLine("Include Statement");
            //Implement in preprocessor
            //throw new NotImplementedException();
        }

        public void ExitInclude(GBASMParser.IncludeContext context)
        {
            //throw new NotImplementedException();
        }

        public void EnterSection(GBASMParser.SectionContext context)
        {
            PrintLine("Starting Section");
            isSection = true;
        }

        public void ExitSection(GBASMParser.SectionContext context)
        {
            isSection = false;
        }

        public void EnterJump(GBASMParser.JumpContext context)
        {
            PrintLine("Label Encountered");
            String label = context.GetText();
            
            if (jumpAddressIndex.ContainsKey(label))
            {
                SetArgVal((short)jumpAddressIndex[label]);
            }
            else
            {
                SetArgVal(0x0000);
                //CALCULATE SPACE REQUIRED
                if(!unProcessedJumpLabels.ContainsKey(label))
                {
                    unProcessedJumpLabels.Add(label, new List<int>());
                }
                //An arg size of NONE should not effect current address calculations
                //SRC should always be set before DST is set
                unProcessedJumpLabels[label].Add(currentInst.GetCurrentOffset());
            }
            SetArgLoc(Locations.WIDE_IMM); //Reserves space for our address
            //unProcessedJumpLabels 
        }

        public void ExitJump(GBASMParser.JumpContext context)
        {
            
        }

        public void EnterLabel(GBASMParser.LabelContext context)
        {
            PrintLine("Label Defined");

            String label = context.GetText();
            label = label.Substring(0, label.Length - 1); //Chop the colon
            jumpAddressIndex.Add(label, rom.Count);
            if(unProcessedJumpLabels.ContainsKey(label))
            {
                foreach(int index in unProcessedJumpLabels[label])
                {
                    rom[index] = (Byte)(rom.Count >> 8);
                    rom[index + 1] = (Byte)(rom.Count & 0xFF);
                }
            }
        }

        public void ExitLabel(GBASMParser.LabelContext context)
        {
            //throw new NotImplementedException();
        }

        public void EnterData(GBASMParser.DataContext context)
        {
            PrintLine("DB");
            db_stack += 1;
            PrintLine("DB at " + db_stack.ToString());
        }

        public void ExitData(GBASMParser.DataContext context)
        {
            db_stack -= 1;

            PrintLine("DB at " + db_stack.ToString());
        }


        public void EnterDb(GBASMParser.DbContext context)
        {
            //throw new NotImplementedException();
        }

        public void ExitDb(GBASMParser.DbContext context)
        {
            //throw new NotImplementedException();
        }

        public void EnterString_data(GBASMParser.String_dataContext context)
        {
            PrintLine("Literal Value");
            if (db_stack > 0)
            {
                String s = context.GetText();
                for (int i = 0; i < s.Length; ++i)
                {
                    rom.Add((Byte)s[i]);
                }
            }
        }

        public void ExitString_data(GBASMParser.String_dataContext context)
        {
            //throw new NotImplementedException();
        }
    }
}
