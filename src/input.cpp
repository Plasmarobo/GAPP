#include "input.h"


Input::Input(GBCPU *gbcpu, Memory *mem)
{
	m_cpu = gbcpu;
	m_mem = mem;
	//Default mappings
	m_keymap[Buttons::BTNA] = KEY_A; //A
	m_keymap[Buttons::BTNB] = KEY_S; //S
	
	m_keymap[Buttons::START] = KEY_START; //Return
	m_keymap[Buttons::SELECT] = KEY_SELECT; //Rshift
	m_keymap[Buttons::UP] = KEY_UP; //up
	m_keymap[Buttons::DOWN] = KEY_DOWN; //down
	m_keymap[Buttons::LEFT] = KEY_LEFT; //left
	m_keymap[Buttons::RIGHT] = KEY_RIGHT; //right

}

void Input::Step()
{
	//nop
	m_mem->SetKeyStates(GetBits());
}

void Input::KeyDown(unsigned int key)
{
	for (int b = Buttons::BTNA; b < Buttons::BUTTONS_COUNT; ++b)
	{
		if (key == m_keymap[b])
		{
			m_keystates[b] = key_down;
			m_cpu->Int(Interrupt::INPUT_INT);
			return;
		}
	}
}

void Input::KeyUp(unsigned int key)
{
	for (int b = Buttons::BTNA; b < Buttons::BUTTONS_COUNT; ++b)
	{
		if (key == m_keymap[b])
		{
			m_keystates[b] = key_up;
			return;
		}
	}
}

void Input::MapKey(unsigned int key, Buttons button)
{
	m_keymap[button] = key;
}

unsigned char Input::GetBits()
{
	unsigned char bits = 0;
	//RLUDABSelSta
	
	bits |= m_keystates[RIGHT];
	bits |= (m_keystates[LEFT] << 1);
	bits |= (m_keystates[UP] << 2);
	bits |= (m_keystates[DOWN] << 3);

	bits |= (m_keystates[BTNA] << 4);
	bits |= (m_keystates[BTNB] << 5);
	bits |= (m_keystates[SELECT] << 6);
	bits |= (m_keystates[START] << 7);

	return bits;
}