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
#include "music.h"

char buf[64];

void onRenderTick(u32 deltaTime)
{
    music_tick(deltaTime);
}

void onRenderFrame(u32 frame, u32 deltaTime)
{
    // draw frame no
    itoa(frame, buf, 64);
    font_str(
        buf,
        0,
        0,
        COLOR(255, 0, 0));

    // draw frame time
    itoa(deltaTime, buf, 64);
    font_str(
        buf,
        0,
        10,
        COLOR(255, 0, 0));

    // controlled in system.c
    const char *notification = get_notification();
    if (notification != NULL)
    {
        font_str(notification, 0, 20, COLOR(6, 1, 1));
    }
}

void _main(u32 magic)
{
    // init kernel
    idt_init();
    isr_init();
    irq_init();
    screen_init();
    timer_init();
    keyboard_init();
    music_init();

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
    u32 frameCount = render(onRenderTick, onRenderFrame);

    // draw "end"
    screen_clear(COLOR(0, 0, 0));
    font_str(
        "END",
        SCREEN_WIDTH / 2,
        SCREEN_HEIGHT / 2,
        COLOR(255, 255, 255));
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
