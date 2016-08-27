#include "mem.h"
#include "cpu.h"
#include "clk.h"
#include "utils.h"
#include <string.h>
#include <fstream>

#define AddrIn(addr,x,y) ((addr>=x)&&(addr<y))

static unsigned char nintendo_graphic[0x30] = {
	0xCE, 0xED, 0x66, 0x66, 0xCC, 0x0D, 0x00, 0x0B, 0x03, 0x73, 0x00, 0x83, 0x00, 0x0C, 0x00, 0x0D,
	0x00, 0x08, 0x11, 0x1F, 0x88, 0x89, 0x00, 0x0E, 0xDC, 0xCC, 0x6E, 0xE6, 0xDD, 0xDD, 0xD9, 0x99,
	0xBB, 0xBB, 0x67, 0x63, 0x6E, 0x0E, 0xEC, 0xCC, 0xDD, 0xDC, 0x99, 0x9F, 0xBB, 0xB9, 0x33, 0x3E,
};

Cart::Cart()
{
	m_rom = NULL;
	m_ram = NULL;
	m_rom_size = 0;
	m_ram_size = 0;
}

Cart::Cart(unsigned char *rom, unsigned int rom_size, unsigned int ram_size)
{
	m_rom = rom;
	m_rom_size = rom_size;
	if (ram_size > 0)
	{
		m_ram = new unsigned char[ram_size];
		m_ram_size = ram_size;
	} else {
		m_ram = NULL;
		m_ram_size = 0;
	}
}

Cart::~Cart()
{
	if (m_rom != NULL)
	{
		delete [] m_rom;
		m_rom = NULL;
	}
	if (m_ram != NULL)
	{
		delete [] m_ram;
		m_ram = NULL;
	}
}

void Cart::WriteMemory(unsigned short addr, unsigned char data)
{
	if (m_rom != NULL)
	{
		//The cart has no ram, drop the write
	}
	else
	{
		Logger::RaiseError("Cart", "Cart data is NULL");
	}
}

unsigned char Cart::ReadMemory(unsigned short addr)
{
	unsigned char val;
	if (m_rom != NULL)
	{
		if (addr < m_rom_size)
		{
			val = m_rom[addr];
		}
		else
		{
			Logger::RaiseError("Cart", "Address out of bounds");
		}
	}
	else
	{
		Logger::RaiseError("Cart", "Cart data is NULL");
	}
	return val;
}

void Cart::WriteRamState(std::string file) {}
void Cart::ReadRamState(std::string file) {}

MBC1Cart::MBC1Cart() : Cart()
{
	m_rom_pointer = NULL;
	m_ram_pointer = NULL;
}


MBC1Cart::MBC1Cart(unsigned char *rom, unsigned int rom_size, unsigned int ram_size) : Cart(rom, rom_size, ram_size)
{
	m_rom_pointer = NULL;
	m_ram_pointer = NULL;
}

MBC1Cart::~MBC1Cart()
{
	m_rom_pointer = NULL;
	m_ram_pointer = NULL;
}

MBC3Cart::MBC3Cart() : MBC1Cart()
{
	
}

MBC3Cart::MBC3Cart(unsigned char *rom, unsigned int rom_size, unsigned int ram_size) : MBC1Cart(rom, rom_size, ram_size)
{
	m_utc_timestamp = GetUtc();
	m_counter = 0;
	m_counter_latch = 0;
	m_write_enable = false;
}

void MBC3Cart::WriteMemory(unsigned short addr, unsigned char data)
{
	if (addr >= 0 && addr <= 0x1FFF)
	{
		if ((data & 0xF) == 0x0A)
		{
			m_write_enable = true;
		}
		else
		{
			m_write_enable = false;
		}
	}
	else if (addr >= 2000 && addr <= 0x3FFF)
	{
		if (data == 0)
		{
			data = 1;
		}
		else if (data <= 0x7F)
		{
			unsigned int offset = data * 0x4000;
			m_rom_pointer = m_rom + offset;
		}
		
	}
	else if (addr >= 0x4000 && addr <= 0x5FFF)
	{
		switch (data)
		{
		case 0x00:
			m_ram_status = BANK_0;
			m_ram_pointer = m_ram;
			break;
		case 0x01:
			m_ram_status = BANK_1;
			m_ram_pointer = m_ram + 0x2000;
			break;
		case 0x02:
			m_ram_status = BANK_2;
			m_ram_pointer = m_ram + 0x4000; 
			break;
		case 0x03:
			m_ram_status = BANK_3;
			m_ram_pointer = m_ram + 0x6000;
			break;

		case 0x08:
			m_ram_status = SECONDS;
			break;
		case 0x09:
			m_ram_status = MINUTES;
			break;
		case 0x0A:
			m_ram_status = HOURS;
			break;
		case 0x0B:
			m_ram_status = DAY_L;
			break;
		case 0x0C:
			m_ram_status = DAY_U;
			break;
		default:
			Logger::RaiseError("MBC3", "Unknown Ram Bank selection");
			break;
		}
	}
	else if (addr >= 0x6000 && addr <= 0x7FFF)
	{
		//Finite State Machine
		if (m_rtc.latch == LatchState::UNLATCHED && data == 0)
		{
			m_rtc.latch = LatchState::LATCHING;
		}
		else if (m_rtc.latch == LatchState::LATCHING && data == 1)
		{
			m_rtc.latch = LatchState::LATCHED;
			m_counter_latch = m_counter;
		}
		else if (m_rtc.latch == LatchState::LATCHED && data == 0)
		{
			m_rtc.latch = LatchState::UNLATCHING;
		}
		else if (m_rtc.latch == LatchState::UNLATCHING && data == 1)
		{
			//This could go wrong if we "power down" while processing
			//However this is a Don't Care Condition, since a reload will resync to UTC anyway
			//We aren't worried about slight differences in rtc
			m_rtc.latch = LatchState::UNLATCHED;
			unsigned long missed = (m_counter - m_counter_latch) %	mbc3_rtc_cycles;
			while(missed > 0)
			{
				--missed;
				UpdateTime();
			}
		}
	}
	else if (addr >= 0xA000 && addr <= 0xBFFF && m_write_enable)
	{
		switch (m_ram_status)
		{
		case BANK_0:
		case BANK_1:
		case BANK_2:
		case BANK_3:
			m_ram_pointer[addr - 0xA000] = data;
			break;
		case SECONDS:
			m_rtc.seconds = data;
			break;
		case MINUTES:
			m_rtc.minutes = data;
			break;
		case HOURS:
			m_rtc.hours = data;
			break;
		case DAY_L:
			m_rtc.day_l = data;
			break;
		case DAY_U:
			m_rtc.day_h.value = data;
			break;
		default:
			break;
		}
	}
}

unsigned char MBC3Cart::ReadMemory(unsigned short addr)
{
	unsigned char val;
	if (addr >= 0 && addr <= 0x3FFF)
	{
		val = m_rom[addr];
	}
	else if (addr >= 0x4000 && addr <= 0x7FFF)
	{
		val = m_rom_pointer[addr];
	}
	else if (addr >= 0xA000 && addr <= 0xBFFF)
	{
		switch (m_ram_status)
		{
		case BANK_0:
		case BANK_1:
		case BANK_2:
		case BANK_3:
			val = m_ram_pointer[addr - 0xA000];
			break;
		case SECONDS:
			val = m_rtc.seconds;
			break;
		case MINUTES:
			val = m_rtc.minutes;
			break;
		case HOURS:
			val = m_rtc.hours;
			break;
		case DAY_L:
			val = m_rtc.day_l;
			break;
		case DAY_U:
			val = m_rtc.day_h.value;
			break;
		default:
			break;
		}
	}
	return val;
}

void MBC3Cart::Step()
{
	//Nop
	m_counter++;
	if (m_counter % mbc3_rtc_cycles == 0)
	{
		//One second has advanced
		if(m_rtc.latch == LatchState::UNLATCHED)
		{
			UpdateTime();
		}
	}
}

void MBC3Cart::UpdateTime()
{
	//32 machine steps
	if(m_rtc.seconds == 59)
	{
		m_rtc.seconds = 0;
		if(m_rtc.minutes == 59)
		{
			m_rtc.minutes = 0;
			if(m_rtc.hours == 23)
			{
				m_rtc.hours = 0;
				if(m_rtc.day_l == 255)
				{
					m_rtc.day_l = 0;
					if(m_rtc.day_h.bits.day == 1)
					{
						m_rtc.day_h.bits.carry = 1;
					}
					m_rtc.day_h.bits.day = ~m_rtc.day_h.bits.day;
				}
				else
				{
					m_rtc.day_l++;
				}
			}
			else
			{
				m_rtc.hours++;
			}
		}
		else
		{
			m_rtc.minutes++;
		}
	}
	else
	{
		m_rtc.seconds++;
	}
}

Memory::Memory()
{
	m_cart = NULL;
}

Memory::~Memory()
{
	if (m_cart != NULL)
	{
		delete m_cart;
	}
}

void Memory::SetCartFromBytes(unsigned char *image, unsigned int size)
{
	unsigned char title[15];
	memcpy(&(title[0]), &(image[0x134]), 15);
	unsigned char color_setting = image[0x143];
	unsigned short new_licensee;
	memcpy(&new_licensee, &(image[0x144]), 2);
	unsigned char sgb = image[0x146];
	unsigned char type = image[0x147];
	unsigned int rom_size = image[0x148];
	unsigned int ram_size = image[0x149];
	unsigned char country = image[0x14A];
	unsigned char licensee = image[0x14B];
	unsigned char header_check = image[0x14C];
	unsigned char global_check = image[0x14D];

	switch (rom_size)
	{
	case 0x00:
		rom_size = 32000;
		break;
	case 0x01:
		rom_size = 64000;
		break;
	case 0x02:
		rom_size = 128000;
		break;
	case 0x03:
		rom_size = 256000;
		break;
	case 0x04:
		rom_size = 512000;
		break;
	case 0x05:
		rom_size = 1024000;
		break;
	case 0x06:
		rom_size = 2048000;
		break;
	case 0x07:
		rom_size = 4096000;
		break;
	default:
		Logger::RaiseError("MEMORY", "Invalid ROM size");
		rom_size = 32000;
		break;
	}

	unsigned char *cart_rom = new unsigned char[rom_size];
	for (unsigned int i = 0; i < rom_size; ++i)
	{
		if (i < size) {
			cart_rom[i] = image[i];
		}
		else {
			cart_rom[i] = 0x00;
		}
	}

	switch (ram_size)
	{
	case 0x00:
		ram_size = 0;
		break;
	case 0x01:
		ram_size = 2000;
		break;
	case 0x02:
		ram_size = 8000;
		break;
	case 0x03:
		ram_size = 32000;
		break;
	default:
		Logger::RaiseError("MEMORY", "Invalid RAM size");
		ram_size = 0;
		break;
	}

	switch (type)
	{
	case 0x00: //ROM
	case 0x08: //ROM+RAM
	case 0x09: //ROM+RAM+BAT
		m_cart = new Cart(cart_rom, rom_size, ram_size);
		break;
	case 0x01: //MBC1
	case 0x02: //MBC1 + RAM
	case 0x03: //MBC1+RAM+BAT
		m_cart = new MBC1Cart(cart_rom, rom_size, ram_size);
		break;
	case 0x05: //MBC2
	case 0x06: //MBC2+BAT
		break;
	case 0x0B: //MMM01
	case 0x0C: //MMM01+RAM
	case 0x0D: //MMM01+RAM+BAT
		break;
	case 0x0F: //MBC3+TIMER+BAT
	case 0x10: //MBC3+RAM+BAT
	case 0x11: //MBC3
	case 0x12: //MBC3+RAM
	case 0x13: //MBC3+RAM+BAT
		m_cart = new MBC3Cart(cart_rom, rom_size, ram_size);
		break;
	case 0x15:
	case 0x16:
	case 0x17:
	case 0x19:
	case 0x1A:
	case 0x1B:
	case 0x1C:
	case 0x1D:
	case 0x1E:
	case 0xFC:
	case 0xFD:
	case 0xFE:
	case 0xFF:
	default:
		m_cart = new Cart(cart_rom, rom_size, ram_size);
		Logger::RaiseError("MEMORY", "Unknown Cart Type");
		break;
	}
}

void Memory::LoadCartFromFile(std::string rom_file)
{
	unsigned char *image;
	std::streamoff size;
	std::ifstream file;
	file.open(rom_file, std::ios::in | std::ios::binary | std::ios::ate);
	if (file.is_open())
	{
		size = file.tellg();
		file.seekg(0);
		image = new unsigned char[size];
		file.read((char*)image, size);
		file.close();
		//CHECK Memory controller
		Logger::PrintInfo("MEMORY", "Loaded " + rom_file);
		SetCartFromBytes(image, size);
		delete [] image;
	}
	else
	{
		Logger::RaiseError("MEMORY", "Cannot open gb file: " + rom_file);
	}
	
}

void Memory::Inc(unsigned short addr)
{
	unsigned char val = Read(addr);
	Write(addr, val + 1);
}

unsigned char Memory::Read(unsigned short addr)
{
	unsigned char val;
	if (AddrIn(addr, MemoryMap::CART, MemoryMap::VRAM) ||
		AddrIn(addr, MemoryMap::RAM_BANK_X, MemoryMap::INTERNAL_RAM_0))
	{
		if (m_cart != NULL)
		{
			val = m_cart->ReadMemory(addr);
		}
		else
		{
			Logger::RaiseError("MEMORY", "No cart");
		}
	}
	else if (AddrIn(addr,MemoryMap::VRAM,MemoryMap::RAM_BANK_X))
	{
		val = m_internal_memory.sections.ram_video[addr - MemoryMap::VRAM];
	}
	else if (AddrIn(addr, MemoryMap::INTERNAL_RAM_0, MemoryMap::SPRITE_ATTRIB))
	{
		if (addr >= MemoryMap::INTERNAL_RAM_ECHO)
		{
			addr = addr - MemoryMap::INTERNAL_RAM_ECHO;
		}
		else
		{
			addr = addr - MemoryMap::INTERNAL_RAM_0;
		}
		val = m_internal_memory.sections.ram_internal[addr];
	}
	else if (AddrIn(addr, MemoryMap::SPRITE_ATTRIB, MemoryMap::UNUSED_0))
	{
		val = m_internal_memory.sections.sprite_attrib_mem[addr - MemoryMap::SPRITE_ATTRIB];
	}
	else if (AddrIn(addr, MemoryMap::UNUSED_0, MemoryMap::IO))
	{
		val = m_internal_memory.sections.io_empty[addr - MemoryMap::UNUSED_0];
	}
	else if (AddrIn(addr, MemoryMap::IO, MemoryMap::UNUSED_1))
	{
		val = m_internal_memory.sections.io_ports[addr - MemoryMap::IO];
	}
	else if (AddrIn(addr, MemoryMap::UNUSED_1, MemoryMap::INTERNAL_RAM_1))
	{
		val = m_internal_memory.sections.io_empty_1[addr - MemoryMap::UNUSED_1];
	}
	else if (AddrIn(addr, MemoryMap::INTERNAL_RAM_1, MemoryMap::INTERRUPT_ENABLE_REG))
	{
		val = m_internal_memory.sections.ram_internal_1[addr - MemoryMap::INTERNAL_RAM_1];
	}
	else if (addr == MemoryMap::INTERRUPT_ENABLE_REG)
	{
		val = m_internal_memory.sections.interrupt_enable;
	}
	return val;
}

void Memory::Write(unsigned short addr, unsigned char byte)
{
	if (AddrIn(addr, MemoryMap::CART, MemoryMap::VRAM) ||
		AddrIn(addr, MemoryMap::RAM_BANK_X, MemoryMap::INTERNAL_RAM_0))
	{
		if (m_cart != NULL)
		{
			m_cart->WriteMemory(addr,byte);
		}
		else
		{
			Logger::RaiseError("MEMORY", "No cart");
		}
	}
	else if (AddrIn(addr, MemoryMap::VRAM, MemoryMap::RAM_BANK_X))
	{
		m_internal_memory.sections.ram_video[addr - MemoryMap::VRAM] = byte;
	}
	else if (AddrIn(addr, MemoryMap::INTERNAL_RAM_0, MemoryMap::SPRITE_ATTRIB))
	{
		if (addr >= MemoryMap::INTERNAL_RAM_ECHO)
		{
			addr = addr - MemoryMap::INTERNAL_RAM_ECHO;
		}
		else
		{
			addr = addr - MemoryMap::INTERNAL_RAM_0;
		}
		m_internal_memory.sections.ram_internal[addr] = byte;
	}
	else if (AddrIn(addr, MemoryMap::SPRITE_ATTRIB, MemoryMap::UNUSED_0))
	{
		m_internal_memory.sections.sprite_attrib_mem[addr - MemoryMap::SPRITE_ATTRIB] = byte;
	}
	else if (AddrIn(addr, MemoryMap::UNUSED_0, MemoryMap::IO))
	{
		m_internal_memory.sections.io_empty[addr - MemoryMap::UNUSED_0] = byte;
	}
	else if (AddrIn(addr, MemoryMap::IO, MemoryMap::UNUSED_1))
	{
		switch (addr)
		{
		case SR_DIV:
			m_internal_memory.sections.io_ports[0x00] = 0;
			break;
		//case SR_TIMA: //Not required, writes normally
		//case SR_TMA: // Not required, writes normally
		//case SR_TAC: // Not required, writes normalls
		/*case SR_NR10:
		case SR_NR11:
		case SR_NR12:
		case SR_NR14:
		case SR_NR21:
		case SR_NR22:
		case SR_NR24:
		case SR_NR30:
		case SR_NR31:
		case SR_NR32:
		case SR_NR33:
		case SR_NR41:
		case SR_NR42:
		case SR_NR43:
		case SR_NR44:
		case SR_NR50:
		case SR_NR51:
		case SR_NR52:*/
		/*case SR_LCDC:
		case SR_SCY:
		case SR_SCX:
		case SR_LY:
		case SR_LYC:*/

		case SR_DMA:
			//160 microsecond DMA transfer
			{
				unsigned short src_addr = byte << 8;
				unsigned short dst_addr = 0xFE00;
				while (dst_addr < 0xFEA0)
				{
					Write(dst_addr++, Read(src_addr++));
				}
			}
			break;
		/*case SR_BGP:
		case SR_OBP0:
		case SR_OBP1:
		case SR_WY:
		case SR_WX:*/
		default:
			m_internal_memory.sections.io_ports[addr - MemoryMap::IO] = byte;
			break;
		}
	}
	else if (AddrIn(addr, MemoryMap::UNUSED_1, MemoryMap::INTERNAL_RAM_1))
	{
		m_internal_memory.sections.io_empty_1[addr - MemoryMap::UNUSED_1] = byte;
	}
	else if (AddrIn(addr, MemoryMap::INTERNAL_RAM_1, MemoryMap::INTERRUPT_ENABLE_REG))
	{
		m_internal_memory.sections.ram_internal_1[addr - MemoryMap::INTERNAL_RAM_1] = byte;
	}
	else if (addr == 0xFFFF)
	{
		m_internal_memory.sections.interrupt_enable = byte;
	}
}

void Memory::LoadState(std::string filename)
{
	//Unimplemented
	
	//Handle RTC/unix timestamp
}

void Memory::SaveState(std::string filename)
{
	//Unimplemented
	
	//Write RTC/unix timestamp
}

void Memory::StepDiv()
{
	//Update DIV
	//Set frequency
	m_div_counter++;
	if(m_div_counter % div_cycles)
	{
		Inc(0xFF04);
	}
}

void Memory::StepTimer()
{
	m_timer_counter++;
	unsigned char timer = Read(SR_TAC);
	if ((timer >> 2) & 0x01)
	{
		timer &= 0x3;
		unsigned int timer_mod;
		switch (timer)
		{
		case 0x00:
			timer_mod = Timer00_Hz;
			break;
		case 0x01:
			timer_mod = Timer01_Hz;
			break;
		case 0x02:
			timer_mod = Timer10_Hz;
			break;
		case 0x03:
			timer_mod = Timer11_Hz;
			break;
		default:
			Logger::RaiseError("Timer", "Unknown frequency");
			break;
		}
		// Div 4 to convert to cycles from hz
		if (m_timer_counter % (timer_mod / 4))
		{
			Inc(SR_TIMA);
			if (Read(SR_TIMA) == 0)
			{
				//Interrupt and load TMA
				//Obscura: This should be delayed one cycle
				//It is not currently, TODO: Fix
				Write(SR_TIMA, Read(SR_TMA));
				m_interrupt_target->Int(Interrupt::TIME_INT);
			}
		}
	}
}

void Memory::Step()
{
	if (m_cart != NULL)
	{
		m_cart->Step();
	}
	//Update TIMER
	//Update DIV
	//Update SB (serial buffer)
}

void Memory::SetKeyStates(unsigned char bits)
{
	//Translate input into memory map
	if (!P15())
	{
		P1((bits >> 4) | 0x10);
		
	}
	else if (!P14())
	{
		P1((bits & 0x0F) | 0x20);
	}
	else
	{
		P1(0);
	}
}