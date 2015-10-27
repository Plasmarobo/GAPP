#ifndef INPUT_H
#define INPUT_H
#include "cpu.h"
#include "mem.h"

enum DefaultKeys
{
	UNKNOWN_KEY = -1,
	KEY_A = 0,
	KEY_S = 18,
	KEY_SELECT = 42,
	KEY_START = 58,
	KEY_LEFT = 68,
	KEY_RIGHT = 69,
	KEY_UP = 70,
	KEY_DOWN = 71,
};

enum Buttons
{
	BTNA = 0,
	BTNB,
	START,
	SELECT,
	UP,
	DOWN,
	LEFT,
	RIGHT,
	BUTTONS_COUNT
};

//Gameboy defines press as 0 and no press as 1
const unsigned char key_down = 0;
const unsigned char key_up = 1;

class Input
{
protected:
	GBCPU* m_cpu;
	Memory* m_mem;
	unsigned int m_keymap[Buttons::BUTTONS_COUNT];
	bool m_keystates[Buttons::BUTTONS_COUNT];
	void SetState(Buttons b, bool state);
public:
	Input(GBCPU* gbcpu, Memory *mem);

	void Step();
	void KeyDown(unsigned int key);
	void KeyUp(unsigned int key);
	void MapKey(unsigned int key, Buttons button);
	unsigned char GetBits();
};

#endif