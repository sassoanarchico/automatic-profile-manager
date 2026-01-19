using System;
using System.Runtime.InteropServices;
using Playnite.SDK;

namespace AutomationProfileManager.Services
{
    public class DisplaySettings
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int RefreshRate { get; set; }
    }

    public class ResolutionService
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private DisplaySettings? originalSettings;

        [DllImport("user32.dll")]
        private static extern int EnumDisplaySettings(string? deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        private static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int CDS_UPDATEREGISTRY = 0x01;
        private const int CDS_TEST = 0x02;
        private const int DISP_CHANGE_SUCCESSFUL = 0;
        private const int DISP_CHANGE_RESTART = 1;

        [StructLayout(LayoutKind.Sequential)]
        private struct DEVMODE
        {
            private const int CCHDEVICENAME = 32;
            private const int CCHFORMNAME = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        public DisplaySettings? GetCurrentSettings()
        {
            try
            {
                DEVMODE dm = new DEVMODE();
                dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

                if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm) != 0)
                {
                    return new DisplaySettings
                    {
                        Width = dm.dmPelsWidth,
                        Height = dm.dmPelsHeight,
                        RefreshRate = dm.dmDisplayFrequency
                    };
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to get current display settings");
            }
            return null;
        }

        public void SaveCurrentSettings()
        {
            originalSettings = GetCurrentSettings();
            if (originalSettings != null)
            {
                logger.Info($"Saved original resolution: {originalSettings.Width}x{originalSettings.Height}@{originalSettings.RefreshRate}Hz");
            }
        }

        public bool ChangeResolution(int width, int height, int refreshRate)
        {
            try
            {
                DEVMODE dm = new DEVMODE();
                dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

                if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm) == 0)
                {
                    logger.Error("Failed to enumerate display settings");
                    return false;
                }

                dm.dmPelsWidth = width;
                dm.dmPelsHeight = height;
                dm.dmDisplayFrequency = refreshRate;
                dm.dmFields = 0x80000 | 0x100000 | 0x400000; // DM_PELSWIDTH | DM_PELSHEIGHT | DM_DISPLAYFREQUENCY

                int testResult = ChangeDisplaySettings(ref dm, CDS_TEST);
                if (testResult != DISP_CHANGE_SUCCESSFUL)
                {
                    logger.Warn($"Resolution change test failed: {width}x{height}@{refreshRate}Hz (code: {testResult})");
                    return false;
                }

                int result = ChangeDisplaySettings(ref dm, CDS_UPDATEREGISTRY);
                if (result == DISP_CHANGE_SUCCESSFUL || result == DISP_CHANGE_RESTART)
                {
                    logger.Info($"Changed resolution to: {width}x{height}@{refreshRate}Hz");
                    return true;
                }

                logger.Error($"Failed to change resolution (code: {result})");
                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception while changing resolution");
                return false;
            }
        }

        public bool RestoreOriginalSettings()
        {
            if (originalSettings == null)
            {
                logger.Warn("No original settings saved to restore");
                return false;
            }

            bool success = ChangeResolution(originalSettings.Width, originalSettings.Height, originalSettings.RefreshRate);
            if (success)
            {
                logger.Info("Restored original resolution");
            }
            return success;
        }

        public static (int width, int height, int refreshRate) ParseResolutionString(string resolution)
        {
            try
            {
                var parts = resolution.Replace("Hz", "").Replace("@", "x").Split('x');
                if (parts.Length >= 2)
                {
                    int width = int.Parse(parts[0].Trim());
                    int height = int.Parse(parts[1].Trim());
                    int refreshRate = parts.Length >= 3 ? int.Parse(parts[2].Trim()) : 60;
                    return (width, height, refreshRate);
                }
            }
            catch { }
            return (1920, 1080, 60);
        }
    }
}
