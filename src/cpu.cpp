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
		//LOAD FROM IMM
	case 0x06:
		m_dest = Location::B;
	case 0x0E:
		if (m_dest == Location::NONE) m_dest = Location::C;
	case 0x16:
		if (m_dest == Location::NONE) m_dest = Location::D;
	case 0x1E:
		if (m_dest == Location::NONE) m_dest = Location::E;
	case 0x26:
		if (m_dest == Location::NONE) m_dest = Location::H;
	case 0x2E:
		if (m_dest == Location::NONE) m_dest = Location::L;
	case 0x3e:
		if (m_dest == Location::NONE) m_dest = Location::A;
		m_instruction = Instruction::LOAD;
		m_source = Location::IMM;
		break;
		//LOAD FROM MEM @ HL	
	case 0x46:
		m_dest = Location::B;
	case 0x4e:
		if (m_dest == Location::NONE) m_dest = Location::C;
	case 0x56:
		if (m_dest == Location::NONE) m_dest = Location::D;
	case 0x5e:
		if (m_dest == Location::NONE) m_dest = Location::E;
	case 0x66:
		if (m_dest == Location::NONE) m_dest = Location::H;
	case 0x6e:
		if (m_dest == Location::NONE) m_dest = Location::L;
	case 0x7E:
	case 0x2A:
	case 0x3A:
		m_address = m_regs.HL();
		address_locked = true;
		if (m_dest == Location::NONE) m_dest = Location::A;
		//LOAD AND DEC/INC
		if (op == 0x3A)
		{
			m_regs.HL(m_regs.HL() - 1);
		}
		else if (op == 0x2A)
		{
			m_regs.HL(m_regs.HL() + 1);
		}

		m_instruction = Instruction::LOAD;
		m_source = Location::MEM;
		
		break;
		//LOAD TO MEM @ HL
	case 0x70:
	case 0x71:
	case 0x72:
	case 0x73:
	case 0x74:
	case 0x75:
		m_source = MapLocation(op - 0x70);
	case 0x22:
	case 0x32:
	case 0x77:
		if (m_source == Location::NONE) m_source = Location::A;
	case 0x36:
		m_address = m_regs.HL();

		if (m_source == Location::NONE) m_source = Location::IMM;
		m_dest = Location::MEM;
		//LOAD AND DEC/INC
		if (op == 0x32){
			m_regs.HL(m_regs.HL() - 1);
		}
		else if (op == 0x22)
		{
			m_regs.HL(m_regs.HL() + 1);
		}

		m_instruction = Instruction::LOAD;
		break;
		//LOAD A
	case 0x78:
	case 0x79:
	case 0x7A:
	case 0x7B:
	case 0x7C:
	case 0x7D:
		m_source = MapLocation(op - 0x78);
	case 0x7f:
		if (m_source == Location::NONE) m_source = Location::A;
		m_instruction = Instruction::LOAD;
		m_dest = Location::A;
		break;
		//LOAD MEM TO A
	case 0x0A:
		address_locked = true;
		m_address = m_regs.BC();
	case 0x1A:
		if (!address_locked)
		{
			address_locked = true;
			m_address = m_regs.DE();
		}
	case 0xF0:
		if (!address_locked)
		{
			address_locked = true;
			m_address = 0xFF00 + FetchPC();
			m_cycles += 4;
		}
	case 0xF2:
		if (!address_locked)
		{
			address_locked = true;
			m_address = 0xFF00 + m_regs.C();
		}
	case 0xFA:
		if (!address_locked)
		{
			address_locked = true;
			m_address = FetchPC16();
			m_cycles = m_cycles + 8;
		}
		
		m_instruction = Instruction::LOAD;
		m_source = Location::MEM;
		m_dest = Location::A;
		break;
		//LOAD B
	case 0x40:
	case 0x41:
	case 0x42:
	case 0x43:
	case 0x44:
	case 0x45:
		m_source = MapLocation(op - 0x40);
	case 0x47:
		if (m_source == Location::NONE) m_source = Location::A;
		m_dest = Location::B;
		m_instruction = Instruction::LOAD;
		break;
		//LOAD C
	case 0x48:
		m_source = Location::B;
	case 0x49:
	case 0x4A:
	case 0x4B:
	case 0x4C:
	case 0x4D:
		m_source = MapLocation(op - 0x49);
	case 0x4F:
		if (m_source == Location::NONE) m_source = Location::A;
		m_dest = Location::C;
		m_instruction = Instruction::LOAD;
		break;
		//LOAD D
	case 0x50:
	case 0x51:
	case 0x52:
	case 0x53:
	case 0x54:
	case 0x55:
		m_source = MapLocation(op - 0x50);
	case 0x57:
		if (m_source == Location::NONE) m_source = Location::A;
		m_dest = Location::D;
		m_instruction = Instruction::LOAD;
		break;
		//LOAD E
	case 0x58:
	case 0x59:
	case 0x5A:
	case 0x5B:
	case 0x5C:
	case 0x5D:
		m_source = MapLocation(op - 0x58);
	case 0x5F:
		if (m_source == Location::NONE) m_source = Location::A;
		m_dest = Location::E;
		m_instruction = Instruction::LOAD;
		break;
		//LOAD H
	case 0x60:
	case 0x61:
	case 0x62:
	case 0x63:
	case 0x64:
	case 0x65:
		m_source = MapLocation(op - 0x60);
	case 0x67:
		if (m_source == Location::NONE) m_source = Location::A;
		m_dest = Location::H;
		m_instruction = Instruction::LOAD;
		break;
		//LOAD L
	case 0x68:
	case 0x69:
	case 0x6A:
	case 0x6B:
	case 0x6C:
	case 0x6D:
		m_source = MapLocation(op - 0x68);
	case 0x6F:
		if (m_source == Location::NONE) m_source = Location::A;
		m_dest = Location::L;
		m_instruction = Instruction::LOAD;
		break;
		//LOAD A TO MEM
	case 0x02:
		m_address = m_regs.BC();
		address_locked = true;
	case 0x12:
		if (!address_locked)
		{
			m_address = m_regs.DE();
			address_locked = true;
		}
	case 0xE0:
		if (!address_locked)
		{
			address_locked = true;
			m_address = 0xFF00 + FetchPC();
			m_cycles += 4;
		}
	case 0xE2:
		//WITH OFFSET
		if (!address_locked)
		{
			address_locked = true;
			m_address = 0xFF00 + m_regs.C();
		}
	case 0xEA:
		if (!address_locked)
		{
			m_address = FetchPC16();
			address_locked = true;
			m_cycles += 8;
		}
		m_instruction = Instruction::LOAD;
		m_dest = Location::MEM;
		m_source = Location::A;
		break;
		//WIDE LOADS







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
	bytes = (bytes << 8) + FetchPC();
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
