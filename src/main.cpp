#include <SFML/Graphics.hpp>
#include "cpu.h"
#include "mem.h"
#include "display.h"
#include "input.h"

#ifdef _DEBUG
double timerCurrent;

#ifdef WIN32
#include <windows.h>
double RealElapsedTime(void) { // granularity about 50 microsecs on my machine
	static LARGE_INTEGER freq, start;
	LARGE_INTEGER count;
	if (!QueryPerformanceCounter(&count))
		exit(-1);
	if (!freq.QuadPart) { // one time initialization
		if (!QueryPerformanceFrequency(&freq))
			exit(-1);
		start = count;
	}
	return (double) (count.QuadPart - start.QuadPart) / freq.QuadPart;
}


#else
#include <time.h>

double RealElapsedTime(void) {
	timespec ts;
	clock_gettime(CLOCK_MONOTONIC, &ts);
	return ts.tv_sec;
}
#endif

#endif


int main()
{
	sf::RenderWindow window(sf::VideoMode(200, 200), "SFML works!");
	sf::Texture tx;
	sf::Sprite spi;
	tx.create(160, 144);
	
	sf::CircleShape shape(100.f);
	shape.setFillColor(sf::Color::Green);
#ifdef _DEBUG
	timerCurrent = RealElapsedTime();
#endif
	GBCPU *gbcpu = new GBCPU();
	Display *display = new Display(gbcpu->GetMem(), gbcpu, &window);
	Input *input = new Input(gbcpu, gbcpu->GetMem());

	gbcpu->RunGBFile("pokemon_blue.gb");
	gbcpu->Start();
	while (window.isOpen())
	{
		sf::Event event;
		while (window.pollEvent(event))
		{
			if (event.type == sf::Event::Closed)
				window.close();

			if (event.type == sf::Event::KeyPressed)
				input->KeyDown(event.key.code);

			if (event.type == sf::Event::KeyReleased)
				input->KeyUp(event.key.code);
		}
#ifdef _DEBUG
		if ((RealElapsedTime() - timerCurrent) > machine_period)
		{
			timerCurrent = RealElapsedTime();
#endif
			input->Step();
			gbcpu->Step(); //may consume more than one cycle!
			display->Step();
			if (display->GetState() == DisplayStates::VBLANK)
			{
				tx.update(display->GetRGBA(), 0, 0);
				spi.setTexture(tx);
				window.draw(spi);
				window.display();
			}
#ifdef _DEBUG
		}
#endif
	}
	delete display;
	delete input;
	delete gbcpu;

	return 0;
}