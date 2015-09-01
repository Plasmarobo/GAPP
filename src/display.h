#ifndef DISPLAY_H
#define DISPLAY_H

class Display
{
	unsigned char m_screen_buffer[256 * 256];
	unsigned char m_display[160 * 144];
	unsigned char m_wndposx;
	unsigned char m_wndposy;
	unsigned char m_scrollx;
	unsigned char m_scrolly;
};

#endif