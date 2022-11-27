#define TRACK_BPM 138

#pragma region Helpers
#include "../os/music.h"

#define NOTE(octave, note, duration) {(octave), (note), (duration * TICKS_PER_SIXTEENTH)}

#define DURATION_WHOLE 16
#define DURATION_HALF 8
#define DURATION_QUARTER 4

#define TREBLE(offset) (OCTAVE_3 + (offset))
#define BASS(offset) (OCTAVE_1 + (offset))

#define REST(duration) NOTE(OCTAVE_1, NOTE_NONE, (duration))
#define WNOTE(octave, note) NOTE((octave), (note), DURATION_WHOLE)
#define HNOTE(octave, note) NOTE((octave), (note), DURATION_HALF)
#define QNOTE(octave, note) NOTE((octave), (note), DURATION_QUARTER)
#pragma endregion

const Note MELODY[] = {
    REST(DURATION_WHOLE * 2),

    // 1
    HNOTE(BASS(0), NOTE_C),
    QNOTE(BASS(0), NOTE_C),
    QNOTE(BASS(1), NOTE_E),

    REST(DURATION_QUARTER),

    QNOTE(BASS(1), NOTE_E),
    QNOTE(BASS(1), NOTE_D),
    QNOTE(BASS(1), NOTE_E),

    HNOTE(BASS(0), NOTE_C),
    QNOTE(BASS(0), NOTE_C),
    QNOTE(BASS(1), NOTE_E),

    REST(DURATION_QUARTER),

    QNOTE(BASS(1), NOTE_E),
    QNOTE(BASS(1), NOTE_D),
    QNOTE(BASS(1), NOTE_E),

    // 2
    HNOTE(BASS(0), NOTE_C),
    QNOTE(BASS(0), NOTE_C),
    QNOTE(BASS(1), NOTE_E),

    REST(DURATION_QUARTER),

    QNOTE(BASS(1), NOTE_E),
    QNOTE(BASS(1), NOTE_D),
    QNOTE(BASS(1), NOTE_E),

    HNOTE(TREBLE(0), NOTE_G),
    QNOTE(TREBLE(0), NOTE_E),
    QNOTE(TREBLE(0), NOTE_G),
    HNOTE(TREBLE(0), NOTE_A),
    QNOTE(TREBLE(0), NOTE_E),
    QNOTE(TREBLE(0), NOTE_A),
    

    // END
    REST(DURATION_WHOLE * 2),
    {END_OCTAVE, END_NOTE, END_DURATION_STOP},
};
