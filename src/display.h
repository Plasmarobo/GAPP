#ifndef DISPLAY_H
#define DISPLAY_H
#include "clk.h"
#include "mem.h"

class Display
{
protected:
	unsigned char m_screen_buffer[256 * 256];
	unsigned char m_display[160 * 144];
	unsigned char m_wndposx;
	unsigned char m_wndposy;
	unsigned char m_scrollx;
	unsigned char m_scrolly;
	const unsigned char m_white = 255;
	const unsigned char m_light = 192;
	const unsigned char m_dark = 96;
	const unsigned char m_black = 0;
	unsigned char m_scanline;
	unsigned int m_vsync_counter;
	Memory *m_mem;
public:
	Display(Memory *mem);
	~Display();

	void Redraw();
	void Drawline();
	void Step();

};

#endif