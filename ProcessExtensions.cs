using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LiveSplit.MinecraftAutoSplitter
{
    public static class ProcessExtensions
    {
        public static T ReadValue<T>(this Process process, IntPtr address) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var buffer = new byte[size];
            int bytesRead;

            if (!ReadProcessMemory(process.Handle, address, buffer, size, out bytesRead) || bytesRead != size)
            {
                throw new Exception("Failed to read process memory");
            }

            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);
    }
}