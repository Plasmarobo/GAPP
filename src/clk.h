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


//Abstract cycle conversions
unsigned long const cycles_per_second = machine_frequency;
unsigned long const vsync_cycle_interval = 17555; //Cycles til vsync 
unsigned long const mbc3_rtc_cycles =      mbc3_rtc_frequency/4; //Cycles til tick
unsigned long const timer_cycle_intervals[] = {
    256,
    64,
    16,
    4
}; //Cycles til tick

unsigned long GetUtc(); //Compiles differently on windows vs linux

#endif