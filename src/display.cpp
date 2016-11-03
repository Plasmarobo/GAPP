#include "display.h"
#include <queue>
#include <vector>

Sprite::Sprite()
{
	x = 0;
	y = 0;
	pattern = 0;
	flags.value = 0;
}

void Sprite::Read(Memory *m, unsigned short offset)
{
	mem = m;
	x = mem->Read(offset);
	y = mem->Read(offset+1);
	pattern = mem->Read(offset+2);
	flags.value = mem->Read(offset+3);

}

void Sprite::Draw(unsigned char *buffer, unsigned char line)
{
	unsigned short addr = 160 * (y + line) + x;
	unsigned char* line_data = FetchLine(line);
	memcpy(buffer + addr, line_data, sizeof(unsigned char) * 8);
}

void Sprite::Blend(unsigned char *buffer, short w, unsigned char line)
{
	if (w <= 0)
		return;
	unsigned short addr = 160 * (y + line) + x;
	unsigned char* line_data = FetchLine(line);
	for (unsigned short offset = 0; offset < 8; ++offset)
	{
		if (buffer[addr + offset] == 0)
		{
			buffer[addr + offset] = line_data[offset];
		}
	}
}

unsigned char* Sprite::FetchLine(unsigned char line)
{
	int offset =  (pattern * 16) + (line * 2); //16 bytes per tile , 2 bytes per line
	unsigned short addr = (unsigned short) (0x8000 + offset);
	unsigned char *px_data = new unsigned char[8];
	//Decompress tile
	unsigned short compressed_tile = (mem->Read(addr) << 8) + mem->Read(addr + 1);
	unsigned char palette[4];
	unsigned char pal = flags.bits.palette ? mem->OBP1() : mem->OBP0();
	ApplyPalette(pal, &(palette[0]));
	for (unsigned char i = 0; i < 8; ++i)
	{
		px_data[i] = palette[compressed_tile >> (i * 2) & 0x3];
	}
	return px_data;
}

Sprite::Sprite(const Sprite &rhs)
{
	x = rhs.x;
	y = rhs.y;
	pattern = rhs.pattern;
	flags.value = rhs.flags.value;
	mem = rhs.mem;
}

bool PrioritySprite::operator()(const Sprite &lhs, const Sprite &rhs)
{
	//Sprites are drawn with highest x first (will get overwritten)
	if (lhs.x > rhs.x)
	{
		return true;
	}
	else
	{
		return false;
	}
}


Display::Display(Memory *pmem, GBCPU *pcpu)
{
	m_mem = pmem;
	m_cpu = pcpu;
	
	m_vsync_counter = 0;
	m_hsync_counter = 0;
	m_display_state = LINE_SPRITES;
	m_scanline = 0;
}

Display::~Display()
{
	
}

DisplayStates Display::GetState()
{
	return m_display_state;
}

unsigned char DecodeColor(unsigned char val)
{
	switch (val)
	{
	case 0:
		return black_color;
	case 1:
		return dark_color;
	case 2:
		return light_color;
	case 3:
		return white_color;
	default:
		return black_color;
	}
}

void ApplyPalette(unsigned char pal, unsigned char* palette)
{
	palette[0] = DecodeColor(pal & 0x3);
	palette[1] = DecodeColor((pal >> 2) & 0x3);
	palette[2] = DecodeColor((pal >> 4) & 0x3);
	palette[3] = DecodeColor((pal >> 6) & 0x3);
}

void Display::WriteStat(unsigned char mode)
{
	unsigned char current = m_mem->STAT();
	unsigned char stat_val = 0x78; //0111 1000

	if (mode == 0 && ((current >> 3) & 0x1))
	{
		//Hblank
		m_cpu->Int(LCDC_INT);
	}
	else if (mode == 1 && ((current >> 4) & 0x1))
	{
		//Vblank
		m_cpu->Int(LCDC_INT);
	}
	else if (mode == 2 && ((current >> 5) & 0x1))
	{
		//Sprite-RAM read
		m_cpu->Int(LCDC_INT);
	}
	else if (mode == 3)
	{
		//Copy to display
		//No interrupt
	}
	
	m_mem->STAT(stat_val |
		(((m_mem->LY() == m_mem->LYC()) ? 1 : 0) >> 2) |
		(((mode == 2 || mode == 3) ? 1 : 0) >> 1) |
		(((mode == 1 || mode == 3) ? 1 : 0)));
}

unsigned char* Display::FetchTileLine(unsigned char tile, unsigned char line, bool signed_flag, unsigned char *palette)
{
	int s_addr = signed_flag ? 0x8800 : 0x8000;
	int offset = ((signed_flag ? ((signed char)tile) : ((unsigned char)tile))* 16) + (line * 2); //16 bytes per tile , 2 bytes per line
	unsigned short addr = (unsigned short)(s_addr + offset);
	unsigned char *px_data = new unsigned char[8];
	//Decompress tile
	unsigned short compressed_tile = (m_mem->Read(addr) << 8) + m_mem->Read(addr+1);
	for (unsigned char i = 0; i < 8; ++i)
	{
		px_data[i] = palette[compressed_tile >> (i * 2) & 0x3];
	}
	return px_data;
}

void Display::Step()
{
	++m_vsync_counter;
	//Implement FSM
	if ((m_display_state == LINE_SPRITES) && (m_vsync_counter == scan_sprite_cycles))
	{
		m_display_state = LINE_VRAM;
		m_vsync_counter = 0;
		WriteStat(2);
	}
	else if ((m_display_state == LINE_VRAM) && (m_vsync_counter == scan_vram_cycles))
	{
		m_vsync_counter = 0;
		Drawline();
		++m_scanline;
		WriteStat(3);
		m_mem->LY(m_scanline);
		if (m_scanline == 144)
		{
			//INTERRUPT
			//Present();
			WriteStat(1);
			m_cpu->Int(VBLANK_INT);
			m_display_state = VBLANK;
		}
		else
		{
			m_display_state = LINE_BLANK;
		}
	}
	else if ((m_display_state == LINE_BLANK) && (m_vsync_counter == hblank_cycles))
	{
		m_vsync_counter = 0;
		m_display_state = LINE_SPRITES;
		WriteStat(0);
	}
	else if ((m_display_state == VBLANK) && (m_vsync_counter == (vblank_cycles/10)))
	{
		//ten line timing
		m_vsync_counter = 0;
		if (m_scanline == 153)
		{
			m_scanline = 0;
			m_display_state = LINE_SPRITES;
			//Do not write scanline, > 143 is a signal for vblank
		}
		else
		{
			++m_scanline;
			m_mem->LY(m_scanline);
		}
		m_cpu->Int(LCDC_INT);
		WriteStat(1);
	}
}



void Display::Drawline()
{
	//READ LCDC
	unsigned char controls = m_mem->LCDC();
	bool lcd_enable = (controls >> 7) & 0x1;
	unsigned short window_tilemap_addr = ((controls >> 6) & 0x1) ? 0x9800 : 0x9C00;
	bool window_enable = (controls >> 5) & 0x1;
	unsigned short tile_data_addr = ((controls >> 4) & 0x1) ? 0x8800 : 0x8000;
	bool signed_tile_data = tile_data_addr == 0x8800;
	unsigned short background_tilemap_addr = ((controls >> 3) & 0x1) ? 0x9800 : 0x9C00;
	bool tall_sprites = (controls >> 2) & 0x1;
	bool sprite_enable = (controls >> 1) & 0x1;
	bool window_background_enable = controls & 0x1;

	if (lcd_enable)
	{
		//Draw bkg
		if (window_background_enable)
		{
			//Background is 32 rows of 32 bytes
			//Divide scrollx and scrolly by 8 (shift 3) to get offset
			unsigned short offset = (m_mem->SCX() >> 3) + (32 * (m_mem->SCY() >> 3));
			offset += 32 * (m_scanline >> 3); //Offset for scanline
			unsigned char line = m_scanline % 8;
			unsigned char xoff = m_mem->SCX() % 8;
			unsigned char yoff = m_mem->SCY() % 8;
			unsigned char tile_id = 0;
			unsigned char x = 0;
			//20 tiles are onscreen, but we need to fetch 21 for partial tile alignment
			for (unsigned int tile_no = 0; tile_no <= 20; ++tile_no)
			{
				//Get the tile
				tile_id = m_mem->Read(background_tilemap_addr + offset + tile_no);
				unsigned char palette[4];
				unsigned char pal = m_mem->BGP();
				ApplyPalette(pal, palette);
				unsigned char *px_data = FetchTileLine(tile_id, line + yoff, signed_tile_data, palette);
				//Copy the tile to the screen, if it's the first tile, respect offset
				for (unsigned int tile_x = xoff; tile_x < 8; ++tile_x)
				{
					if (x > 159)
					{
						break;
					}
					m_display [(160 * m_scanline)+x] = px_data[tile_x+xoff];
					++x;
				}
				xoff = 0;
			}
		}

		//Draw window
		if (window_enable)
		{
			unsigned short offset = (m_mem->WX() >> 3) + (32 * (m_mem->WY() >> 3));
			offset += 32 * m_scanline;
			unsigned char line = m_scanline % 8;
			unsigned char xoff = m_mem->SCX() % 8;
			unsigned char yoff = m_mem->SCY() % 8;
			unsigned char tile_id = 0;
			unsigned char tile_span = ((160 - (m_mem->WX() - 7)) >> 3) + 1;
			unsigned char x = m_mem->WX()-7;
			//20 tiles are onscreen, but we need to fetch 21 for partial tile alignment
			for (unsigned int tile_no = 0; tile_no <= tile_span; ++tile_no)
			{
				//Get the tile
				tile_id = m_mem->Read(background_tilemap_addr + offset + tile_no);
				unsigned char palette[4];
				unsigned char pal = m_mem->BGP();
				ApplyPalette(pal, palette);
				unsigned char *px_data = FetchTileLine(tile_id, line + yoff, signed_tile_data, palette);
				//Copy the tile to the screen, if it's the first tile, respect offset
				for (unsigned int tile_x = xoff; tile_x < 8; ++tile_x)
				{
					if (x > 159)
					{
						break;
					}
					m_display[(160 * m_scanline) + x] = px_data[tile_x + xoff];
					++x;
				}
				xoff = 0;
			}
		}

		//Draw Sprites
		if (sprite_enable)
		{
			Sprite sprite;
			std::priority_queue<Sprite, std::vector<Sprite>, PrioritySprite> spriteStream;
			for (unsigned short sprite_index = 39; sprite_index >= 0; --sprite_index)
			{
				sprite.Read(m_mem, 0xFE00 + sprite_index);
				if ((((m_scanline - sprite.y) < 8) && (tall_sprites == false)) ||
					((m_scanline - sprite.y < 16) && (tall_sprites == true)))
				spriteStream.push(sprite);
			}
			while (!spriteStream.empty())
			{
				sprite = spriteStream.top();
				spriteStream.pop();
				if (sprite.flags.bits.priority == 0)
				{
					sprite.Draw(m_display, m_scanline);
				}
				else
				{
					if ((((sprite.x - m_mem->WX()) ^ 2) < 64) || (((sprite.y - m_mem->WY()) ^ 2) < 64))
						sprite.Blend(m_display, m_mem->WX() - sprite.x, m_scanline);
					else
						sprite.Blend(m_display, 8, m_scanline);
				}
			}
		}
	}
}

unsigned char* Display::GetRGBA()
{
	
	unsigned char* screenbuffer = new unsigned char[160 * 144 * 4];
	for (int i = 0; i < 160 * 144; ++i)
	{
		screenbuffer[(i * 4)] = m_display[i];
		screenbuffer[(i * 4) + 1] = m_display[i];
		screenbuffer[(i * 4) + 2] = m_display[i];
		screenbuffer[(i * 4) + 3] = 255;
	}

	return screenbuffer;
}