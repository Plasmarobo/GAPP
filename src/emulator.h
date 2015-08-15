#ifndef EMULATOR_H
#define EMULATOR_H

class Gameboy;
class Cpu;
class Apu;

class Input;
class Screen;
class GameCart;


class Apu
{

};

class Input
{

};

class Screen
{
protected:
	const unsigned int m_width = 160;
	const unsigned int m_height = 144;
};

class GameCart
{

};

class Gameboy
{
protected:
	Cpu m_cpu;
	Apu m_apu;
	Ram m_ram;
	Ram m_video_ram;
	Input m_input;
	Screen m_screen;
	GameCart m_gamecart;
public:
	Gameboy();
	unsigned char *GetFrame(); 

};

#endif