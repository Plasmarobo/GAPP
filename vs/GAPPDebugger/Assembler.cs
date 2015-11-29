using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System.Globalization;

namespace GAPPDebugger
{
    class Assembler : IGBASMListener
    {
        private GAPPDebugger.Controls.AssemblerProgress window;
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

        public enum Instructions
        {
            LD = 0,
            JR,
            JP,
            OR,
            CP,
            RL,
            RR,
            DI,
            EI,

            LDD,
            LDI,
            ADD,
            ADC,
            SBC,
            BIT,
            RES,
            SET,
            RET,
            INC,
            DEC,
            SUB,
            AND,
            XOR,
            RLC,
            RRC,
            POP,

            SLA,
            SRA,

            SRL,
            NOP,
            RLA,
            RRA,
            DAA,
            CPL,
            SCF,
            CCF,
            LDH,
            RST,
            CALL,

            PUSH,

            SWAP,
            RLCA,
            RRCA,
            STOP,
            HALT,
            RETI,
            NUM_INSTRUCTIONS,
            ERR_INSTRUCTION
        };

        protected Instructions MapOp(String str)
        {
            switch (str.ToUpper())
            {
                case "LD":
                    return Instructions.LD;
                case "JR":
                    return Instructions.JR;
                case "JP":
                    return Instructions.JP;
                case "OR":
                    return Instructions.OR;
                case "CP":
                    return Instructions.CP;
                case "RL":
                    return Instructions.RL;
                case "RR":
                    return Instructions.RR;
                case "DI":
                    return Instructions.DI;
                case "EI":
                    return Instructions.EI;
                case "LDD":
                    return Instructions.LDD;
                case "LDI":
                    return Instructions.LDI;
                case "ADD":
                    return Instructions.ADD;
                case "ADC":
                    return Instructions.ADC;
                case "SBC":
                    return Instructions.SBC;
                case "BIT":
                    return Instructions.BIT;
                case "RES":
                    return Instructions.RES;
                case "SET":
                    return Instructions.SET;
                case "RET":
                    return Instructions.RET;
                case "INC":
                    return Instructions.INC;
                case "DEC":
                    return Instructions.DEC;
                case "SUB":
                    return Instructions.SUB;
                case "AND":
                    return Instructions.AND;
                case "XOR":
                    return Instructions.XOR;
                case "RLC":
                    return Instructions.RLC;
                case "RRC":
                    return Instructions.RRC;
                case "POP":
                    return Instructions.POP;
                case "SLA":
                    return Instructions.SLA;
                case "SRA":
                    return Instructions.SRA;
                case "SRL":
                    return Instructions.SRL;
                case "NOP":
                    return Instructions.NOP;
                case "RLA":
                    return Instructions.RLA;
                case "RRA":
                    return Instructions.RRA;
                case "DAA":
                    return Instructions.DAA;
                case "CPL":
                    return Instructions.CPL;
                case "SCF":
                    return Instructions.SCF;
                case "CCF":
                    return Instructions.CCF;
                case "LDH":
                    return Instructions.LDH;
                case "RST":
                    return Instructions.RST;
                case "CALL":
                    return Instructions.CALL;
                case "PUSH":
                    return Instructions.PUSH;
                case "SWAP":
                    return Instructions.SWAP;
                case "RRCA":
                    return Instructions.RRCA;
                case "STOP":
                    return Instructions.STOP;
                case "HALT":
                    return Instructions.HALT;
                case "RETI":
                    return Instructions.RETI;
                default:
                    return Instructions.ERR_INSTRUCTION;
            }
        }

        public enum Locations
        {
            NONE = -1,
            B = 0,
            C,
            D,
            E,
            H,
            L,
            A,
            F,
            AF,
            BC,
            DE,
            HL,
            SP,
            PC,
            Z_FLAG,
            C_FLAG,
            NC_FLAG,
            NZ_FLAG,
            MEM, //Memory address 16b
            IMM, //8b
            WIDE_IMM, //16b
            OFFSET, //Memory address given by FF00 + n (8b)
            WIDE_OFFSET, //Memory address given by FF00+n (16b)
            //LABEL,
            STACK,
            NUM_LOCATIONS
        };

        public class LocationInfo
        {
            public Locations loc;
            public bool isMem;
            public bool isReg;
            public bool isFlag;
            public bool isOffset;
            public bool isWide;
            public Int16 val;

            public LocationInfo()
            {
                loc = Locations.NONE;
                isMem = false;
                isReg = false;
                isFlag = false;
                isOffset = false;
                isWide = false;
                val = 0;
            }
        }

        public class Instruction
        {
            public Instructions op;
            //By convention src is arg 1, dst is arg 2 (even if some ops reverse this)
            public LocationInfo src;
            public LocationInfo dst;

            public Instruction()
            {
                op = Instructions.NOP;
                src = new LocationInfo();
                dst = new LocationInfo();
            }

            protected static void InsertLocationInfo(List<Byte> rom, LocationInfo src)
            {
                if (src.isWide)
                {
                    rom.Add((Byte)(src.val >> 8));
                    rom.Add((Byte)(src.val));
                }
                else
                {
                    rom.Add((Byte)src.val);
                }
            }

            protected static Byte GetLinearOffset(LocationInfo src, Byte start)
            {
                Byte offset = start;
                if (src.loc >= Locations.B && src.loc <= Locations.L)
                {
                    offset += (Byte)src.loc;
                }
                else if (src.loc == Locations.HL && src.isMem)
                {
                    offset += 0x06;
                }
                else if (src.loc == Locations.A)
                {
                    offset += 0x07;
                }
                return offset;
            }

            protected static Byte GetBlockOffset(LocationInfo src, Byte start)
            {
                Byte offset = start;
                switch (src.loc)
                {
                    case Locations.BC:
                        offset += 0x00;
                        break;
                    case Locations.DE:
                        offset += 0x10;
                        break;
                    case Locations.HL:
                        offset += 0x20;
                        break;
                    case Locations.AF:
                        offset += 0x30;
                        break;
                    default:
                        //ERROR
                        break;
                }
                return offset;
            }

            protected static Byte GetTableOffset(LocationInfo src, LocationInfo dst, Byte start)
            {
                Byte offset = offset = GetLinearOffset(dst, (Byte)(start + (src.val >> 2)));
                if(src.val % 2 != 0)
                {
                    offset += 0x08;
                }
                return offset;
            }

            public void AppendTo(List<Byte> rom)
            {
                //Compile instruction to opcode
                bool encode_src = false;
                bool encode_dst = false;
                switch (op)
                {
                    case Instructions.LD:
                        //No pattern given to some LOAD inst
                        if(src.isMem)
                        {
                            switch(src.loc)
                            {
                                case Locations.BC:
                                    rom.Add(0x02);
                                    encode_dst = true;
                                    break;
                                case Locations.DE:
                                    rom.Add(0x12);
                                    encode_dst = true;
                                    break;
                                case Locations.HL:
                                    if(dst.loc >= Locations.B && dst.loc <= Locations.A)
                                    {
                                        rom.Add(GetLinearOffset(dst, 0x70));
                                    }
                                    else
                                    {
                                        rom.Add(0x36);
                                        encode_dst = true;
                                    }
                                    break;
                                case Locations.SP:
                                    if(dst.loc == Locations.HL)
                                    {
                                        rom.Add(0xF9);
                                    }
                                    else
                                    {
                                        encode_dst = true;
                                        rom.Add(0x31);
                                    }
                                    break;
                                case Locations.C:
                                    rom.Add(0xE2);
                                    break;
                                case Locations.IMM:
                                case Locations.WIDE_IMM:
                                    src.isWide = true;
                                    encode_src = true;
                                    if (dst.loc == Locations.SP)
                                    {
                                        rom.Add(0x08);
                                    }
                                    else
                                    {
                                        rom.Add(0xEA);
                                        
                                    }
                                    break;
                                default:
                                    break;

                            }
                         
                        }
                        else
                        {
                            //Not loading into memory
                            if (dst.loc >= Locations.B && dst.loc <= Locations.A)
                            {
                                switch (src.loc)
                                {
                                    case Locations.B:
                                        rom.Add(GetLinearOffset(dst, 0x40));
                                        break;
                                    case Locations.C:
                                        rom.Add(GetLinearOffset(dst, 0x48));
                                        break;
                                    case Locations.D:
                                        rom.Add(GetLinearOffset(dst, 0x50));
                                        break;
                                    case Locations.E:
                                        rom.Add(GetLinearOffset(dst, 0x58));
                                        break;
                                    case Locations.H:
                                        rom.Add(GetLinearOffset(dst, 0x60));
                                        break;
                                    case Locations.L:
                                        rom.Add(GetLinearOffset(dst, 0x68));
                                        break;
                                    case Locations.A:
                                        rom.Add(GetLinearOffset(dst, 0x78));
                                        break;


                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                encode_dst = true;
                                switch (src.loc)
                                {
                                    case Locations.B:
                                        rom.Add(0x06);
                                        break;
                                    case Locations.C:
                                        rom.Add(0x0E);
                                        break;
                                    case Locations.D:
                                        rom.Add(0x16);
                                        break;
                                    case Locations.E:
                                        rom.Add(0x1E);
                                        break;
                                    case Locations.H:
                                        rom.Add(0x26);
                                        break;
                                    case Locations.L:
                                        rom.Add(0x2E);
                                        break;
                                    case Locations.A:
                                        if (dst.isMem)
                                        {
                                            switch(dst.loc)
                                            {
                                                case Locations.BC:
                                                    rom.Add(0x1A);
                                                    break;
                                                case Locations.DE:
                                                    rom.Add(0x2A);
                                                    break;
                                                case Locations.HL:
                                                    rom.Add(0x7E);
                                                    break;
                                                case Locations.WIDE_IMM:
                                                    rom.Add(0xFA);
                                                    break;
                                                case Locations.C:
                                                    rom.Add(0xF2);
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            rom.Add(0x3E);
                                        }
                                        break;
                                    case Locations.BC:
                                        rom.Add(0x01);
                                        break;
                                    case Locations.DE:
                                        rom.Add(0x11);
                                        break;
                                    case Locations.HL:
                                        if (dst.loc != Locations.OFFSET)
                                        {
                                            rom.Add(0x21);
                                        }
                                        else
                                        {
                                            rom.Add(0xF8);
                                        }
                                        break;
                                    case Locations.SP:
                                        rom.Add(0x31);
                                        break;
                                }
                            }

                        }
                        
                        break;
                    case Instructions.JR:
                        if(src.isFlag)
                        {
                            encode_dst = true;
                            dst.isWide = false;
                            switch(src.loc)
                            {
                                case Locations.NZ_FLAG:
                                    rom.Add(0x20);
                                    break;
                                case Locations.NC_FLAG:
                                    rom.Add(0x30);
                                    break;
                                case Locations.C_FLAG:
                                    rom.Add(0x38);
                                    break;
                                case Locations.Z_FLAG:
                                    rom.Add(0x28);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else if (src.loc == Locations.IMM)
                        {
                            rom.Add(0x18);
                            encode_src = true;
                            src.isWide = false;
                        }
                        break;
                    case Instructions.JP:
                        if(src.isFlag)
                        {
                            encode_dst = true;
                            dst.isWide = true;
                            switch(src.loc)
                            {
                                case Locations.NZ_FLAG:
                                    rom.Add(0xC2);
                                    break;
                                case Locations.NC_FLAG:
                                    rom.Add(0xD2);
                                    break;
                                case Locations.C_FLAG:
                                    rom.Add(0xDA);
                                    break;
                                case Locations.Z_FLAG:
                                    rom.Add(0xCA);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else if (src.isMem)
                        {
                            rom.Add(0xE9);
                        }
                        else if (src.loc == Locations.IMM || src.loc == Locations.WIDE_IMM)
                        {
                            src.isWide = true;
                            rom.Add(0xC3);
                            encode_src = true;
                        }
                        break;
                    case Instructions.OR:
                        switch (src.loc)
                        {
                            case Locations.B:
                            case Locations.C:
                            case Locations.E:
                            case Locations.H:
                            case Locations.L:
                            case Locations.HL:
                            case Locations.A:
                                rom.Add(GetLinearOffset(src, 0xB0));
                                break;
                            case Locations.IMM:
                                rom.Add(0xF6);
                                encode_src = true;
                                break;

                        }
                        break;
                    case Instructions.CP:
                        switch (src.loc)
                        {
                            case Locations.B:
                            case Locations.C:
                            case Locations.E:
                            case Locations.H:
                            case Locations.L:
                            case Locations.HL:
                            case Locations.A:
                                rom.Add(GetLinearOffset(src, 0xB8));
                                break;
                            case Locations.IMM:
                                rom.Add(0xFE);
                                encode_src = true;
                                break;

                        }
                        break;
                    case Instructions.RL:
                        rom.Add(0xCB);
                        rom.Add(GetLinearOffset(src, 0x10));
                        break;
                    case Instructions.RR:
                        rom.Add(0xCB);
                        rom.Add(GetLinearOffset(src, 0x18));
                        break;
                    case Instructions.DI:
                        rom.Add(0xF3);
                        break;
                    case Instructions.EI:
                        rom.Add(0xF8);
                        break;
                    case Instructions.LDD:
                        if(src.loc == Locations.A)
                        {
                            rom.Add(0x3A);
                        }
                        else
                        {
                            rom.Add(0x32);
                        }
                        break;
                    case Instructions.LDI:
                        if(src.loc == Locations.A)
                        {
                            rom.Add(0x2A);
                        }
                        else
                        {
                            rom.Add(0x22);
                        }
                        break;
                    case Instructions.ADD:
                            if (src.loc != Locations.HL && dst.isMem)
                            {
                                rom.Add(0x86);
                                break;
                            }
                            else
                            {
                                switch (dst.loc)
                                {
                                    case Locations.B:
                                    case Locations.C:
                                    case Locations.E:
                                    case Locations.H:
                                    case Locations.L:
                                    case Locations.A:
                                        rom.Add(GetLinearOffset(src, 0x80));
                                        break;
                                    case Locations.IMM:
                                    case Locations.WIDE_IMM:
                                        if (src.loc == Locations.A)
                                        {
                                            rom.Add(0xC6);
                                        }
                                        else if (src.loc == Locations.SP)
                                        {
                                            rom.Add(0xE8);
                                        }
                                        encode_dst = true;
                                        break;

                                    case Locations.SP:
                                        rom.Add(0x39);
                                        break;
                                    case Locations.HL:
                                        rom.Add(0x29);
                                        break;
                                    case Locations.DE:
                                        rom.Add(0x19);
                                        break;
                                    case Locations.BC:
                                        rom.Add(0x09);
                                        break;


                                }
                            }
                        
                        break;
                    case Instructions.ADC:
                        switch (src.loc)
                        {
                            case Locations.B:
                            case Locations.C:
                            case Locations.E:
                            case Locations.H:
                            case Locations.L:
                            case Locations.HL:
                            case Locations.A:
                                rom.Add(GetLinearOffset(src, 0x88));
                                break;
                            case Locations.IMM:
                                rom.Add(0xCE);
                                encode_dst = true;
                                break;

                        }
                        break;
                    case Instructions.SBC:
                        switch (src.loc)
                        {
                            case Locations.B:
                            case Locations.C:
                            case Locations.E:
                            case Locations.H:
                            case Locations.L:
                            case Locations.HL:
                            case Locations.A:
                                rom.Add(GetLinearOffset(src, 0x98));
                                break;
                            case Locations.IMM:
                                rom.Add(0xDE);
                                encode_dst = true;
                                break;

                        }
                        break;
                    case Instructions.BIT:
                        rom.Add(0xCB);
                        rom.Add(GetTableOffset(src,dst, 0x40));
                        break;
                    case Instructions.RES:
                        rom.Add(0xCB);
                        rom.Add(GetTableOffset(src,dst, 0x80));
                        break;
                    case Instructions.SET:
                        rom.Add(0xCB);
                        rom.Add(GetTableOffset(src, dst, 0xC0));
                        break;
                    case Instructions.RET:
                        if(src.isFlag)
                        {
                            switch(src.loc)
                            {
                                case Locations.NC_FLAG:
                                    rom.Add(0xD0);
                                    break;
                                case Locations.NZ_FLAG:
                                    rom.Add(0xC0);
                                    break;
                                case Locations.C_FLAG:
                                    rom.Add(0xD8);
                                    break;
                                case Locations.Z_FLAG:
                                    rom.Add(0xC8);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            rom.Add(0xC9);
                        }
                        break;
                    case Instructions.INC:
                        if(src.isMem)
                        {
                            rom.Add(0x34);
                        }
                        else
                        {
                            switch(src.loc)
                            {
                                case Locations.BC:
                                    rom.Add(0x03);
                                    break;
                                case Locations.DE:
                                    rom.Add(0x13);
                                    break;
                                case Locations.HL:
                                    rom.Add(0x23);
                                    break;
                                case Locations.SP:
                                    rom.Add(0x33);
                                    break;
                                case Locations.B:
                                    rom.Add(0x04);
                                    break;
                                case Locations.D:
                                    rom.Add(0x14);
                                    break;
                                case Locations.H:
                                    rom.Add(0x24);
                                    break;
                                case Locations.C:
                                    rom.Add(0x0C);
                                    break;
                                case Locations.L:
                                    rom.Add(0x2C);
                                    break;
                                case Locations.E:
                                    rom.Add(0x1C);
                                    break;
                                case Locations.A:
                                    rom.Add(0x3C);
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case Instructions.DEC:
                        if(src.isMem)
                        {
                            rom.Add(0x35);
                        }
                        else
                        {
                            switch(src.loc)
                            {
                                case Locations.BC:
                                    rom.Add(0x0B);
                                    break;
                                case Locations.DE:
                                    rom.Add(0x1B);
                                    break;
                                case Locations.HL:
                                    rom.Add(0x2B);
                                    break;
                                case Locations.SP:
                                    rom.Add(0x3B);
                                    break;
                                case Locations.B:
                                    rom.Add(0x05);
                                    break;
                                case Locations.D:
                                    rom.Add(0x15);
                                    break;
                                case Locations.H:
                                    rom.Add(0x25);
                                    break;
                                case Locations.C:
                                    rom.Add(0x0D);
                                    break;
                                case Locations.L:
                                    rom.Add(0x2D);
                                    break;
                                case Locations.E:
                                    rom.Add(0x1D);
                                    break;
                                case Locations.A:
                                    rom.Add(0x3D);
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case Instructions.SUB:
                        switch (src.loc)
                        {
                            case Locations.B:
                            case Locations.C:
                            case Locations.E:
                            case Locations.H:
                            case Locations.L:
                            case Locations.HL:
                            case Locations.A:
                                rom.Add(GetLinearOffset(src, 0x90));
                                break;
                            case Locations.IMM:
                                rom.Add(0xD6);
                                encode_src = true;
                                break;

                        }
                        break;
                    case Instructions.AND:
                        switch (src.loc)
                        {
                            case Locations.B:
                            case Locations.C:
                            case Locations.E:
                            case Locations.H:
                            case Locations.L:
                            case Locations.HL:
                            case Locations.A:
                                rom.Add(GetLinearOffset(src, 0xA0));
                                break;
                            case Locations.IMM:
                                rom.Add(0xE6);
                                encode_src = true;
                                break;

                        }
                        break;
                    case Instructions.XOR:
                        switch (src.loc)
                        {
                            case Locations.B:
                            case Locations.C:
                            case Locations.E:
                            case Locations.H:
                            case Locations.L:
                            case Locations.HL:
                            case Locations.A:
                                rom.Add(GetLinearOffset(src, 0xA8));
                                break;
                            case Locations.IMM:
                                rom.Add(0xEE);
                                encode_src = true;
                                break;

                        }
                        break;
                    case Instructions.RLC:
                        rom.Add(0xCB);
                        rom.Add(GetLinearOffset(src, 0x00));
                        break;
                    case Instructions.RRC:
                        rom.Add(0xCB);
                        rom.Add(GetLinearOffset(src, 0x08));
                        break;
                    case Instructions.POP:
                        rom.Add(GetBlockOffset(src, 0xC1));
                        break;
                    case Instructions.SLA:
                        rom.Add(0xCB);
                        rom.Add(GetLinearOffset(src, 0x20));
                        break;
                    case Instructions.SRA:
                         rom.Add(0xCB);
                        rom.Add(GetLinearOffset(src, 0x28));
                        break;
                    case Instructions.SRL:
                        rom.Add(0xCB);
                        rom.Add(GetLinearOffset(src, 0x38));
                        break;
                    case Instructions.NOP:
                        rom.Add(0x00);
                        break;
                    case Instructions.RLA:
                        rom.Add(0x17);
                        break;
                    case Instructions.RRA:
                        rom.Add(0x1F);
                        break;
                    case Instructions.DAA:
                        rom.Add(0x27);
                        break;
                    case Instructions.CPL:
                        rom.Add(0x2F);
                        break;
                    case Instructions.SCF:
                        rom.Add(0x37);
                        break;
                    case Instructions.CCF:
                        rom.Add(0x3F);
                        break;
                    case Instructions.LDH:
                        if(src.loc == Locations.A)
                        {
                            rom.Add(0xF0);
                            encode_dst = true;
                        }
                        else
                        {
                            rom.Add(0xE0);
                            encode_src = true;
                        }
                        break;
                    case Instructions.RST:
                        if (src.val % 0x10 == 0)
                        {
                            rom.Add((Byte)(0xC7 + src.val));
                        }
                        else
                        {
                            rom.Add((Byte)(0xCF + src.val - 0x08));
                        }
                        break;
                    case Instructions.CALL:
                        {
                            if(src.loc == Locations.NZ_FLAG)
                            {
                                encode_dst = true;
                                dst.isWide = true;
                                rom.Add(0xC4);
                            }
                            else if (src.loc == Locations.NC_FLAG)
                            {
                                encode_dst = true;
                                dst.isWide = true;
                                rom.Add(0xD4);
                            }
                            else if (src.loc == Locations.Z_FLAG)
                            {
                                encode_dst = true;
                                dst.isWide = true;
                                rom.Add(0xCC);
                            }
                            else if (src.loc == Locations.C_FLAG)
                            {
                                encode_dst = true;
                                dst.isWide = true;
                                rom.Add(0xDC);
                            }
                            else
                            {
                                rom.Add(0xCD);
                                encode_src = true;
                                src.isWide = true;
                            }
                        }
                        break;
                    case Instructions.PUSH:
                        rom.Add(GetBlockOffset(src, 0xC5));
                        break;
                    case Instructions.SWAP:
                        rom.Add(0xCB);
                        rom.Add(GetLinearOffset(src, 0x30));
                        break;
                    case Instructions.RLCA:
                        rom.Add(0x07);
                        break;
                    case Instructions.RRCA:
                        rom.Add(0x0F);
                        break;
                    case Instructions.STOP:
                        rom.Add(0x10);
                        break;
                    case Instructions.HALT:
                        rom.Add(0x76);
                        break;
                    case Instructions.RETI:
                        rom.Add(0xD9);
                        break;
                    default:
                        break;
                }
                if(encode_src)
                {
                    InsertLocationInfo(rom, src);
                }

                if(encode_dst)
                {
                    InsertLocationInfo(rom, dst);
                }
            }

            public int GetCurrentOffset()
            {
                List<Byte> nullrom = new List<byte>();
                this.AppendTo(nullrom);
                return nullrom.Count;
            }

        }

        

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

        public Assembler()
        {
            window = new GAPPDebugger.Controls.AssemblerProgress();

        }

        public void PrintLine(String line)
        {
            window.ConsoleWin.Text += line + "\n";
            window.ConsoleWin.UpdateLayout();
        }

        public List<Byte> Assemble()
        {
            window.Show();
            window.Focus();
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
            lines.Add(inputStream.GetText(new Interval((int)context.Start.StartIndex, (int)context.Stop.StopIndex)));
            currentInst = new Instruction();
        }

        public void ExitOp(GBASMParser.OpContext context)
        {
            PrintLine("End Op");
            currentInst.AppendTo(rom);
            String str = "";
            for (int i = rom_ptr; i < rom.Count; ++i)
            {
                str = str + "0x" + rom[i].ToString("X2") + " ";
            }
            lineToBytes.Add(str);
        }

        public void EnterMonad(GBASMParser.MonadContext context)
        {
            PrintLine("Monad Detected");
            argFSM = ArgFSM.COMPLETE;
            currentInst.op = MapOp(context.GetText());
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
            currentInst.op = MapOp(context.GetText());
        }

        public void ExitBiad(GBASMParser.BiadContext context)
        {
            //throw new NotImplementedException();
        }

        public void EnterTriad(GBASMParser.TriadContext context)
        {
            PrintLine("Triad Detected");
            argFSM = ArgFSM.SRC;
            currentInst.op = MapOp(context.GetText());
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
            DecArg();
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
                    SetArgLoc(Locations.Z_FLAG);
                    break;
                case "C":
                    SetArgLoc(Locations.C_FLAG);
                    break;
                case "NZ":
                    SetArgLoc(Locations.NZ_FLAG);
                    break;
                case "NC":
                    SetArgLoc(Locations.NC_FLAG);
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
            PrintLine(ctx.GetText());
        }

        public void ExitEveryRule(ParserRuleContext ctx)
        {
            //throw new NotImplementedException();
        }

        public void VisitErrorNode(Antlr4.Runtime.Tree.IErrorNode node)
        {
            PrintLine("!!ERROR!!");
            PrintLine(node.GetText());
            //node.
            //throw new NotImplementedException();
        }

        public void VisitTerminal(Antlr4.Runtime.Tree.ITerminalNode node)
        {
            //PrintLine("No more rules");
            //throw new NotImplementedException();
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
            String str = "";
            for (int i = rom_ptr; i < rom.Count; ++i)
            {
                str = str + "0x" + rom[i].ToString("X2") + " ";
            }
            lineToBytes.Add(str);
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
