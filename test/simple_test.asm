; Start with a nop, nops delimit tests
nop
;Disable all interrupts
DI
ld HL,FFFF
ld (HL),0
;Basic loading
nop
ld A,0
ld B,1
ld C,2
ld D,3
ld E,4
ld H,5
ld L,6
;push/pop
nop
push BC
push DE
push HL
pop DE
pop BC
pop HL
;sub
nop
ld A,5
ld B,3
sub A,B
nop
;add
nop
ld A,5
ld C,3
add A,C
;inc/dec
nop
ld B,7
inc B
dec B
;Relative jump
ld A,5
ld B,5
ld C,5
jr 3
ld A,7
ld B,8
ld C,9
;jump
nop
ld A,5
jp jumptest
ld A,8
nop :jumptest
;JR/loop
nop
ld A,5
dec A :looptest
jr nz looptest
ld A,F5
inc A :looptest2
jr nc looptest2






