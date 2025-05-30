UNAME := $(shell uname)

ifeq ($(UNAME),Linux)
	CC=gcc -elf_i386
	AS=as --32
	LD=ld -m elf_i386
else
	CC=i386-elf-gcc
	AS=i386-elf-as
	LD=i386-elf-ld
endif

GFLAGS=
CCFLAGS=-m32 -std=c11 -O2 -g -Wall -Wextra -Wpedantic -Wstrict-aliasing
CCFLAGS+=-Wno-pointer-arith -Wno-unused-parameter
CCFLAGS+=-nostdlib -nostdinc -ffreestanding -fno-pie -fno-stack-protector
CCFLAGS+=-fno-builtin-function -fno-builtin
ASFLAGS=
LDFLAGS=

BOOTSECT_SRCS=\
	src/lib/boot/stage0.S

BOOTSECT_OBJS=$(BOOTSECT_SRCS:.S=.o)

KERNEL_C_SRCS=\
	$(wildcard src/*.c) \
	$(wildcard src/**/*.c)
KERNEL_S_SRCS=$(filter-out $(BOOTSECT_SRCS), $(wildcard src/lib/boot/*.S))
KERNEL_OBJS=$(KERNEL_C_SRCS:.c=.o) $(KERNEL_S_SRCS:.S=.o)

BOOTSECT=bootsect.bin
KERNEL=kernel.bin
IMG=boot.img

all: dirs bootsect kernel

clean:
	rm -f ./**/*.o
	rm -f ./**/**/*.o
	rm -f ./*.img
	rm -f ./**/*.elf
	rm -f ./**/*.bin

%.o: %.c
	$(CC) -o $@ -c $< $(GFLAGS) $(CCFLAGS)

%.o: %.S
	$(AS) -o $@ -c $< $(GFLAGS) $(ASFLAGS)

dirs:
	mkdir -p bin

bootsect: $(BOOTSECT_OBJS)
	$(LD) -o ./bin/$(BOOTSECT) $^ -Ttext 0x7C00 --oformat=binary

kernel: $(KERNEL_OBJS)
	$(LD) -o ./bin/$(KERNEL) $^ $(LDFLAGS) -Tsrc/lib/link.ld

img: dirs bootsect kernel
	dd if=/dev/zero of=$(IMG) bs=512 count=2880
	dd if=./bin/$(BOOTSECT) of=$(IMG) conv=notrunc bs=512 seek=0 count=1
	dd if=./bin/$(KERNEL) of=$(IMG) conv=notrunc bs=512 seek=1 count=2048

qemu-mac: img
	qemu-system-i386 -drive format=raw,file=$(IMG) -d cpu_reset -monitor stdio -device sb16 -audiodev coreaudio,id=coreaudio,out.frequency=48000,out.channels=2,out.format=s32

qemu-pulse: img
	qemu-system-i386 -drive format=raw,file=$(IMG) -d cpu_reset -monitor stdio -device sb16 -audiodev pulseaudio,id=pulseaudio,out.frequency=48000,out.channels=2,out.format=s32

qemu-sdl: img
	qemu-system-i386 -display sdl -drive format=raw,file=$(IMG) -d cpu_reset -monitor stdio -audiodev sdl,id=sdl,out.frequency=48000,out.channels=2,out.format=s32 -device sb16,audiodev=sdl

qemu-no-audio: img
	qemu-system-i386 -drive format=raw,file=$(IMG) -d cpu_reset -monitor stdio

