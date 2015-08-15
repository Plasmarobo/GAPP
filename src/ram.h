#ifndef RAM_H
#define RAM_H


class Ram;

unsigned char nintendo_graphic[0x30] = {
	0xCE, 0xED, 0x66, 0x66, 0xCC, 0x0D, 0x00, 0x0B, 0x03, 0x73, 0x00, 0x83, 0x00, 0x0C, 0x00, 0x0D,
	0x00, 0x08, 0x11, 0x1F, 0x88, 0x89, 0x00, 0x0E, 0xDC, 0xCC, 0x6E, 0xE6, 0xDD, 0xDD, 0xD9, 0x99,
	0xBB, 0xBB, 0x67, 0x63, 0x6E, 0x0E, 0xEC, 0xCC, 0xDD, 0xDC, 0x99, 0x9F, 0xBB, 0xB9, 0x33, 0x3E,
};

class Ram
{
protected:
	union {
		unsigned char bytes[0x10000];
		struct {
			unsigned char rom_bank_0[0x4000];        //0x0000-0x3FFF, 16kB ROM BANK #0
			unsigned char rom_bank_sw[0x4000];       //0x4000-0x7FFF, 16kB SWITCHABLE ROM BANK
			unsigned char ram_video[0x2000];         //0x8000-0x9FFF, 8kB Video RAM
			unsigned char ram_bank_sw[0x2000];       //0xA000-0xBFFF, 8kB switchable ram bank
			unsigned char ram_internal[0x2000];      //0xC000-0xDFFF, 8kB internal ram
			unsigned char ram_internal_echo[0x1E00]; //0xE000-0xFDFF translates to 0xC000-0xDE00
			unsigned char sprite_attrib_mem[0xA0];   //0xFE00-0xFE9F, sprite attribute memory
			unsigned char io_empty[0x60];            //0xFEA0-0xFEFF, empty, but usable for io
			unsigned char io_ports[0x4C];            //0xFF00-0xFF4B, I/O ports
			unsigned char io_empty_2[0x34];          //0xFF4C-0xFF7F, empty, but usable for io
			unsigned char ram_internal_2[0x6F];      //0xFF80-0xFFFE, internal ram
			unsigned char interrupt_enable;          //0xFFFF, interrupt enable register

		} segments;
	} m_memory;

public:
	Ram();
	unsigned char Read(unsigned short addr);
	void Write(unsigned short addr, unsigned char byte);

};
#endif