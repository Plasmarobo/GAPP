#ifndef DISPLAY_H
#define DISPLAY_H
#include "clk.h"
#include "mem.h"
#include "cpu.h"
#include <SFML\Graphics.hpp>

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
	sf::RenderWindow* m_window;
	sf::Texture m_texture;
	static unsigned char DecodeColor(unsigned char val);
	static void ApplyPalette(unsigned char pal, unsigned char *palette);
	void WriteStat(unsigned char mode);
	unsigned char* FetchTileLine(unsigned char tile, unsigned char line, bool signed_flag, unsigned char *palette);
	
public:
	Display(Memory *pmem, GBCPU *pcpu, sf::RenderWindow *window);
	~Display();

	void Present();
	void Drawline();
	void Step();

};

#endif