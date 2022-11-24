#include "lib/util.h"
#include "lib/screen.h"
#include "lib/idt.h"
#include "lib/isr.h"
#include "lib/irq.h"
#include "lib/timer.h"
#include "lib/font.h"
#include "lib/system.h"
#include "lib/keyboard.h"
#include "sleep.h"
#include "renderer.h"

void _main(u32 magic)
{
    // init kernel
    idt_init();
    isr_init();
    irq_init();
    screen_init();
    timer_init();
    keyboard_init();

    // draw "ready"
    screen_clear(COLOR(0, 0, 0));
    font_str(
        "READY",
        SCREEN_WIDTH / 2,
        SCREEN_HEIGHT / 2,
        COLOR(255, 255, 255));
    screen_swap();
    sleep(5);

    // render the full movie
    u32 frameCount = render();

    // draw "end"
    screen_clear(COLOR(0, 0, 0));
    font_str(
        "END",
        SCREEN_WIDTH / 2,
        SCREEN_HEIGHT / 2,
        COLOR(255, 255, 255));
    char buf[64];
    itoa(frameCount, buf, 64);
    font_str(
        buf,
        0,
        0,
        COLOR(255, 0, 0));
    screen_swap();
    while (true)
        ;
}
