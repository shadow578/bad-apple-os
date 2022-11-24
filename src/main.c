#include "lib/util.h"
#include "lib/screen.h"
#include "lib/idt.h"
#include "lib/isr.h"
#include "lib/irq.h"
#include "lib/timer.h"
#include "lib/font.h"
#include "lib/system.h"
#include "lib/keyboard.h"
#include "frames.h"

#define FPS 7
#define WAIT_FRAMES (FPS * 5)
#define next(ptr) (*((ptr)++))
#define FLAG_LAST_RECT 0x8
#define FLAG_B 0x4
#define FLAG_C 0x2
#define FLAG_D 0x1

char buf[64];

/**
 * working configs for rectangles renderer:
 * skip | count | nth   | min_rect_size | max_rect_count    | est. size
 * 0    | 250   | 3     | 100           | 100               | 80k
 * 0    | MAX   | 15    | 100           | 50                | 100k
 * 
 * seems like we are limited by the number of sectors loaded in stage0 (~130k)...
 * ---
 * stage0 now loads 0x5FF sectors without too much complaining, which should give us ~780k to work with.
 * however, going over ~400k starts breaking the screen buffer (idk)
 * 
 * skip | count | nth   | min_rect_size | max_rect_count    | est. size
 * 0    | -     | 5     | 100           | 70                | 372k      
 * 0    | -     | 5     | 100           | 76                | 394k  
 *      -> using max_rect_count = 77 (est. size= 397k) starts breaking the screen
 * 
 * final config:
 * 0    | -     | 5     | 50            | 70                | 392k 
 */
bool render(const u8 *rects, u32 frameNo)
{
    /*
     * format of the rects data is as follows:
     * - first two bytes are the screen and rectangle colors
     * - following that are 1..n rectangles:
     *  - 5 bytes per rectangle, with x,y,w,h each with 9 bit and 4 bit flags
     *  - like so: ABCDxxxx xxxxxyyy yyyyyyww wwwwwwwh hhhhhhhh
     *  - flag A indicates that this is the last rectangle, and following it is the start of a new frame
     *  - flags BCD are not used
     * - end condition: screen and rect color are equal
     */
    // get screen and rectangle color
    u8 screenColor = next(rects);
    u8 rectColor = next(rects);

    // check for end condition
    if (screenColor == rectColor)
        return true;

    // clear the screen with the screen color
    screen_clear(screenColor);

    // read rectangle data until we hit the last rectangle
    u64 data = 0;
    size_t x, y, w, h;
    u8 flags;
    u8 rectsCount = 0;
    do
    {
        // read 5 bytes into data
        for (u8 i = 0; i < 5; i++)
        {
            data <<= 8;
            data |= next(rects);
        }

        // extract values
        flags = (data >> 36) & 0xF;
        x = (data >> 27) & 0x1FF;
        y = (data >> 18) & 0x1FF;
        w = (data >> 9) & 0x1FF;
        h = data & 0x1FF;

        // draw rectangle to screen
        for (size_t sx = x; sx < (x + w); sx++)
            for (size_t sy = y; sy < (y + h); sy++)
                screen_offset(sx, sy) = rectColor;

        rectsCount++;
    } while ((flags & FLAG_LAST_RECT) == 0);

    // draw frame no
    itoa(frameNo, buf, 64);
    font_str(
        buf,
        0,
        0,
        COLOR(255, 0, 0));

    // draw number of rects
    itoa(rectsCount, buf, 64);
    font_str(
        buf,
        0,
        10,
        COLOR(255, 0, 0));

    return false;
}

void _main(u32 magic)
{
    idt_init();
    isr_init();
    irq_init();
    screen_init();
    timer_init();
    keyboard_init();

    screen_clear(COLOR(0, 0, 0));
    font_str(
        "READY",
        SCREEN_WIDTH / 2,
        SCREEN_HEIGHT / 2,
        COLOR(255, 255, 255));
    screen_swap();

    u32 last_frame = 0, waitFrames = 0, frameCounter = 0;
    const u8 *rects;

    while (true)
    {
        const u32 now = (u32)timer_get();

        if ((now - last_frame) > (TIMER_TPS / FPS))
        {
            last_frame = now;

            if (waitFrames > WAIT_FRAMES)
            {
                // get rects for next frame
                rects = rectData[frameCounter];

                // render next frame
                bool eof = render(rects, frameCounter);

                // increment frame counter
                frameCounter++;

                // pause on last frame
                if (eof)
                {
                    screen_clear(COLOR(0, 0, 0));
                    font_str(
                        "END",
                        SCREEN_WIDTH / 2,
                        SCREEN_HEIGHT / 2,
                        COLOR(255, 255, 255));
                    itoa(frameCounter, buf, 64);
                    font_str(
                        buf,
                        0,
                        0,
                        COLOR(255, 0, 0));
                    screen_swap();
                    while (true)
                        ;
                }
            }
            else
                waitFrames++;

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
