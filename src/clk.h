#ifndef CLK_H
#define CLK_H

unsigned long const cpu_frequency =      4194304; //Hz
unsigned long const machine_frequency =  1048576; //Hz
unsigned long const hsync_frequency =    9198000; //Hz
unsigned long const vsync_frequency =    60; //Hz, actually 59.73
unsigned long const mbc3_rtc_frequency = 32768; //Hz
unsigned long const timer_frequencies[] = {
    4096, 
    16384, 
    65536,
    262144
}; //Hz
unsigned long const div_frequency = 16384; //Hz

//Abstract cycle conversions
unsigned long const cycles_per_second =    machine_frequency;
const double machine_period = 1.0 / (double) machine_frequency;
unsigned long const scan_sprite_cycles = 20;
unsigned long const scan_vram_cycles = 43;
unsigned long const hblank_cycles = 51;
unsigned long const vblank_cycles = 1140; 
unsigned long const render_cycles = 17556;
unsigned long const mbc3_rtc_cycles =      mbc3_rtc_frequency/4; //Cycles til tick
unsigned long const timer_cycle_intervals[] = {
    256,
    64,
    16,
    4
}; //Cycles til tick
unsigned long const div_cycles = 64;

unsigned long GetUtc(); //Compiles differently on windows vs linux

#endif