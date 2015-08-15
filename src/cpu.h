#ifndef CPU_H
#define CPU_H
#include "ram.h"
#include "utils.h"

class Register
{
private:
	unsigned char m_upper;
	unsigned char m_lower;
public:
	Register();
	unsigned char ReadUpper() { return m_upper; }
	unsigned char ReadLower() { return m_lower; }
	unsigned char WriteUpper(unsigned char value) { m_upper = value; return m_upper; }
	unsigned char WriteLower(unsigned char value) { m_lower = value; return m_lower; }
	unsigned short Read() { return ((unsigned short)m_upper << 8) + m_lower; }
	unsigned char Write(unsigned short value) { m_lower = value & 0xFF; m_upper = ((value >> 8) & 0xFF); return Read(); }
};


class RegFile
{
protected:
	unsigned short m_accumulator;
	Register m_bc;
	Register m_de;
	Register m_hl;
	unsigned short m_pc;
	unsigned short m_sp;
	
public:
	union
	{
		struct {
			unsigned char unused : 4;
			unsigned char carry : 1;
			unsigned char half_carry : 1;
			unsigned char subtract : 1;
			unsigned char zero : 1;
		} bits;
		unsigned char value;
	} m_flags;
	
	
	RegFile(){
		m_bc.Write(0);
		m_de.Write(0);
		m_hl.Write(0);
		// PC is set to location 0x100
		m_pc = 0x100;
		// SP set to ram max
		m_sp = 0xFFFE;
	}

	unsigned char A() { return m_accumulator; }
	unsigned char A(unsigned char value) { m_accumulator = value;  return m_accumulator; }
	unsigned char B() { return m_bc.ReadUpper(); }
	unsigned char B(unsigned char value) { return m_bc.WriteUpper(value); }
	unsigned char C() { return m_bc.ReadLower(); }
	unsigned char C(unsigned char value) { return m_bc.WriteLower(value); }
	unsigned char D() { return m_de.ReadUpper(); }
	unsigned char D(unsigned char value) { return m_de.WriteUpper(value); }
	unsigned char E() { return m_de.ReadLower(); }
	unsigned char E(unsigned char value) { return m_de.WriteLower(value); }
	unsigned char F() { return m_flags.value; }
	unsigned char F(unsigned char value) { m_flags.value = value; return m_flags.value; }
	unsigned char H() { return m_hl.ReadUpper(); }
	unsigned char H(unsigned char value) { return m_hl.WriteUpper(value); }
	unsigned char L() { return m_hl.ReadLower(); }
	unsigned char L(unsigned char value) { return m_hl.WriteLower(value); }
	unsigned short AF() { return (m_accumulator << 8) + m_flags.value; }
	unsigned short BC() { return m_bc.Read(); }
	unsigned short DE() { return m_de.Read(); }
	unsigned short HL() { return m_hl.Read(); }
	unsigned short AF(unsigned short value) { m_accumulator = (value >> 8) & 0xFF; m_flags.value = (value & 0xFF); return AF(); }
	unsigned short BC(unsigned short value) { return m_bc.Write(value); }
	unsigned short DE(unsigned short value) { return m_de.Write(value); }
	unsigned short HL(unsigned short value) { return m_hl.Write(value); }

	unsigned short PC() { return m_pc; }
	unsigned short PC(unsigned short value) { m_pc = value; return m_pc; }
	unsigned short IncPC() { ++m_pc; return m_pc; }
	unsigned short SP() { return m_sp; }
	unsigned short SP(unsigned short value) { m_sp = value; return m_sp; }
	unsigned short IncSP() { ++m_sp; return m_sp; }
	unsigned short DecSP() { --m_sp; return m_sp; }

};

typedef enum Location
{
	NONE = 0,
	A,
	B,
	C,
	D,
	E,
	H,
	L,
	F,
	AF,
	BC,
	DE,
	HL,
	SP,
	PC,
	MEM, //Memory address 16b
	IMM, //8b
	WIDE_IMM, //16b
	OFFSET, //Memory address given by FF00 + n (8b)
	WIDE_OFFSET, //Memory address given by FF00+n (16b)
	PORT,
	STACK,
};

typedef enum Instruction
{
	NONE = 0,
	NOP,
	LOAD, //Load 8b
	LOAD_AND_DEC,
	LOAD_AND_INC,
	LOAD_WIDE, //Load 16b
	INC, 
	DEC,
	JR,
	JP,
	ADD,
	ADC,
	SUB,
	SBC,
	STOP,
	HALT,
	AND,
	OR,
	XOR,
	CP,
	RLC,
	RRC,
	RL,
	RR,
	SLA,
	SRA,
	SLL,
	SRL,
	DA,
	CPL,
	SCF,
	CCF,
	RET,
	POP,
	PUSH,
	CALL,
	RST,
	DI,
	EI,
	BIT,
	RES,
	SET,
	IM,
	NEG,
	RETN,
	RETI,
	SWAP
};

typedef enum Condition
{
	NONE = 0,
	NonZero,
	Zero,
	NoCarry,
	Carry,
};

typedef enum BitwiseOp
{
	NONE,
	RLC,
	RRC,
	RL,
	RR,
	SLA,
	SRA,
	SLL,
	SRL,
};

typedef enum Interrupt
{
	NONE,
	Zero,
	ZeroOne,
	One,
	Two,
};

typedef enum BlockOp
{
	NONE,
	LDI,
	CPI,
	INI,
	OUTI,
	LDD,
	CPD,
	IND,
	OUTD,
	LDIR,
	CPIR,
	INIR,
	OUTIR,
	LDDR,
	CPDR,
	INDR,
	OUTDR,
};


class CPU
{
	unsigned long m_cycles;
	unsigned char m_source;
	unsigned char m_dest;
	unsigned short m_immediate;
	unsigned short m_address;
	unsigned short m_offset;
	unsigned short m_instruction;
	
	Ram m_ram;
	RegFile m_regs;

	void DecodeInstruction();
	void DecodeCB();
	void ExecuteInstruction();
	unsigned char FetchPC();
	unsigned short FetchPC16();

	unsigned char RegisterTable(unsigned char index);
	Location WideRegisterTableSP(unsigned char index);
	Location WideRegisterTableAF(unsigned char index);
	Condition ConditionTable(unsigned char index);
	Instruction AluTable(unsigned char index);
	BitwiseOp BitwiseTable(unsigned char index);
	Interrupt InterruptTable(unsigned char index);
	BlockOp BlockTable(unsigned char a, unsigned char b);
	bool ConditionMet(unsigned char index);
	Location MapLocation(unsigned char offset);
public:
	CPU();
	CPU();



};


#endif