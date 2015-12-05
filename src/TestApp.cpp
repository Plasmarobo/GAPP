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
	sf::CircleShape shape(100.f);
	shape.setFillColor(sf::Color::Green);
#ifdef _DEBUG
	timerCurrent = RealElapsedTime();
#endif
	const unsigned int n_tests = 6;
	std::string tests [n_tests] = {
		//"..\\..\\test\\cgb_sound.gb",
		"..\\..\\test\\cpu_instrs.gb",
		//"..\\..\\test\\dmg_sound.gb",
		//"..\\..\\test\\dmg_sound-2.gb",
		"..\\..\\test\\instr_timing.gb",
		"..\\..\\test\\mem_timing.gb",
		"..\\..\\test\\mem_timing-2.gb",
		"..\\..\\test\\oam_bug.gb",
		"..\\..\\test\\oam_bug-2.gb",
	};
	for (unsigned int i = 0; i < n_tests; ++i)
	{
		GBCPU *gbcpu = new GBCPU();
		Display *display = new Display(gbcpu->GetMem(), gbcpu);
		Input *input = new Input(gbcpu, gbcpu->GetMem());

		gbcpu->LoadGBFile(tests[i]);
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
#ifdef _DEBUG
			}
#endif
		}
		delete display;
		delete input;
		delete gbcpu;
	}

	return 0;
}