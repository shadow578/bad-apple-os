#include "util.h"
#include "screen.h"
#include "idt.h"
#include "isr.h"
#include "irq.h"
#include "timer.h"
#include "font.h"
#include "system.h"
#include "keyboard.h"
#include "speaker.h"
#include "fpu.h"
#include "sound.h"
#include "music.h"
#include "frames.h"

#define FPS 30

void render(const u8 *frame, u32 frameNo)
{
    // draw the frame
    size_t x = 0, y = 0;
    u8 color, repeats;
    while (true)
    {
        // get next value
        color = *(frame++);
        repeats = *(frame++);

        // check for end condition
        if (repeats == 0x0)
            break;

        // draw pixels
        while (repeats--)
        {
            screen_offset(x, y) = color;

            x++;
            if (x >= SCREEN_WIDTH)
            {
                x = 0;
                y++;
            }

            // failsafe, if the data is ok this should not happen
            if (y >= SCREEN_HEIGHT)
                y = 0;
        }
    }

    // draw frame no
    char bufFrameNo[64];
    itoa(frameNo, bufFrameNo, 64);
    font_str(
        bufFrameNo,
        0,
        0,
        COLOR(255, 0, 0));
}

void _main(u32 magic)
{
    idt_init();
    isr_init();
    fpu_init();
    irq_init();
    screen_init();
    timer_init();
    keyboard_init();
    sound_init();

    if (sound_enabled())
    {
        music_init();
        sound_master(255);
    }

    u32 last_frame = 0, last = 0, frameCounter = 0;
    const u8 *thisFrame;

    font_str(
        "READY",
        SCREEN_WIDTH / 2,
        SCREEN_HEIGHT / 2,
        COLOR(255, 0, 0));
    screen_swap();

    while (true)
    {
        const u32 now = (u32)timer_get();

        if (sound_enabled() && now != last)
        {
            //music_tick();
            //last = now;
        }

        if ((now - last_frame) > (TIMER_TPS / FPS))
        {
            last_frame = now;

            // get next frame
            thisFrame = frames[frameCounter];

            // check for frames end condition (index 0 (color)= 0x0)
            if (thisFrame[0] == 0x0)
            {
                // restart playback
                //frameCounter = 0;
                //thisFrame = frames[frameCounter];
                panic("EOF");
            }

            // render the frame
            render(thisFrame, frameCounter);

            // increment frame counter
            frameCounter++;

            // controlled in system.c
            const char *notification = get_notification();
            if (notification != NULL)
            {
                font_str(notification, 0, 0, COLOR(6, 1, 1));
            }

            screen_swap();
        }
    }
}
