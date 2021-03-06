#ifndef DISPLAY_H
#define DISPLAY_H
#include "clk.h"
#include "mem.h"
#include "cpu.h"


typedef enum 
{
	IDLE = -1,
	VBLANK = 0,
	LINE_SPRITES,
	LINE_VRAM,
	LINE_BLANK
} DisplayStates;

const unsigned char white_color = 255;
const unsigned char light_color = 192;
const unsigned char dark_color = 96;
const unsigned char black_color = 0;
unsigned char DecodeColor(unsigned char val);
void ApplyPalette(unsigned char pal, unsigned char *palette);

class Sprite
{
protected:
	unsigned char* FetchLine(unsigned char line);
public:
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
	Memory *mem;
	Sprite();
	Sprite(const Sprite &rhs);
	void Read(Memory *m, unsigned short offset);
	void Draw(unsigned char *buffer, unsigned char line);
	void Blend(unsigned char *buffer, short w, unsigned char line);
	
};

class PrioritySprite
{
public:
	bool operator()(const Sprite &lhs, const Sprite &rhs);
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
	
	unsigned char m_scanline;
	unsigned int m_vsync_counter;
	unsigned int m_hsync_counter;
	DisplayStates m_display_state;
	Memory *m_mem;
	GBCPU *m_cpu;

	void WriteStat(unsigned char mode);
	unsigned char* FetchTileLine(unsigned char tile, unsigned char line, bool signed_flag, unsigned char *palette);
	
public:
	Display(Memory *pmem, GBCPU *pcpu);
	~Display();
	
	unsigned char* GetRGBA();
	DisplayStates GetState();
	void Drawline();
	void Step();

};

#endif