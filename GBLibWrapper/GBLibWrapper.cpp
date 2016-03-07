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

void GBLib::Step()
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

Int32 GBLib::Inspect(int location, Int16 addr)
{
	Int32 value = -1; //ERROR, can't be represented by an int 16
	Memory* m = gbcpu->GetMem();
	switch (location)
	{
	case NONE:
		break;
	case B:
		value = gbcpu->GetRegs()->B();
		break;
	case C:
		value = gbcpu->GetRegs()->C();
		break;
	case D:
		value = gbcpu->GetRegs()->D();
		break;
	case E:
		value = gbcpu->GetRegs()->E();
		break;
	case H:
		value = gbcpu->GetRegs()->H();
		break;
	case L:
		value = gbcpu->GetRegs()->L();
		break;
	case A:
		value = gbcpu->GetRegs()->A();
		break;
	case F:
		value = gbcpu->GetRegs()->F();
		break;
	case AF:
		value = gbcpu->GetRegs()->AF();
		break;
	case BC:
		value = gbcpu->GetRegs()->BC();
		break;
	case DE:
		value = gbcpu->GetRegs()->DE();
		break;
	case HL:
		value = gbcpu->GetRegs()->HL();
		break;
	case SP:
		value = gbcpu->GetRegs()->SP();
		break;
	case PC:
		value = gbcpu->GetRegs()->PC();
		break;
	case STACK:
		value = m->Read(gbcpu->GetRegs()->SP());
		break;
	case MEM:
		value = gbcpu->GetMem()->Read(addr);
		break;
	case IMM:
	case WIDE_IMM:
	case OFFSET:
	case WIDE_OFFSET:
	case PORT:
		break;
	default:
		break;
	}
	return value & 0xFFFF;
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
	gbcpu->LoadGBFile(str);
}

void GBLib::SetRom(List<Byte> ^ rom)
{
	using namespace Runtime::InteropServices;
	unsigned char *bytes = new unsigned char[rom->Count];
	for (int i = 0; i < rom->Count; ++i)
	{
		bytes[i] = (unsigned char) rom[i];
	}
	gbcpu->SetRom(bytes, rom->Count);
	delete [] bytes;
}

Int64 GBLib::GetCycles()
{
	return gbcpu->GetCycles();
}