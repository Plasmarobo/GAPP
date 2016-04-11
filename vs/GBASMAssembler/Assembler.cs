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
    public delegate void OnAssemblerError(String message);
    
    public class LabelInfo
    {
        public int startByte;
        public int endByte;
        public int labelOffset;
        public bool isRelative;
        public int byteStringIndex;

        public LabelInfo()
        {
            startByte = 0;
            endByte = 0;
            labelOffset = 0;
            isRelative = false;
            byteStringIndex = 0;
        }

        public LabelInfo(int s, int e, int o, bool r, int bs)
        {
            startByte = s;
            endByte = e;
            labelOffset = 0;
            isRelative = r;
            byteStringIndex = bs;
        }

        
    }

    public class Assembler : IGBASMListener
    {
        public event OnProgressUpdatedHandler ProgressUpdated;
        public event OnMessagePrintedHandler MessagePrinted;
        public event OnAssemblyComplete AssemblyComplete;
        public event OnVerboseUpdate VerboseUpdate;
        public event OnAssemblerError AssemblerError;

        private List<Byte> rom;
        private List<String> lines;
        private List<String> byteLines;
        private Dictionary<int, List<int>> lineToByteIndex;
        private Dictionary<int, List<int>> byteToLineIndex;
        private AntlrInputStream inputStream;
        private GBASMLexer lexer;
        private CommonTokenStream tokenStream;
        private GBASMParser parser;
        private bool isSectionArg;
        private bool isReptArg;
        private int db_stack;
        private Dictionary<String, int> jumpAddressIndex;
        private Dictionary<String, List<LabelInfo>> unProcessedJumpLabels;
        private int rom_ptr;

        public class RepInfo
        {
            public int romStart;
            public int linesStart;
            public int reps;

            public RepInfo(int start, int lines)
            {
                romStart = start;
                reps = 1;
                linesStart = lines;
            }
        }
        private Stack<RepInfo> rep_stack;
        public class SectionInfo
        {
            public int romStart;
            public int lineStart;
            public int size;

            public SectionInfo(int start, int lines)
            {
                romStart = start;
                size = 1;
                lineStart = lines;
            }
        }
        private Stack<SectionInfo> sec_stack;

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

        private void MapByteToLine(int b, int l)
        {
            if(!byteToLineIndex.ContainsKey(b))
            {
                byteToLineIndex[b] = new List<int>();
            }
            byteToLineIndex[b].Add(l);
        }

        private void MapLineToByte(int l, int b)
        {
            if(!lineToByteIndex.ContainsKey(l))
            {
                lineToByteIndex[l] = new List<int>();
            }
            lineToByteIndex[l].Add(b);
        }

        private void RomPush(Byte value)
        {
            rom.Add(value);
            MapByteToLine(value, lines.Count-1);
            MapLineToByte(lines.Count-1, value);
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
            byteLines = new List<String>();
            lineToByteIndex = new Dictionary<int, List<int>>();
            byteToLineIndex = new Dictionary<int, List<int>>();
            lexer = new GBASMLexer(inputStream);
            tokenStream = new CommonTokenStream(lexer);
            parser = new GBASMParser(tokenStream);
            argFSM = ArgFSM.COMPLETE;
            db_stack = 0;
            isSectionArg = false;
            isReptArg = false;
            rep_stack = new Stack<RepInfo>();
            sec_stack = new Stack<SectionInfo>();
            jumpAddressIndex = new Dictionary<string,int>();
            unProcessedJumpLabels = new Dictionary<string, List<LabelInfo>>(); ;
            parser.BuildParseTree = true;
            Antlr4.Runtime.Tree.IParseTree tree = parser.eval();
            Antlr4.Runtime.Tree.ParseTreeWalker.Default.Walk(this, tree);

            if (AssemblyComplete != null)
            {
                AssemblyComplete(rom);
            }
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
            if ((context.Stop != null) && (context.Start.StartIndex < context.Stop.StopIndex))
            {
                lines.Add(inputStream.GetText(new Interval(context.Start.StartIndex, context.Stop.StopIndex)));
            }
            else
            {
                lines.Add(inputStream.GetText(new Interval(context.Start.StartIndex, context.Start.StopIndex)));
            }
            currentInst = new Instruction();
        }

        public void ExitOp(GBASMParser.OpContext context)
        {
            PrintLine("End Op");
            currentInst.AppendTo(rom);
            byteLines.Add(BuildByteString());
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

        public static Locations ParseLocation(String src)
        {
            Locations l = Locations.NONE;
            switch (src)
            {
                case "A":
                    l = Locations.A;
                    break;
                case "B":
                    l = Locations.B;
                    break;
                case "C":
                    l = Locations.C;
                    break;
                case "D":
                    l = Locations.D;
                    break;
                case "E":
                    l = Locations.E;
                    break;
                case "F":
                    l = Locations.F;
                    break;
                case "H":
                    l = Locations.H;
                    break;
                case "L":
                    l = Locations.L;
                    break;
                case "AF":
                    l = Locations.AF;
                    break;
                case "BC":
                    l = Locations.BC;
                    break;
                case "DE":
                    l = Locations.DE;
                    break;
                case "HL+":
                case "HLI":
                case "HL-":
                case "HLD":
                case "HL":
                    l = Locations.HL;
                    break;
                case "SP":
                    l = Locations.SP;
                    break;
                default:
                    break;
            }
            return l;
        }

        public void EnterRegister(GBASMParser.RegisterContext context)
        {
            PrintLine("Register");
            LocationInfo l = GetCurrentArg();
            l.isReg = true;
            String target = context.GetText();
            SetArgLoc(ParseLocation(target));
            if(target == "HL+" || target == "HLI")
            {
                currentInst.op = Instructions.LDI;
            }
            if (target == "HL-" || target == "HLD")
            {
                currentInst.op = Instructions.LDD;
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

        protected Int32 ParseNum(String s)
        {
            Int32 value;
            if(s.Length > 0 && s[0] == '$')
            {
                value = Int32.Parse(s.Substring(1), NumberStyles.HexNumber);
            }
            else if (s.Length > 2 && s[1] == 'x')
            {
                value = Int32.Parse(s.Substring(2), NumberStyles.HexNumber);
            }
            else if (s.Length > 1 && s.Last() == 'h' || s.Last() == 'H')
            {
                value = Int32.Parse(s.Substring(0, s.Length-1), NumberStyles.HexNumber);
            }
            else
            {
                value = Int32.Parse(s);
            }
            if (value > Int16.MaxValue || value < Int16.MinValue)
            {
                ErrorMsg("Integer cannot be represented in 16 bits");
            }
            return value;
        }

        public void EnterValue(GBASMParser.ValueContext context)
        {
            
            PrintLine("Numeric Value");
            
            String s = context.GetText();
           
            PrintLine(s);
            Int16 value = (Int16)ParseNum(s);
            if (isReptArg)
            {
                isReptArg = false;
                rep_stack.Peek().reps = value-1; //Offset for the fact the original code will be copied
            }
            else if (isSectionArg)
            {
                isSectionArg = false;
                sec_stack.Peek().size = value;
            }
            else
            {
                if (db_stack > 0)
                {
                    RomPush((Byte)value);
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
                RomPush((Byte)value);
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
            ErrorMsg(node.GetText());
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
            return byteLines[i];
        }

        public String GetAsmLine(int i)
        {
            return lines[i];
        }

        public void EnterSys(GBASMParser.SysContext context)
        {
            PrintLine("Starting Sys");
            rom_ptr = rom.Count;
           
        }

        public void ExitSys(GBASMParser.SysContext context)
        {
            PrintLine("Ending Sys"); 
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
            if (context.Stop != null)
            {
                lines.Add(inputStream.GetText(new Interval((int)context.Start.StartIndex, (int)context.Stop.StopIndex)));
            }
            else
            {
                lines.Add(inputStream.GetText(new Interval((int)context.Start.StartIndex, (int)context.Start.StopIndex)));
            }

            isSectionArg = true;
            sec_stack.Push(new SectionInfo(rom.Count, lines.Count));

        }

        public void ExitSection(GBASMParser.SectionContext context)
        {
            //Use Label Buf
            SectionInfo info = sec_stack.Pop();
            int distance_req = info.size - rom.Count;
            for(int i = 0; i < distance_req; ++i)
            {
                RomPush(0x00);
            }
            byteLines.Add(BuildByteString());
        }

        public void EnterJump(GBASMParser.JumpContext context)
        {
            PrintLine("Label Dereference");
            String label = context.GetText();

            LabelInfo l = new LabelInfo();
            OffsetInfo o;
            l.startByte = rom.Count;
            l.isRelative = currentInst.op == Instructions.JR;
            
            o = currentInst.GetCurrentOffset();
            l.labelOffset = l.startByte + o.total; //This will be 0 if no src is encoded
            l.endByte = l.labelOffset + ((l.isRelative) ? 1 : 2);
            l.byteStringIndex = byteLines.Count;

            if (jumpAddressIndex.ContainsKey(label))
            {
                SetArgVal((Int16)ComputeJump(jumpAddressIndex[label], l));
            }
            else
            {
                SetArgVal(0);
                //CALCULATE SPACE REQUIRED
                if(!unProcessedJumpLabels.ContainsKey(label))
                {
                    unProcessedJumpLabels.Add(label, new List<LabelInfo>());
                }
                //An arg size of NONE should not effect current address calculations
                //SRC should always be set before DST is set
                unProcessedJumpLabels[label].Add(l);
            }

            switch (currentInst.op)
            {
                case Instructions.JR:
                    SetArgLoc(Locations.IMM);
                    break;
                case Instructions.JP:
                default:
                    SetArgLoc(Locations.WIDE_IMM);
                    break;
            }
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
                foreach(LabelInfo info in unProcessedJumpLabels[label])
                {
                    ResolveLabel(jumpAddressIndex[label], info);
                }
            }
        }

        public void ExitLabel(GBASMParser.LabelContext context)
        {
            
        }

        public void EnterData(GBASMParser.DataContext context)
        {
            PrintLine("DB");
            db_stack += 1;
            PrintLine("DB at " + db_stack.ToString());
            currentInst.op = Instructions.DATA_INSTRUCTION;
        }

        public void ExitData(GBASMParser.DataContext context)
        {
            db_stack -= 1;

            PrintLine("DB at " + db_stack.ToString());
        }


        public void EnterDb(GBASMParser.DbContext context)
        {
           
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
                //Remove quotations
                for (int i = 1; i < s.Length-1; ++i)
                {
                    RomPush((Byte)s[i]);
                }
            }
        }

        public void ExitString_data(GBASMParser.String_dataContext context)
        {
            //throw new NotImplementedException();
        }


        private int ComputeJump(int addr, LabelInfo info)
        {
            if (info.isRelative)
            {
                //Determine distance and direction
                int delta = addr - info.startByte;
                //We can't insert, because that would be destructive to all following labels and jumps
                if (delta > 127 || delta < -128)
                {
                    ErrorMsg("Realtive Jump out of range (-128 to 127)");
                }
                return (delta & 0xFF);
            }
            else
            {
                return addr;
            }
        }

        private void ResolveLabel(int addr, LabelInfo info)
        {
            //Assume size has been allocated
            addr = ComputeJump(addr, info);
            if(info.isRelative)
            {
                rom[info.labelOffset] = (Byte)addr;
            }
            else
            {
                rom[info.labelOffset] = (Byte)((addr >> 8) & 0xFF);
                rom[info.labelOffset + 1] = (Byte)(addr & 0xFF);
            }

            String bs = "";
            for (int i = info.startByte; i < info.endByte; ++i)
            {
                bs += "0x" + rom[i].ToString("X2");
                if (i < (info.endByte - 1))
                {
                    bs += " ";
                }
            }
            byteLines[info.byteStringIndex] = bs;
        }

        protected void ErrorMsg(String msg)
        {
            if(AssemblerError != null)
            {
                Int32 lineno = lines.Count + 1;
                AssemblerError("Line " + lineno.ToString() + ":" + msg);
            }
        }


        public void EnterRepblock(GBASMParser.RepblockContext context)
        {
            //We do NOT support label resolution in rep blocks
            //Absolute labels MIGHT work
            isReptArg = true;
            rep_stack.Push(new RepInfo(rom.Count, lines.Count));
            //Need to pad byteLines
            //byteLines.Add("");
        }

        public void ExitRepblock(GBASMParser.RepblockContext context)
        {
            RepInfo r = rep_stack.Pop();

            int romEnd = rom.Count;
            int lineEnd = lines.Count;

            if (context.Stop != null)
            {
                lines[lines.Count-1] += "; " + (inputStream.GetText(new Interval((int)context.Start.StartIndex, (int)context.Stop.StopIndex)));
            }
            else
            {
                lines[lines.Count-1] += "; " + (inputStream.GetText(new Interval((int)context.Start.StartIndex, (int)context.Start.StopIndex)));
            }

            while (r.reps > 0)
            {
                for (int index = r.linesStart; index < lineEnd; ++index )
                {
                    lines.Add(lines[index] + "; REPT" + index);
                    byteLines.Add(byteLines[index]);
                }
                for (int index = r.romStart; index < romEnd; ++index)
                {
                    //This is cheating: Will tag all with the last line of block
                    RomPush(rom[index]);
                }
                
                r.reps--;
            }
        }
    }
}
