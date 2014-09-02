using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using Microsoft.VisualBasic;

namespace DisplaySettings
{
    public partial class Form1 : Form
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAY_DEVICE
        {
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public int StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
            public string DeviceKey;

            public DISPLAY_DEVICE(int flags)
            {
                cb = 0;
                StateFlags = flags;
                DeviceName = new string((char)32, 32);
                DeviceString = new string((char)32, 128);
                DeviceID = new string((char)32, 128);
                DeviceKey = new string((char)32, 128);
                cb = Marshal.SizeOf(this);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public short dmOrientation;
            public short dmPaperSize;
            public short dmPaperLength;
            public short dmPaperWidth;
            public short dmScale;
            public short dmCopies;
            public short dmDefaultSource;
            public short dmPrintQuality;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmUnusedPadding;
            public short dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
        }

        public Form1()
        {
            InitializeComponent();
            EnumDevices();
        }

        private void EnumModes(int devNum)
        {
            listSettings.Items.Clear();

            string devName = GetDeviceName(devNum);
            DEVMODE devMode = new DEVMODE();
            int modeNum = 0;
            bool result = true;
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
        }

        private DEVMODE GetDevmode(int devNum, int modeNum)
        { //populates DEVMODE for the specified device and mode
            DEVMODE devMode = new DEVMODE();
            string devName = GetDeviceName(devNum);
            EnumDisplaySettings(devName, modeNum, ref devMode);
            return devMode;
        }

        private string DevmodeToString(DEVMODE devMode)
        {
            return devMode.dmPelsWidth.ToString() +
                " x " + devMode.dmPelsHeight.ToString();
        }

        private void EnumDevices()
        { //populates Display Devices list
            this.listDevices.Items.Clear();
            DISPLAY_DEVICE d = new DISPLAY_DEVICE(0);

            int devNum = 0;
            bool result;
            do
            {
                result = EnumDisplayDevices(IntPtr.Zero, 
                    devNum, ref d, 0);

                if (result)
                {
                    string item = devNum.ToString() + 
                        ". " + d.DeviceString.Trim();
                    if ((d.StateFlags & 4) != 0) item += " - main";
                    this.listDevices.Items.Add(item);
                }
                devNum++;
            } while (result);
        }

        private string GetDeviceName(int devNum)
        {
            DISPLAY_DEVICE d = new DISPLAY_DEVICE(0);
            bool result = EnumDisplayDevices(IntPtr.Zero, 
                devNum, ref d, 0);
            return (result ? d.DeviceName.Trim() : "#error#");
        }

        private bool MainDevice(int devNum)
        { //whether the specified device is the main device
            DISPLAY_DEVICE d = new DISPLAY_DEVICE(0);
            if (EnumDisplayDevices(IntPtr.Zero, devNum, ref d, 0))
            {
                return ((d.StateFlags & 4) != 0);
            } return false;
        }

        private void listDevices_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            int devNum = listDevices.SelectedIndex;
            bool isMain = MainDevice(devNum);
            btnSet.Enabled = isMain; // enable only for the main device
            EnumModes(devNum);
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            char temps = (char)34;
            System.Diagnostics.Process program = 
                System.Diagnostics.Process.Start("Setting.exe", temps.ToString() + listSettings.SelectedItem + temps.ToString());

            this.Close();
        }

        [DllImport("User32.dll")]
        private static extern bool EnumDisplayDevices(
            IntPtr lpDevice, int iDevNum, 
            ref DISPLAY_DEVICE lpDisplayDevice, int dwFlags);

        [DllImport("User32.dll")]
        private static extern bool EnumDisplaySettings(
            string devName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        public static extern int ChangeDisplaySettings(
            ref DEVMODE devMode, int flags);

    }

}
