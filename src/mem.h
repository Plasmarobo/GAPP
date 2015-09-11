#ifndef MEM_H
#define MEM_H
#include <string>

class Memory;

typedef enum
{
	RST_0 = 0x0000,
	RST_8 = 0x0008,
	RST_10 = 0x0010,
	RST_18 = 0x0018,
	RST_20 = 0x0020,
	RST_28 = 0x0028,
	RST_30 = 0x0030,
	RST_38 = 0x0038,
	VBLANK_INT = 0x0040,
	LCDC_INT = 0x0048,
	TIMER_INT = 0x0050,
	SERIAL_INT = 0x0058,
	P10P13_INT = 0x0060,
} IntLocation;

typedef enum
{
	CART = 0x0000,
	CART_BANK_0 = 0x0000,
	CART_BANK_X = 0x4000,
	VRAM = 0x8000,
	RAM_BANK_X = 0xA000,
	INTERNAL_RAM_0 = 0xC000,
	INTERNAL_RAM_ECHO = 0xE000,
	SPRITE_ATTRIB = 0xFE00,
	UNUSED_0 = 0xFEA0,
	IO = 0xFF00,
	UNUSED_1 = 0xFF4C,
	INTERNAL_RAM_1 = 0xFF80,
	INTERRUPT_ENABLE_REG = 0xFFFF,
} MemoryMap;

typedef enum{
	ROM = 0,
	MBC1,
	MBC2,
	MBC3,
	MBC5,
	Rumble,
	HuC1,

} CartType;

class Cart //ROM CONTROLLER
{
protected:
	unsigned char *m_rom;
	unsigned char *m_ram; //Unused
	unsigned int m_rom_size;
	unsigned int m_ram_size;
public:
	Cart();
	Cart(unsigned char *rom, unsigned int rom_size, unsigned int ram_size);
	~Cart();
	virtual void WriteMemory(unsigned short addr, unsigned char data);
	virtual unsigned char ReadMemory(unsigned short addr);
	virtual void WriteRamState(std::string file);
	virtual void ReadRamState(std::string file);
	virtual void Step() {};
};

class MBC1Cart : public Cart
{
protected:
	unsigned char m_mode;
	static const unsigned char MODE_16_8 = 0;
	static const unsigned char MODE_4_32 = 1;
	static const unsigned char mode_select_mask = 0x01; //Single select bit
	static const unsigned char rom_select_mask = 0x1F;
	unsigned char *m_rom_pointer; //Selected rom bank
	unsigned char *m_ram_pointer; //Selected ram bank
public:
	MBC1Cart();
	MBC1Cart(unsigned char *rom, unsigned int rom_size, unsigned int ram_size);
	~MBC1Cart();
};

class MBC3Cart : public MBC1Cart
{
protected:
	//RTC
	struct {
		unsigned char seconds;
		unsigned char minutes;
		unsigned char hours;
		unsigned char day_l;
		union {
			unsigned char value;
			struct {
				unsigned char day : 1;
				unsigned char unused : 5;
				unsigned char halt : 1;
				unsigned char carry : 1;
			} bits;
		}day_h;
		enum {
			UNLATCHED,
			LATCHING,
			LATCHED,
			UNLATCHING
		} latch;
	}m_rtc;
	unsigned long m_counter;
	unsigned long m_counter_latch;
	unsigned long m_utc_timestamp;
	bool m_write_enable;

	enum {
		BANK_0 = 0,
		BANK_1,
		BANK_2,
		BANK_3,
		SECONDS,
		MINUTES,
		HOURS,
		DAY_L,
		DAY_U
	} m_ram_status;


	void UpdateTime();
public:
	MBC3Cart();
	MBC3Cart(unsigned char *rom, unsigned int rom_size, unsigned int ram_size);
	~MBC3Cart();
	virtual void WriteMemory(unsigned short addr, unsigned char data);
	virtual unsigned char ReadMemory(unsigned short addr);
	void Step(); //Machine step (to sync RTC)
};

class Memory
{
protected:

	const unsigned int Timer00_Hz = 4096;
	const unsigned int Timer01_Hz = 262144;
	const unsigned int Timer10_Hz = 65563;
	const unsigned int Timer11_Hz = 16384;

	struct {
		unsigned char ram_video[0x2000];         //0x8000-0x9FFF, 8kB Video RAM
		unsigned char ram_bank_sw[0x2000];       //0xA000-0xBFFF, 8kB switchable ram bank
		unsigned char ram_internal[0x2000];      //0xC000-0xDFFF, 8kB internal ram
		unsigned char sprite_attrib_mem[0xA0];   //0xFE00-0xFE9F, sprite attribute memory
		unsigned char io_empty[0x60];            //0xFEA0-0xFEFF, empty, but usable for io
		unsigned char io_ports[0x4C];            //0xFF00-0xFF4B, I/O ports
		unsigned char io_empty_1[0x34];          //0xFF4C-0xFF7F, empty, but usable for io
		unsigned char ram_internal_1[0x6F];      //0xFF80-0xFFFE, internal ram, fast? paged?
		unsigned char interrupt_enable;          //0xFFFF, interrupt enable register
	} m_internal_memory;
	
	enum {
		TIMA = 0xFF05,
		TMA,
		TAC,
		NR10, = 0xFF10,
		NR11,
		NR12,
		NR14 = 0xFF14,
		NR21 = 0xFF16,
		NR22,
		NR24 = 0xFF19,
		NR30 = 0xFF1A,
		NR31,
		NR32,
		NR33 = 0xFF1E,
		NR41 = 0xFF20,
		NR42,
		NR43,
		NR44,
		NR50,
		NR51,
		NR52,
		LCDC = 0xFF40,
		SCY = 0xFF42,
		SCX,
		LYC = 0xFF45,
		BGP = 0xFF47,
		OBP0, //0BP0
		OBP1, //0BP1
		WY = 0xFF4A,
		WX,
		IE = 0xFFFF,
	} softregs;

	Cart *m_cart;
public:
	Memory();
	~Memory();
	unsigned char Read(unsigned short addr);
	void Write(unsigned short addr, unsigned char byte);
	void LoadCartFromFile(std::string rom_file);
	void LoadState(std::string filename);
	void SaveState(std::string filename);
	void Step();
	//Softreg Functions
	//P1 - INPUT softregs
	unsigned char P1() { return m_internal_memory.io_ports[0]; }
	void P1(unsigned char val) { m_internal_memory.io_ports[0] = val; }
	bool P10() { return m_internal_memory.io_ports[0] & 0x1; }
	bool P11() { return m_internal_memory.io_ports[0] & 0x2; }
	bool P12() { return m_internal_memory.io_ports[0] & 0x4; }
	bool P13() { return m_internal_memory.io_ports[0] & 0x8; }
	bool P14() { return m_internal_memory.io_ports[0] & 0x10; }
	bool P15() { return m_internal_memory.io_ports[0] & 0x20; }
	bool Right() { return m_internal_memory.io_ports[0] & 0x11; }
	bool Left() { return m_internal_memory.io_ports[0] & 0x12; }
	bool Up() { return m_internal_memory.io_ports[0] & 0x14; }
	bool Down() { return m_internal_memory.io_ports[0] & 0x18; }
	bool A() { return m_internal_memory.io_ports[0] & 0x21; }
	bool B() { return m_internal_memory.io_ports[0] & 0x22; }
	bool Sel() { return m_internal_memory.io_ports[0] & 0x24; }
	bool Start() { return m_internal_memory.io_ports[0] & 0x28; }
	//SERIAL
	void SB(unsigned char val) { m_internal_memory.io_ports[1] = val; }
	unsigned char SB() { return m_internal_memory.io_ports[1]; }
	void SC(unsigned char val) { m_internal_memory.io_ports[2] = val; }
	unsigned char SC() { return m_internal_memory.io_ports[2]; }
	void StartTransfer(bool val); //Sets bit 7
	void InternalShiftClk(bool val); //Sets bit 0
	//DIV
	void DIV(unsigned char val) { m_internal_memory.io_ports[4] = 0x00; } // Writing sets to zero
	unsigned char DIV() { return m_internal_memory.io_ports[4];	}
	//TIMA
	unsigned char TIMA() { return m_internal_memory.io_ports[5]; }
	void TIMA(unsigned char val) { m_internal_memory.io_ports[5] = val; }
	//TMA
	unsigned char TMA() { return m_internal_memory.io_ports[6]; }
	void TMA(unsigned char val) { m_internal_memory.io_ports[6] = val; }
	//TAC
	unsigned char TMA() { return m_internal_memory.io_ports[7]; }
	void TMA(unsigned char val) { m_internal_memory.io_ports[7] = val; }
	void StartTimer();
	void StopTimer();
	void SetTimerClk(unsigned char val);
	//IF
	unsigned char IF() { return m_internal_memory.io_ports[0xF];	}
	void IF(unsigned char val) { m_internal_memory.io_ports[0xF] = val; }
	unsigned char GetHPI(); //Get highest priority interrupt code from IF
	void ResetIF() { m_internal_memory.io_ports[0xF] = 0; }
	//Sound Mode 1 / NR 10
	unsigned char NR10() { return m_internal_memory.io_ports[0x10]; }
	void NR10(unsigned char val) { m_internal_memory.io_ports[0x10] = val; }
	//Sound Mode 1 / NR 11
	unsigned char NR11() { return m_internal_memory.io_ports[0x11]; }
	void NR11(unsigned char val) { m_internal_memory.io_ports[0x11] = val; }
	//Sound Mode 1 / NR 12
	unsigned char NR12() { return m_internal_memory.io_ports[0x12]; }
	void NR12(unsigned char val) { m_internal_memory.io_ports[0x12] = val; }
	//Sound Mode 1 / NR 13
	unsigned char NR13() { return m_internal_memory.io_ports[0x13]; }
	void NR13(unsigned char val) { m_internal_memory.io_ports[0x13] = val; }
	//Sound Mode 1 / NR 14
	unsigned char NR14() { return m_internal_memory.io_ports[0x14]; }
	void NR14(unsigned char val) { m_internal_memory.io_ports[0x14] = val; }
	//Sound Mode 2 / NR 2X
	unsigned char NR21() { return m_internal_memory.io_ports[0x16]; }
	void NR21(unsigned char val) { m_internal_memory.io_ports[0x16] = val; }
	unsigned char NR22() { return m_internal_memory.io_ports[0x17]; }
	void NR22(unsigned char val) { m_internal_memory.io_ports[0x17] = val; }
	unsigned char NR23() { return m_internal_memory.io_ports[0x18]; }
	void NR23(unsigned char val) { m_internal_memory.io_ports[0x18] = val; }
	unsigned char NR24() { return m_internal_memory.io_ports[0x19]; }
	void NR24(unsigned char val) { m_internal_memory.io_ports[0x19] = val; }
	//Sound Mode 3 / NR 3X
	unsigned char NR30() { return m_internal_memory.io_ports[0x1A]; }
	void NR30(unsigned char val) { m_internal_memory.io_ports[0x1A] = val; }
	unsigned char NR31() { return m_internal_memory.io_ports[0x1B]; }
	void NR3(unsigned char val) { m_internal_memory.io_ports[0x1B] = val; }
	unsigned char NR32() { return m_internal_memory.io_ports[0x1C]; }
	void NR32(unsigned char val) { m_internal_memory.io_ports[0x1C] = val; }
	unsigned char NR33() { return m_internal_memory.io_ports[0x1D]; }
	void NR33(unsigned char val) { m_internal_memory.io_ports[0x1D] = val; }
	unsigned char NR34() { return m_internal_memory.io_ports[0x1E]; }
	void NR34(unsigned char val) { m_internal_memory.io_ports[0x1E] = val; }
	//Sound Mode 4 / NR 4X
	unsigned char NR41() { return m_internal_memory.io_ports[0x20]; }
	void NR41(unsigned char val) { m_internal_memory.io_ports[0x20] = val; }
	unsigned char NR42() { return m_internal_memory.io_ports[0x21]; }
	void NR42(unsigned char val) { m_internal_memory.io_ports[0x21] = val; }
	unsigned char NR43() { return m_internal_memory.io_ports[0x22]; }
	void NR43(unsigned char val) { m_internal_memory.io_ports[0x22] = val; }
	unsigned char NR44() { return m_internal_memory.io_ports[0x23]; }
	void NR44(unsigned char val) { m_internal_memory.io_ports[0x23] = val; }
	//Sound Mode 5 / NR 5X
	unsigned char NR50() { return m_internal_memory.io_ports[0x24]; }
	void NR50(unsigned char val) { m_internal_memory.io_ports[0x24] = val; }
	unsigned char NR51() { return m_internal_memory.io_ports[0x25]; }
	void NR51(unsigned char val) { m_internal_memory.io_ports[0x25] = val; }
	unsigned char NR52() { return m_internal_memory.io_ports[0x26]; }
	void NR52(unsigned char val) { m_internal_memory.io_ports[0x26] = val; }
	//WAVE RAM
	unsigned char WaveRam(unsigned short offset){ return m_internal_memory.io_ports[0x30 + offset]; }
	void WaveRam(unsigned short offset, unsigned char val) { m_internal_memory.io_ports[0x30 + offset] = val; }
	//LCDC
	unsigned char LCDC() { return m_internal_memory.io_ports[0x40]; }
	void LCDC(unsigned char val) { m_internal_memory.io_ports[0x40] = val; }
	unsigned char STAT() { return m_internal_memory.io_ports[0x41]; }
	void STAT(unsigned char val) { m_internal_memory.io_ports[0x41] = val; }
	//Video
	unsigned char SCY() { return m_internal_memory.io_ports[0x42]; }
	void SCY(unsigned char val) { m_internal_memory.io_ports[0x42] = val; }
	unsigned char SCX() { return m_internal_memory.io_ports[0x43]; }
	void SCY(unsigned char val) { m_internal_memory.io_ports[0x43] = val; }
	unsigned char LYC() { return m_internal_memory.io_ports[0x45]; }
	void LYC(unsigned char val) { m_internal_memory.io_ports[0x45] = val; }
	unsigned char BGP() { return m_internal_memory.io_ports[0x47]; }
	void BGP(unsigned char val) { m_internal_memory.io_ports[0x47] = val; }
	unsigned char OBP0() { return m_internal_memory.io_ports[0x48]; }
	void OBP0(unsigned char val) { m_internal_memory.io_ports[0x48] = val; }
	unsigned char OBP1() { return m_internal_memory.io_ports[0x49]; }
	void OBP1(unsigned char val) { m_internal_memory.io_ports[0x49] = val; }
	unsigned char WY() { return m_internal_memory.io_ports[0x4A]; }
	void WY(unsigned char val) { m_internal_memory.io_ports[0x4A] = val; }
	unsigned char WX() { return m_internal_memory.io_ports[0x4B]; }
	void WX(unsigned char val) { m_internal_memory.io_ports[0x4B] = val; }


	
	unsigned char IE() { return m_internal_memory.interrupt_enable; }
	void IE(unsigned char val) { m_internal_memory.interrupt_enable = val; }

};
#endif