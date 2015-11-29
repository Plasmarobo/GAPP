// This is the main DLL file.

#include "stdafx.h"

#include "GBLibWrapper.h"

using namespace GBLibWrapper;

AssemblyToken::AssemblyToken()
{
	token = gcnew String("");
	lineNo = 0;
	lineTokenIndex = 0;
	lineTokenLength = 0;
}

AssemblyToken::AssemblyToken(String ^line)
{
	token = line;
	lineNo = 0;
	lineTokenIndex = 0;
	lineTokenLength = 0;
}

String^ AssemblyToken::GetTokenString()
{
	return token;
}

AssemblyToken::AssemblyToken(Queue<Byte>^ stream)
{
	unsigned char op = stream->Dequeue();
	unsigned int n;
	switch (op)
	{
	case 0x00:
		token = gcnew String("NOP");
		break;
	case 0x01:
	{
		n = stream->Dequeue();
		n = (n << 8) + stream->Dequeue();
		token = String::Format("LD BC 0x{0}", n);
	}
		break;
	case 0x02:
	case 0x03:
	case 0x04:
	default:
		break;
	}
}

GBLib::GBLib()
{
	gbcpu = new GBCPU();
	display = new Display(gbcpu->GetMem(), gbcpu);
	input = new Input(gbcpu, gbcpu->GetMem());
}

GBLib::~GBLib()
{
	delete gbcpu;
	delete display;
	delete input;
}

void GBLib::Keydown(unsigned int key)
{
	input->KeyDown(key);
}

void GBLib::Keyup(unsigned int key)
{
	input->KeyUp(key);
}

void GBLib::ClockStep()
{
	input->Step();
	gbcpu->Step();
	display->Step();
	if (display->GetState() == DisplayStates::VBLANK)
	{
		array<unsigned char>^ _data = gcnew array<unsigned char>(160 * 144 * 4);
		unsigned char* raw = display->GetRGBA();
		System::Runtime::InteropServices::Marshal::Copy((IntPtr)raw, _data, 0, 160*144*4);
		
		onDisplayUpdate(_data, 160, 144);
		delete [] raw;
	}
}

void GBLib::DebugStep()
{
	//Do nothing for now
}

List<Byte>^ GBLib::DumpMemory()
{
	List<Byte>^ b = gcnew List<Byte>();
	Memory *m = gbcpu->GetMem();
	for (int i = 0; i < 0x8000; ++i)
	{
		b->Add(m->Read(i));
	}
	return b;
}

List<Byte>^ GBLib::Assemble(List<String^> ^text)
{
	List<Byte>^ rom = gcnew List<Byte>();
	

	return rom;
}

List<String^>^ GBLib::Decompile(Queue<Byte> ^rom)
{
	List<String^>^ text = gcnew List<String^>();
	while (rom->Count > 0)
	{
		AssemblyToken^ t = gcnew AssemblyToken(rom);

		text->Add(t->GetTokenString());
	}
	return text;
}

void GBLib::LoadRom(String ^ filename)
{
	using namespace Runtime::InteropServices;
	const char* chars =
		(const char*) (Marshal::StringToHGlobalAnsi(filename)).ToPointer();
	std::string str = chars;
	Marshal::FreeHGlobal(IntPtr((void*) chars));
	gbcpu->RunGBFile(str);
}
