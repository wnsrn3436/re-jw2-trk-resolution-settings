# The Ranker resolution patch

Setting.exe copies the bundled `trk.ex_` under a new name, then edits a few places in the copy's data to change the resolution and the position of the interface. `trk.ex_` is the executable of Jurassic Primitive War 2: The Ranker, with its code already edited so that it reads the resolution and interface coordinates from data. The edited spots are left as runs of `NOP` (`0x90`).

All addresses are virtual addresses. The image base is `0x400000`, and the `.data` section maps virtual address `0x700000` to file offset `0x300000`, so a file offset is the virtual address minus `0x400000`. Setting.exe also subtracts 4194304 (`0x400000`) from every address to find the file position.


## Resolution

The screen width and height live at `0x143FFF0` and `0x1440004`, and the code at `0x407416` sets them to 800 and 600.

```
C7 05 F0 FF 43 01 20 03 00 00    mov dword ptr [0x143FFF0], 800
C7 05 04 00 44 01 58 02 00 00    mov dword ptr [0x1440004], 600
```

At `0x4D6E97`, `trk.ex_` reads the values at `0x8635CC` and `0x8635C8`, stores them in these two variables, and calls the function `0x4F4060` with them as arguments. Everything between the reads and the writes, from `0x4D6EA3` through `0x4D6EDC`, is `NOP`.

```
8B 15 CC 35 86 00    mov edx, [0x8635CC]
8B 1D C8 35 86 00    mov ebx, [0x8635C8]
90 ...               nop
89 15 CC 34 73 00    mov [0x7334CC], edx
89 1D D0 34 73 00    mov [0x7334D0], ebx
89 15 F0 FF 43 01    mov [0x143FFF0], edx
89 1D 04 00 44 01    mov [0x1440004], ebx
E8 ...               call
FF 35 EC FF 43 01    push [0x143FFEC]
FF 35 04 00 44 01    push [0x1440004]
FF 35 F0 FF 43 01    push [0x143FFF0]
FF 35 6C FF 43 01    push [0x143FF6C]
E8 49 D1 01 00       call 0x4F4060
```

Setting.exe writes the width to `0x8635CC` and the height to `0x8635C8`. These two addresses are the first two lines of `DB.dat`.


## Interface coordinate table

The coordinates of the interface at the bottom of the screen are in a table starting at `0x8635C8`. The code at `0x4E2BD3` reads coordinates from the slot `ecx * 0x60 + race * 0x18`, and the race number is at `0x725C04`.

```
2B C9                   sub ecx, ecx
41                      inc ecx
90 ...                  nop
89 0D 88 35 86 00       mov [0x863588], ecx
6B C1 60                imul eax, ecx, 0x60
6B 1D 04 5C 72 00 18    imul ebx, [0x725C04], 0x18
03 C3                   add eax, ebx
A3 10 5C 72 00          mov [0x725C10], eax
8B 90 C8 35 86 00       mov edx, [eax+0x8635C8]
8B 98 CC 35 86 00       mov ebx, [eax+0x8635CC]
```

In `trk.ex_`, this function begins by setting `ecx` to 1 at `0x4E2BB7`. The table is therefore always read from `0x60 + race * 0x18`, and `0x8635C8` and `0x8635CC`, the slot for `ecx` 0, are where the resolution is kept. The same `ecx` value is stored at `0x863588` and is also used by code that reads other tables, such as `0x4E2FA2`.

The 54 addresses in `DB.dat` are base addresses into this table. Two lines form a pair: the first is y and the second is x. They fall into 9 groups of 3 pairs, spaced `0x138` apart starting at `0x8635C8`.

| Group | Base addresses (y, x) |
|---|---|
| 1 | `0x8635CC, 0x8635C8` `0x8635D4, 0x8635D0` `0x8635DC, 0x8635D8` |
| 2 | `0x863704, 0x863700` `0x86370C, 0x863708` `0x863714, 0x863710` |
| 3 | `0x86383C, 0x863838` `0x863844, 0x863840` `0x86384C, 0x863848` |
| 4 | `0x863974, 0x863970` `0x86397C, 0x863978` `0x863984, 0x863980` |
| 5 | `0x863AAC, 0x863AA8` `0x863AB4, 0x863AB0` `0x863ABC, 0x863AB8` |
| 6 | `0x863BE4, 0x863BE0` `0x863BEC, 0x863BE8` `0x863BF4, 0x863BF0` |
| 7 | `0x863D1C, 0x863D18` `0x863D24, 0x863D20` `0x863D2C, 0x863D28` |
| 8 | `0x863E54, 0x863E50` `0x863E5C, 0x863E58` `0x863E64, 0x863E60` |
| 9 | `0x863F8C, 0x863F88` `0x863F94, 0x863F90` `0x863F9C, 0x863F98` |


## Values Setting.exe edits

When a resolution `W x H` is chosen, Setting.exe computes `xpos = W - 800` and `ypos = H - 600`. Adding `ypos` to each y and `xpos / 2` to each x of the 800x600 coordinates moves the interface to the bottom center of the screen.

| Address | Value | Edit |
|---|---|---|
| `0x8635CC`, `0x8635C8` | Width, height | Writes `W` and `H` |
| Each `DB.dat` address + `0x60 + 0x18 * race` (race 0 to 3) | Interface coordinate table | Adds `ypos` to y and `round(xpos / 2)` to x |
| `0x8640A8`, `0x8640AC` | x, y | Writes `19 + round(xpos / 2)` and `518 + ypos`. Read at `0x4E5C48` |
| 4 values from `0x8635A0` | One value per race | Adds `ypos`. `0x4E2CF7` subtracts it from the screen height |
| `0x8642DC + 16`, `0x8642E0 + 16` | x, y | Adds `round(xpos / 2)` to x and `ypos` to y |
| `0x8642E4 + 16`, `0x8642E8 + 16` | x, y | Same as above |
| `0x864314 + 8`, `0x864318 + 8` | x, y | Same as above |
| `0x86444C`, `0x864450` + `8 * n` (n from 16 to 31) | 16 x, y pairs | Same as above |
| `0x864170`, `0x864174` + `8 * n` (n from 14 to 27) | 14 x, y pairs | Same as above |
| `0x864354`, `0x864358` + `8 * n` (n from 5 to 9) | 5 x, y pairs | Same as above |
| `0x8645FC`, `0x864600` + `8 * n` (n from 6 to 11) | 6 x, y pairs | Same as above |
| `0x8646C4`, `0x8646C8` + `8 * n` (n from 7 to 13) | 7 x, y pairs | Same as above |

Both `+ 16` and `+ 8 * n` select the slot for `ecx` 1. `0x4E2FA2` shifts `ecx` left by 4 bits to read the `0x8642DC` table, so one slot is 16 bytes there, and in a table of n pairs per slot the slot for `ecx` 1 starts at the n-th pair.


## Other addresses in the notes

These addresses appear in the working notes but are not covered above.

| Address | Note | What was found |
|---|---|---|
| `0x4D284D` | Resolution | The `push [0x143FF6C]` right after `push 600` and `push 800`. This is where the same function `0x4F4060` is called with the constants 800 and 600 |
| `0x4E02A5` | Interface drawing | The second of the four function calls made by the function at `0x4E02A0` |
| `0x4E2F98` | Single-player picture | Inside the `NOP` run from `0x4E2F8E` to `0x4E2FA1` |
| `0x420580` | Fog | Start of a function that reads `0x7071A8` and `0x7071AC`. Setting.exe does not edit this function. Fog display at resolutions above 1024x768 still needs to be checked |
| `0x7071A8` | x view | |
| `0x863598` | x view maximum | |
