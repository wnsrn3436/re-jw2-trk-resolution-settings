# Jurassic Primitive War 2: The Ranker Resolution Settings

[한국어](README.ko.md)

A tool that runs Jurassic Primitive War 2: The Ranker at any resolution the monitor supports. Trk Resolution Settings, written in C#, shows the monitor's resolution list and hands the chosen one to Setting.exe, made with GameMaker 8. Setting.exe copies `trk.ex_`, the bundled The Ranker executable, and builds a new executable from it with the screen size and the coordinates of the interface at the bottom of the screen changed to fit that resolution. The addresses from the analysis are in [docs/resolution-patch.md](docs/resolution-patch.md).

When it was distributed, users were told to use it with Jurassic Primitive War 2: The Ranker v2003.12.29 or later.

<p>
  <img src="docs/screenshots/screenshot-1.jpg" width="480" alt="The Ranker running at a wide resolution with the interface at the bottom center">
</p>


## How to use

The Releases download contains Trk Resolution Settings.exe, Setting.exe, DB.dat, and trk.ex_. Put the four files in one folder and run Trk Resolution Settings.exe.

Pick a monitor in the upper list and the lower list shows the resolutions that monitor supports. The Set button works only when the primary monitor is selected. Choose a resolution and press Set: the window closes and a save dialog opens, with the resolution in the default file name, as in `TheRanker 1280 x 1024.exe`. Save it in the folder where The Ranker is installed and start the game with that executable. The original executable is left as it is.

The interface moves by however much the screen has grown past 800x600, toward the center horizontally and downward vertically.


## How it works

**The resolution list comes from the Win32 API.** The tool reads the display devices one by one with `EnumDisplayDevices`, then calls `EnumDisplaySettings` with the chosen device's name for mode numbers from 0 until the call fails, collecting `width x height` strings. Modes that differ only in refresh rate or color depth produce the same string, so that string goes in only once. Set is enabled only when the device's `StateFlags` has the primary device bit (4).

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

**`trk.ex_` is The Ranker's executable with its code already edited to read the resolution and interface coordinates from data.** The code at `0x4D6E97` that sets the screen width and height reads the values at `0x8635CC` and `0x8635C8`. The interface coordinate table is read at `ecx * 0x60 + race * 0x18`, and the function that reads it sets `ecx` to 1 right at the start. As a result only the slots where `ecx` is 1 are used, and the first slot for `ecx` 0 (`0x8635C8`, `0x8635CC`) is where the resolution values go. Setting.exe takes the coordinates in the slots where `ecx` is 1 as values for 800x600 and moves them. The edited spots remain as sections filled with `NOP`.

**Setting.exe edits only the copy's data.** It splits the `width x height` argument into numbers, reads the hex addresses in `DB.dat`, and copies `trk.ex_` under the name chosen in the save dialog. It writes the width and height to the first two addresses and adds `ypos = height - 600` and half of `xpos = width - 800` to the other coordinates. The addresses are virtual, so subtracting the image base `0x400000` gives the file position. The coordinate table slots are at each `DB.dat` address plus `0x60 + 0x18 * race`, meaning the four race slots where `ecx` is 1. The first line of each pair is y and the second is x.

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

For coordinate tables not in `DB.dat`, the addresses are written directly in the code, and the x and y in their slots where `ecx` is 1 are moved the same way. GameMaker 8's `file_bin_*` functions work one byte at a time, so the `file_bin_read_int` and `file_bin_write_int` scripts pack and unpack 4-byte integers in little endian.


## Files

| Path | Content |
|---|---|
| `source/Trk Resolution Settings/` | Visual Studio solution for Trk Resolution Settings (C#, .NET Framework 4.5) |
| `source/Setting.gmk` | GameMaker 8 project file for Setting.exe |
| `source/split/` | Text tree split out with GmkSplitter |
| `docs/resolution-patch.md` | Resolution and interface coordinate addresses |
| `docs/screenshots/` | Screenshots |
| Releases | Trk Resolution Settings.exe, Setting.exe, DB.dat, trk.ex_, and usage instructions |


## Credits

The display device and resolution enumeration code of Trk Resolution Settings (the `DISPLAY_DEVICE` and `DEVMODE` structs, `EnumDevices`, `EnumModes`, `GetDeviceName`, `MainDevice`, and the `user32.dll` declarations) was taken from example 374 on news2news.com by Anatoliy Mogylevets.


## License

zlib. See [LICENSE](LICENSE). Bundled libraries made by other people keep their own licenses.
