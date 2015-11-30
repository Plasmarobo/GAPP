grammar GBASM;

eval : exp EOF;
exp : exp op | exp sys | op | sys;

op : monad | biad arg | triad arg SEPARATOR arg;
sys : include | section | data | label;

triad : JR|JP|CALL|LD|LDD|LDI|LDH|ADD|ADC|SBC|BIT|RES|SET;

biad : INC|DEC|SUB|AND|XOR|OR|CP|POP|PUSH|RLC|RRC|RL|RR|SLA|SRA|SWAP|SRL|JP|JR|STOP|RST|RET;

monad : NOP|RLCA|RRCA|STOP|RLA|RRA|DAA|CPL|SCF|CCF|HALT|RETI|DI|EI|RET|STOP;

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

value : Number;
negvalue : (Neg Number);


Neg : '-';
Plus : '+';
Number : Digit+ | '0' [xX] HexDigit+ | '$' HexDigit+ | HexDigit+ [hH];
fragment HexDigit : Digit | ('a'..'f') | ('A'..'F');
fragment Digit : ('0'..'9');


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
RST : 'RST' | 'rst';
CALL : 'CALL' | 'call';

PUSH : 'PUSH' | 'push';

SWAP : 'SWAP' | 'swap';
RLCA : 'RLCA' | 'rlca';
RRCA : 'RRCA' | 'rrca';
STOP : 'STOP' | 'stop';
HALT: 'HALT' | 'halt';
RETI: 'RETI' | 'reti';

HOME: 'HOME';
SECTION: 'SECTION';
INCLUDE: 'INCLUDE';

STRINGLITERAL : '"' ~["\r\n]* '"';
LIMSTRING : ('_'|'a'..'z'|'A'..'Z')+;
SEPARATOR : ',';
WS : (' '|'\t'|'\n'|'\r') ->channel(HIDDEN);
COMMENT : ';' ~('\n'|'\r')* '\r'? '\n' ->channel(HIDDEN);