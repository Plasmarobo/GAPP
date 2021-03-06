#include "cpu.h"
#include "mem.h"

#ifdef _DEBUG
#include <iostream>
#endif

#define SetIfNone(x,y) if(x==Location::NONE) x=y
//Use the carry and borrow only with 32 bit signed types
//Use shift for sign extension
#define HalfCarry(dst,src) (((dst & 0x0F) + (src & 0x0F)) >= 0x10)
#define HalfBorrow(dst,src) ((src & 0x0F) > (dst & 0x0F))
#define Carry8b(dst,src) ((dst + src) >= 0x100)
#define Borrow8b(dst,src) ((src & 0xFF) > (dst & 0xFF))

// Stored High, Low
#define PushWide(value) StackPush((value >> 8) & 0xFF);StackPush(value & 0xFF)
// Read Low, High
#define PopWide(value) value = StackPop(); value |= (StackPop() << 8)

// Jump from just after JR
// Compensate for reading data byte
#define CalcRelJump (static_cast<char> (FetchPC()))

const std::string location_names[NUM_LOCATIONS] = {
	"NONE",
	"B",
	"C",
	"D",
	"E",
	"H",
	"L",
	"A",
	"F",
	"AF",
	"BC",
	"DE",
	"HL",
	"SP",
	"PC",
	"MEM", //Memory address 16b
	"IMM", //8b
	"WIDE_IMM", //16b
	"OFFSET", //Memory address given by FF00 + n (8b)
	"WIDE_OFFSET", //Memory address given by FF00+n (16b)
	"PORT",
	"STACK"
};

const
std::string instruction_names[NUM_INSTRUCTIONS] =
{
	"NON",
	"NOP",
	"LOAD",
	"ADD",
	"SUB",
	"STOP",
	"HALT",
	"AND",
	"OR",
	"XOR",
	"CP",
	"RLC",
	"RL",
	"RRC",
	"RR",
	"SRS", //Shift right signed
	"SL",
	"SR",
	"DAA",
	"CPL",
	"SCF",
	"CCF",
	"POP",
	"PUSH",
	"DI",
	"EI",
	"BIT",
	"RES",
	"SET",
	"SWAP"
};


Register::Register()
{
	m_upper = 0;
	m_lower = 0;
}

Location GBCPU::MapLocation(unsigned char offset)
{
	if (offset <= (Location::B - Location::L))
	{
		return (Location)(Location::B + offset);
	}
	else
	{
		return Location::NONE;
	}
}

bool InstructionPacket::IsDestWide()
{
	switch(dest)
	{
	case Location::AF:
	case Location::BC:
	case Location::DE:
	case Location::HL:
	case Location::PC:
	case Location::SP:
	case Location::WIDE_MEM:
		return true;
	default:
		return false;
	}
}

void GBCPU::StackPush(unsigned char byte)
{
	m_regs.DecSP();
	WriteMem(m_regs.SP(), byte);
}

void GBCPU::PushPC()
{
	unsigned short pc = m_regs.PC();
	PushWide(pc);
}

unsigned char GBCPU::StackPop()
{
	unsigned char val = ReadMem(m_regs.SP());
	m_regs.IncSP();
	return val;
}

void GBCPU::PopPC()
{
	unsigned short pc;
	PopWide(pc);
}

InstructionPacket GBCPU::DecodeInstruction()
{
	InstructionPacket packet;
	HandleInterrupts();
	if ((!m_halted) && (!m_stopped))
	{
		//Fetches addresses inline
		//Immediates must be fetched by execution
		unsigned char op = FetchPC();
		
		bool address_locked = false;

		switch (op)
		{
			//LOAD IMM -> REG
		case 0x06: //B
			SetIfNone(packet.dest, Location::B);
		case 0x0E: //C
			SetIfNone(packet.dest, Location::C);
		case 0x16: //D
			SetIfNone(packet.dest, Location::D);
		case 0x1E: //E
			SetIfNone(packet.dest, Location::E);
		case 0x26: //H
			SetIfNone(packet.dest, Location::H);
		case 0x2E: //L
			SetIfNone(packet.dest, Location::L);
			packet.source = Location::IMM;
			packet.instruction = Instruction::LOAD;
			break;
			//LOAD REG -> REG
			//LOAD IN TO A
		case 0x78: //B
		case 0x79: //C
		case 0x7A: //D
		case 0x7B: //E
		case 0x7C: //H
		case 0x7D: //L
		case 0x7E: //MEM(HL)
		case 0x7F: //A
			packet.source = RegisterTable(op - 0x78, packet);
		case 0x3E: //IMM
			SetIfNone(packet.source, Location::IMM);
			packet.instruction = Instruction::LOAD;
			packet.dest = Location::A;
			break;
			//WIDE
		case 0x0A: //MEM(BC)
			packet.address = m_regs.BC();
		case 0x1A: //MEM(DE)
			SetIfNone(packet.address, m_regs.DE());
		case 0x3A: //MEM(HL--)
			if (packet.address == Location::NONE)
			{
				packet.address = m_regs.HL();
				m_regs.HL(m_regs.HL() - 1);
			}
		case 0x2A: //MEM(HL++)
			if (packet.address == Location::NONE)
			{
				packet.address = m_regs.HL();
				m_regs.HL(m_regs.HL() + 1);
			}
		case 0xF0: //MEM(0xFF+IMM)
			SetIfNone(packet.address,0xFF00 + FetchPC());
		case 0xF2: //MEM(0xFF00 + C)
			SetIfNone(packet.address, 0xFF00 + m_regs.C());
		case 0xFA: //MEM(WIDE IMM LS -> MS)
			packet.instruction = Instruction::LOAD;
			packet.source = Location::MEM;
			SetIfNone(packet.address, FetchPC16());
			packet.dest = Location::A;
			break;
			//LOAD INTO B
		case 0x40: //B
		case 0x41:
		case 0x42:
		case 0x43:
		case 0x44:
		case 0x45:
		case 0x46: //MEM(HL)
		case 0x47: //A
			packet.source = RegisterTable(op - 0x40, packet);
			packet.instruction = Instruction::LOAD;
			packet.dest = Location::B;
			break;
			//LOAD INTO C
		case 0x48: //B
		case 0x49:
		case 0x4A:
		case 0x4B:
		case 0x4C:
		case 0x4D:
		case 0x4E: //MEM(HL)
		case 0x4F: //A
			packet.source = RegisterTable(op - 0x48, packet);
			packet.instruction = Instruction::LOAD;
			packet.dest = Location::C;
			break;
			//LOAD INTO D
		case 0x50: //B
		case 0x51:
		case 0x52:
		case 0x53:
		case 0x54:
		case 0x55:
		case 0x56: //MEM(HL)
		case 0x57: //A
			packet.source = RegisterTable(op - 0x50, packet);
			packet.instruction = Instruction::LOAD;
			packet.dest = Location::D;
			break;
			//LOAD INTO E
		case 0x58: //B
		case 0x59:
		case 0x5A:
		case 0x5B:
		case 0x5C:
		case 0x5D:
		case 0x5E: //MEM(HL)
		case 0x5F: //A
			packet.source = RegisterTable(op - 0x58, packet);
			packet.instruction = Instruction::LOAD;
			packet.dest = Location::E;
			break;
			//LOAD INTO H
		case 0x60: //B
		case 0x61:
		case 0x62:
		case 0x63:
		case 0x64:
		case 0x65:
		case 0x66: //MEM(HL)
		case 0x67: //A
			packet.source = RegisterTable(op - 0x60, packet);
			packet.instruction = Instruction::LOAD;
			packet.dest = Location::H;
			break;
			//LOAD INTO L
		case 0x68: //B
		case 0x69:
		case 0x6A:
		case 0x6B:
		case 0x6C:
		case 0x6D:
		case 0x6E: //MEM(HL)
		case 0x6F: //A
			packet.source = RegisterTable(op - 0x68, packet);
			packet.instruction = Instruction::LOAD;
			packet.dest = Location::L;
			break;
			//LOAD INTO MEM(HL)
		case 0x70: //B
		case 0x71:
		case 0x72:
		case 0x73:
		case 0x74:
		case 0x75: //L
			packet.source = RegisterTable(op - 0x70, packet);
			packet.instruction = Instruction::LOAD;
			packet.dest = Location::MEM;
			packet.address = m_regs.HL();
			break;
		case 0x36: //IMM
			packet.instruction = Instruction::LOAD;
			packet.source = Location::IMM;
			packet.dest = Location::MEM;
			packet.address = m_regs.HL();
			break;
			//MEM(BC)
		case 0x02: //A
			packet.address = m_regs.BC();
			//MEM(DE)
		case 0x12: //A
			SetIfNone(packet.address, m_regs.DE());
			//MEM(HL)
		case 0x22:
		case 0x32:
		case 0x77: //A
			SetIfNone(packet.address, m_regs.HL());
			if (op == 0x32) m_regs.HL(m_regs.HL() - 1);
			if (op == 0x22) m_regs.HL(m_regs.HL() + 1);
			//LOAD MEM(0xFF00+IMM)
		case 0xE0: //A
			SetIfNone(packet.address, 0xFF00 + FetchPC());
			//LOAD INTO MEM(0xFF+C)
		case 0xE2: //A
			SetIfNone(packet.address, 0xFF00 + m_regs.C());
			//MEM(WIDE IMM LS->MS)
		case 0xEA: //A
			SetIfNone(packet.address, FetchPC16());
			packet.dest = Location::MEM;
			packet.source = Location::A;
			packet.instruction = Instruction::LOAD;
			break;
			//WIDE LOAD
			//BC
		case 0x01: //WIDE-IMM
			packet.dest = Location::BC;
			//DE
		case 0x11: //WIDE-IMM
			SetIfNone(packet.dest, Location::DE);
			//HL
		case 0x21: //WIDE-IMM
			SetIfNone(packet.dest, Location::HL);
			//SP
		case 0x31: //WIDE-IMM
			SetIfNone(packet.dest, Location::SP);
			packet.source = Location::WIDE_IMM;
			packet.instruction = Instruction::LOAD;
			break;
			//LOAD SP
		case 0xF9: //HL
			packet.instruction = Instruction::LOAD;
			packet.source = Location::HL;
			packet.dest = Location::SP;
			packet.cycles += 4;
			break;
		case 0xF8: //SP + n: reset zero, reset nonzero may set h, may set c
			packet.instruction = Instruction::LOAD;
			packet.source = Location::SP;
			packet.dest = Location::HL;
			packet.offset = FetchPC();
			packet.cycles += 4;
			break;
			//LOAD SP
		case 0x08: //WIDE-IMM
			packet.instruction = Instruction::LOAD;
			packet.dest = Location::WIDE_MEM;
			packet.source = Location::SP;
			packet.addr_offset = 1;
			packet.address = FetchPC16();
			break;
			//PUSH -> SP - 2
		case 0xF5: //AF
			SetIfNone(packet.source, Location::AF);
		case 0xC5: //BC
			SetIfNone(packet.source, Location::BC);
		case 0xD5: //DE
			SetIfNone(packet.source, Location::DE);
		case 0xE5: //HL
			SetIfNone(packet.source, Location::HL);
			packet.address = m_regs.SP();
			packet.dest = Location::MEM;
			packet.instruction = Instruction::PUSH;
			packet.cycles += 4;
			break;
			//POP
		case 0xF1: //AF
			SetIfNone(packet.dest, Location::AF);
		case 0xC1: //BC
			SetIfNone(packet.dest, Location::BC);
		case 0xD1: //DE
			SetIfNone(packet.dest, Location::DE);
		case 0xE1: //HL
			SetIfNone(packet.dest, Location::HL);
			packet.address = m_regs.SP();
			packet.source = Location::MEM;
			packet.instruction = Instruction::POP;
			break;
			//ADD to A, set zero if zero, reset nonzero, set h if carry bit 3, set c if carry from 7
		case 0x80: //B
		case 0x81: //C
		case 0x82: //D
		case 0x83: //E
		case 0x84: //H
		case 0x85: //L
		case 0x86: //MEM(HL)
		case 0x87: //A
			packet.source = RegisterTable(op - 0x80, packet);
		case 0xC6: //IMM
			SetIfNone(packet.source, Location::IMM);
			packet.instruction = Instruction::ADD;
			packet.dest = Location::A;
			break;
			//ADC -> Add X plus carry to A
		case 0x88: //B
		case 0x89: //C
		case 0x8A: //D
		case 0x8B: //E
		case 0x8C: //H
		case 0x8D: //L
		case 0x8E: //MEM(HL)
		case 0x8F: //A
			packet.source = RegisterTable(op - 0x88, packet);
		case 0xCE: //IMM
			packet.instruction = Instruction::ADD;
			SetIfNone(packet.source, Location::IMM);
			packet.dest = Location::A;
			packet.offset = m_regs.Carry() ? 1 : 0;
			break;
			//SUB from A
		case 0x90: //B
		case 0x91: //C
		case 0x92: //D
		case 0x93: //E
		case 0x94: //H
		case 0x95: //L
		case 0x96: //MEM(HL)
		case 0x97: //A
			packet.source = RegisterTable(op - 0x90, packet);
		case 0xD6: //IMM
			packet.instruction = Instruction::SUB;
			SetIfNone(packet.source, Location::IMM);
			packet.dest = Location::A;
			break;
			//SBC -> Sub X plus carry from A
		case 0xDE: // IMM
			SetIfNone(packet.source, Location::IMM);
		case 0x98: //B
		case 0x99: //C
		case 0x9A: //D
		case 0x9B: //E
		case 0x9C: //H
		case 0x9D: //L
		case 0x9E: //MEM(HL)
		case 0x9F: //A
			packet.instruction = Instruction::SUB;
			SetIfNone(packet.source, RegisterTable(op - 0x98, packet));
			packet.offset = m_regs.Carry() ? 1 : 0;
			packet.dest = Location::A;
			break;
			//AND with A
		case 0xA0: //B
		case 0xA1: //C
		case 0xA2: //D
		case 0xA3: //E
		case 0xA4: //H
		case 0xA5: //L
		case 0xA6: //MEM(HL)
		case 0xA7: //A
			packet.source = RegisterTable(op - 0xA0, packet);
		case 0xE6: //IMM
			SetIfNone(packet.source, Location::IMM);
			packet.instruction = Instruction::AND;
			packet.dest = Location::A;
			break;
			//OR with A
		case 0xB0: //B
		case 0xB1: //C
		case 0xB2: //D
		case 0xB3: //E
		case 0xB4: //H
		case 0xB5: //L
		case 0xB6: //MEM(HL)
		case 0xB7: //A
			packet.source = RegisterTable(op - 0xB0, packet);
		case 0xF6: //IMM
			SetIfNone(packet.source, Location::IMM);
			packet.instruction = Instruction::OR;
			packet.dest = Location::A;
			break;
			//XOR with A
		case 0xA8: //B
		case 0xA9: //C
		case 0xAA: //D
		case 0xAB: //E
		case 0xAC: //H
		case 0xAD: //L
		case 0xAE: //MEM(HL)
		case 0xAF: //A
			packet.source = RegisterTable(op - 0xA8, packet);
		case 0xEE: //IMM
			SetIfNone(packet.source, Location::IMM);
			packet.instruction = Instruction::XOR;
			packet.dest = Location::A;
			break;
			//COMPARE
		case 0xB8: //B
		case 0xB9: //C
		case 0xBA: //D
		case 0xBB: //E
		case 0xBC: //H
		case 0xBD: //L
		case 0xBE: //MEM(HL)
		case 0xBF: //A
			packet.source = RegisterTable(op - 0xB8, packet);
		case 0xFE: //IMM
			SetIfNone(packet.source, Location::IMM);
			packet.instruction = Instruction::CP;
			packet.dest = Location::A;
			break;
			//INC
		case 0x04: //B
			SetIfNone(packet.dest, Location::B);
		case 0x0C: //C
			SetIfNone(packet.dest, Location::C);
		case 0x14: //D
			SetIfNone(packet.dest, Location::D);
		case 0x1C: //E
			SetIfNone(packet.dest, Location::E);
		case 0x24: //H
			SetIfNone(packet.dest, Location::H);
		case 0x2C: //L
			SetIfNone(packet.dest, Location::L);
		case 0x34: //MEM(HL)
			if (op == 0x34)
			{
				SetIfNone(packet.dest, Location::MEM);
				packet.address = m_regs.HL();
			}
		case 0x3C: //A
			SetIfNone(packet.dest, Location::A);
			packet.offset = 1;
			packet.source = packet.dest;
			packet.instruction = Instruction::LOAD;
			break;
			//DEC
		case 0x05: //B
			SetIfNone(packet.dest, Location::B);
		case 0x0D: //C
			SetIfNone(packet.dest, Location::C);
		case 0x15: //D
			SetIfNone(packet.dest, Location::D);
		case 0x1D: //E
			SetIfNone(packet.dest, Location::E);
		case 0x25: //H
			SetIfNone(packet.dest, Location::H);
		case 0x2D: //L
			SetIfNone(packet.dest, Location::L);
		case 0x35: //MEM(HL)
			if (op == 0x35)
			{
				SetIfNone(packet.dest, Location::MEM);
				packet.address = m_regs.HL();
			}
		case 0x3D: //A
			SetIfNone(packet.dest, Location::A);
			packet.offset = -1;
			packet.source = packet.dest;
			packet.instruction = Instruction::LOAD;
			break;
			//ADD 16
			//ADD TO HL
		case 0x09: //BC
			SetIfNone(packet.source, Location::BC);
		case 0x19: //DE
			SetIfNone(packet.source, Location::DE);
		case 0x29: //HL
			SetIfNone(packet.source, Location::HL);
		case 0x39: //SP
			SetIfNone(packet.source, Location::SP);
			packet.dest = Location::HL;
			packet.instruction = Instruction::ADD;
			packet.cycles += 4;
			break;
			//ADD SP
		case 0xE8: //IMM
			packet.dest = Location::SP;
			packet.source = Location::IMM;
			packet.instruction = Instruction::ADD;
			packet.cycles += 8;
			break;
			//INC16
		case 0x03: //BC
			SetIfNone(packet.dest, Location::BC);
		case 0x13: //DE
			SetIfNone(packet.dest, Location::DE);
		case 0x23: //HL
			SetIfNone(packet.dest, Location::HL);
		case 0x33: //SP
			SetIfNone(packet.dest, Location::SP);
			packet.offset = 1;
			packet.source = packet.dest;
			packet.instruction = Instruction::LOAD;
			break;
			//DEC16
		case 0x0B: //BC
			SetIfNone(packet.dest, Location::BC);
		case 0x1B: //DE
			SetIfNone(packet.dest, Location::DE);
		case 0x2B: //HL
			SetIfNone(packet.dest, Location::HL);
		case 0x3B: //SP
			SetIfNone(packet.dest, Location::SP);
			packet.offset = -1;
			packet.source = packet.dest;
			packet.instruction = Instruction::LOAD;
			break;
			//CB EXT
		case 0xCB:
			DecodeCB(packet);
			break;
			//DAA
		case 0x27: //Decimal Adjust A
			packet.instruction = Instruction::DAA;
			packet.source = Location::A;
			packet.dest = Location::A;
			break;
			//CPL
		case 0x2F: //A
			packet.instruction = Instruction::CPL;
			packet.source = Location::A;
			packet.dest = Location::A;
			break;
			//CCF 
		case 0x3F: //Complement carry flag
			packet.instruction = Instruction::CCF;
			packet.source = Location::F;
			packet.dest = Location::F;
			break;
			//SCF
		case 0x37: //Set carry flag
			packet.instruction = Instruction::SCF;
			packet.source = Location::F;
			packet.dest = Location::F;
			break;
			//NOP
		case 0x00: //NOP
			packet.instruction = Instruction::NOP;
			break;
			//HALT
		case 0x76: //Power down GBCPU until interrupt
			packet.instruction = Instruction::HALT;
			break;
			//STOP
		case 0x10:
			packet.instruction = Instruction::STOP;
			FetchPC();
			break;
			//DI
		case 0xF3: //Disable interrupt after exec
			packet.instruction = Instruction::DI;
			break;
			//EI
		case 0xFB: //Enable interrupt after exec
			packet.instruction = Instruction::EI;
			break;
			//ROT LEFT
		case 0x17: //Rotate A left through carry flag
			packet.instruction = Instruction::RL;
			packet.source = Location::A;
			packet.dest = Location::A;
			break;
		case 0x07: //Rotate A left, bit 7 to carry
			packet.instruction = Instruction::RLC;
			packet.source = Location::A;
			packet.dest = Location::A;
			break;
			//ROT RIGHT
		case 0x1F: //Rotate A right through carry flag
			packet.instruction = Instruction::RR;
			packet.source = Location::A;
			packet.dest = Location::A;
			break;
		case 0x0F:
			packet.instruction = Instruction::RRC;
			packet.source = Location::A;
			packet.dest = Location::A;
			break;
			//JUMP
		case 0xC3: //WIDE-IMM
			packet.instruction = Instruction::LOAD;
			packet.source = Location::WIDE_IMM;
			packet.dest = Location::PC;
			m_cycles += 4;
			break;
		case 0xE9: //MEM(HL)
			packet.instruction = Instruction::LOAD;
			packet.source = Location::MEM;
			packet.address = m_regs.HL();
			packet.dest = Location::PC;
			m_cycles -= 4;
			break;
			//JUMP IF
		case 0xC2: //Z reset
			packet.source = Location::WIDE_IMM;
			packet.dest = Location::PC;
			if (!m_regs.Zero())
			{	
				m_cycles += 4;
				packet.instruction = Instruction::LOAD;
			}
			else
			{
				packet.instruction = Instruction::NOP;
			}
			break;
		case 0xCA: //Z set
			packet.source = Location::WIDE_IMM;
			packet.dest = Location::PC;
			if (m_regs.Zero())
			{
				m_cycles += 4;
				packet.instruction = Instruction::LOAD;
			}
			else
			{
				packet.instruction = Instruction::NOP;
			}
			break;
		case 0xD2: //C reset (no carry)
			packet.source = Location::WIDE_IMM;
			packet.dest = Location::PC;
			if (!m_regs.Carry())
			{
				m_cycles += 4;
				packet.instruction = Instruction::LOAD;
			}
			else
			{
				packet.instruction = Instruction::NOP;
			}
			break;
		case 0xDA: //C set (carry)
			packet.source = Location::WIDE_IMM;
			packet.dest = Location::PC;
			if (m_regs.Carry())
			{
				m_cycles += 4;
				packet.instruction = Instruction::LOAD;
			}
			else
			{
				packet.instruction = Instruction::NOP;
			}
			break;
			//JUMP REL
		case 0x18: //signed IMM
			packet.source = Location::PC;
			packet.dest = Location::PC;
			packet.instruction = Instruction::LOAD;
			packet.offset = CalcRelJump;
			m_cycles += 4;
			break;
			//JUMP REL IF
		case 0x20: //NZ: Z reset, IMM signed
			packet.source = Location::PC;
			packet.dest = Location::PC;
			packet.offset = CalcRelJump;
			if (!m_regs.Zero())
			{
				m_cycles += 4;
				packet.instruction = Instruction::LOAD;
			}
			else
			{
				packet.instruction = Instruction::NOP;
			}
			break;
		case 0x28: //Z set. IMM signed
			packet.source = Location::PC;
			packet.dest = Location::PC;
			packet.offset = CalcRelJump;
			if (m_regs.Zero())
			{
				m_cycles += 4;
				packet.instruction = Instruction::LOAD;
			}
			else
			{
				packet.instruction = Instruction::NOP;
			}
			break;
		case 0x30: //C reset, IMM signed
			packet.source = Location::PC;
			packet.dest = Location::PC;
			packet.offset = CalcRelJump;
			if (!m_regs.Carry())
			{
				m_cycles += 4;
				packet.instruction = Instruction::LOAD;
			}
			else
			{
				packet.instruction = Instruction::NOP;
			}
			break;
		case 0x38: //C set, IMM signed
			packet.source = Location::PC;
			packet.dest = Location::PC;
			packet.offset = CalcRelJump;
			if (m_regs.Carry())
			{
				m_cycles += 4;
				packet.instruction = Instruction::LOAD;
			}
			else
			{
				packet.instruction = Instruction::NOP;
			}
			break;
			//CALL
		case 0xCD: //WIDE-IMM
			PushWide(m_regs.PC() + 2);
			packet.instruction = Instruction::LOAD;
			packet.dest = Location::PC;
			packet.source = Location::WIDE_IMM;
			m_cycles += 4;
			break;
			//CALL IF
		case 0xC4: //Z reset
			packet.source = Location::WIDE_IMM;
			packet.dest = Location::PC;
			if (!m_regs.Zero())
			{
				PushWide(m_regs.PC() + 2);
				packet.instruction = Instruction::LOAD;
				m_cycles += 4;
			}
			else
			{
				packet.instruction = Instruction::NOP;
			}
			break;
		case 0xCC: //Z set
			packet.source = Location::WIDE_IMM;
			packet.dest = Location::PC;
			if (m_regs.Zero())
			{
				PushWide(m_regs.PC() + 2);
				packet.instruction = Instruction::LOAD;
				m_cycles += 4;
			}
			else
			{
				packet.instruction = Instruction::NOP;
			}
			break;
		case 0xD4: //C reset
			packet.source = Location::WIDE_IMM;
			packet.dest = Location::PC;
			if (!m_regs.Carry())
			{
				PushWide(m_regs.PC() + 2);
				packet.instruction = Instruction::LOAD;
				m_cycles += 4;
			}
			else
			{
				packet.instruction = Instruction::NOP;
			}
			break;
		case 0xDC: //C set
			packet.source = Location::WIDE_IMM;
			packet.dest = Location::PC;
			if (m_regs.Carry())
			{
				PushWide(m_regs.PC() + 2);
				packet.instruction = Instruction::LOAD;
				m_cycles += 4;
			}
			else
			{
				packet.instruction = Instruction::NOP;
			}
			break;
			//RST (push PC onto stack and jump to 0 + n
		case 0xC7: //00H
		case 0xCF: //08H
		case 0xD7: //10H
		case 0xDF: //18H
		case 0xE7: //20H
		case 0xEF: //28H
		case 0xF7: //30H
		case 0xFF: //38H
			PushPC();
			packet.source = Location::NONE;
			packet.offset = op - 0xC7;
			packet.dest = Location::PC;
			packet.instruction = Instruction::LOAD;
			packet.cycles += 4;
			break;
			//RET
		case 0xC9: //pop two bytes and jump to that address
			packet.source = Location::STACK;
			packet.dest = Location::PC;
			packet.instruction = Instruction::POP;
			m_cycles += 4;
			break;
			//RET IF
		case 0xC8: //Z set
			packet.source = Location::STACK;
			packet.dest = Location::PC;
			if (m_regs.Zero())
			{
				m_cycles += 8;
				packet.instruction = Instruction::POP;
			} else {
				m_cycles += 4;
				packet.instruction = Instruction::NOP;
			}
			break;
		case 0xC0: //Z reset
			packet.source = Location::STACK;
			packet.dest = Location::PC;
			if (!m_regs.Zero())
			{
				m_cycles += 8;
				packet.instruction = Instruction::POP;
			} else {
				m_cycles += 4;
				packet.instruction = Instruction::NOP;
			}
			break;
		case 0xD8: //C set
			packet.source = Location::STACK;
			packet.dest = Location::PC;
			if (m_regs.Carry())
			{
				m_cycles += 8;
				packet.instruction = Instruction::POP;
			} else {
				m_cycles += 4;
				packet.instruction = Instruction::NOP;
			}
			break;
		case 0xD0: //C reset
			packet.source = Location::STACK;
			packet.dest = Location::PC;
			if (!m_regs.Carry())
			{
				m_cycles += 8;
				packet.instruction = Instruction::POP;
			} else {
				m_cycles += 4;
				packet.instruction = Instruction::NOP;
			}
			break;
			//RETI
		case 0xD9: //pop two bytes and jump to that address, then enable interrupts
			packet.source = Location::STACK;
			packet.dest = Location::PC;
			packet.instruction = Instruction::POP;
			m_cycles += 4;
			this->m_interrupt_enable = true;
			break;
		default:
			Logger::RaiseError("CPU", "Unknown Opcode");
			break;
		}
	}
	return packet;
}

void GBCPU::DecodeCB(InstructionPacket &packet)
{
	HandleInterrupts();
	if (!m_halted && ! m_stopped)
	{
		unsigned char op = FetchPC();

		switch (op)
		{
			//SWAP (MSB <-> LSB)
		case 0x30: //B
		case 0x31: //C
		case 0x32: //D
		case 0x33: //E
		case 0x34: //H
		case 0x35: //L
		case 0x36: //MEM(HL)
		case 0x37: //A
			packet.source = RegisterTable(op - 0x30, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::SWAP;
			break;
			//ROT LEFT, bit 7 to carry
		case 0x00: //B
		case 0x01: //C
		case 0x02: //D
		case 0x03: //E
		case 0x04: //H
		case 0x05: //L
		case 0x06: //MEM(HL)
		case 0x07: //A
			packet.source = RegisterTable(op - 0x00, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::RLC;
			break;
			//ROT LEFT through carry flag
		case 0x10: //B
		case 0x11: //C
		case 0x12: //D
		case 0x13: //E
		case 0x14: //H
		case 0x15: //L
		case 0x16: //MEM(HL)
		case 0x17: //A
			packet.source = RegisterTable(op - 0x10, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::RL;
			break;
			//ROT RIGHT, 0 bit to carry
		case 0x08: //B
		case 0x09: //C
		case 0x0A: //D
		case 0x0B: //E
		case 0x0C: //H
		case 0x0D: //L
		case 0x0E: //MEM(HL)
		case 0x0F: //A
			packet.source = RegisterTable(op - 0x08, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::RRC;
			break;
			//ROT RIGHT, through carry
		case 0x18: //B
		case 0x19: //C
		case 0x1A: //D
		case 0x1B: //E
		case 0x1C: //H
		case 0x1D: //L
		case 0x1E: //MEM(HL)
		case 0x1F: //A
			packet.source = RegisterTable(op - 0x18, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::RR;
			break;
			//SLA - Shift left into carry
		case 0x20: //B
		case 0x21: //C
		case 0x22: //D
		case 0x23: //E
		case 0x24: //H
		case 0x25: //L
		case 0x26: //MEM(HL)
		case 0x27: //A
			packet.source = RegisterTable(op - 0x20, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::SL;
			break;
			//SRA - Shift right into carry, replicate MSB
		case 0x28: //B
		case 0x29: //C
		case 0x2A: //D
		case 0x2B: //E
		case 0x2C: //H
		case 0x2D: //L
		case 0x2E: //MEM(HL)
		case 0x2F: //A
			packet.source = RegisterTable(op - 0x28, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::SRS;
			break;
			//SRL - shift righ into carry, set msb to 0
		case 0x38: //B
		case 0x39: //C
		case 0x3A: //D
		case 0x3B: //E
		case 0x3C: //H
		case 0x3D: //L
		case 0x3E: //MEM(HL)
		case 0x3F: //A
			packet.source = RegisterTable(op - 0x38, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::SR;
			break;
			//BIT 0
		case 0x40: //B
		case 0x41: //C
		case 0x42: //D
		case 0x43: //E
		case 0x44: //H
		case 0x45: //L
		case 0x46: //MEM(HL)
		case 0x47: //A
			packet.source = RegisterTable(op - 0x40, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::BIT;
			packet.offset = 0;
			break;
			//BIT 1
		case 0x48: //B
		case 0x49: //C
		case 0x4A: //D
		case 0x4B: //E
		case 0x4C: //H
		case 0x4D: //L
		case 0x4E: //MEM(HL)
		case 0x4F: //A
			packet.source = RegisterTable(op - 0x48, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::BIT;
			packet.offset = 1;
			break;
			//BIT 2
		case 0x50: //B
		case 0x51: //C
		case 0x52: //D
		case 0x53: //E
		case 0x54: //H
		case 0x55: //L
		case 0x56: //MEM(HL)
		case 0x57: //A
			packet.source = RegisterTable(op - 0x50, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::BIT;
			packet.offset = 2;
			break;
			//BIT 3
		case 0x58: //B
		case 0x59: //C
		case 0x5A: //D
		case 0x5B: //E
		case 0x5C: //H
		case 0x5D: //L
		case 0x5E: //MEM(HL)
		case 0x5F: //A
			packet.source = RegisterTable(op - 0x58, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::BIT;
			packet.offset = 3;
			break;
			//BIT 4
		case 0x60: //B
		case 0x61: //C
		case 0x62: //D
		case 0x63: //E
		case 0x64: //H
		case 0x65: //L
		case 0x66: //MEM(HL)
		case 0x67: //A
			packet.source = RegisterTable(op - 0x60, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::BIT;
			packet.offset = 4;
			break;
			//BIT 5
		case 0x68: //B
		case 0x69: //C
		case 0x6A: //D
		case 0x6B: //E
		case 0x6C: //H
		case 0x6D: //L
		case 0x6E: //MEM(HL)
		case 0x6F: //A
			packet.source = RegisterTable(op - 0x68, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::BIT;
			packet.offset = 5;
			break;
			//BIT 6
		case 0x70: //B
		case 0x71: //C
		case 0x72: //D
		case 0x73: //E
		case 0x74: //H
		case 0x75: //L
		case 0x76: //MEM(HL)
		case 0x77: //A
			packet.source = RegisterTable(op - 0x70, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::BIT;
			packet.offset = 6;
			break;
			//BIT 7
		case 0x78: //B
		case 0x79: //C
		case 0x7A: //D
		case 0x7B: //E
		case 0x7C: //H
		case 0x7D: //L
		case 0x7E: //MEM(HL)
		case 0x7F: //A
			packet.source = RegisterTable(op - 0x78, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::BIT;
			packet.offset = 7;
			break;
			//RESET b
			//BIT 0
		case 0x80: //B
		case 0x81: //C
		case 0x82: //D
		case 0x83: //E
		case 0x84: //H
		case 0x85: //L
		case 0x86: //MEM(HL)
		case 0x87: //A
			packet.source = RegisterTable(op - 0x80, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::RES;
			packet.offset = 0;
			break;
			//BIT 1
		case 0x88: //B
		case 0x89: //C
		case 0x8A: //D
		case 0x8B: //E
		case 0x8C: //H
		case 0x8D: //L
		case 0x8E: //MEM(HL)
		case 0x8F: //A
			packet.source = RegisterTable(op - 0x88, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::RES;
			packet.offset = 1;
			break;
			//BIT 2
		case 0x90: //B
		case 0x91: //C
		case 0x92: //D
		case 0x93: //E
		case 0x94: //H
		case 0x95: //L
		case 0x96: //MEM(HL)
		case 0x97: //A
			packet.source = RegisterTable(op - 0x90, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::RES;
			packet.offset = 2;
			break;
			//BIT 3
		case 0x98: //B
		case 0x99: //C
		case 0x9A: //D
		case 0x9B: //E
		case 0x9C: //H
		case 0x9D: //L
		case 0x9E: //MEM(HL)
		case 0x9F: //A
			packet.source = RegisterTable(op - 0x98, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::RES;
			packet.offset = 3;
			break;
			//BIT 4
		case 0xA0: //B
		case 0xA1: //C
		case 0xA2: //D
		case 0xA3: //E
		case 0xA4: //H
		case 0xA5: //L
		case 0xA6: //MEM(HL)
		case 0xA7: //A
			packet.source = RegisterTable(op - 0xA0, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::RES;
			packet.offset = 4;
			break;
			//BIT 5
		case 0xA8: //B
		case 0xA9: //C
		case 0xAA: //D
		case 0xAB: //E
		case 0xAC: //H
		case 0xAD: //L
		case 0xAE: //MEM(HL)
		case 0xAF: //A
			packet.source = RegisterTable(op - 0xA8, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::RES;
			packet.offset = 5;
			break;
			//BIT 6
		case 0xB0: //B
		case 0xB1: //C
		case 0xB2: //D
		case 0xB3: //E
		case 0xB4: //H
		case 0xB5: //L
		case 0xB6: //MEM(HL)
		case 0xB7: //A
			packet.source = RegisterTable(op - 0xB0, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::RES;
			packet.offset = 6;
			break;
			//BIT 7
		case 0xB8: //B
		case 0xB9: //C
		case 0xBA: //D
		case 0xBB: //E
		case 0xBC: //H
		case 0xBD: //L
		case 0xBE: //MEM(HL)
		case 0xBF: //A
			packet.source = RegisterTable(op - 0xB8, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::RES;
			packet.offset = 7;
			break;
			//SET 
			//BIT 0
		case 0xC0: //B
		case 0xC1: //C
		case 0xC2: //D
		case 0xC3: //E
		case 0xC4: //H
		case 0xC5: //L
		case 0xC6: //MEM(HL)
		case 0xC7: //A
			packet.source = RegisterTable(op - 0xC0, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::SET;
			packet.offset = 0;
			break;
			//BIT 1
		case 0xC8: //B
		case 0xC9: //C
		case 0xCA: //D
		case 0xCB: //E
		case 0xCC: //H
		case 0xCD: //L
		case 0xCE: //MEM(HL)
		case 0xCF: //A
			packet.source = RegisterTable(op - 0xC8, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::SET;
			packet.offset = 1;
			break;
			//BIT 2
		case 0xD0: //B
		case 0xD1: //C
		case 0xD2: //D
		case 0xD3: //E
		case 0xD4: //H
		case 0xD5: //L
		case 0xD6: //MEM(HL)
		case 0xD7: //A
			packet.source = RegisterTable(op - 0xD0, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::SET;
			packet.offset = 2;
			break;
			//BIT 3
		case 0xD8: //B
		case 0xD9: //C
		case 0xDA: //D
		case 0xDB: //E
		case 0xDC: //H
		case 0xDD: //L
		case 0xDE: //MEM(HL)
		case 0xDF: //A
			packet.source = RegisterTable(op - 0xD8, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::SET;
			packet.offset = 3;
			break;
			//BIT 4
		case 0xE0: //B
		case 0xE1: //C
		case 0xE2: //D
		case 0xE3: //E
		case 0xE4: //H
		case 0xE5: //L
		case 0xE6: //MEM(HL)
		case 0xE7: //A
			packet.source = RegisterTable(op - 0xE0, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::SET;
			packet.offset = 4;
			break;
			//BIT 5
		case 0xE8: //B
		case 0xE9: //C
		case 0xEA: //D
		case 0xEB: //E
		case 0xEC: //H
		case 0xED: //L
		case 0xEE: //MEM(HL)
		case 0xEF: //A
			packet.source = RegisterTable(op - 0xE8, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::SET;
			packet.offset = 5;
			break;
			//BIT 6
		case 0xF0: //B
		case 0xF1: //C
		case 0xF2: //D
		case 0xF3: //E
		case 0xF4: //H
		case 0xF5: //L
		case 0xF6: //MEM(HL)
		case 0xF7: //A
			packet.source = RegisterTable(op - 0xF0, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::SET;
			packet.offset = 6;
			break;
			//BIT 7
		case 0xF8: //B
		case 0xF9: //C
		case 0xFA: //D
		case 0xFB: //E
		case 0xFC: //H
		case 0xFD: //L
		case 0xFE: //MEM(HL)
		case 0xFF: //A
			packet.source = RegisterTable(op - 0xF8, packet);
			packet.dest = packet.source;
			packet.instruction = Instruction::SET;
			packet.offset = 7;
			break;
		default:
			Logger::RaiseError("CPU", "Unknown CB code");
			break;
		}
	}
}

unsigned char GBCPU::FetchPC()
{
	unsigned char byte = m_mem->Read(m_regs.PC());
	m_regs.IncPC();
	m_cycles += 4;
#ifdef _DEBUG
	std::cout << "FetchPC returned 0x" << std::hex << (int)byte << std::endl;
#endif
	return byte;
}

unsigned short GBCPU::FetchPC16()
{
	//Memory stored in Low, High Order
	unsigned short bytes = FetchPC();
	bytes |= (((unsigned short)FetchPC()) << 8);
#ifdef _DEBUG
	std::cout << "FetchPC16 returned 0x" << std::hex << (int)bytes << std::endl;
#endif
	return bytes;
}

unsigned char GBCPU::ReadMem(unsigned short addr)
{
	m_cycles += 4;
	return m_mem->Read(addr);
}

void GBCPU::WriteMem(unsigned short addr, unsigned char value)
{
	m_cycles += 4;
	m_mem->Write(addr, value);
}

Location GBCPU::RegisterTable(unsigned char index, InstructionPacket &packet)
{
	switch (index)
	{
	case 0:
		return Location::B;
		break;
	case 1:
		return Location::C;
		break;
	case 2:
		return Location::D;
		break;
	case 3:
		return Location::E;
		break;
	case 4:
		return Location::H;
		break;
	case 5:
		return Location::L;
		break;
	case 6:
		packet.address = m_regs.HL();
		return Location::MEM;
		break;
	case 7:
		return Location::A;
		break;
	default:
		Logger::RaiseError("GBCPU", "Unknown location table index");
		break;
	}
	return Location::NONE;
}

int GBCPU::ReadLocation(Location l, InstructionPacket &packet)
{
	unsigned int value = 0;
	switch (l)
	{
	case Location::NONE:
		value = 0;
		break;
	case Location::A:
		value = m_regs.A();
		break;
	case Location::B:
		value = m_regs.B();
		break;
	case Location::C:
		value = m_regs.C();
		break;
	case Location::D:
		value = m_regs.D();
		break;
	case Location::E:
		value = m_regs.E();
		break;
	case Location::H:
		value = m_regs.H();
		break;
	case Location::L:
		value = m_regs.L();
		break;
	case Location::F:
		value = m_regs.F();
		break;
	case Location::AF:
		value = m_regs.AF();
		break;
	case Location::BC:
		value = m_regs.BC();
		break;
	case Location::DE:
		value = m_regs.DE();
		break;
	case Location::HL:
		value = m_regs.HL();
		break;
	case Location::SP:
		value = m_regs.SP();
		break;
	case Location::PC:
		value = m_regs.PC();
		break;
	case Location::MEM:
		value = ReadMem(packet.address);
		break;
	case Location::IMM:
		value = FetchPC();
		break;
	case Location::WIDE_IMM:
		value = FetchPC16();
		break;
	case Location::PORT:
	case Location::STACK:
		break;
	default:
		Logger::RaiseError("GBCPU", "Attempted read from unknown location");
		break;
	}
	return value;
}

void GBCPU::WriteLocation(Location l, InstructionPacket &packet, int value)
{
#ifdef _DEBUG
	std::cout << "Writing location " << location_names[packet.dest] << " with " << std::hex << value << std::endl;
#endif
	switch (packet.dest)
	{
	case Location::A:
		m_regs.A(value);
		break;
	case Location::B:
		m_regs.B(value);
		break;
	case Location::C:
		m_regs.C(value);
		break;
	case Location::D:
		m_regs.D(value);
		break;
	case Location::E:
		m_regs.E(value);
		break;
	case Location::H:
		m_regs.H(value);
		break;
	case Location::L:
		m_regs.L(value);
		break;
	case Location::F:
		m_regs.F(value);
		break;
	case Location::AF:
		m_regs.AF(value);
		break;
	case Location::BC:
		m_regs.BC(value);
		break;
	case Location::DE:
		m_regs.DE(value);
		break;
	case Location::HL:
		m_regs.HL(value);
		break;
	case Location::SP:
		m_regs.SP(value);
		break;
	case Location::PC:
		m_regs.PC(value);
		break;
	case Location::WIDE_MEM:
		WriteMem(packet.address + packet.addr_offset, (value >> 8) & 0xFF);
	case Location::MEM:
		WriteMem(packet.address, value & 0xFF);
		break;
	case Location::IMM:
	case Location::WIDE_IMM:
	case Location::PORT:
	case Location::STACK:
	default:
		Logger::RaiseError("GBCPU", "Attempted write to unknown location");
		break;
	}
}


void GBCPU::ExecuteInstruction(InstructionPacket& packet)
{
	
	switch (packet.instruction)
	{
	case Instruction::NON:
		//Consistent with a stopped or halted processor
		break;
	case Instruction::NOP:
		ReadLocation(packet.source, packet);
		break;
	case Instruction::LOAD:
		{
			int val = ReadLocation(packet.source, packet);
			
			if (packet.offset != 0)
			{
				int res = val + packet.offset;
				if (packet.offset == 1)
				{
					//INC
					if (!packet.IsDestWide())
					{
						//SET Z if zero
						//Reset N 
						//Set H if carry from 3
						//Do not change C
						m_regs.SetFlags((res & 0xFF) == 0, false, HalfCarry(val, 1), m_regs.Carry());
					} else {
						packet.cycles += 4;
					}
				}
				else if (packet.offset == -1)
				{
					//DEC
					if(!packet.IsDestWide())
					{
						//SET Z if zero
						//Set N 
						//Set H if carry from 3
						//Do not change C
						m_regs.SetFlags((res & 0xFF) == 0, true, !HalfBorrow(val, 1), m_regs.Carry());
					} else {
						packet.cycles += 4;
					}
				}
				WriteLocation(packet.dest, packet, res);
			}
			else
			{
				WriteLocation(packet.dest, packet, val);
			}
			
		}
		break;
	case Instruction::ADD:
		{
			int a = ReadLocation(packet.source, packet);
			int b = ReadLocation(packet.dest, packet);
			int res = a + b + packet.offset;

			//SET Z if zero
			//Reset N
			//Set H if carry from 3
			//Set C if carry from 7
			m_regs.SetFlags((res & 0xFF) == 0, false, HalfCarry(a, b), Carry8b(a, b));
			WriteLocation(packet.dest, packet, res);
		}
		break;
	case Instruction::SUB:
		{
			int a = ReadLocation(packet.source, packet);
			int b = ReadLocation(packet.dest, packet);
			int res = b - (a + packet.offset);
			//SET Z if zero
			//Set N
			//Set H if no borrow from 4
			//Set C if no borrow
			m_regs.SetFlags((res & 0xFF) == 0, true, !HalfBorrow(b, a), !Borrow8b(b, a));
			WriteLocation(packet.dest, packet, res);
		}
		break;
	case Instruction::STOP:
		m_stopped = true;
		break;
	case Instruction::HALT:
		//PC will point at HALT
		//We must advance to store the instruction after HALT
		//If interrupts are not enabled, we eat the instruction
		m_regs.IncPC();
		if (m_interrupt_enable) {
			m_halted = true;
		}
		break;
	case Instruction::AND:
		{
			int a = ReadLocation(packet.source, packet);
			int b = ReadLocation(packet.dest, packet);
			int res = a & b;
			//SET Z if zero
			//Reset N
			//Set H
			//Reset C
			m_regs.SetFlags(res == 0, false, true, false);
			WriteLocation(packet.dest, packet, res);
		}
		break;
	case Instruction::OR:
		{
			int a = ReadLocation(packet.source, packet);
			int b = ReadLocation(packet.dest, packet);
			int res = a | b;
			//SET Z if zero
			//Reset N
			//Reset H
			//Reset C
			m_regs.SetFlags(res == 0, false, false, false);
			WriteLocation(packet.dest, packet, res);
		}
		break;
	case Instruction::XOR:
		{
			int a = ReadLocation(packet.source, packet);
			int b = ReadLocation(packet.dest, packet);
			int res = a ^ b;
			//SET Z if zero
			//Reset N
			//Reset H
			//Reset C
			m_regs.SetFlags(res == 0, false, false, false);
			WriteLocation(packet.dest, packet, res);
		}
		break;
	case Instruction::CP:
		{
			int a = ReadLocation(packet.source, packet);
			int b = ReadLocation(packet.dest, packet);
			int res = a - b;
			//SET Z if zero
			//Set N
			//Set H if no borrow from but 4
			//Set for no borrow (A < n)
			m_regs.SetFlags((res & 0xFF) == 0, true, !HalfBorrow(b, a), !Borrow8b(b, a));
			//No writeback
		}
		break;
	case Instruction::RLC:
		{
			//Rotate Left, not including carry flag
			unsigned char r = ReadLocation(packet.dest, packet);
			bool carry = r & 0x80;
			r = r << 1;
			if (carry) {
				r += 1;
			}
			m_regs.SetFlags(r == 0, false, false, carry);
			WriteLocation(packet.dest, packet, r);
		}
		break;
	case Instruction::RL:
		{
			//Rotate Left, including carry flag
			unsigned char r = ReadLocation(packet.dest, packet);
			bool carry = r & 0x80;
			r = r << 1;
			if (m_regs.Carry()) {
				r += 1;
			}
			m_regs.SetFlags(r == 0, false, false, carry);
			WriteLocation(packet.dest, packet, r);
		}
		break;
	case Instruction::RRC:
		{
			//Rotate Right, not including carry flag
			unsigned char r = ReadLocation(packet.dest, packet);
			bool carry = r & 0x01;
			r = r >> 1;
			if (carry) {
				r += 0x80;
			}
			m_regs.SetFlags(r == 0, false, false, carry);
			WriteLocation(packet.dest, packet, r);
		}
		break;
	case Instruction::RR:
		{
			//Rotate right, including carry flag
			unsigned char r = ReadLocation(packet.dest, packet);
			bool carry = r & 0x01;
			r = r >> 1;
			if (m_regs.Carry()) r += 0x80;
			m_regs.SetFlags(r == 0, false, false, carry);
			WriteLocation(packet.dest, packet, r);
		}
		break;
	case Instruction::SRS: //Shift right signed
		{
			unsigned char r = ReadLocation(packet.dest, packet);
			//Copy out msb & lsb
			bool rmsb = (r >> 7) == 1;
			bool rlsb = r & 0x1;
			//Shift right
			r = r >> 1;
			if (rmsb)
			{
				//SET
				r |= 0x80;
			}
			else
			{
				//RESET
				r &= 0x7F;
			}
			//Set flags
			WriteLocation(packet.dest, packet, r);
			m_regs.SetFlags(r == 0, false, false, rlsb);
		}
		break;
	case Instruction::SL:
		{
			unsigned char r = ReadLocation(packet.dest, packet);
			//Copy out msb
			bool rmsb = r >> 7 & 0x1;
			//Shift left
			r = r << 1;
			//Set flags
			WriteLocation(packet.dest, packet, r);
			m_regs.SetFlags(r == 0, false, false, rmsb);
		}
		break;
	case Instruction::SR:
		{
			unsigned char r = ReadLocation(packet.dest, packet);
			//Copy out msb & lsb
			bool rmsb = (r >> 7) == 1;
			bool rlsb = r & 0x1;
			//Shift right
			r = r >> 1;
			r &= 0x7F;
			//Set flags
			WriteLocation(packet.dest, packet, r);
			m_regs.SetFlags(r == 0, false, false, rlsb);
		}
		break;
	case Instruction::DAA:
		{	
			unsigned short a = m_regs.A();
			if (!m_regs.Subtract()) {
				if (m_regs.Half() || ((a & 0xF) > 9)) {
					a += 0x06;
				}

				if (m_regs.Carry() || (a > 0x9F)) {
					a += 0x60;
				}
			}
			else {
				if (m_regs.Half()) {
					a = (a - 6) & 0xFF;
				}

				if (m_regs.Carry()) {
					a -= 0x60;
				}
			}

			m_regs.SetFlags(a == 0, m_regs.Subtract(), false, (a > 0x100));
			m_regs.A(a & 0xFF);
		}
		break;
	case Instruction::CPL:
		{
			m_regs.A(~m_regs.A());
			m_regs.SetFlags(m_regs.Zero(), true, true, m_regs.Carry());
		}
		break;
	case Instruction::SCF:
		{
			m_regs.SetFlags(m_regs.Zero(), false, false, true);
		}
		break;
	case Instruction::CCF:
		{
			m_regs.SetFlags(m_regs.Zero(), false, false, !m_regs.Carry());
		}
		break;
	case Instruction::POP:
		{
			unsigned short val;
			PopWide(val);
			WriteLocation(packet.dest, packet, val);
		}
		break;
	case Instruction::PUSH:
		{
			unsigned short val = ReadLocation(packet.source, packet) + packet.offset;
			PushWide(val);
		}
		break;
	case Instruction::DI:
		{
			m_interrupt_enable = false;
		}
		break;
	case Instruction::EI:
		{
			m_interrupt_enable = true;
		}
		break;
	case Instruction::BIT:
		{
			unsigned char val = ReadLocation(packet.source, packet);
			val &= (1 << packet.offset);
			if (packet.source == Location::MEM) {
				// Shadow writeback?
				packet.cycles += 4;
			}
			m_regs.SetFlags(val == 0, false, true, m_regs.Carry());
		}
		break;
	case Instruction::RES:
		{
			unsigned char val = ReadLocation(packet.source, packet);
			val &= ~(1 << packet.offset);
			WriteLocation(packet.dest, packet, val);
		}
		break;
	case Instruction::SET:
		{
			unsigned char val = ReadLocation(packet.source, packet);
			val |= (1 << packet.offset);
			WriteLocation(packet.dest, packet, val);
		}
		break;
	case Instruction::SWAP:
		{
			unsigned char val = ReadLocation(packet.source, packet);
			unsigned char tmp = val >> 4;
			val = (val << 4) | tmp;
			WriteLocation(packet.dest, packet, val);
			m_regs.SetFlags(val == 0, false, false, false);
		}
		break;
	default:
		Logger::RaiseError("GBCPU", "Unknown instruction encountered");
		break;
	}
	m_cycles += packet.cycles;
};


GBCPU::GBCPU()
{
	m_mem = new Memory();
	m_mem->SetInterruptTarget(this);
	m_cycles = 0;
	m_stopped = true;
	m_halted = true;
}

GBCPU::~GBCPU()
{
	delete m_mem;
}

void GBCPU::Start(bool use_bios)
{
	//Ignore startup sequence (we don't need no stinkin BIOS!)
	if (!use_bios) {
		m_regs.PC(0x100);
		m_regs.AF(0x01); // CODE - Gameboy
		m_regs.F(0xB0);
		m_regs.BC(0x0013);
		m_regs.DE(0x00D8);
		m_regs.HL(0x014D);
		m_regs.SP(0xFFFE);
		m_mem->TIMA(0x00);
		m_mem->TMA(0x00);
		m_mem->TAC(0x00);
		m_mem->NR10(0x80);
		m_mem->NR11(0xBF);
		m_mem->NR12(0xF3);
		m_mem->NR14(0xBF);
		m_mem->NR21(0x3F);
		m_mem->NR22(0x00);
		m_mem->NR24(0xBF);
		m_mem->NR30(0x7F);
		m_mem->NR31(0xFF);
		m_mem->NR32(0x9F);
		m_mem->NR33(0xBF);
		m_mem->NR41(0xFF);
		m_mem->NR42(0x00);
		m_mem->NR43(0x00);
		m_mem->NR44(0xBF);
		m_mem->NR50(0x77);
		m_mem->NR51(0xF3);
		m_mem->NR52(0xF1);
		m_mem->LCDC(0x91);
		m_mem->SCY(0x00);
		m_mem->SCX(0x00);
		m_mem->LYC(0x00);
		m_mem->BGP(0xFC);
		m_mem->OBP0(0xFF);
		m_mem->OBP1(0xFF);
		m_mem->WY(0x00);
		m_mem->WX(0x00);
	}

	m_mem->IE(0x00);
	m_mem->IF(0);
	m_halted = false;
	m_stopped = false;
}

unsigned long GBCPU::Step()
{
	unsigned long start_cycles = m_cycles;
	InstructionPacket packet = DecodeInstruction();
	ExecuteInstruction(packet);
	m_mem->Step();
	return m_cycles - start_cycles;
}

void GBCPU::Int(Interrupt int_code)
{
	m_mem->IF(m_mem->IF() | int_code);
	if (int_code == INPUT_INT) {
		m_stopped = false;
	}
}

void GBCPU::HandleInterrupts()
{
	int int_code;

	if (!this->m_interrupt_enable || m_mem->IF() == 0) {
		return;
	}

	for(int_code = VBLANK_INT; int_code < MAX_INT; int_code <<= 1)
	{
		if ((m_mem->IE() & int_code) && (m_mem->IF() & int_code))
		{
			DoInterrupt(int_code);
			return;
		}
	}
}

void GBCPU::DoInterrupt(int int_code) {
	this->m_halted = false;
	this->m_interrupt_enable = false;
	this->PushPC();
	m_mem->IF(0);
	switch (int_code)
	{
	case VBLANK_INT:
		m_regs.PC(VBLANK_ROUTINE);
		break;
	case LCDC_INT:
		m_regs.PC(LCDC_ROUTINE);
		break;
	case TIME_INT:
		m_regs.PC(TIMER_ROUTINE);
		break;
	case SERIAL_INT:
		m_regs.PC(SERIAL_ROUTINE);
		break;
	case INPUT_INT:
		m_regs.PC(INPUT_ROUTINE);
		break;
	default:
		break;
	}
}

Memory *GBCPU::GetMem()
{
	return m_mem;
}

RegFile *GBCPU::GetRegs()
{
	return &m_regs;
}

void GBCPU::LoadGBFile(std::string filename)
{
	m_mem->LoadCartFromFile(filename);
}

void GBCPU::SetRom(unsigned char *rom, unsigned int size)
{
	m_mem->SetCartFromBytes(rom, size);
}

unsigned long GBCPU::GetCycles()
{
	return m_cycles;
}

bool GBCPU::GetInterruptFlag()
{
	return m_interrupt_enable;
}

bool GBCPU::Stopped() {
	return m_stopped;
}
