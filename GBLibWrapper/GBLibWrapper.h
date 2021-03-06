// GBLibWrapper.h
#include "..\src\display.h"
#include "..\src\cpu.h"
#include "..\src\mem.h"
#include "..\src\input.h"
#pragma once

using namespace System;
using namespace System::Collections::Generic;
namespace GBLibWrapper {
	public ref class AssemblyToken
	{

		String^ token;
		unsigned long lineNo;
		unsigned int lineTokenIndex;
		unsigned int lineTokenLength;
	public:
		AssemblyToken();
		AssemblyToken(String^ line);
		AssemblyToken(Queue<Byte> ^stream);
		String^ GetTokenString();

	};
	
	public delegate void OnSerialDataReady(Byte data);
	public delegate void OnDisplayUpdate(array<unsigned char>^ image, unsigned int width, unsigned int height);
	public delegate void OnBreakpoint(unsigned long lineNo, unsigned int instruction);

	public ref class GBLib
	{
	private:
		Display *display;
		GBCPU *gbcpu;
		Input *input;
		bool serial_transfer;

	public:

		GBLib();
		
		event OnSerialDataReady ^ onSerialDataReady;
		event OnDisplayUpdate ^ onDisplayUpdate;
		event OnBreakpoint ^ onBreakpoint;

		void Keydown(unsigned int key);
		void Keyup(unsigned int key);
		void Step();
		void DebugStep();
		Int64 GetCycles();
		bool InterruptFlag();
		List<Byte>^ DumpMemory();
		Int32 Inspect(int location, UInt16 addr);
		List<Byte>^ Assemble(List<String^> ^text);
		List<String^>^ Decompile(Queue<Byte> ^rom);
		void LoadRom(String ^ filename);
		void SetRom(List<Byte> ^ rom);
		virtual ~GBLib();
		void ForceInterrupt(int code);
		void Start();
		bool Stopped();
	};
}
