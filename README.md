# Jurassic Primitive War 2: The Ranker Resolution Settings

A tool that runs Jurassic Primitive War 2: The Ranker at any resolution the monitor supports. Trk Resolution Settings, written in C#, shows the monitor's resolution list and hands the chosen one to Setting.exe, made with GameMaker 8. Setting.exe copies the bundled The Ranker executable `trk.ex_` into a new executable whose screen size and bottom interface coordinates are edited for that resolution. The analyzed addresses are in [docs/resolution-patch.md](docs/resolution-patch.md).

It was distributed for Jurassic Primitive War 2: The Ranker v2003.12.29 or later.

<p>
  <img src="docs/screenshots/screenshot-1.jpg" width="480" alt="The Ranker running at a wide resolution with the interface at the bottom center">
</p>


## How to use

The Releases download contains Trk Resolution Settings.exe, Setting.exe, DB.dat, and trk.ex_. Keep the four files in one folder and run Trk Resolution Settings.exe.

Picking a monitor in the upper list fills the lower list with the resolutions it supports. The Set button is enabled only when the primary monitor is picked. Picking a resolution and pressing Set closes the window and opens a save dialog. The default name carries the resolution, such as `TheRanker 1280 x 1024.exe`. Save it in the folder where The Ranker is installed and start the game with that executable. The original executable stays as it is.

As the screen grows beyond 800x600, the interface moves horizontally to the center and vertically to the bottom.


## How it works

**The resolution list comes from the Win32 API.** `EnumDisplayDevices` reads the display devices one by one, and `EnumDisplaySettings` is called with the chosen device's name from mode number 0 until it fails, collecting `width x height` strings. Modes that differ only in refresh rate or color depth give the same string, so each is added once. Set is enabled only when the device's `StateFlags` has the primary device bit (4).

```csharp
do
{
    result = EnumDisplaySettings(devName, 
        modeNum, ref devMode);

    if (result)
    {
        string item = DevmodeToString(devMode);
        if (listSettings.FindString(item, 0)==-1)
        {
            listSettings.Items.Add(item);
        }
    }
    modeNum++;
} while (result);
```

Set wraps the chosen string in quotes and passes it to Setting.exe as an argument.

```csharp
char temps = (char)34;
System.Diagnostics.Process program = 
    System.Diagnostics.Process.Start("Setting.exe", temps.ToString() + listSettings.SelectedItem + temps.ToString());
```

**`trk.ex_` is an executable edited to read the resolution and interface coordinates from data.** The code at `0x4D6E97` that sets the screen width and height reads the values at `0x8635CC` and `0x8635C8`. The interface coordinate table is read from the slot `ecx * 0x60 + race * 0x18`, and the function that reads it sets `ecx` to 1 at its start. So only the slots for `ecx` 1 are used, and the first slot for `ecx` 0 (`0x8635C8`, `0x8635CC`) becomes the place to keep the resolution. Setting.exe treats the coordinates in the `ecx` 1 slots as 800x600-based values and moves them. The edited places remain as runs filled with `NOP`.

**Setting.exe edits only the data of the copy.** It splits the `width x height` argument into numbers, reads the hex addresses in `DB.dat`, and copies `trk.ex_` to the name chosen in the save dialog. It writes the width and height to the first two addresses and adds half of `xpos = width - 800` and `ypos = height - 600` to the other coordinates. The addresses are virtual addresses, so the file position is found by subtracting the image base `0x400000`. The coordinate table slots are each `DB.dat` address plus `0x60 + 0x18 * race`, which are the four race slots for `ecx` 1. The first line of a pair is y and the second x.

```gml
for(i=0; i!=4; i+=1)
{
    int = (24*i)+96-4194304
    for(j=0; j!=ds_list_size(db); j+=1)
    {
        file_bin_seek(file, ds_list_find_value(db, j)+int)
        int2 = file_bin_read_int(file)
        file_bin_seek(file, ds_list_find_value(db, j)+int)
        if(j mod 2){
            file_bin_write_int(file, int2+round(xpos/2))
        }
        else{
            file_bin_write_int(file, int2+ypos)
        }
    }
}
```

Coordinate tables not listed in `DB.dat` have their addresses written directly in the code, and the x and y of their `ecx` 1 slots are moved the same way. The `file_bin_*` functions of GameMaker 8 work in bytes, so 4-byte integers are packed and unpacked in little endian by the `file_bin_read_int` and `file_bin_write_int` scripts.


## Files

| Path | Content |
|---|---|
| `source/Trk Resolution Settings/` | Visual Studio solution of Trk Resolution Settings (C#, .NET Framework 4.5) |
| `source/Setting.gmk` | GameMaker 8 project file of Setting.exe |
| `source/split/` | Text tree produced by GmkSplitter |
| `docs/resolution-patch.md` | Resolution and interface coordinate addresses |
| `docs/screenshots/` | Screenshots |
| Releases | Trk Resolution Settings.exe, Setting.exe, DB.dat, trk.ex_, and a manual |


## Credits

The display device and resolution enumeration code of Trk Resolution Settings (the `DISPLAY_DEVICE` and `DEVMODE` structs, `EnumDevices`, `EnumModes`, `GetDeviceName`, `MainDevice`, and the `user32.dll` declarations) was taken from example 374 on news2news.com by Anatoliy Mogylevets.


## License

zlib. See [LICENSE](LICENSE). Bundled libraries made by other people keep their own licenses.
