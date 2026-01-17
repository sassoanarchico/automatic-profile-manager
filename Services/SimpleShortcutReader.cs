using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AutomationProfileManager.Services
{
    // Simple LNK file reader using Windows Shell32 COM
    public class SimpleShortcutReader
    {
        [ComImport]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShow);
            void SetShowCmd(int iShow);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([Out, MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([Out, MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPersistFile
        {
            [PreserveSig]
            int GetClassID(out Guid pClassID);
            [PreserveSig]
            int IsDirty();
            [PreserveSig]
            int Load([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
            [PreserveSig]
            int Save([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [In, MarshalAs(UnmanagedType.Bool)] bool fRemember);
            [PreserveSig]
            int SaveCompleted([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
            [PreserveSig]
            int GetCurFile([In, MarshalAs(UnmanagedType.LPWStr)] string ppszFileName);
        }

        private const int MAX_PATH = 260;
        private const int STGM_READ = 0;

        public static string GetShortcutTarget(string shortcutPath)
        {
            try
            {
                if (!File.Exists(shortcutPath) || !shortcutPath.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                var link = (IShellLink)new ShellLink();
                var file = (IPersistFile)link;
                file.Load(shortcutPath, STGM_READ);
                
                var path = new StringBuilder(MAX_PATH);
                link.GetPath(path, path.Capacity, out IntPtr pfd, 0);
                
                return path.ToString();
            }
            catch
            {
                return null;
            }
        }

        [ComImport]
        [ClassInterface(ClassInterfaceType.None)]
        [Guid("00021401-0000-0000-C000-000000000046")]
        private class ShellLink
        {
        }
    }
}
