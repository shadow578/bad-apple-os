: script to build the image using WSL, then 
: launch qemu
: %1 is forwarded to the make command
@echo off
: build image
wsl make %*

: start qemu
if "%~1" == "img" (
    goto :launch_qemu
)
if "%~2" == "img" (
    goto :launch_qemu
)

goto:eof



:launch_qemu
qemu-system-i386 -drive format=raw,file=boot.img -display sdl -audiodev sdl,id=audio0 -machine pcspk-audiodev=audio0
