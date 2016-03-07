using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using GBASMAssembler;
using System.IO;
using System.Text.RegularExpressions;
using GBLibWrapper;

namespace AssemblerTest
{
    [TestClass]
    public class UnitTests
    {
        protected Dictionary<String,String> all_ops = new Dictionary<String,String>() {
            {"NOP",					"0x00"},
            {"LD BC,0x1111", 		"0x01 0x11 0x11"},
			{"LD (BC),A",			"0x02"},
			{"INC BC",				"0x03"},
			{"INC B",				"0x04"},
			{"DEC B",				"0x05"},
			{"LD B,0xFA",			"0x06 0xFA"},
			{"RLCA",				"0x07"},
			{"LD ($1010),SP",		"0x08 0x10 0x10"},
			{"ADD HL,BC",			"0x09"},
			{"LD A,(BC)",			"0x0A"},
			{"DEC BC",				"0x0B"},
			{"INC C",				"0x0C"},
			{"DEC C",				"0x0D"},
			{"LD C,0x02",			"0x0E 0x02"},
			{"RRCA",				"0x0F"},
			{"STOP 0",				"0x10"},
			{"LD DE,0x10FF",		"0x11 0x10 0xFF"},
			{"LD (DE),A",			"0x12"},
			{"INC DE",				"0x13"},
			{"INC D",				"0x14"},
			{"DEC D",				"0x15"},
			{"LD D,0x01",			"0x16 0x01"},
			{"RLA",					"0x17"},
			{"JR 0x01",				"0x18 0x01"},
			{"ADD HL,DE",			"0x19"},
			{"LD A,(DE)",			"0x1A"},
			{"DEC DE",				"0x1B"},
			{"INC E",				"0x1C"},
			{"DEC E",				"0x1D"},
			{"LD E,2Ah",			"0x1E 0x2A"},
			{"RRA",					"0x1F"},
			{"JR NZ,$A2",			"0x20 0xA2"},
			{"LD HL,0x10",			"0x21 0x00 0x10"},
			{"LD (HL+),A",			"0x22"},
			{"INC HL",				"0x23"},
			{"INC H",				"0x24"},
			{"DEC H",				"0x25"},
			{"LD H,0xEA",			"0x26 0xEA"},
			{"DAA",					"0x27"},
			{"JR Z,0x11",			"0x28 0x11"},
			{"ADD HL,HL",			"0x29"},
			{"LD A,(HL+)",			"0x2A"},
			{"DEC HL",				"0x2B"},
			{"INC L",				"0x2C"},
			{"DEC L",				"0x2D"},
			{"LD L,0xF1",			"0x2E 0xF1"},
			{"CPL",					"0x2F"},
			{"JR NC,0x001F",		"0x30 0x1F"},
			{"LD SP,1000H",			"0x31 0x10 0x00"},
			{"LD (HL-),A",			"0x32"},
			{"INC SP",				"0x33"},
			{"INC (HL)",			"0x34"},
			{"DEC (HL)",			"0x35"},
			{"LD (HL),$00",			"0x36 0x00"},
			{"SCF",					"0x37"},
			{"JR C,01H",			"0x38 0x01"},
			{"ADD HL,SP",			"0x39"},
			{"LD A,(HL-)",			"0x3A"},
			{"DEC SP",				"0x3B"},
			{"INC A",				"0x3C"},
			{"DEC A",				"0x3D"},
			{"LD A,0xCB",			"0x3E 0xCB"},
			{"CCF",					"0x3F"},
			{"LD B,B",				"0x40"},
			{"LD B,C",				"0x41"},
			{"LD B,D",				"0x42"},
			{"LD B,E",				"0x43"},
			{"LD B,H",				"0x44"},
			{"LD B,L",				"0x45"},
			{"LD B,(HL)",			"0x46"},
			{"LD B,A",				"0x47"},
			{"LD C,B",				"0x48"},
			{"LD C,C",				"0x49"},
			{"LD C,D",				"0x4A"},
			{"LD C,E",				"0x4B"},
			{"LD C,H",				"0x4C"},
			{"LD C,L",				"0x4D"},
			{"LD C,(HL)",			"0x4E"},
			{"LD C,A",				"0x4F"},
			{"LD D,B",				"0x50"},
			{"LD D,C",				"0x51"},
			{"LD D,D",				"0x52"},
			{"LD D,E",				"0x53"},
			{"LD D,H",				"0x54"},
			{"LD D,L",				"0x55"},
			{"LD D,(HL)",			"0x56"},
			{"LD D,A",				"0x57"},
			{"LD E,B",				"0x58"},
			{"LD E,C",				"0x59"},
			{"LD E,D",				"0x5A"},
			{"LD E,E",				"0x5B"},
			{"LD E,H",				"0x5C"},
			{"LD E,L",				"0x5D"},
			{"LD E,(HL)",			"0x5E"},
			{"LD E,A",				"0x5F"},
			{"LD H,B",				"0x60"},
			{"LD H,C",				"0x61"},
			{"LD H,D",				"0x62"},
			{"LD H,E",				"0x63"},
			{"LD H,H",				"0x64"},
			{"LD H,L",				"0x65"},
			{"LD H,(HL)",			"0x66"},
			{"LD H,A",				"0x67"},
			{"LD L,B",				"0x68"},
			{"LD L,C",				"0x69"},
			{"LD L,D",				"0x6A"},
			{"LD L,E",				"0x6B"},
			{"LD L,H",				"0x6C"},
			{"LD L,L",				"0x6D"},
			{"LD L,(HL)",			"0x6E"},
			{"LD L,A",				"0x6F"},
			{"LD (HL),B",			"0x70"},
			{"LD (HL),C",			"0x71"},
			{"LD (HL),D",			"0x72"},
			{"LD (HL),E",			"0x73"},
			{"LD (HL),H",			"0x74"},
			{"LD (HL),L",			"0x75"},
			{"HALT",				"0x76"},
			{"LD (HL),A",			"0x77"},
			{"LD A,B",				"0x78"},
			{"LD A,C",				"0x79"},
			{"LD A,D",				"0x7A"},
			{"LD A,E",				"0x7B"},
			{"LD A,H",				"0x7C"},
			{"LD A,L",				"0x7D"},
			{"LD A,(HL)",			"0x7E"},
			{"LD A,A",				"0x7F"},
			{"ADD A,B",				"0x80"},
			{"ADD A,C",				"0x81"},
			{"ADD A,D",				"0x82"},
			{"ADD A,E",				"0x83"},
			{"ADD A,H",				"0x84"},
			{"ADD A,L",				"0x85"},
			{"ADD A,(HL)",			"0x86"},
			{"ADD A,A",				"0x87"},
			{"ADC A,B",				"0x88"},
			{"ADC A,C",				"0x89"},
			{"ADC A,D",				"0x8A"},
			{"ADC A,E",				"0x8B"},
			{"ADC A,H",				"0x8C"},
			{"ADC A,L",				"0x8D"},
			{"ADC A,(HL)",			"0x8E"},
			{"ADC A,A",				"0x8F"},
			{"SUB B",				"0x90"},
			{"SUB C",				"0x91"},
			{"SUB D",				"0x92"},
			{"SUB E",				"0x93"},
			{"SUB H",				"0x94"},
			{"SUB L",				"0x95"},
			{"SUB (HL)",			"0x96"},
			{"SUB A",				"0x97"},
			{"SBC A,B",				"0x98"},
			{"SBC A,C",				"0x99"},
			{"SBC A,D",				"0x9A"},
			{"SBC A,E",				"0x9B"},
			{"SBC A,H",				"0x9C"},
			{"SBC A,L",				"0x9D"},
			{"SBC A,(HL)",			"0x9E"},
			{"SBC A,A",				"0x9F"},
			{"AND B",				"0xA0"},
			{"AND C",				"0xA1"},
			{"AND D",				"0xA2"},
			{"AND E",				"0xA3"},
			{"AND H",				"0xA4"},
			{"AND L",				"0xA5"},
			{"AND (HL)",			"0xA6"},
			{"AND A",				"0xA7"},
			{"XOR B",				"0xA8"},
			{"XOR C",				"0xA9"},
			{"XOR D",				"0xAA"},
			{"XOR E",				"0xAB"},
			{"XOR H",				"0xAC"},
			{"XOR L",				"0xAD"},
			{"XOR (HL)",			"0xAE"},
			{"XOR A",				"0xAF"},
			{"OR B",				"0xB0"},
			{"OR C",				"0xB1"},
			{"OR D",				"0xB2"},
			{"OR E",				"0xB3"},
			{"OR H",				"0xB4"},
			{"OR L",				"0xB5"},
			{"OR (HL)",				"0xB6"},
			{"OR A",				"0xB7"},
			{"CP B",				"0xB8"},
			{"CP C",				"0xB9"},
			{"CP D",				"0xBA"},
			{"CP E",				"0xBB"},
			{"CP H",				"0xBC"},
			{"CP L",				"0xBD"},
			{"CP (HL)",				"0xBE"},
			{"CP A",				"0xBF"},
			{"RET NZ",				"0xC0"},
			{"POP BC",				"0xC1"},
			{"JP NZ,00h",			"0xC2 0x00 0x00"},
			{"JP 1",				"0xC3 0x00 0x01"},
			{"CALL NZ,21",			"0xC4 0x00 0x15"},
			{"PUSH BC",				"0xC5"},
			{"ADD A,32",			"0xC6 0x20"},
			{"RST 00H",				"0xC7"},
			{"RET Z",				"0xC8"},
			{"RET",					"0xC9"},
			{"JP Z,800",			"0xCA 0x03 0x20"}, 
			//{"PREFIX CB",			"0xCB
			{"CALL Z,1001h",		"0xCC 0x10 0x01"},
			{"CALL $1001",			"0xCD 0x10 0x01"},
			{"ADC A,1",				"0xCE 0x01"},
			{"RST 08H",				"0xCF"},
			{"RET NC",				"0xD0"},
			{"POP DE",				"0xD1"},
			{"JP NC,0",				"0xD2 0x00 0x00"},
			{"CALL NC,0x200",		"0xD4 0x02 0x00"},
			{"PUSH DE",				"0xD5"},
			{"SUB 10",				"0xD6 0x0A"},
			{"RST 10H",				"0xD7"},
			{"RET C",				"0xD8"},
			{"RETI",				"0xD9"},
			{"JP C,0x1000", 		"0xDA 0x10 0x00"},
			{"CALL C,0x1010",		"0xDC 0x10 0x10"},
			{"SBC A,1",				"0xDE 0x01"},
			{"RST 18H",				"0xDF"},
			{"LDH (1000),A",		"0xE0 0x03 0xE8"},
			{"POP HL",				"0xE1"},
			{"LD (C),A",			"0xE2"},
			{"RST 20H",				"0xE7"},
			{"PUSH HL",				"0xE5"},
			{"AND 01",				"0xE6 0x01"},
			{"ADD SP,10",			"0xE8 0x0A"},
			{"JP (HL)",				"0xE9"},
			{"LD (100),A",			"0xEA 0x00 0x64"},
			{"XOR 1",				"0xEE 0x01"},
			{"RST 28H",				"0xEF"},
			{"LDH A,(1000)",		"0xF0 0x03 0xE8"},
			{"POP AF",				"0xF1"},
			{"LD A,(C)",			"0xF2"},
			{"DI",					"0xF3"},
			{"PUSH AF",				"0xF5"},
			{"OR 0",				"0xF6 0x00"},
			{"RST 30H",				"0xF7"},
			{"LD HL,SP+10",			"0xF8 0x0A"},
			{"LD SP,HL",			"0xF9"},
			{"LD A,(0x0)",			"0xFA 0x00 0x00"},
			{"EI",					"0xFB"},
			{"CP 0",				"0xFE 0x00"},
			{"RST 38H",				"0xFF"},
			{"RLC B",				"0xCB 0x00"},
			{"RLC C",				"0xCB 0x01"},
			{"RLC D",				"0xCB 0x02"},
			{"RLC E",				"0xCB 0x03"},
			{"RLC H",				"0xCB 0x04"},
			{"RLC L",				"0xCB 0x05"},
			{"RLC (HL)",			"0xCB 0x06"},
			{"RLC A",				"0xCB 0x07"},
			{"RRC B",				"0xCB 0x08"},
			{"RRC C",				"0xCB 0x09"},
			{"RRC D",				"0xCB 0x0A"},
			{"RRC E",				"0xCB 0x0B"},
			{"RRC H",				"0xCB 0x0C"},
			{"RRC L",				"0xCB 0x0D"},
			{"RRC (HL)",			"0xCB 0x0E"},
			{"RRC A",				"0xCB 0x0F"},
			{"RL B",				"0xCB 0x10"},
			{"RL C",				"0xCB 0x11"},
			{"RL D",				"0xCB 0x12"},
			{"RL E",				"0xCB 0x13"},
			{"RL H",				"0xCB 0x14"},
			{"RL L",				"0xCB 0x15"},
			{"RL (HL)",				"0xCB 0x16"},
			{"RL A",				"0xCB 0x17"},
			{"RR B",				"0xCB 0x18"},
			{"RR C",				"0xCB 0x19"},
			{"RR D",				"0xCB 0x1A"},
			{"RR E",				"0xCB 0x1B"},
			{"RR H",				"0xCB 0x1C"},
			{"RR L",				"0xCB 0x1D"},
			{"RR (HL)",				"0xCB 0x1E"},
			{"RR A",				"0xCB 0x1F"},
			{"SLA B",				"0xCB 0x20"},
			{"SLA C",				"0xCB 0x21"},
			{"SLA D",				"0xCB 0x22"},
			{"SLA E",				"0xCB 0x23"},
			{"SLA H",				"0xCB 0x24"},
			{"SLA L",				"0xCB 0x25"},
			{"SLA (HL)",			"0xCB 0x26"},
			{"SLA A",				"0xCB 0x27"},
			{"SRA B",				"0xCB 0x28"},
			{"SRA C",				"0xCB 0x29"},
			{"SRA D",				"0xCB 0x2A"},
			{"SRA E",				"0xCB 0x2B"},
			{"SRA H",				"0xCB 0x2C"},
			{"SRA L",				"0xCB 0x2D"},
			{"SRA (HL)",			"0xCB 0x2E"},
			{"SRA A",				"0xCB 0x2F"},
			{"SWAP B",				"0xCB 0x30"},
			{"SWAP C",				"0xCB 0x31"},
			{"SWAP D",				"0xCB 0x32"},
			{"SWAP E",				"0xCB 0x33"},
			{"SWAP H",				"0xCB 0x34"},
			{"SWAP L",				"0xCB 0x35"},
			{"SWAP (HL)",			"0xCB 0x36"},
			{"SWAP A",				"0xCB 0x37"},
			{"SRL B",				"0xCB 0x38"},
			{"SRL C",				"0xCB 0x39"},
			{"SRL D",				"0xCB 0x3A"},
			{"SRL E",				"0xCB 0x3B"},
			{"SRL H",				"0xCB 0x3C"},
			{"SRL L",				"0xCB 0x3D"},
			{"SRL (HL)",			"0xCB 0x3E"},
			{"SRL A",				"0xCB 0x3F"},
			{"BIT 0,B",				"0xCB 0x40"},
			{"BIT 0,C",				"0xCB 0x41"},
			{"BIT 0,D",				"0xCB 0x42"},
			{"BIT 0,E",				"0xCB 0x43"},
			{"BIT 0,H",				"0xCB 0x44"},
			{"BIT 0,L",				"0xCB 0x45"},
			{"BIT 0,(HL)",			"0xCB 0x46"},
			{"BIT 0,A",				"0xCB 0x47"},
			{"BIT 1,B",				"0xCB 0x48"},
			{"BIT 1,C",				"0xCB 0x49"},
			{"BIT 1,D",				"0xCB 0x4A"},
			{"BIT 1,E",				"0xCB 0x4B"},
			{"BIT 1,H",				"0xCB 0x4C"},
			{"BIT 1,L",				"0xCB 0x4D"},
			{"BIT 1,(HL)",			"0xCB 0x4E"},
			{"BIT 1,A",				"0xCB 0x4F"},
			{"BIT 2,B",				"0xCB 0x50"},
			{"BIT 2,C",				"0xCB 0x51"},
			{"BIT 2,D",				"0xCB 0x52"},
			{"BIT 2,E",				"0xCB 0x53"},
			{"BIT 2,H",				"0xCB 0x54"},
			{"BIT 2,L",				"0xCB 0x55"},
			{"BIT 2,(HL)",			"0xCB 0x56"},
			{"BIT 2,A",				"0xCB 0x57"},
			{"BIT 3,B",				"0xCB 0x58"},
			{"BIT 3,C",				"0xCB 0x59"},
			{"BIT 3,D",				"0xCB 0x5A"},
			{"BIT 3,E",				"0xCB 0x5B"},
			{"BIT 3,H",				"0xCB 0x5C"},
			{"BIT 3,L",				"0xCB 0x5D"},
			{"BIT 3,(HL)",			"0xCB 0x5E"},
			{"BIT 3,A",				"0xCB 0x5F"},
			{"BIT 4,B",				"0xCB 0x60"},
			{"BIT 4,C",				"0xCB 0x61"},
			{"BIT 4,D",				"0xCB 0x62"},
			{"BIT 4,E",				"0xCB 0x63"},
			{"BIT 4,H",				"0xCB 0x64"},
			{"BIT 4,L",				"0xCB 0x65"},
			{"BIT 4,(HL)",			"0xCB 0x66"},
			{"BIT 4,A",				"0xCB 0x67"},
			{"BIT 5,B",				"0xCB 0x68"},
			{"BIT 5,C",				"0xCB 0x69"},
			{"BIT 5,D",				"0xCB 0x6A"},
			{"BIT 5,E",				"0xCB 0x6B"},
			{"BIT 5,H",				"0xCB 0x6C"},
			{"BIT 5,L",				"0xCB 0x6D"},
			{"BIT 5,(HL)",			"0xCB 0x6E"},
			{"BIT 5,A",				"0xCB 0x6F"},
			{"BIT 6,B",				"0xCB 0x70"},
			{"BIT 6,C",				"0xCB 0x71"},
			{"BIT 6,D",				"0xCB 0x72"},
			{"BIT 6,E",				"0xCB 0x73"},
			{"BIT 6,H",				"0xCB 0x74"},
			{"BIT 6,L",				"0xCB 0x75"},
			{"BIT 6,(HL)",			"0xCB 0x76"},
			{"BIT 6,A",				"0xCB 0x77"},
			{"BIT 7,B",				"0xCB 0x78"},
			{"BIT 7,C",				"0xCB 0x79"},
			{"BIT 7,D",				"0xCB 0x7A"},
			{"BIT 7,E",				"0xCB 0x7B"},
			{"BIT 7,H",				"0xCB 0x7C"},
			{"BIT 7,L",				"0xCB 0x7D"},
			{"BIT 7,(HL)",			"0xCB 0x7E"},
			{"BIT 7,A",				"0xCB 0x7F"},
			{"RES 0,B",				"0xCB 0x80"},
			{"RES 0,C",				"0xCB 0x81"},
			{"RES 0,D",				"0xCB 0x82"},
			{"RES 0,E",				"0xCB 0x83"},
			{"RES 0,H",				"0xCB 0x84"},
			{"RES 0,L",				"0xCB 0x85"},
			{"RES 0,(HL)",			"0xCB 0x86"},
			{"RES 0,A",				"0xCB 0x87"},
			{"RES 1,B",				"0xCB 0x88"},
			{"RES 1,C",				"0xCB 0x89"},
			{"RES 1,D",				"0xCB 0x8A"},
			{"RES 1,E",				"0xCB 0x8B"},
			{"RES 1,H",				"0xCB 0x8C"},
			{"RES 1,L",				"0xCB 0x8D"},
			{"RES 1,(HL)",			"0xCB 0x8E"},
			{"RES 1,A",				"0xCB 0x8F"},
			{"RES 2,B",				"0xCB 0x90"},
			{"RES 2,C",				"0xCB 0x91"},
			{"RES 2,D",				"0xCB 0x92"},
			{"RES 2,E",				"0xCB 0x93"},
			{"RES 2,H",				"0xCB 0x94"},
			{"RES 2,L",				"0xCB 0x95"},
			{"RES 2,(HL)",			"0xCB 0x96"},
			{"RES 2,A",				"0xCB 0x97"},
			{"RES 3,B",				"0xCB 0x98"},
			{"RES 3,C",				"0xCB 0x99"},
			{"RES 3,D",				"0xCB 0x9A"},
			{"RES 3,E",				"0xCB 0x9B"},
			{"RES 3,H",				"0xCB 0x9C"},
			{"RES 3,L",				"0xCB 0x9D"},
			{"RES 3,(HL)",			"0xCB 0x9E"},
			{"RES 3,A",				"0xCB 0x9F"},
			{"RES 4,B",				"0xCB 0xA0"},
			{"RES 4,C",				"0xCB 0xA1"},
			{"RES 4,D",				"0xCB 0xA2"},
			{"RES 4,E",				"0xCB 0xA3"},
			{"RES 4,H",				"0xCB 0xA4"},
			{"RES 4,L",				"0xCB 0xA5"},
			{"RES 4,(HL)",			"0xCB 0xA6"},
			{"RES 4,A",				"0xCB 0xA7"},
			{"RES 5,B",				"0xCB 0xA8"},
			{"RES 5,C",				"0xCB 0xA9"},
			{"RES 5,D",				"0xCB 0xAA"},
			{"RES 5,E",				"0xCB 0xAB"},
			{"RES 5,H",				"0xCB 0xAC"},
			{"RES 5,L",				"0xCB 0xAD"},
			{"RES 5,(HL)",			"0xCB 0xAE"},
			{"RES 5,A",				"0xCB 0xAF"},
			{"RES 6,B",				"0xCB 0xB0"},
			{"RES 6,C",				"0xCB 0xB1"},
			{"RES 6,D",				"0xCB 0xB2"},
			{"RES 6,E",				"0xCB 0xB3"},
			{"RES 6,H",				"0xCB 0xB4"},
			{"RES 6,L",				"0xCB 0xB5"},
			{"RES 6,(HL)",			"0xCB 0xB6"},
			{"RES 6,A",				"0xCB 0xB7"},
			{"RES 7,B",				"0xCB 0xB8"},
			{"RES 7,C",				"0xCB 0xB9"},
			{"RES 7,D",				"0xCB 0xBA"},
			{"RES 7,E",				"0xCB 0xBB"},
			{"RES 7,H",				"0xCB 0xBC"},
			{"RES 7,L",				"0xCB 0xBD"},
			{"RES 7,(HL)",			"0xCB 0xBE"},
			{"RES 7,A",				"0xCB 0xBF"},
			{"SET 0,B",				"0xCB 0xC0"},
			{"SET 0,C",				"0xCB 0xC1"},
			{"SET 0,D",				"0xCB 0xC2"},
			{"SET 0,E",				"0xCB 0xC3"},
			{"SET 0,H",				"0xCB 0xC4"},
			{"SET 0,L",				"0xCB 0xC5"},
			{"SET 0,(HL)",			"0xCB 0xC6"},
			{"SET 0,A",				"0xCB 0xC7"},
			{"SET 1,B",				"0xCB 0xC8"},
			{"SET 1,C",				"0xCB 0xC9"},
			{"SET 1,D",				"0xCB 0xCA"},
			{"SET 1,E",				"0xCB 0xCB"},
			{"SET 1,H",				"0xCB 0xCC"},
			{"SET 1,L",				"0xCB 0xCD"},
			{"SET 1,(HL)",			"0xCB 0xCE"},
			{"SET 1,A",				"0xCB 0xCF"},
			{"SET 2,B",				"0xCB 0xD0"},
			{"SET 2,C",				"0xCB 0xD1"},
			{"SET 2,D",				"0xCB 0xD2"},
			{"SET 2,E",				"0xCB 0xD3"},
			{"SET 2,H",				"0xCB 0xD4"},
			{"SET 2,L",				"0xCB 0xD5"},
			{"SET 2,(HL)",			"0xCB 0xD6"},
			{"SET 2,A",				"0xCB 0xD7"},
			{"SET 3,B",				"0xCB 0xD8"},
			{"SET 3,C",				"0xCB 0xD9"},
			{"SET 3,D",				"0xCB 0xDA"},
			{"SET 3,E",				"0xCB 0xDB"},
			{"SET 3,H",				"0xCB 0xDC"},
			{"SET 3,L",				"0xCB 0xDD"},
			{"SET 3,(HL)",			"0xCB 0xDE"},
			{"SET 3,A",				"0xCB 0xDF"},
			{"SET 4,B",				"0xCB 0xE0"},
			{"SET 4,C",				"0xCB 0xE1"},
			{"SET 4,D",				"0xCB 0xE2"},
			{"SET 4,E",				"0xCB 0xE3"},
			{"SET 4,H",				"0xCB 0xE4"},
			{"SET 4,L",				"0xCB 0xE5"},
			{"SET 4,(HL)",			"0xCB 0xE6"},
			{"SET 4,A",				"0xCB 0xE7"},
			{"SET 5,B",				"0xCB 0xE8"},
			{"SET 5,C",				"0xCB 0xE9"},
			{"SET 5,D",				"0xCB 0xEA"},
			{"SET 5,E",				"0xCB 0xEB"},
			{"SET 5,H",				"0xCB 0xEC"},
			{"SET 5,L",				"0xCB 0xED"},
			{"SET 5,(HL)",			"0xCB 0xEE"},
			{"SET 5,A",				"0xCB 0xEF"},
			{"SET 6,B",				"0xCB 0xF0"},
			{"SET 6,C",				"0xCB 0xF1"},
			{"SET 6,D",				"0xCB 0xF2"},
			{"SET 6,E",				"0xCB 0xF3"},
			{"SET 6,H",				"0xCB 0xF4"},
			{"SET 6,L",				"0xCB 0xF5"},
			{"SET 6,(HL)",			"0xCB 0xF6"},
			{"SET 6,A",				"0xCB 0xF7"},
			{"SET 7,B",				"0xCB 0xF8"},
			{"SET 7,C",				"0xCB 0xF9"},
			{"SET 7,D",				"0xCB 0xFA"},
			{"SET 7,E",				"0xCB 0xFB"},
			{"SET 7,H",				"0xCB 0xFC"},
			{"SET 7,L",				"0xCB 0xFD"},
			{"SET 7,(HL)",			"0xCB 0xFE"},
			{"SET 7,A",				"0xCB 0xFF"},
        };

        private void PrintTrace(String message)
        {
            System.Diagnostics.Trace.WriteLine(message);
        }
        //Assembler
        [TestMethod]
        public void AssemblerAllOpcodes()
        {
            Assembler assembler = new Assembler();
            //assembler.MessagePrinted += PrintTrace;

            foreach(String input in all_ops.Keys)
            {
                System.Diagnostics.Trace.WriteLine(input);
                assembler.AssembleString(input);
                String result = assembler.GetByteString(0);
                Assert.AreEqual(all_ops[input], result);
            }
        }

        
        [TestMethod]
        public void AssemblerNumberFormats()
        {
            Assembler assembler = new Assembler();
            
            assembler.AssembleString("LD C,0x10\nLD C,10h\nLD C,$10\nLD C,10");
            Assert.AreEqual("0x0E 0x10", assembler.GetByteString(0));
            Assert.AreEqual("0x0E 0x10", assembler.GetByteString(1));
            Assert.AreEqual("0x0E 0x10", assembler.GetByteString(2));
            Assert.AreEqual("0x0E 0x0A", assembler.GetByteString(3));
            assembler.AssembleString("JP 0x1\nJP 0x10\nJP 0x100\nJP 0x1000");
            Assert.AreEqual("0xC3 0x00 0x01", assembler.GetByteString(0));
            Assert.AreEqual("0xC3 0x00 0x10", assembler.GetByteString(1));
            Assert.AreEqual("0xC3 0x01 0x00", assembler.GetByteString(2));
            Assert.AreEqual("0xC3 0x10 0x00", assembler.GetByteString(3));
            assembler.AssembleString("JR -1\nJR -0x1\nJR -$2A\nJR-127");
            Assert.AreEqual("0x18 0xFF", assembler.GetByteString(0));
            Assert.AreEqual("0x18 0xFF", assembler.GetByteString(1));
            Assert.AreEqual("0x18 0xD6", assembler.GetByteString(2));
            Assert.AreEqual("0x18 0x81", assembler.GetByteString(3));
        }

        [TestMethod]
        public void AssemblerLabels()
        {
            Assembler assembler = new Assembler();
            assembler.MessagePrinted += PrintTrace;
            //Absolute labels
            assembler.AssembleString("ZERO: JP ZERO");
            Assert.AreEqual("0xC3 0x00 0x00", assembler.GetByteString(0));
            assembler.AssembleString("JP NINE\nNOP\nNOP\nNOP\nNOP\nNOP\nNOP\nNINE:NOP");
            Assert.AreEqual("0xC3 0x00 0x09", assembler.GetByteString(0));
            for(int i = 1; i < 8; ++i)
            {
                Assert.AreEqual("0x00", assembler.GetByteString(i));
            }
            //Relative labels
            assembler.AssembleString("JR X\nNOP\nNOP\nNOP\nNOP\nNOP\nNOP\nX: NOP");
            Assert.AreEqual("0x18 0x08", assembler.GetByteString(0));
            assembler.AssembleString("ZERO: NOP\nNOP\nNOP\nNOP\nNOP\nNOP\nNOP\nJR ZERO");
            for (int i = 0; i < 7; ++i)
            {
                Assert.AreEqual("0x00", assembler.GetByteString(i));
            }
            Assert.AreEqual("0x18 0xF9", assembler.GetByteString(7));
        }

       

        [TestMethod]
        public void AssemblerData()
        {
            Assembler assembler = new Assembler();
            assembler.MessagePrinted += PrintTrace;
            //DB commands
            assembler.AssembleString("DB 0x44");
            Assert.AreEqual("0x44", assembler.GetByteString(0));
            assembler.AssembleString("DB \"TEST\"");
            Assert.AreEqual("0x54 0x45 0x53 0x54", assembler.GetByteString(0));
            assembler.AssembleString("DB 0x44,\"TEST\"");
            Assert.AreEqual("0x44 0x54 0x45 0x53 0x54", assembler.GetByteString(0));
        }

        [TestMethod]
        public void AssemblerRept()
        {
            Assembler assembler = new Assembler();
            assembler.MessagePrinted += PrintTrace;
            //REPT blocks
            assembler.AssembleString("rept 0x02\n DB 0x01\nendr\nNOP");
            Assert.AreEqual("0x01", assembler.GetByteString(0));
            Assert.AreEqual("0x01", assembler.GetByteString(1));
            Assert.AreEqual("0x00", assembler.GetByteString(2));
        }

        [TestMethod]
        public void AssemblerSections()
        {
            Assembler assembler = new Assembler();
            assembler.MessagePrinted += PrintTrace;
            //Test a section
            List<Byte> rom = assembler.AssembleString("SECTION \"ENTRY\",HOME[$100]\nDB 0x01");
            Assert.AreEqual("0x01", assembler.GetByteString(1));
            for(int i = 0; i < 0x100; ++i)
            {
                Assert.AreEqual(0, rom[i]);
            }
        }

        //  Annotated Assembly Test Format (*.aat)
        public enum GBLocations //MUST MATCH ENUM DEFINED IN cpu.h
        {
            NONE = 0,
            B,
            C,
            D,
            E,
            H,
            L,
            A,
            F,
            AF,
            BC,
            DE,
            HL,
            SP,
            PC,
            MEM, //Memory address 16b
            IMM, //8b
            WIDE_IMM, //16b
            OFFSET, //Memory address given by FF00 + n (8b)
            WIDE_OFFSET, //Memory address given by FF00+n (16b)
            PORT,
            STACK,
            NUM_LOCATIONS
        };

        protected class GBTestExpections
        {
            protected Dictionary<short, Byte> ram;  //Expected RAM after EXEC
            protected Dictionary<GBLocations, Int16> regs; //Expected REG states after EXEC
            protected long cycles;//Expected Timing, cycles to run

            public GBTestExpections()
            {
                ram = new Dictionary<short, byte>();
                regs = new Dictionary<GBLocations, short>();
                cycles = 0;
            }

            public GBTestExpections(String line)
            {
                ram = new Dictionary<short, byte>();
                regs = new Dictionary<GBLocations, short>();
                cycles = 0;
                PreprocessLine(line);
            }

            public void PreprocessLine(String line)
            {
                String[] line_parts = line.Split(';');
                if (line_parts.Length == 2)
                {
                    String asm = line_parts[0];
                    String[] expected = line_parts[1].Split(' ');
                    for (int index = 0; index < expected.Length; ++index)
                    {
                        String[] expected_parts = expected[index].Split('=');
                        if (expected_parts.Length == 2)
                        {
                            Int32 value = Int32.Parse(expected_parts[1]);
                            Regex r = new Regex(@"MEM\[0x([0-9a-fA-F]+)\]");
                            Match m = r.Match(expected_parts[0]);
                            if (m.Length > 0)
                            {
                                AddRamExpection(Int16.Parse(m.Value), (Byte)value);
                            }
                            else
                            {
                                GBLocations l = GBLocations.NONE;
                                switch (expected_parts[0])
                                {

                                    case "A":
                                        l = GBLocations.A;
                                        break;
                                    case "B":
                                        l = GBLocations.B;
                                        break;
                                    case "C":
                                        l = GBLocations.C;
                                        break;
                                    case "D":
                                        l = GBLocations.D;
                                        break;
                                    case "E":
                                        l = GBLocations.E;
                                        break;
                                    case "F":
                                        l = GBLocations.F;
                                        break;
                                    case "H":
                                        l = GBLocations.H;
                                        break;
                                    case "L":
                                        l = GBLocations.L;
                                        break;
                                    case "AF":
                                        l = GBLocations.AF;
                                        break;
                                    case "BC":
                                        l = GBLocations.BC;
                                        break;
                                    case "DE":
                                        l = GBLocations.DE;
                                        break;
                                    case "HL":
                                        l = GBLocations.HL;
                                        break;
                                    case "SP":
                                        l = GBLocations.SP;
                                        break;
                                    case "PC":
                                        l = GBLocations.PC;
                                        break;
                                    case "CYCLES":
                                        l = GBLocations.NONE;
                                        cycles = value;
                                        break;
                                    default:

                                        break;

                                }
                                if (l != GBLocations.NONE)
                                {
                                    AddRegExpection(l, (Int16)value);
                                }
                            }
                        }
                    }
                }
            }

            public void SetExpectedCycles(long c)
            {
                cycles = c;
            }

            public void AddRamExpection(short addr, Byte expected_value)
            {
                ram.Add(addr, expected_value);
            }

            public void AddRegExpection(GBLocations reg, short expected_value)
            {
                regs[reg] = expected_value;
            }

            public void CheckCycles(long c)
            {
                Assert.AreEqual(cycles, c);
            }

            public void CheckRam(GBLib sys)
            {
                foreach (short loc in ram.Keys)
                {
                    Assert.AreEqual(sys.Inspect((int)GBLocations.MEM, loc), ram[loc]);
                }
            }

            public void CheckRegs(GBLib sys)
            {
                foreach (GBLocations reg in regs.Keys)
                {
                    Assert.AreEqual(sys.Inspect((int)reg, 0), regs[reg]);
                }
            }

        }
        //GBLib
        public class GBAnnotatedAssemblyTest
        {
            
          
            

            static public void Run(String filename)
            {
                Assembler assembler = new Assembler();
                System.IO.StreamReader file = new System.IO.StreamReader(filename);
                List<Byte> rom = assembler.AssembleString(file.ReadToEnd());
                
                GBLib sys = new GBLib();
                sys.SetRom(rom);

                int pc = sys.Inspect((int)GBLocations.PC, 0);
                long starting_cycles = 0;
                while(pc < rom.Count)
                {
                    String s = assembler.GetAsmLine(pc);
                    GBTestExpections expectation = new GBTestExpections(s);
                    starting_cycles = sys.GetCycles();
                    sys.Step();
                    expectation.CheckCycles(sys.GetCycles()-starting_cycles);
                    
                    
                    pc = sys.Inspect((int)GBLocations.PC, 0);
                }

            }

         

            
        }

        [TestMethod]
        public void GBAllOps()
        {
            GBAnnotatedAssemblyTest.Run(@"..\..\test\test_configs\allops.aat");
        }
    }
}
