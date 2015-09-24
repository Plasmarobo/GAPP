#ifndef DISPLAY_H
#define DISPLAY_H
#include "clk.h"
#include "mem.h"
#include "cpu.h"


enum DisplayStates
{
	VBLANK = 0,
	LINE_SPRITES,
	LINE_VRAM,
	LINE_BLANK
};

class Sprite
{
protected:
	unsigned char x;
	unsigned char y;
	unsigned char pattern;
	union {
		unsigned char value;
		struct {
			unsigned char priority : 1;
			unsigned char flip_y : 1;
			unsigned char flip_x : 1;
			unsigned char palette : 1;
		} bits;
	} flags;
public:
	Sprite();
	
	void Read(Memory *mem, unsigned short offset);
	void Draw(unsigned char *buffer);
};

class Display
{
protected:
	//unsigned char m_screen_buffer[256 * 256];
	unsigned char m_display[160 * 144];
	unsigned char m_wndposx;
	unsigned char m_wndposy;
	unsigned char m_scrollx;
	unsigned char m_scrolly;
	const unsigned char m_white_color = 255;
	const unsigned char m_light_color = 192;
	const unsigned char m_dark_color = 96;
	const unsigned char m_black_color = 0;
	unsigned char m_scanline;
	unsigned int m_vsync_counter;
	unsigned int m_hsync_counter;
	unsigned char m_display_state;
	Memory *m_mem;
	GBCPU *m_cpu;
	unsigned char DecodeColor(unsigned char val);
	void WriteStat(unsigned char mode);
	unsigned char* FetchTileLine(unsigned char tile, unsigned char line, bool signed_flag, unsigned char *palette);
	unsigned char ApplyPalette(unsigned short addr, unsigned char px);
	
public:
	Display(Memory *pmem, GBCPU *pcpu);
	~Display();

	void Present();
	void Drawline();
	void Step();

};

#endif