#ifndef MUSIC_H
#define MUSIC_H
#include "lib/util.h"
#include "lib/sound.h"
#include "lib/timer.h"

#include "lib/screen.h"
#include "lib/font.h"

#define END_OCTAVE 0xff
#define END_NOTE 0xff
#define END_DURATION_STOP 0x00
#define END_DURATION_LOOP 0xff

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

void music_init();

void music_tick(u32 ticks);

#define TRACK_BPM 150
#define TRACK_BPS (TRACK_BPM / 60.0)
#define TICKS_PER_BEAT (TIMER_TPS / TRACK_BPS)
#define TICKS_PER_SIXTEENTH (TICKS_PER_BEAT / 16.0)
#define NOTE(octave, note, duration)                       \
    {                                                      \
        (octave), (note), (duration * TICKS_PER_SIXTEENTH) \
    }

static const struct Note MELODY[] = {
    NOTE(OCTAVE_5, NOTE_E, 16),
    NOTE(OCTAVE_4, NOTE_B, 8),
    NOTE(OCTAVE_5, NOTE_C, 8),
    NOTE(OCTAVE_5, NOTE_D, 16),

    NOTE(OCTAVE_5, NOTE_C, 8),
    NOTE(OCTAVE_4, NOTE_B, 8),
    NOTE(OCTAVE_4, NOTE_A, 16),

    NOTE(OCTAVE_4, NOTE_A, 8),
    NOTE(OCTAVE_5, NOTE_C, 8),
    NOTE(OCTAVE_5, NOTE_E, 16),

    NOTE(OCTAVE_5, NOTE_D, 8),
    NOTE(OCTAVE_5, NOTE_C, 8),
    NOTE(OCTAVE_4, NOTE_B, 16),

    NOTE(OCTAVE_4, NOTE_B, 8),
    NOTE(OCTAVE_5, NOTE_C, 8),
    NOTE(OCTAVE_5, NOTE_D, 16),
    NOTE(OCTAVE_5, NOTE_E, 16),
    NOTE(OCTAVE_5, NOTE_C, 16),
    NOTE(OCTAVE_4, NOTE_A, 16),
    NOTE(OCTAVE_4, NOTE_A, 16),
    NOTE(OCTAVE_4, NOTE_NONE, 24),

    NOTE(OCTAVE_5, NOTE_D, 16),
    NOTE(OCTAVE_5, NOTE_F, 8),
    NOTE(OCTAVE_5, NOTE_A, 8),
    NOTE(OCTAVE_5, NOTE_A, 4),
    NOTE(OCTAVE_5, NOTE_A, 4),
    NOTE(OCTAVE_5, NOTE_G, 8),
    NOTE(OCTAVE_5, NOTE_F, 8),
    NOTE(OCTAVE_5, NOTE_E, 16),
    NOTE(OCTAVE_5, NOTE_NONE, 8),

    NOTE(OCTAVE_5, NOTE_C, 8),
    NOTE(OCTAVE_5, NOTE_E, 8),
    NOTE(OCTAVE_5, NOTE_E, 4),
    NOTE(OCTAVE_5, NOTE_E, 4),
    NOTE(OCTAVE_5, NOTE_D, 8),
    NOTE(OCTAVE_5, NOTE_C, 8),
    NOTE(OCTAVE_4, NOTE_B, 16),

    NOTE(OCTAVE_4, NOTE_B, 8),
    NOTE(OCTAVE_5, NOTE_C, 8),
    NOTE(OCTAVE_5, NOTE_D, 16),
    NOTE(OCTAVE_5, NOTE_E, 16),
    NOTE(OCTAVE_5, NOTE_C, 16),
    NOTE(OCTAVE_4, NOTE_A, 16),
    NOTE(OCTAVE_4, NOTE_A, 16),
    NOTE(OCTAVE_4, NOTE_NONE, 16),

    // END
    {END_OCTAVE, END_NOTE, END_DURATION_LOOP},
};
#endif