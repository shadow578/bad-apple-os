#include "music.h"

ActiveNote activeNote;
bool isFinished = false;

void setActiveNote(u32 i)
{
    Note n = MELODY[i];
    activeNote.note = &n;
    activeNote.index = i;
    activeNote.remainingTicks = n.durationInTicks;
}

void music_init()
{
    sound_init();
    setActiveNote(0);
    isFinished = false;
}

void music_tick(u32 ticks)
{
    // if finished playing, do nothing
    if (isFinished)
        return;

    // if there are no more ticks left, move on to the next note
    if (activeNote.remainingTicks <= ticks)
    {
        // set new note
        setActiveNote(activeNote.index + 1);
        Note *n = activeNote.note;

        // check if END note
        if (n->note == END_NOTE && n->octave == END_OCTAVE)
        {
            if (n->durationInTicks == END_DURATION_STOP)
            {
                // STOP
                isFinished = true;
                sound_set_enabled(false);
                return;
            }
            else if (n->durationInTicks == END_DURATION_LOOP)
            {
                // LOOP
                isFinished = false;
                setActiveNote(0);
            }
        }

        // update the currently playing note
        sound_note(0, n->octave, n->note);
    }
    else
    {
        // remove ticks from current note
        activeNote.remainingTicks -= ticks;
    }

    // tick sound
    sound_tick();
}
