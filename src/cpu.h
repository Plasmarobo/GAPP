#ifndef CPU_H
#define CPU_H
#include "mem.h"
#include "utils.h"


typedef enum Location
{
	NONE = 0,
	B,
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
	NON = 0,
	NOP,
	LOAD,
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
	RL,
	RRC,
	RR,
	SRS, //Shift right signed
	SL,
	SR,
	DAA,
	CPL,
	SCF,
	CCF,
	POP,
	PUSH,
	DI,
	EI,
	BIT,
	RES,
	SET,
	SWAP
};


typedef enum Interrupt
{
	IZero,
	IZeroOne,
	IOne,
	ITwo,
};


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

typedef union
{
	struct {
		unsigned char unused : 4;
		unsigned char carry : 1;
		unsigned char half_carry : 1;
		unsigned char nonzero : 1;
		unsigned char zero : 1;
	} bits;
	unsigned char value;
} Flags;

class RegFile
{
protected:
	unsigned short m_accumulator;
	Register m_bc;
	Register m_de;
	Register m_hl;
	unsigned short m_pc;
	unsigned short m_sp;
	Flags m_flags;
	
public:
	
	
	
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
	bool Carry() { return m_flags.bits.carry == 1; }
	bool Half() { return m_flags.bits.half_carry == 1; }
	bool Zero() { return m_flags.bits.zero == 1; }
	bool Nonzero() { return m_flags.bits.nonzero == 1; }
	void SetFlags(bool z, bool nz, bool h, bool c)
	{
		m_flags.bits.carry = c ? 1 : 0;
		m_flags.bits.half_carry = h ? 1 : 0;
		m_flags.bits.zero = z ? 1 : 0;
		m_flags.bits.nonzero = nz ? 1 : 0;
	}

};

struct InstructionPacket
{
	Location source;
	Location dest;
	int address;
	short offset;
	unsigned short cycles;
	Instruction instruction;
	Flags flag_mask;

	InstructionPacket()
	{
		source = Location::NONE;
		dest = Location::NONE;
		address = -1;
		offset = 0;
		cycles = 0;
		instruction = Instruction::NON;
		flag_mask.value = 0;
	}
};

class CPU
{
	unsigned long m_cycles;
	
	Memory m_mem;
	RegFile m_regs;
	bool m_interrupt_enable;
	bool m_halted;
	bool m_stopped;

	InstructionPacket DecodeInstruction();
	void DecodeCB(InstructionPacket &packet);
	void ExecuteInstruction(InstructionPacket &packet);
	unsigned char FetchPC();
	unsigned short FetchPC16();
	unsigned char ReadMem(unsigned short addr); //Consumes 4 cycles
	void WriteMem(unsigned short addr, unsigned char value); //Consumes 8 cycles
	void StackPush(unsigned char val);
	unsigned char StackPop();
	void PushPC();
	void PopPC();

	int ReadLocation(Location l, InstructionPacket &packet);
	void WriteLocation(Location l, InstructionPacket &packet, int value);

	Location RegisterTable(unsigned char index, InstructionPacket &packet);
	Location MapLocation(unsigned char offset);
	void StepTimer();
	void StepVblank();
public:
	CPU();
	~CPU();
	void Start();
	void Step();
	void INT(unsigned short addr);
	void RunGBFile(std::string rom_file);
	void SaveState(std::string filename);
	void LoadState(std::string filename);

};


#endif