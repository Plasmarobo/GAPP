grammar GBASM;

eval : exp EOF;
exp : op exp | op;

op : cmd | regop (register|wideregister|memory|value) | jumpflag | complexop;

jumpflag : flagop (memory|value|Label|flag) | flagop flag Separator (memory|value|Label);
complexop : complex loc Separator loc;

cmd : NOP|RLCA|RRCA|STOP|RLA|RRA|DAA|CPL|SCF|CCF|HALT|RETI|DI|EI|RST|RET;

regop : INC|DEC|SUB|AND|XOR|OR|CP|POP|PUSH|RLC|RRC|RL|RR|SLA|SRA|SWAP|SRL;

flagop : RET | JR | JP | CALL;

complex : LD|LDD|LDI|LDH|ADD|ADC|SBC|BIT|RES|SET;

loc : (register|wideregister|value|negvalue|flag|offset|Label|memory);

memory : MEMSTART (register|wideregister|value) MEMEND;

offset : (register|wideregister) Plus value | (register|wideregister) negvalue;

register : A|B|C|D|E|F|H|L;

wideregister : AF|BC|DE|HL|SP;

flag : NZ | NC | Z | C;

Letters : ('a'..'z'|'A'..'Z')+;


Z : 'Z';

A : 'A';
B : 'B';
C : 'C';
D : 'D';
E : 'E';
F : 'F';
H : 'H';
L : 'L';
AF : 'AF';
BC : 'BC';
DE : 'DE';
HL : 'HL';
SP : 'SP';
NZ : 'NZ';
NC : 'NC';

RST_VALUE : '0x' RST_DIGITS | RST_DIGITS 'H';
RST_DIGITS : '00' | '10' | '20' | '30' | '08' | '18' | '28' | '38';

Label : ':' Letters;


value : (Hexval|Integer);
negvalue : Neg value;


Integer : (Digit+);
Hexval  : ('0x' HexDigit+)|(HexDigit+ ('h'|'H'));
Neg : '-';
Plus : '+';
fragment HexDigit : ('0'..'9'|'a'..'f'|'A'..'F');
fragment Digit : '0'..'9';

HLplus : 'HL+' | 'HLI';
HLminus : 'HL-' | 'HLD';
MEMSTART : '(';
MEMEND : ')';

LD : 'LD' | 'ld';
JR : 'JR' | 'jr';
JP : 'JP' | 'jp';
OR : 'OR' | 'or';
CP : 'CP' | 'cp';
RL : 'RL' | 'rl';
RR : 'RR' | 'rr';
DI : 'DI' | 'di';
EI : 'EI' | 'ei';

LDD : 'LDD' | 'ldd';
LDI : 'LDI' | 'ldi';
ADD: 'ADD' | 'add';
ADC : 'ADC' | 'adc';
SBC : 'SBC' | 'sbc';
BIT : 'BIT' | 'bit';
RES : 'RES' | 'res';
SET : 'SET' | 'set';
RET: 'RET' | 'ret';
INC : 'INC' | 'inc';
DEC : 'DEC' | 'dec';
SUB : 'SUB' | 'sub';
AND : 'AND' | 'and';
XOR : 'XOR' | 'xor';
RLC : 'RLC' | 'rlc';
RRC : 'RRC' | 'rrc';
POP: 'POP' | 'pop';

SLA : 'SLA' | 'sla';
SRA : 'SRA' | 'sra';

SRL : 'SRL' | 'srl';
NOP : 'NOP' | 'nop';
RLA : 'RLA' | 'rla';
RRA : 'RRA' | 'rra';
DAA : 'DAA' | 'daa';
CPL : 'CPL' | 'cpl';
SCF : 'SCF' | 'scf';
CCF : 'CCF' | 'ccf';
LDH : 'LDH' | 'ldh';
RST : 'RST' RST_VALUE | 'rst' RST_VALUE;
CALL : 'CALL' | 'call';

PUSH : 'PUSH' | 'push';

SWAP : 'SWAP' | 'swap';
RLCA : 'RLCA' | 'rlca';
RRCA : 'RRCA' | 'rrca';
STOP : 'STOP 0' | 'STOP' | 'stop 0' | 'stop';
HALT: 'HALT' | 'halt';
RETI: 'RETI' | 'reti';


Separator : ',';
WS : (' '|'\t'|'\n'|'\r') ->skip;
Comment : ';' ~('\n'|'\r')* '\r'? '\n' ->skip;