using System;
using System.Runtime.InteropServices;
using System.Text;

namespace WpfApp6.Services.Launch
{
    internal class Injector // unused injector, works tho
    {
        public static void Inject(int processId, string path)
        {
            IntPtr hProcess = Win32.OpenProcess(1082, false, processId);
            IntPtr procAddress = Win32.GetProcAddress(Win32.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            uint num1 = (uint)((path.Length + 1) * Marshal.SizeOf(typeof(char)));
            IntPtr num2 = Win32.VirtualAllocEx(hProcess, IntPtr.Zero, num1, 12288U, 4U);
            Win32.WriteProcessMemory(hProcess, num2, Encoding.Default.GetBytes(path), num1, out UIntPtr _);
            Win32.CreateRemoteThread(hProcess, IntPtr.Zero, 0U, procAddress, num2, 0U, IntPtr.Zero);
        }
    }
}