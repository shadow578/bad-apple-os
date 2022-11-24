#include "sleep.h"

void sleep(u32 seconds)
{
    u64 start = timer_get(),
        now;
    do
    {
        now = timer_get();
    } while ((now - start) < (TIMER_TPS * seconds));
}
