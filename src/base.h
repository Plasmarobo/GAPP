#ifndef BASE_H
#define BASE_H


enum Interrupt
{
	
	VBLANK_INT = 0x1,
	LCDC_INT = 0x2, //Hblank int
	TIME_INT = 0x4,
	SERIAL_INT = 0x8,
	INPUT_INT = 0x10,
	MAX_INT = 0x20,
};

class Interruptable
{
public:
	virtual void Int(Interrupt int_code) = 0;
};

class Addressable
{
public:
	virtual unsigned char Read(unsigned short addr) = 0;
	virtual void Write(unsigned short addr, unsigned char val) = 0;
};

#endif