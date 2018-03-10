using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBASMAssembler
{
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
            //Special Instruction Flags
            DATA_INSTRUCTION,
            ERR_INSTRUCTION
        };

    public class OffsetInfo
    {
        public int op; //Always zero
        public int src;
        public int dst;
        public int total;
        public OffsetInfo()
        {
            op = 0;
            src = 0;
            dst = 0;
            total = 0;
        }
    }
    public class Instruction
    {
        public Instructions op;
        //By convention src is arg 1, dst is arg 2 (even if some ops reverse this)
        public LocationInfo src;
        public LocationInfo dst;


        public static Instructions MapOp(String str)
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
                case "STOP 0":
                    return Instructions.STOP;
                case "HALT":
                    return Instructions.HALT;
                case "RETI":
                    return Instructions.RETI;
                case "RLCA":
                    return Instructions.RLCA;
                case "DB":
                    return Instructions.DATA_INSTRUCTION;
                default:
                    return Instructions.ERR_INSTRUCTION;
            }
        }


        public Instruction()
        {
            op = Instructions.ERR_INSTRUCTION;
            src = new LocationInfo();
            dst = new LocationInfo();
        }

        protected static void InsertLocationInfo(List<Byte> rom, LocationInfo src)
        {
            if (src.isWide)
            {
                //Store Low, High 
                rom.Add((Byte)(src.val & 0xFF));
                rom.Add((Byte)(src.val >> 8));
            }
            else
            {
                //Store byte
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
            Byte offset = offset = GetLinearOffset(dst, (Byte)(start));
            if (src.val % 2 != 0)
            {
                offset += 0x08;
            }
            offset += (Byte)((src.val / 2) * 0x10);
            return offset;
        }

        public class OpEncodeOptions
        {
            public Boolean encode_dst;
            public Boolean encode_src;

            public OpEncodeOptions()
            {
                encode_dst = false;
                encode_src = false;
            }
        }

        public void EncodeOp(List<Byte> rom, OpEncodeOptions opt)
        {
            switch (op)
            {
                case Instructions.LD:
                    //No pattern given to some LOAD inst
                    if (src.isMem)
                    {
                        switch (src.loc)
                        {
                            case Locations.BC:
                                rom.Add(0x02);
                                break;
                            case Locations.DE:
                                rom.Add(0x12);
                                break;
                            case Locations.HL:
                                if (dst.loc >= Locations.B && dst.loc <= Locations.A)
                                {
                                    rom.Add(GetLinearOffset(dst, 0x70));
                                }
                                else
                                {
                                    rom.Add(0x36);
                                }
                                break;
                            case Locations.SP:
                                if (dst.loc == Locations.HL)
                                {
                                    rom.Add(0xF9);
                                }
                                else
                                {
                                    //encode_dst = true;
                                    rom.Add(0x31);
                                }
                                break;
                            case Locations.C:
                                rom.Add(0xE2);
                                break;
                            case Locations.WIDE_IMM:
                                src.isWide = true;
                                if (dst.loc == Locations.SP)
                                {
                                    rom.Add(0x08);
                                }
                                break;
                            case Locations.IMM:
                                rom.Add(0xEA);
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
                                    if (dst.isMem && dst.loc == Locations.C)
                                    {
                                        rom.Add(0xF2);
                                    }
                                    else
                                    {
                                        rom.Add(GetLinearOffset(dst, 0x78));
                                    }
                                    break;
                                case Locations.SP:
                                    if(dst.loc == Locations.HL)
                                    {
                                        rom.Add(0xF9);
                                    }
                                    else
                                    {
                                        rom.Add(0x31);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
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
                                        switch (dst.loc)
                                        {
                                            case Locations.BC:
                                                rom.Add(0x0A);
                                                break;
                                            case Locations.DE:
                                                rom.Add(0x1A);
                                                break;
                                            case Locations.HL:
                                                rom.Add(0x7E);
                                                break;
                                            case Locations.WIDE_IMM:
                                            case Locations.IMM:
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
                                    dst.isWide = true;
                                    
                                    break;
                                case Locations.DE:
                                    rom.Add(0x11);
                                    dst.isWide = true;
                                    break;
                                case Locations.HL:
                                    if (dst.loc != Locations.OFFSET)
                                    {
                                        rom.Add(0x21);
                                        dst.isWide = true;
                                    }
                                    else
                                    {
                                        rom.Add(0xF8);
                                        opt.encode_dst = true;
                                        dst.isWide = false;
                                    }
                                    break;
                                case Locations.SP:
                                    rom.Add(0x31);
                                    dst.isWide = true;
                                    break;
                            }
                        }

                    }

                    break;
                case Instructions.JR:
                    if (src.isFlag || src.loc == Locations.C)
                    {
                        dst.isWide = false;
                        switch (src.loc)
                        {
                            case Locations.NZ:
                                rom.Add(0x20);
                                break;
                            case Locations.NC:
                                rom.Add(0x30);
                                break;
                            case Locations.C:
                                rom.Add(0x38);
                                break;
                            case Locations.Z:
                                rom.Add(0x28);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        rom.Add(0x18);
                        src.isWide = false;
                    }
                    break;
                case Instructions.JP:
                    if (src.isFlag || src.loc == Locations.C)
                    {
                        dst.isWide = true;
                        switch (src.loc)
                        {
                            case Locations.NZ:
                                rom.Add(0xC2);
                                break;
                            case Locations.NC:
                                rom.Add(0xD2);
                                break;
                            case Locations.C:
                                rom.Add(0xDA);
                                break;
                            case Locations.Z:
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
                    else
                    {
                        src.isWide = true;
                        rom.Add(0xC3);
                    }
                    break;
                case Instructions.OR:
                    switch (src.loc)
                    {
                        case Locations.B:
                        case Locations.C:
                        case Locations.D:
                        case Locations.E:
                        case Locations.H:
                        case Locations.L:
                        case Locations.HL:
                        case Locations.A:
                            rom.Add(GetLinearOffset(src, 0xB0));
                            break;
                        case Locations.IMM:
                            rom.Add(0xF6);
                            break;

                    }
                    break;
                case Instructions.CP:
                    switch (src.loc)
                    {
                        case Locations.B:
                        case Locations.C:
                        case Locations.D:
                        case Locations.E:
                        case Locations.H:
                        case Locations.L:
                        case Locations.HL:
                        case Locations.A:
                            rom.Add(GetLinearOffset(src, 0xB8));
                            break;
                        case Locations.IMM:
                            rom.Add(0xFE);
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
                    rom.Add(0xFB);
                    break;
                case Instructions.LDD:
                    if (src.loc == Locations.A)
                    {
                        rom.Add(0x3A);
                    }
                    else
                    {
                        rom.Add(0x32);
                    }
                    break;
                case Instructions.LDI:
                    if (src.loc == Locations.A)
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
                            case Locations.D:
                            case Locations.E:
                            case Locations.H:
                            case Locations.L:
                            case Locations.A:
                                rom.Add(GetLinearOffset(dst, 0x80));
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
                                opt.encode_dst = true;
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
                    switch (dst.loc)
                    {
                        case Locations.B:
                        case Locations.C:
                        case Locations.D:
                        case Locations.E:
                        case Locations.H:
                        case Locations.L:
                        case Locations.HL:
                        case Locations.A:
                            rom.Add(GetLinearOffset(dst, 0x88));
                            break;
                        case Locations.IMM:
                            rom.Add(0xCE);
                            break;

                    }
                    break;
                case Instructions.SBC:
                    switch (dst.loc)
                    {
                        case Locations.B:
                        case Locations.C:
                        case Locations.D:
                        case Locations.E:
                        case Locations.H:
                        case Locations.L:
                        case Locations.HL:
                        case Locations.A:
                            rom.Add(GetLinearOffset(dst, 0x98));
                            break;
                        case Locations.IMM:
                            rom.Add(0xDE);
                            break;

                    }
                    break;
                case Instructions.BIT:
                    rom.Add(0xCB);
                    rom.Add(GetTableOffset(src, dst, 0x40));
                    opt.encode_src = false;
                    break;
                case Instructions.RES:
                    rom.Add(0xCB);
                    rom.Add(GetTableOffset(src, dst, 0x80));
                    opt.encode_src = false;
                    break;
                case Instructions.SET:
                    rom.Add(0xCB);
                    rom.Add(GetTableOffset(src, dst, 0xC0));
                    opt.encode_src = false;
                    break;
                case Instructions.RET:
                    if (src.isFlag || src.loc == Locations.C)
                    {
                        switch (src.loc)
                        {
                            case Locations.NC:
                                rom.Add(0xD0);
                                break;
                            case Locations.NZ:
                                rom.Add(0xC0);
                                break;
                            case Locations.C:
                                rom.Add(0xD8);
                                break;
                            case Locations.Z:
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
                    if (src.isMem)
                    {
                        rom.Add(0x34);
                    }
                    else
                    {
                        switch (src.loc)
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
                    if (src.isMem)
                    {
                        rom.Add(0x35);
                    }
                    else
                    {
                        switch (src.loc)
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
                        case Locations.D:
                        case Locations.E:
                        case Locations.H:
                        case Locations.L:
                        case Locations.HL:
                        case Locations.A:
                            rom.Add(GetLinearOffset(src, 0x90));
                            break;
                        case Locations.IMM:
                            rom.Add(0xD6);
                            break;

                    }
                    break;
                case Instructions.AND:
                    switch (src.loc)
                    {
                        case Locations.B:
                        case Locations.C:
                        case Locations.D:
                        case Locations.E:
                        case Locations.H:
                        case Locations.L:
                        case Locations.HL:
                        case Locations.A:
                            rom.Add(GetLinearOffset(src, 0xA0));
                            break;
                        case Locations.IMM:
                            rom.Add(0xE6);
                            break;

                    }
                    break;
                case Instructions.XOR:
                    switch (src.loc)
                    {
                        case Locations.B:
                        case Locations.C:
                        case Locations.D:
                        case Locations.E:
                        case Locations.H:
                        case Locations.L:
                        case Locations.HL:
                        case Locations.A:
                            rom.Add(GetLinearOffset(src, 0xA8));
                            break;
                        case Locations.IMM:
                            rom.Add(0xEE);
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
                    if (src.loc == Locations.A)
                    {
                        rom.Add(0xF0);
                    }
                    else
                    {
                        rom.Add(0xE0);
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
                    opt.encode_src = false;
                    break;
                case Instructions.CALL:
                    {
                        if (src.loc == Locations.NZ)
                        {
                            dst.isWide = true;
                            rom.Add(0xC4);
                        }
                        else if (src.loc == Locations.NC)
                        {
                            dst.isWide = true;
                            rom.Add(0xD4);
                        }
                        else if (src.loc == Locations.Z)
                        {
                            dst.isWide = true;
                            rom.Add(0xCC);
                        }
                        else if (src.loc == Locations.C)
                        {
                            dst.isWide = true;
                            rom.Add(0xDC);
                        }
                        else
                        {
                            rom.Add(0xCD);
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
                    opt.encode_src = false;
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
}

        public void AppendTo(List<Byte> rom)
        {
            //Compile instruction to opcode
            if (this.op <= Instructions.NUM_INSTRUCTIONS)
            {
                OpEncodeOptions opt = new OpEncodeOptions();
                opt.encode_dst = (dst.loc == Locations.WIDE_IMM) || (dst.loc == Locations.IMM) || (dst.loc == Locations.OFFSET);
                opt.encode_src = (src.loc == Locations.WIDE_IMM) || (src.loc == Locations.IMM) || (src.loc == Locations.OFFSET);
                EncodeOp(rom, opt);
                if (opt.encode_src)
                {
                    InsertLocationInfo(rom, src);
                }

                if (opt.encode_dst)
                {
                    InsertLocationInfo(rom, dst);
                }
            }
        }

        public OffsetInfo GetCurrentOffset()
        {
            OffsetInfo offset = new OffsetInfo();
            List<Byte> nullrom = new List<byte>();
            OpEncodeOptions opt = new OpEncodeOptions();
            opt.encode_dst = (dst.loc == Locations.WIDE_IMM) || (dst.loc == Locations.IMM) || (dst.loc == Locations.OFFSET);
            opt.encode_src = (src.loc == Locations.WIDE_IMM) || (src.loc == Locations.IMM) || (src.loc == Locations.OFFSET);
            EncodeOp(nullrom, opt);
            offset.src = nullrom.Count;
            if (opt.encode_src)
            {
                InsertLocationInfo(nullrom, src);
            }
            offset.dst = nullrom.Count;
            if (opt.encode_dst)
            {
                InsertLocationInfo(nullrom, dst);
            }
            offset.total = nullrom.Count;
            return offset;
        }

        

    }

        
}
