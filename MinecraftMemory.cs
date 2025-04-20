using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;

namespace LiveSplit.MinecraftAutoSplitter
{
    public class MinecraftMemory
    {
        private Process _process;
        private IntPtr _coordinatesAddress;
        private double _lastY; // Track last known valid Y
        private DateTime _lastUpdateTime; // Track when we last saw a change
        private const int STALE_DATA_TIMEOUT = 1000; // 1 second timeout for stale data

        public MinecraftMemory(Process process)
        {
            _process = process;
            _coordinatesAddress = IntPtr.Zero;
            _lastUpdateTime = DateTime.Now;
        }

         public bool TryGetPlayerCoordinates(out double x, out double y, out double z)
        {
            x = 0; y = 0; z = 0;
            try
            {
                if (_coordinatesAddress == IntPtr.Zero)
                {
                    var addresses = FindAllPossibleCoordinates();
                    foreach (var addr in addresses)
                    {
                        // Try to read and validate coordinates
                        double tempX = _process.ReadValue<double>(addr);
                        double tempY = _process.ReadValue<double>(addr + 8);
                        double tempZ = _process.ReadValue<double>(addr + 16);

                        if (IsValidCoordinate(tempX) && IsValidCoordinate(tempY) && IsValidCoordinate(tempZ))
                        {
                            _coordinatesAddress = addr;
                            _lastY = tempY;
                            _lastUpdateTime = DateTime.Now;
                            x = tempX;
                            y = tempY;
                            z = tempZ;
                            return true;
                        }
                    }
                    return false;
                }

                // Read from existing address
                x = _process.ReadValue<double>(_coordinatesAddress);
                y = _process.ReadValue<double>(_coordinatesAddress + 8);
                z = _process.ReadValue<double>(_coordinatesAddress + 16);

                // Check for stale data
                if (y != _lastY)
                {
                    _lastY = y;
                    _lastUpdateTime = DateTime.Now;
                }
                else if ((DateTime.Now - _lastUpdateTime).TotalMilliseconds > STALE_DATA_TIMEOUT)
                {
                    _coordinatesAddress = IntPtr.Zero;
                    return false;
                }

                return true;
            }
            catch
            {
                _coordinatesAddress = IntPtr.Zero;
                return false;
            }
        }

        private List<IntPtr> FindAllPossibleCoordinates()
        {
            var addresses = new List<IntPtr>();
            try
            {
                // Get all memory pages in the process
                var memoryInfo = new MEMORY_BASIC_INFORMATION();
                IntPtr currentAddress = IntPtr.Zero;

                while (VirtualQueryEx(_process.Handle, currentAddress, out memoryInfo, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION))) != 0)
                {
                    // Check if this memory region is readable
                    if (memoryInfo.State == MEM_COMMIT && 
                        (memoryInfo.Protect & PAGE_READWRITE) != 0)
                    {
                        byte[] buffer = new byte[memoryInfo.RegionSize.ToInt64()];
                        int bytesRead;

                        // Read the memory region
                        if (ReadProcessMemory(_process.Handle, memoryInfo.BaseAddress, buffer, buffer.Length, out bytesRead))
                        {
                            // Scan the buffer for potential coordinate sets
                            for (int i = 0; i < buffer.Length - 24; i += 8) // 24 bytes = 3 doubles
                            {
                                try
                                {
                                    // Read three consecutive doubles
                                    double x = BitConverter.ToDouble(buffer, i);
                                    double y = BitConverter.ToDouble(buffer, i + 8);
                                    double z = BitConverter.ToDouble(buffer, i + 16);

                                    // Basic validation of the values
                                    if (IsValidCoordinate(x) && IsValidCoordinate(y) && IsValidCoordinate(z))
                                    {
                                        // Add the address if it looks like valid coordinates
                                        IntPtr potentialAddress = IntPtr.Add(memoryInfo.BaseAddress, i);
                                        addresses.Add(potentialAddress);
                                    }
                                }
                                catch
                                {
                                    continue;
                                }
                            }
                        }
                    }

                    // Move to next memory region
                    currentAddress = new IntPtr(memoryInfo.BaseAddress.ToInt64() + memoryInfo.RegionSize.ToInt64());
                }
            }
            catch
            {
                // If anything goes wrong, return what we have so far
            }

            return addresses;
        }

        private bool ValidateCoordinateAddress(IntPtr address)
        {
            try
            {
                // Read coordinates
                double x = _process.ReadValue<double>(address);
                double y = _process.ReadValue<double>(address + 8);
                double z = _process.ReadValue<double>(address + 16);

                // Basic range validation
                if (!IsValidCoordinate(x) || !IsValidCoordinate(y) || !IsValidCoordinate(z))
                    return false;

                // Wait a tiny bit and check if values update when moving
                Thread.Sleep(50);
                double newY = _process.ReadValue<double>(address + 8);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidCoordinate(double value)
        {
            return !double.IsInfinity(value) && !double.IsNaN(value) 
                && value > -30000000 && value < 30000000;
        }

        private T ReadMemoryValue<T>(IntPtr address) where T : struct
        {
            return _process.ReadValue<T>(address);
        }

        private byte[] ReadMemoryBytes(IntPtr address, int length)
        {
            return _process.ReadBytes(address, length);
        }

        // P/Invoke declarations for memory reading
        [DllImport("kernel32.dll")]
        private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        private const int MEM_COMMIT = 0x1000;
        private const int PAGE_READWRITE = 0x04;
    }
} 