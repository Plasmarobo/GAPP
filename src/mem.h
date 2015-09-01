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
		unsigned long counter;
	}m_rtc;
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
};
#endif