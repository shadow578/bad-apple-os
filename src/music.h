#ifndef MUSIC_H
#define MUSIC_H
#include "lib/util.h"
#include "lib/sound.h"
#include "lib/timer.h"

#define END_OCTAVE 0xff
#define END_NOTE 0xff
#define END_DURATION_STOP 0x00
#define END_DURATION_LOOP 0xff

#ifndef TRACK_BPM
#define TRACK_BPM 100
#endif

#define TRACK_BPS (TRACK_BPM / 60.0)
#define TICKS_PER_BEAT (TIMER_TPS / TRACK_BPS)
#define TICKS_PER_SIXTEENTH (TICKS_PER_BEAT / 16.0)

typedef struct Note
{
    u8 octave;
    u8 note;
    u16 durationInTicks;
} Note;

typedef struct ActiveNote
{
    Note *note;
    u32 index;
    u32 remainingTicks;
} ActiveNote;

extern const Note MELODY[];

void music_init();

void music_tick(u32 ticks);
#endif