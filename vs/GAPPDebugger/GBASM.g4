grammar GBASM;

eval : exp EOF;
exp : exp op | exp sys | op | sys;

sys : include | section | label | data;
op : monad | biad arg | triad arg SEPARATOR arg;

monad : NOP|RLCA|RRCA|STOP|RLA|RRA|DAA|CPL|SCF|CCF|HALT|RETI|DI|EI|RST|RET;

biad : INC|DEC|SUB|AND|XOR|OR|CP|POP|PUSH|RLC|RRC|RL|RR|SLA|SRA|SWAP|SRL|JP|JR;

triad : RET|JR|JP|CALL|LD|LDD|LDI|LDH|ADD|ADC|SBC|BIT|RES|SET;

arg : (register|value|negvalue|flag|offset|jump|memory);

memory : MEMSTART (register|value|jump) MEMEND;

offset : register Plus value | register negvalue;

register : A|B|C|D|E|F|H|L|AF|BC|DE|HL|SP|HLPLUS|HLMINUS;

flag : NZ | NC | Z | C;

data : DB db;
db : string_data | value | string_data SEPARATOR db | value SEPARATOR db;
include : INCLUDE string_data;
section : SECTION string_data SEPARATOR HOME '[' value ']';

string_data: STRINGLITERAL;
jump : LIMSTRING;
label : LIMSTRING ':';

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

value : (Hexval|Integer);
negvalue : Neg value;

Integer : (Digit+);
Hexval  : ('0x' HexDigit+)|(HexDigit+ ('h'|'H'))|('$' HexDigit+);
Neg : '-';
Plus : '+';
fragment HexDigit : ('0'..'9'|'a'..'f'|'A'..'F');
fragment Digit : '0'..'9';

HLPLUS : 'HL+' | 'HLI';
HLMINUS : 'HL-' | 'HLD';
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

DB : 'DB';

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

HOME: 'HOME';
SECTION: 'SECTION';
INCLUDE: 'INCLUDE';

STRINGLITERAL : '"' ~["\r\n]* '"';
LIMSTRING : ('_'|'a'..'z'|'A'..'Z'|'0'..'9')+;
SEPARATOR : ',';
WS : (' '|'\t'|'\n'|'\r') ->channel(HIDDEN);
COMMENT : ';' ~('\n'|'\r')* '\r'? '\n' ->channel(HIDDEN);