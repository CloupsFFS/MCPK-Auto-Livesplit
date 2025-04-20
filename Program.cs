using System;
using System.Diagnostics;
using System.Threading;
using LiveSplit.MinecraftAutoSplitter;

namespace LiveSplit.MinecraftAutoSplitter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Minecraft Auto Splitter Starting...");
            
            try
            {
                Process[] processes = Process.GetProcessesByName("javaw");
                if (processes.Length == 0)
                {
                    Console.WriteLine("Minecraft is not running! Please start Minecraft first.");
                    return;
                }

                var minecraft = processes[0];
                var memory = new MinecraftMemory(minecraft);

                Console.WriteLine("Found Minecraft process. Starting coordinate monitoring...");
                Console.WriteLine("Press Ctrl+C to exit.");

                while (true)
                {
                    if (memory.TryGetPlayerCoordinates(out double x, out double y, out double z))
                    {
                        Console.WriteLine($"Coordinates - X: {x:F2}, Y: {y:F2}, Z: {z:F2}");
                    }
                    else
                    {
                        Console.WriteLine("Searching for coordinates...");
                    }
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}