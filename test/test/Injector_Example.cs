// Example injector code - This shows how to use your DLL with RemoteHooking.Inject
// This is just for reference, you would put this in your injector application

using System;
using System.Diagnostics;
using EasyHook;

class InjectorExample
{
    static void Main(string[] args)
    {
        try
        {
            // Get target process ID (replace with your method of getting the PID)
            int targetPID = GetTargetProcessId("notepad"); // example: hook notepad
            
            if (targetPID == 0)
            {
                Console.WriteLine("Target process not found!");
                return;
            }

            // Path to your compiled DLL
            string dllPath = @"c:\Users\hathh\Downloads\test\test\bin\Release\test.dll";
            
            Console.WriteLine($"Injecting into process {targetPID}...");
            
            // This is the injection call you mentioned
            RemoteHooking.Inject(
                targetPID,
                dllPath,
                dllPath // 32-bit and 64-bit path, same if process matches
            );
            
            Console.WriteLine("Injection successful! Press any key to exit.");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Injection failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
    
    static int GetTargetProcessId(string processName)
    {
        Process[] processes = Process.GetProcessesByName(processName);
        if (processes.Length > 0)
        {
            return processes[0].Id;
        }
        return 0;
    }
}
