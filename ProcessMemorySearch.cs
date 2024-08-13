using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MxbcRobOrderWinFormsApp
{
    internal static class ProcessMemorySearch
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize,
            out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        private const int ProcessVmRead = 0x0010;

        /// <summary>
        /// 在指定进程的内存中搜索字符串。
        /// </summary>
        /// <param name="processName">要搜索的进程名称。</param>
        /// <param name="searchString">要搜索的字符串。</param>
        /// <param name="readLen">要读取的内存长度，单位为字节，以获取完整的字符串 </param>
        /// <returns>找到的字符串，如果未找到则返回 null。</returns>
        public static string? SearchProcessMemory(string processName, string searchString, int readLen)
        {
            // 将字符串转换为字节数组
            var searchBytes = Encoding.ASCII.GetBytes(searchString);

            // 获取所有匹配进程名的进程
            var processes = Process.GetProcessesByName(processName);
            
            // 循环查找每个进程
            foreach (var process in processes)
            {
                // 打开进程以供读取
                var processHandle = OpenProcess(ProcessVmRead, false, process.Id);

                if (processHandle == IntPtr.Zero)
                {
                    // 如果打开进程失败，则记录错误并继续查找下一个进程
                    Console.WriteLine($@"无法打开进程: {processName} (PID: {process.Id})");
                    continue;
                }

                try
                {
                    // 获取进程内存大小
                    var memorySize = process.MainModule!.ModuleMemorySize;
                    // 创建缓冲区来存储读取的内存
                    var buffer = new byte[memorySize];
                    // 读取进程内存
                    ReadProcessMemory(processHandle, process.MainModule.BaseAddress, buffer, memorySize,
                        out var bytesRead);
                    // 在缓冲区中搜索字符串
                    var index = IndexOf(buffer, searchBytes);
                    if (index != -1)
                    {
                        // 从找到的位置开始读取更多内存，以获取完整的字符串
                        var extraBuffer = new byte[readLen];
                        ReadProcessMemory(processHandle,
                            process.MainModule.BaseAddress + index + searchBytes.Length, extraBuffer,
                            readLen, out bytesRead);
                        // 将找到的字符串和额外读取的内存转换为字符串
                        var foundString = Encoding.UTF8.GetString(buffer, index, searchBytes.Length + bytesRead);
                        Console.WriteLine("找到字符串：" + foundString);
                        return foundString;
                    }
                    else
                    {
                        // 如果未找到字符串，则记录错误
                        Console.WriteLine($@"未找到Token (PID: {process.Id})");
                    }
                }
                finally
                {
                    // 关闭进程句柄
                    CloseHandle(processHandle);
                }
            }

            // 如果在所有进程中都未找到字符串，则返回 null
            return null;
        }


        // 在字节数组中查找另一个字节数组
        private static int IndexOf(byte[] haystack, byte[] needle)
        {
            for (int i = 0; i <= haystack.Length - needle.Length; i++)
            {
                var match = !needle.Where((t, j) => haystack[i + j] != t).Any();

                if (match)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}