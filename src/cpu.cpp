#include "cpu.h"


Location CPU::MapLocation(unsigned char offset)
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

void CPU::DecodeInstruction()
{
	unsigned char op = FetchPC();
	
	m_cycles = 0;
	m_source = Location::NONE;
	m_dest = Location::NONE;
	m_immediate = 0;
	m_address = 0;
	bool address_locked = false;
	m_offset = 0;
	m_instruction = Instruction::NONE;

	switch (op)
	{
		//LOAD IMM -> REG
	case 0x06: //B
	case 0x0E: //C
	case 0x16: //D
	case 0x1E: //E
	case 0x26: //H
	case 0x2E: //L
		break;
		//LOAD REG -> REG
		//LOAD INTO A
	case 0x7F: //A
	case 0x78: //B
	case 0x79: //C
	case 0x7A: //D
	case 0x7B: //E
	case 0x7C: //H
	case 0x7D: //L
	case 0x7E: //MEM(HL)
	case 0x0A: //MEM(BC)
	case 0x1A: //MEM(DE)
	case 0xFA: //MEM(WIDE IMM LS -> MS)
	case 0x3E: //IMM
		//LOAD INTO B
	case 0x40: //B
	case 0x41:
	case 0x42:
	case 0x43:
	case 0x44:
	case 0x45:
	case 0x46: //MEM(HL)
	case 0x47: //A
		//LOAD INTO C
	case 0x48: //B
	case 0x49:
	case 0x4A:
	case 0x4B:
	case 0x4C:
	case 0x4D:
	case 0x4E: //MEM(HL)
	case 0x4F: //A
		//LOAD INTO D
	case 0x50: //B
	case 0x51:
	case 0x52:
	case 0x53:
	case 0x54:
	case 0x55:
	case 0x56: //MEM(HL)
	case 0x57: //A
		//LOAD INTO E
	case 0x58: //B
	case 0x59:
	case 0x5A:
	case 0x5B:
	case 0x5C:
	case 0x5D:
	case 0x5E: //MEM(HL)
	case 0x5F: //A
		//LOAD INTO H
	case 0x60: //B
	case 0x61:
	case 0x62:
	case 0x63:
	case 0x64:
	case 0x65:
	case 0x66: //MEM(HL)
	case 0x67: //A
		//LOAD INTO L
	case 0x68: //B
	case 0x69:
	case 0x6A:
	case 0x6B:
	case 0x6C:
	case 0x6D:
	case 0x6E: //MEM(HL)
	case 0x6F: //A
		//LOAD INTO MEM(HL)
	case 0x70: //B
	case 0x71:
	case 0x72:
	case 0x73:
	case 0x74:
	case 0x75: //L
	case 0x36: //IMM
		//MEM(BC)
	case 0x02: //A
		//MEM(DE)
	case 0x12: //A
		//MEM(HL)
	case 0x77: //A
		//MEM(WIDE IMM LS->MS)
	case 0xEA: //A
		//LOAD INTO A
	case 0xF2: //MEM(0xFF00 + C)
		//LOAD INTO MEM(0xFF+C)
	case 0xE2: //A
		//LOAD AND DEC
		//LOAD A
	case 0x3A: //MEM(HL--)
		//LOAD MEM(HL--)
	case 0x32: //A
		//LOAD AND INC
		//A
	case 0x2A: //MEM(HL++)
		//MEM(HL++)
	case 0x22: //A
		//LOAD MEM(0xFF00+IMM)
	case 0xE0: //A
		//LOAD A
	case 0xF0: //MEM(0xFF+IMM)
		//WIDE LOAD
		//BC
	case 0x01: //WIDE-IMM
		//DE
	case 0x11: //WIDE-IMM
		//HL
	case 0x21: //WIDE-IMM
		//SP
	case 0x31: //WIDE-IMM
		//LOAD SP
	case 0xF9: //HL
		//LOAD HL
	case 0xF8: //SP + n: reset zero, reset nonzero may set h, may set c
		//LOAD SP
	case 0x08: //WIDE-IMM


		//PUSH -> SP - 2
	case 0xF5: //AF
	case 0xC5: //BC
	case 0xD5: //DE
	case 0xE5: //HL
		//POP
	case 0xF1: //AF
	case 0xC1: //BC
	case 0xD1: //DE
	case 0xE1: //HL
		//ADD to A, set zero if zero, reset nonzero, set h if carry bit 3, set c if carry from 7
	case 0x87: //A
	case 0x80: //B
	case 0x81: //C
	case 0x82: //D
	case 0x83: //E
	case 0x84: //H
	case 0x85: //L
	case 0x86: //MEM(HL)
	case 0xC6: //IMM
		//ADC -> Add X plus carry to A
	case 0x8F: //A
	case 0x88: //B
	case 0x89: //C
	case 0x8A: //D
	case 0x8B: //E
	case 0x8C: //H
	case 0x8D: //L
	case 0x8E: //MEM(HL)
	case 0xCE: //IMM
		//SUB from A
	case 0x97: //A
	case 0x90: //B
	case 0x91: //C
	case 0x92: //D
	case 0x93: //E
	case 0x94: //H
	case 0x95: //L
	case 0x96: //MEM(HL)
	case 0xD6: //IMM
		//SBC -> Sub X plus carry from A
	case 0x9F: //A
	case 0x98: //B
	case 0x99: //C
	case 0x9A: //D
	case 0x9B: //E
	case 0x9C: //H
	case 0x9D: //L
	case 0x9E: //MEM(HL)
	//case 0x?? //IMM
		//AND with A
	case 0xA7: //A
	case 0xA0: //B
	case 0xA1: //C
	case 0xA2: //D
	case 0xA3: //E
	case 0xA4: //H
	case 0xA5: //L
	case 0xA6: //MEM(HL)
	case 0xE6: //IMM
		//OR with A
	case 0xB7: //A
	case 0xB0: //B
	case 0xB1: //C
	case 0xB2: //D
	case 0xB3: //E
	case 0xB4: //H
	case 0xB5: //L
	case 0xB6: //MEM(HL)
	case 0xF6: //IMM
		//XOR with A
	case 0xAF: //A
	case 0xA8: //B
	case 0xA9: //C
	case 0xAA: //D
	case 0xAB: //E
	case 0xAC: //H
	case 0xAD: //L
	case 0xAE: //MEM(HL)
	case 0xEE: //IMM
		//COMPARE
	case 0xBF: //A
	case 0xB8: //B
	case 0xB9: //C
	case 0xBA: //D
	case 0xBB: //E
	case 0xBC: //H
	case 0xBD: //L
	case 0xBE: //MEM(HL)
	case 0xFE: //IMM
		//INC
	case 0x3C: //A
	case 0x04: //B
	case 0x0C: //C
	case 0x14: //D
	case 0x1C: //E
	case 0x24: //H
	case 0x2C: //L
	case 0x34: //MEM(HL)
		//DEC
	case 0x3D: //A
	case 0x05: //B
	case 0x0D: //C
	case 0x15: //D
	case 0x1D: //E
	case 0x25: //H
	case 0x2D: //L
	case 0x35: //MEM(HL)
		//ADD 16
		//ADD TO HL
	case 0x09: //BC
	case 0x19: //DE
	case 0x29: //HL
	case 0x39: //SP
		//ADD SP
	case 0xE8: //IMM
		//INC16
	case 0x03: //BC
	case 0x13: //DE
	case 0x23: //HL
	case 0x33: //SP
		//DEC16
	case 0x0B: //BC
	case 0x1B: //DE
	case 0x2B: //HL
	case 0x3B: //SP
		//CB EXT
	case 0xCB:
		DecodeCB();
		//DAA
	case 0x27: //Decimal Adjust A
		//CPL
	case 0x2F: //A
		//CCF 
	case 0x3F: //Complement carry flag
		//SCF
	case 0x37: //Set carry flag
		//NOP
	case 0x00: //NOP
		//HALT
	case 0x76: //Power down CPU until interrupt
		//STOP
	case 0x10:
		if (FetchPC() == 0)
			;//STOP
		//DI
	case 0xF3: //Disable interrupt after exec
		//EI
	case 0xFB: //Enable interrupt after exec
		//ROT LEFT
	case 0x07: //Rotate A left, 0 to carry
	case 0x17: //Rotate A left through carry flag
		//ROT RIGHT
	case 0x0F: //Rotate A right, 0 to carry
	case 0x1F: //Rotate A right through carry flag
		//JUMP
	case 0xC3: //WIDE-IMM
	case 0xE9: //MEM(HL)
		//JUMP IF
	case 0xC2: //Z reset
	case 0xCA: //Z set
	case 0xD2: //C reset (no carry)
	case 0xDA: //C set (carry)
		//JUMP REL
	case 0x18: //signed IMM
		//JUMP REL IF
	case 0x20: //Z reset, IMM signed
	case 0x28: //Z set. IMM signed
	case 0x30: //C reset, IMM signed
	case 0x38: //C set, IMM signed
		//CALL
	case 0xCD: //WIDE-IMM
		//CALL IF
	case 0xC4: //Z reset
	case 0xCC: //Z set
	case 0xD4: //C reset
	case 0xDC: //C set
		//RST (push PC onto stack and jump to 0 + n
	case 0xC7: //00H
	case 0xCF: //08H
	case 0xD7: //10H
	case 0xDF: //18H
	case 0xE7: //20H
	case 0xEF: //28H
	case 0xF7: //30H
	case 0xFF: //38H
		//RET
	case 0xC9: //pop two bytes and jump to that address
		//RET IF
	case 0xC0: //Z set
	case 0xC8: //Z reset
	case 0xD0: //C set
	case 0xD8: //C reset
		//RETI
	case 0xD9: //pop two bytes and dump to that address, then enable interrupts


	}
}
void CPU::DecodeCB()
{
	unsigned char op = FetchPC();
	unsigned char x = (op >> 6) & 0x3;
	unsigned char y = (op >> 3) & 0x7;
	unsigned char z = (op & 0x7);

	m_source = RegisterTable(z);
	m_dest = m_source;
	switch (op)
	{
		//SWAP (MSB <-> LSB)
	case 0x37: //A
	case 0x30: //B
	case 0x31: //C
	case 0x32: //D
	case 0x33: //E
	case 0x34: //H
	case 0x35: //L
	case 0x36: //MEM(HL)
		//ROT LEFT, bit 7 to carry
	case 0x07: //A
	case 0x00: //B
	case 0x01: //C
	case 0x02: //D
	case 0x03: //E
	case 0x04: //H
	case 0x05: //L
	case 0x06: //MEM(HL)
		//ROT LEFT through carry flag
	case 0x17: //A
	case 0x10: //B
	case 0x11: //C
	case 0x12: //D
	case 0x13: //E
	case 0x14: //H
	case 0x15: //L
	case 0x16: //MEM(HL)
		//ROT RIGHT, 0 bit to carry
	case 0x0F: //A
	case 0x08: //B
	case 0x09: //C
	case 0x0A: //D
	case 0x0B: //E
	case 0x0C: //H
	case 0x0D: //L
	case 0x0E: //MEM(HL)
		//ROT RIGHT, through carry
	case 0x1F: //A
	case 0x18: //B
	case 0x19: //C
	case 0x1A: //D
	case 0x1B: //E
	case 0x1C: //H
	case 0x1D: //L
	case 0x1E: //MEM(HL)
		//SLA - Shift left into carry
	case 0x27: //A
	case 0x20: //B
	case 0x21: //C
	case 0x22: //D
	case 0x23: //E
	case 0x24: //H
	case 0x25: //L
	case 0x26: //MEM(HL)
		//SRA - Shift right into carry, replicate MSB
	case 0x2F: //A
	case 0x28: //B
	case 0x29: //C
	case 0x2A: //D
	case 0x2B: //E
	case 0x2C: //H
	case 0x2D: //L
	case 0x2E: //MEM(HL)
		//SRL - shift righ into carry, set msb to 0
	case 0x3F: //A
	case 0x38: //B
	case 0x39: //C
	case 0x3A: //D
	case 0x3B: //E
	case 0x3C: //H
	case 0x3D: //L
	case 0x3E: //MEM(HL)
		//BIT 0
	case 0x47: //A
	case 0x40: //B
	case 0x41: //C
	case 0x42: //D
	case 0x43: //E
	case 0x44: //H
	case 0x45: //L
	case 0x46: //MEM(HL)
		//BIT 1
	case 0x48: //B
		//BIT 2
		//BIT 3
		//BIT 4
		//BIT 5
		//BIT 6
		//BIT 7
		//SET b
	case 0xC7: //A
	case 0xC0: //B
	case 0xC1: //C
	case 0xC2: //D
	case 0xC3: //E
	case 0xC4: //H
	case 0xC5: //L
	case 0xC6: //MEM(HL)
		//RESET b
	case 0x87: //A
	case 0x80: //B
	case 0x81: //C
	case 0x82: //D
	case 0x83: //E
	case 0x84: //H
	case 0x85: //L
	case 0x86: //MEM(HL)


	}


	switch (x)
	{
	case 0:
		m_instruction = BitwiseTable(y);
		break;
	case 1:
		m_instruction = Instruction::BIT;
		m_immediate = y;
		break;
	case 2:
		m_instruction = Instruction::RES;
		m_immediate = y;
		break;
	case 3:
		m_instruction = Instruction::SET;
		m_immediate = y;
		break;
	default:
		Logger::RaiseError("CPU", "Invalid x value");
		m_instruction = Instruction::NONE;
		break;
	}
}

unsigned char CPU::FetchPC()
{
	unsigned char byte = m_ram.Read(m_regs.PC());
	m_regs.IncPC();
	return byte;
}

unsigned short CPU::FetchPC16()
{
	unsigned short bytes = FetchPC();
	bytes = (FetchPC() << 8) + bytes;
	return bytes;
}

unsigned char CPU::RegisterTable(unsigned char index)
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
		m_address = m_regs.HL();
		return Location::MEM;
		break;
	case 7:
		return Location::A;
		break;
	default:
		Logger::RaiseError("CPU", "Unknown location table index");
		break;
	}
	return Location::NONE;
}

Location CPU::WideRegisterTableSP(unsigned char index)
{
	switch (index)
	{
	case 0:
		return Location::BC;
		break;
	case 1:
		return Location::DE;
		break;
	case 2:
		return Location::HL;
		break;
	case 3:
		return Location::SP;
		break;
	default:
		Logger::RaiseError("CPU", "Unknown wide location table index");
		break;
	}
	return Location::NONE;
}

Location CPU::WideRegisterTableAF(unsigned char index)
{
	switch (index)
	{
	case 0:
		return Location::BC;
		break;
	case 1:
		return Location::DE;
		break;
	case 2:
		return Location::HL;
		break;
	case 3:
		return Location::AF;
		break;
	default:
		Logger::RaiseError("CPU", "Unknown wide location table index");
		break;
	}
	return Location::NONE;
}

Condition CPU::ConditionTable(unsigned char index)
{
	switch (index)
	{
	case 0:
		return Condition::NonZero;
		break;
	case 1:
		return Condition::Zero;
		break;
	case 2:
		return Condition::NoCarry;
		break;
	case 3:
		return Condition::Carry;
		break;
	case 4:
	case 5:
	case 6:
	case 7:
		Logger::RaiseError("CPU", "Unimplemented condition table index");
		break;
	default:
		Logger::RaiseError("CPU", "Unknown condition table index");
		break;
	}
	return Condition::NONE;
}

Instruction CPU::AluTable(unsigned char index)
{
	switch (index)
	{
	case 0:
		return Instruction::ADD;
		break;
	case 1:
		return Instruction::ADC;
		break;
	case 2:
		return Instruction::SUB;
		break;
	case 3:
		return Instruction::SBC;
		break;
	case 4:
		return Instruction::AND;
		break;
	case 5:
		return Instruction::XOR;
		break;
	case 6:
		return Instruction::OR;
		break;
	case 7:
		return Instruction::CP;
		break;
	default:
		Logger::RaiseError("CPU", "Unknown ALU op table index");
		break;
	}
	return Instruction::NONE;
}

BitwiseOp CPU::BitwiseTable(unsigned char index)
{
	switch (index)
	{
	case 0:
		return BitwiseOp::RLC;
		break;
	case 1:
		return BitwiseOp::RRC;
		break;
	case 2:
		return BitwiseOp::RL;
		break;
	case 3:
		return BitwiseOp::RR;
		break;
	case 4:
		return BitwiseOp::SLA;
		break;
	case 5:
		return BitwiseOp::SRA;
		break;
	case 6:
		return BitwiseOp::SLL;
		break;
	case 7:
		return BitwiseOp::SRL;
		break;
	default:
		Logger::RaiseError("CPU", "Unknown BitwiseOp table index");
		break;
	}
	return BitwiseOp::NONE;
}

BlockOp CPU::BlockTable(unsigned char a, unsigned char b)
{
	return BlockOp::NONE;
}

bool CPU::ConditionMet(unsigned char index)
{
	bool condition_met = false;
	switch (ConditionTable(index))
	{
	case Condition::NonZero:
		condition_met = m_regs.m_flags.bits.zero == 0;
		break;
	case Condition::Zero:
		condition_met = m_regs.m_flags.bits.zero == 1;
		break;
	case Condition::NoCarry:
		condition_met = m_regs.m_flags.bits.carry == 0;
		break;
	case Condition::Carry:
		condition_met = m_regs.m_flags.bits.carry == 1;
		break;
	default:
		break;
	}
	return condition_met;
}
