#ifndef BASE_H
#define BASE_H


typedef enum Interrupt
{
	
	VBLANK_INT = 0,
	LCDC_INT, //Hblank int
	TIME_INT,
	SERIAL_INT,
	INPUT_INT,
	NUM_INTS
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