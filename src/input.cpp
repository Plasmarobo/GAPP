#include "input.h"

Input::Input(GBCPU *gbcpu, Memory *mem)
{
	m_cpu = gbcpu;
	m_mem = mem;
	//Default mappings
	m_keymap[Buttons::BTNA] = sf::Keyboard::Key::A;
	m_keymap[Buttons::BTNB] = sf::Keyboard::Key::S;
	m_keymap[Buttons::START] = sf::Keyboard::Key::Return;
	m_keymap[Buttons::SELECT] = sf::Keyboard::Key::RShift;
	m_keymap[Buttons::UP] = sf::Keyboard::Key::Up;
	m_keymap[Buttons::DOWN] = sf::Keyboard::Key::Down;
	m_keymap[Buttons::LEFT] = sf::Keyboard::Key::Left;
	m_keymap[Buttons::RIGHT] = sf::Keyboard::Key::Right;

}

void Input::Step()
{
	//nop
	m_mem->SetKeyStates(GetBits());
}

void Input::KeyDown(sf::Keyboard::Key key)
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

void Input::KeyUp(sf::Keyboard::Key key)
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

void Input::MapKey(sf::Keyboard::Key key, Buttons button)
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