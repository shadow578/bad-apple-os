#include "lib/util.h"
#include "lib/screen.h"
#include "lib/timer.h"
#include "lib/font.h"

#define FPS 7
#define WAIT_FRAMES (FPS * 5)
#define next(ptr) (*((ptr)++))
#define FLAG_LAST_RECT 0x8
#define FLAG_B 0x4
#define FLAG_C 0x2
#define FLAG_D 0x1

/**
 * render the full movie
 *
 * @return total frames rendered
 */
u32 render();
