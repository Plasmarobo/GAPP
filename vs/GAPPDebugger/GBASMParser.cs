//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.5.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from GBASM.g4 by ANTLR 4.5.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591

using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.5.1")]
[System.CLSCompliant(false)]
public partial class GBASMParser : Parser {
	public const int
		Z=1, A=2, B=3, C=4, D=5, E=6, F=7, H=8, L=9, AF=10, BC=11, DE=12, HL=13, 
		SP=14, NZ=15, NC=16, RST_VALUE=17, RST_DIGITS=18, Integer=19, Hexval=20, 
		Neg=21, Plus=22, HLPLUS=23, HLMINUS=24, MEMSTART=25, MEMEND=26, LD=27, 
		JR=28, JP=29, OR=30, CP=31, RL=32, RR=33, DI=34, EI=35, LDD=36, LDI=37, 
		ADD=38, ADC=39, SBC=40, BIT=41, RES=42, SET=43, RET=44, INC=45, DEC=46, 
		SUB=47, AND=48, XOR=49, RLC=50, RRC=51, POP=52, SLA=53, SRA=54, SRL=55, 
		NOP=56, RLA=57, RRA=58, DAA=59, CPL=60, SCF=61, CCF=62, LDH=63, RST=64, 
		CALL=65, PUSH=66, SWAP=67, RLCA=68, RRCA=69, STOP=70, HALT=71, RETI=72, 
		LABEL=73, SEPARATOR=74, WS=75, COMMENT=76;
	public const int
		RULE_eval = 0, RULE_exp = 1, RULE_op = 2, RULE_monad = 3, RULE_biad = 4, 
		RULE_triad = 5, RULE_arg = 6, RULE_memory = 7, RULE_offset = 8, RULE_register = 9, 
		RULE_flag = 10, RULE_value = 11, RULE_negvalue = 12;
	public static readonly string[] ruleNames = {
		"eval", "exp", "op", "monad", "biad", "triad", "arg", "memory", "offset", 
		"register", "flag", "value", "negvalue"
	};

	private static readonly string[] _LiteralNames = {
		null, "'Z'", "'A'", "'B'", "'C'", "'D'", "'E'", "'F'", "'H'", "'L'", "'AF'", 
		"'BC'", "'DE'", "'HL'", "'SP'", "'NZ'", "'NC'", null, null, null, null, 
		"'-'", "'+'", null, null, "'('", "')'", null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, "','"
	};
	private static readonly string[] _SymbolicNames = {
		null, "Z", "A", "B", "C", "D", "E", "F", "H", "L", "AF", "BC", "DE", "HL", 
		"SP", "NZ", "NC", "RST_VALUE", "RST_DIGITS", "Integer", "Hexval", "Neg", 
		"Plus", "HLPLUS", "HLMINUS", "MEMSTART", "MEMEND", "LD", "JR", "JP", "OR", 
		"CP", "RL", "RR", "DI", "EI", "LDD", "LDI", "ADD", "ADC", "SBC", "BIT", 
		"RES", "SET", "RET", "INC", "DEC", "SUB", "AND", "XOR", "RLC", "RRC", 
		"POP", "SLA", "SRA", "SRL", "NOP", "RLA", "RRA", "DAA", "CPL", "SCF", 
		"CCF", "LDH", "RST", "CALL", "PUSH", "SWAP", "RLCA", "RRCA", "STOP", "HALT", 
		"RETI", "LABEL", "SEPARATOR", "WS", "COMMENT"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "GBASM.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string SerializedAtn { get { return _serializedATN; } }

	public GBASMParser(ITokenStream input)
		: base(input)
	{
		Interpreter = new ParserATNSimulator(this,_ATN);
	}
	public partial class EvalContext : ParserRuleContext {
		public ExpContext exp() {
			return GetRuleContext<ExpContext>(0);
		}
		public ITerminalNode Eof() { return GetToken(GBASMParser.Eof, 0); }
		public EvalContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_eval; } }
		public override void EnterRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.EnterEval(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.ExitEval(this);
		}
	}

	[RuleVersion(0)]
	public EvalContext eval() {
		EvalContext _localctx = new EvalContext(Context, State);
		EnterRule(_localctx, 0, RULE_eval);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 26; exp();
			State = 27; Match(Eof);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class ExpContext : ParserRuleContext {
		public OpContext op() {
			return GetRuleContext<OpContext>(0);
		}
		public ExpContext exp() {
			return GetRuleContext<ExpContext>(0);
		}
		public ExpContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_exp; } }
		public override void EnterRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.EnterExp(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.ExitExp(this);
		}
	}

	[RuleVersion(0)]
	public ExpContext exp() {
		ExpContext _localctx = new ExpContext(Context, State);
		EnterRule(_localctx, 2, RULE_exp);
		try {
			State = 33;
			switch ( Interpreter.AdaptivePredict(TokenStream,0,Context) ) {
			case 1:
				EnterOuterAlt(_localctx, 1);
				{
				State = 29; op();
				State = 30; exp();
				}
				break;
			case 2:
				EnterOuterAlt(_localctx, 2);
				{
				State = 32; op();
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class OpContext : ParserRuleContext {
		public MonadContext monad() {
			return GetRuleContext<MonadContext>(0);
		}
		public BiadContext biad() {
			return GetRuleContext<BiadContext>(0);
		}
		public ArgContext[] arg() {
			return GetRuleContexts<ArgContext>();
		}
		public ArgContext arg(int i) {
			return GetRuleContext<ArgContext>(i);
		}
		public TriadContext triad() {
			return GetRuleContext<TriadContext>(0);
		}
		public ITerminalNode SEPARATOR() { return GetToken(GBASMParser.SEPARATOR, 0); }
		public OpContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_op; } }
		public override void EnterRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.EnterOp(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.ExitOp(this);
		}
	}

	[RuleVersion(0)]
	public OpContext op() {
		OpContext _localctx = new OpContext(Context, State);
		EnterRule(_localctx, 4, RULE_op);
		try {
			State = 44;
			switch ( Interpreter.AdaptivePredict(TokenStream,1,Context) ) {
			case 1:
				EnterOuterAlt(_localctx, 1);
				{
				State = 35; monad();
				}
				break;
			case 2:
				EnterOuterAlt(_localctx, 2);
				{
				State = 36; biad();
				State = 37; arg();
				}
				break;
			case 3:
				EnterOuterAlt(_localctx, 3);
				{
				State = 39; triad();
				State = 40; arg();
				State = 41; Match(SEPARATOR);
				State = 42; arg();
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class MonadContext : ParserRuleContext {
		public ITerminalNode NOP() { return GetToken(GBASMParser.NOP, 0); }
		public ITerminalNode RLCA() { return GetToken(GBASMParser.RLCA, 0); }
		public ITerminalNode RRCA() { return GetToken(GBASMParser.RRCA, 0); }
		public ITerminalNode STOP() { return GetToken(GBASMParser.STOP, 0); }
		public ITerminalNode RLA() { return GetToken(GBASMParser.RLA, 0); }
		public ITerminalNode RRA() { return GetToken(GBASMParser.RRA, 0); }
		public ITerminalNode DAA() { return GetToken(GBASMParser.DAA, 0); }
		public ITerminalNode CPL() { return GetToken(GBASMParser.CPL, 0); }
		public ITerminalNode SCF() { return GetToken(GBASMParser.SCF, 0); }
		public ITerminalNode CCF() { return GetToken(GBASMParser.CCF, 0); }
		public ITerminalNode HALT() { return GetToken(GBASMParser.HALT, 0); }
		public ITerminalNode RETI() { return GetToken(GBASMParser.RETI, 0); }
		public ITerminalNode DI() { return GetToken(GBASMParser.DI, 0); }
		public ITerminalNode EI() { return GetToken(GBASMParser.EI, 0); }
		public ITerminalNode RST() { return GetToken(GBASMParser.RST, 0); }
		public ITerminalNode RET() { return GetToken(GBASMParser.RET, 0); }
		public MonadContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_monad; } }
		public override void EnterRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.EnterMonad(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.ExitMonad(this);
		}
	}

	[RuleVersion(0)]
	public MonadContext monad() {
		MonadContext _localctx = new MonadContext(Context, State);
		EnterRule(_localctx, 6, RULE_monad);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 46;
			_la = TokenStream.La(1);
			if ( !(((((_la - 34)) & ~0x3f) == 0 && ((1L << (_la - 34)) & ((1L << (DI - 34)) | (1L << (EI - 34)) | (1L << (RET - 34)) | (1L << (NOP - 34)) | (1L << (RLA - 34)) | (1L << (RRA - 34)) | (1L << (DAA - 34)) | (1L << (CPL - 34)) | (1L << (SCF - 34)) | (1L << (CCF - 34)) | (1L << (RST - 34)) | (1L << (RLCA - 34)) | (1L << (RRCA - 34)) | (1L << (STOP - 34)) | (1L << (HALT - 34)) | (1L << (RETI - 34)))) != 0)) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class BiadContext : ParserRuleContext {
		public ITerminalNode INC() { return GetToken(GBASMParser.INC, 0); }
		public ITerminalNode DEC() { return GetToken(GBASMParser.DEC, 0); }
		public ITerminalNode SUB() { return GetToken(GBASMParser.SUB, 0); }
		public ITerminalNode AND() { return GetToken(GBASMParser.AND, 0); }
		public ITerminalNode XOR() { return GetToken(GBASMParser.XOR, 0); }
		public ITerminalNode OR() { return GetToken(GBASMParser.OR, 0); }
		public ITerminalNode CP() { return GetToken(GBASMParser.CP, 0); }
		public ITerminalNode POP() { return GetToken(GBASMParser.POP, 0); }
		public ITerminalNode PUSH() { return GetToken(GBASMParser.PUSH, 0); }
		public ITerminalNode RLC() { return GetToken(GBASMParser.RLC, 0); }
		public ITerminalNode RRC() { return GetToken(GBASMParser.RRC, 0); }
		public ITerminalNode RL() { return GetToken(GBASMParser.RL, 0); }
		public ITerminalNode RR() { return GetToken(GBASMParser.RR, 0); }
		public ITerminalNode SLA() { return GetToken(GBASMParser.SLA, 0); }
		public ITerminalNode SRA() { return GetToken(GBASMParser.SRA, 0); }
		public ITerminalNode SWAP() { return GetToken(GBASMParser.SWAP, 0); }
		public ITerminalNode SRL() { return GetToken(GBASMParser.SRL, 0); }
		public ITerminalNode JP() { return GetToken(GBASMParser.JP, 0); }
		public ITerminalNode JR() { return GetToken(GBASMParser.JR, 0); }
		public BiadContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_biad; } }
		public override void EnterRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.EnterBiad(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.ExitBiad(this);
		}
	}

	[RuleVersion(0)]
	public BiadContext biad() {
		BiadContext _localctx = new BiadContext(Context, State);
		EnterRule(_localctx, 8, RULE_biad);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 48;
			_la = TokenStream.La(1);
			if ( !(((((_la - 28)) & ~0x3f) == 0 && ((1L << (_la - 28)) & ((1L << (JR - 28)) | (1L << (JP - 28)) | (1L << (OR - 28)) | (1L << (CP - 28)) | (1L << (RL - 28)) | (1L << (RR - 28)) | (1L << (INC - 28)) | (1L << (DEC - 28)) | (1L << (SUB - 28)) | (1L << (AND - 28)) | (1L << (XOR - 28)) | (1L << (RLC - 28)) | (1L << (RRC - 28)) | (1L << (POP - 28)) | (1L << (SLA - 28)) | (1L << (SRA - 28)) | (1L << (SRL - 28)) | (1L << (PUSH - 28)) | (1L << (SWAP - 28)))) != 0)) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class TriadContext : ParserRuleContext {
		public ITerminalNode RET() { return GetToken(GBASMParser.RET, 0); }
		public ITerminalNode JR() { return GetToken(GBASMParser.JR, 0); }
		public ITerminalNode JP() { return GetToken(GBASMParser.JP, 0); }
		public ITerminalNode CALL() { return GetToken(GBASMParser.CALL, 0); }
		public ITerminalNode LD() { return GetToken(GBASMParser.LD, 0); }
		public ITerminalNode LDD() { return GetToken(GBASMParser.LDD, 0); }
		public ITerminalNode LDI() { return GetToken(GBASMParser.LDI, 0); }
		public ITerminalNode LDH() { return GetToken(GBASMParser.LDH, 0); }
		public ITerminalNode ADD() { return GetToken(GBASMParser.ADD, 0); }
		public ITerminalNode ADC() { return GetToken(GBASMParser.ADC, 0); }
		public ITerminalNode SBC() { return GetToken(GBASMParser.SBC, 0); }
		public ITerminalNode BIT() { return GetToken(GBASMParser.BIT, 0); }
		public ITerminalNode RES() { return GetToken(GBASMParser.RES, 0); }
		public ITerminalNode SET() { return GetToken(GBASMParser.SET, 0); }
		public TriadContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_triad; } }
		public override void EnterRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.EnterTriad(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.ExitTriad(this);
		}
	}

	[RuleVersion(0)]
	public TriadContext triad() {
		TriadContext _localctx = new TriadContext(Context, State);
		EnterRule(_localctx, 10, RULE_triad);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 50;
			_la = TokenStream.La(1);
			if ( !(((((_la - 27)) & ~0x3f) == 0 && ((1L << (_la - 27)) & ((1L << (LD - 27)) | (1L << (JR - 27)) | (1L << (JP - 27)) | (1L << (LDD - 27)) | (1L << (LDI - 27)) | (1L << (ADD - 27)) | (1L << (ADC - 27)) | (1L << (SBC - 27)) | (1L << (BIT - 27)) | (1L << (RES - 27)) | (1L << (SET - 27)) | (1L << (RET - 27)) | (1L << (LDH - 27)) | (1L << (CALL - 27)))) != 0)) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class ArgContext : ParserRuleContext {
		public RegisterContext register() {
			return GetRuleContext<RegisterContext>(0);
		}
		public ValueContext value() {
			return GetRuleContext<ValueContext>(0);
		}
		public NegvalueContext negvalue() {
			return GetRuleContext<NegvalueContext>(0);
		}
		public FlagContext flag() {
			return GetRuleContext<FlagContext>(0);
		}
		public OffsetContext offset() {
			return GetRuleContext<OffsetContext>(0);
		}
		public ITerminalNode LABEL() { return GetToken(GBASMParser.LABEL, 0); }
		public MemoryContext memory() {
			return GetRuleContext<MemoryContext>(0);
		}
		public ArgContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_arg; } }
		public override void EnterRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.EnterArg(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.ExitArg(this);
		}
	}

	[RuleVersion(0)]
	public ArgContext arg() {
		ArgContext _localctx = new ArgContext(Context, State);
		EnterRule(_localctx, 12, RULE_arg);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 59;
			switch ( Interpreter.AdaptivePredict(TokenStream,2,Context) ) {
			case 1:
				{
				State = 52; register();
				}
				break;
			case 2:
				{
				State = 53; value();
				}
				break;
			case 3:
				{
				State = 54; negvalue();
				}
				break;
			case 4:
				{
				State = 55; flag();
				}
				break;
			case 5:
				{
				State = 56; offset();
				}
				break;
			case 6:
				{
				State = 57; Match(LABEL);
				}
				break;
			case 7:
				{
				State = 58; memory();
				}
				break;
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class MemoryContext : ParserRuleContext {
		public ITerminalNode MEMSTART() { return GetToken(GBASMParser.MEMSTART, 0); }
		public ITerminalNode MEMEND() { return GetToken(GBASMParser.MEMEND, 0); }
		public RegisterContext register() {
			return GetRuleContext<RegisterContext>(0);
		}
		public ValueContext value() {
			return GetRuleContext<ValueContext>(0);
		}
		public MemoryContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_memory; } }
		public override void EnterRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.EnterMemory(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.ExitMemory(this);
		}
	}

	[RuleVersion(0)]
	public MemoryContext memory() {
		MemoryContext _localctx = new MemoryContext(Context, State);
		EnterRule(_localctx, 14, RULE_memory);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 61; Match(MEMSTART);
			State = 64;
			switch (TokenStream.La(1)) {
			case A:
			case B:
			case C:
			case D:
			case E:
			case F:
			case H:
			case L:
			case AF:
			case BC:
			case DE:
			case HL:
			case SP:
			case HLPLUS:
			case HLMINUS:
				{
				State = 62; register();
				}
				break;
			case Integer:
			case Hexval:
				{
				State = 63; value();
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			State = 66; Match(MEMEND);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class OffsetContext : ParserRuleContext {
		public RegisterContext register() {
			return GetRuleContext<RegisterContext>(0);
		}
		public ITerminalNode Plus() { return GetToken(GBASMParser.Plus, 0); }
		public ValueContext value() {
			return GetRuleContext<ValueContext>(0);
		}
		public NegvalueContext negvalue() {
			return GetRuleContext<NegvalueContext>(0);
		}
		public OffsetContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_offset; } }
		public override void EnterRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.EnterOffset(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.ExitOffset(this);
		}
	}

	[RuleVersion(0)]
	public OffsetContext offset() {
		OffsetContext _localctx = new OffsetContext(Context, State);
		EnterRule(_localctx, 16, RULE_offset);
		try {
			State = 75;
			switch ( Interpreter.AdaptivePredict(TokenStream,4,Context) ) {
			case 1:
				EnterOuterAlt(_localctx, 1);
				{
				State = 68; register();
				State = 69; Match(Plus);
				State = 70; value();
				}
				break;
			case 2:
				EnterOuterAlt(_localctx, 2);
				{
				State = 72; register();
				State = 73; negvalue();
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class RegisterContext : ParserRuleContext {
		public ITerminalNode A() { return GetToken(GBASMParser.A, 0); }
		public ITerminalNode B() { return GetToken(GBASMParser.B, 0); }
		public ITerminalNode C() { return GetToken(GBASMParser.C, 0); }
		public ITerminalNode D() { return GetToken(GBASMParser.D, 0); }
		public ITerminalNode E() { return GetToken(GBASMParser.E, 0); }
		public ITerminalNode F() { return GetToken(GBASMParser.F, 0); }
		public ITerminalNode H() { return GetToken(GBASMParser.H, 0); }
		public ITerminalNode L() { return GetToken(GBASMParser.L, 0); }
		public ITerminalNode AF() { return GetToken(GBASMParser.AF, 0); }
		public ITerminalNode BC() { return GetToken(GBASMParser.BC, 0); }
		public ITerminalNode DE() { return GetToken(GBASMParser.DE, 0); }
		public ITerminalNode HL() { return GetToken(GBASMParser.HL, 0); }
		public ITerminalNode SP() { return GetToken(GBASMParser.SP, 0); }
		public ITerminalNode HLPLUS() { return GetToken(GBASMParser.HLPLUS, 0); }
		public ITerminalNode HLMINUS() { return GetToken(GBASMParser.HLMINUS, 0); }
		public RegisterContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_register; } }
		public override void EnterRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.EnterRegister(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.ExitRegister(this);
		}
	}

	[RuleVersion(0)]
	public RegisterContext register() {
		RegisterContext _localctx = new RegisterContext(Context, State);
		EnterRule(_localctx, 18, RULE_register);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 77;
			_la = TokenStream.La(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << A) | (1L << B) | (1L << C) | (1L << D) | (1L << E) | (1L << F) | (1L << H) | (1L << L) | (1L << AF) | (1L << BC) | (1L << DE) | (1L << HL) | (1L << SP) | (1L << HLPLUS) | (1L << HLMINUS))) != 0)) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class FlagContext : ParserRuleContext {
		public ITerminalNode NZ() { return GetToken(GBASMParser.NZ, 0); }
		public ITerminalNode NC() { return GetToken(GBASMParser.NC, 0); }
		public ITerminalNode Z() { return GetToken(GBASMParser.Z, 0); }
		public ITerminalNode C() { return GetToken(GBASMParser.C, 0); }
		public FlagContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_flag; } }
		public override void EnterRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.EnterFlag(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.ExitFlag(this);
		}
	}

	[RuleVersion(0)]
	public FlagContext flag() {
		FlagContext _localctx = new FlagContext(Context, State);
		EnterRule(_localctx, 20, RULE_flag);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 79;
			_la = TokenStream.La(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << Z) | (1L << C) | (1L << NZ) | (1L << NC))) != 0)) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class ValueContext : ParserRuleContext {
		public ITerminalNode Hexval() { return GetToken(GBASMParser.Hexval, 0); }
		public ITerminalNode Integer() { return GetToken(GBASMParser.Integer, 0); }
		public ValueContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_value; } }
		public override void EnterRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.EnterValue(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.ExitValue(this);
		}
	}

	[RuleVersion(0)]
	public ValueContext value() {
		ValueContext _localctx = new ValueContext(Context, State);
		EnterRule(_localctx, 22, RULE_value);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 81;
			_la = TokenStream.La(1);
			if ( !(_la==Integer || _la==Hexval) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class NegvalueContext : ParserRuleContext {
		public ITerminalNode Neg() { return GetToken(GBASMParser.Neg, 0); }
		public ValueContext value() {
			return GetRuleContext<ValueContext>(0);
		}
		public NegvalueContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_negvalue; } }
		public override void EnterRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.EnterNegvalue(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IGBASMListener typedListener = listener as IGBASMListener;
			if (typedListener != null) typedListener.ExitNegvalue(this);
		}
	}

	[RuleVersion(0)]
	public NegvalueContext negvalue() {
		NegvalueContext _localctx = new NegvalueContext(Context, State);
		EnterRule(_localctx, 24, RULE_negvalue);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 83; Match(Neg);
			State = 84; value();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public static readonly string _serializedATN =
		"\x3\x430\xD6D1\x8206\xAD2D\x4417\xAEF1\x8D80\xAADD\x3NY\x4\x2\t\x2\x4"+
		"\x3\t\x3\x4\x4\t\x4\x4\x5\t\x5\x4\x6\t\x6\x4\a\t\a\x4\b\t\b\x4\t\t\t\x4"+
		"\n\t\n\x4\v\t\v\x4\f\t\f\x4\r\t\r\x4\xE\t\xE\x3\x2\x3\x2\x3\x2\x3\x3\x3"+
		"\x3\x3\x3\x3\x3\x5\x3$\n\x3\x3\x4\x3\x4\x3\x4\x3\x4\x3\x4\x3\x4\x3\x4"+
		"\x3\x4\x3\x4\x5\x4/\n\x4\x3\x5\x3\x5\x3\x6\x3\x6\x3\a\x3\a\x3\b\x3\b\x3"+
		"\b\x3\b\x3\b\x3\b\x3\b\x5\b>\n\b\x3\t\x3\t\x3\t\x5\t\x43\n\t\x3\t\x3\t"+
		"\x3\n\x3\n\x3\n\x3\n\x3\n\x3\n\x3\n\x5\nN\n\n\x3\v\x3\v\x3\f\x3\f\x3\r"+
		"\x3\r\x3\xE\x3\xE\x3\xE\x3\xE\x2\x2\xF\x2\x4\x6\b\n\f\xE\x10\x12\x14\x16"+
		"\x18\x1A\x2\b\a\x2$%..:@\x42\x42\x46J\x5\x2\x1E#/\x39\x44\x45\x6\x2\x1D"+
		"\x1F&.\x41\x41\x43\x43\x4\x2\x4\x10\x19\x1A\x5\x2\x3\x3\x6\x6\x11\x12"+
		"\x3\x2\x15\x16V\x2\x1C\x3\x2\x2\x2\x4#\x3\x2\x2\x2\x6.\x3\x2\x2\x2\b\x30"+
		"\x3\x2\x2\x2\n\x32\x3\x2\x2\x2\f\x34\x3\x2\x2\x2\xE=\x3\x2\x2\x2\x10?"+
		"\x3\x2\x2\x2\x12M\x3\x2\x2\x2\x14O\x3\x2\x2\x2\x16Q\x3\x2\x2\x2\x18S\x3"+
		"\x2\x2\x2\x1AU\x3\x2\x2\x2\x1C\x1D\x5\x4\x3\x2\x1D\x1E\a\x2\x2\x3\x1E"+
		"\x3\x3\x2\x2\x2\x1F \x5\x6\x4\x2 !\x5\x4\x3\x2!$\x3\x2\x2\x2\"$\x5\x6"+
		"\x4\x2#\x1F\x3\x2\x2\x2#\"\x3\x2\x2\x2$\x5\x3\x2\x2\x2%/\x5\b\x5\x2&\'"+
		"\x5\n\x6\x2\'(\x5\xE\b\x2(/\x3\x2\x2\x2)*\x5\f\a\x2*+\x5\xE\b\x2+,\aL"+
		"\x2\x2,-\x5\xE\b\x2-/\x3\x2\x2\x2.%\x3\x2\x2\x2.&\x3\x2\x2\x2.)\x3\x2"+
		"\x2\x2/\a\x3\x2\x2\x2\x30\x31\t\x2\x2\x2\x31\t\x3\x2\x2\x2\x32\x33\t\x3"+
		"\x2\x2\x33\v\x3\x2\x2\x2\x34\x35\t\x4\x2\x2\x35\r\x3\x2\x2\x2\x36>\x5"+
		"\x14\v\x2\x37>\x5\x18\r\x2\x38>\x5\x1A\xE\x2\x39>\x5\x16\f\x2:>\x5\x12"+
		"\n\x2;>\aK\x2\x2<>\x5\x10\t\x2=\x36\x3\x2\x2\x2=\x37\x3\x2\x2\x2=\x38"+
		"\x3\x2\x2\x2=\x39\x3\x2\x2\x2=:\x3\x2\x2\x2=;\x3\x2\x2\x2=<\x3\x2\x2\x2"+
		">\xF\x3\x2\x2\x2?\x42\a\x1B\x2\x2@\x43\x5\x14\v\x2\x41\x43\x5\x18\r\x2"+
		"\x42@\x3\x2\x2\x2\x42\x41\x3\x2\x2\x2\x43\x44\x3\x2\x2\x2\x44\x45\a\x1C"+
		"\x2\x2\x45\x11\x3\x2\x2\x2\x46G\x5\x14\v\x2GH\a\x18\x2\x2HI\x5\x18\r\x2"+
		"IN\x3\x2\x2\x2JK\x5\x14\v\x2KL\x5\x1A\xE\x2LN\x3\x2\x2\x2M\x46\x3\x2\x2"+
		"\x2MJ\x3\x2\x2\x2N\x13\x3\x2\x2\x2OP\t\x5\x2\x2P\x15\x3\x2\x2\x2QR\t\x6"+
		"\x2\x2R\x17\x3\x2\x2\x2ST\t\a\x2\x2T\x19\x3\x2\x2\x2UV\a\x17\x2\x2VW\x5"+
		"\x18\r\x2W\x1B\x3\x2\x2\x2\a#.=\x42M";
	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN.ToCharArray());
}
