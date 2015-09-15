#include "display.h"

Display::Display(Memory *mem)
{
	m_mem = mem;
	m_vsync_counter = 0;
}

void Display::Step()
{
	++m_vsync_counter;
	if (m_vsync_counter % vsync_cycle_interval)
	{
		m_scanline = 0;
	}
	if ((m_scanline < 144) && (m_vsync_counter % ))
	{

	}

}