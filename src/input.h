#ifndef INPUT_H
#define INPUT_H
#include "cpu.h"
#include "mem.h"
#include <SFML/Window.hpp>

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
	sf::Keyboard::Key m_keymap[Buttons::BUTTONS_COUNT];
	bool m_keystates[Buttons::BUTTONS_COUNT];
	void SetState(Buttons b, bool state);
public:
	Input(GBCPU* gbcpu, Memory *mem);

	void Step();
	void KeyDown(sf::Keyboard::Key key);
	void KeyUp(sf::Keyboard::Key key);
	void MapKey(sf::Keyboard::Key key, Buttons button);
	unsigned char GetBits();
};

#endif