using System.Runtime.InteropServices;

static public class WinApi
{

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static public extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("kernel32.dll", SetLastError = true)]
    static public extern bool CloseHandle(IntPtr hHandle);
    
    //  https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-memory_basic_information
    public enum MemoryInformationType : int
    {
        MEM_IMAGE = 0x1000000,
        MEM_MAPPED = 0x40000,
        MEM_PRIVATE = 0x20000,
    }

    
    [DllImport("kernel32.dll")]
    static public extern bool ReadProcessMemory(IntPtr hProcess, ulong lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, ref UIntPtr lpNumberOfBytesRead);

    
    //  https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-memory_basic_information
    public enum MemoryInformationState : int
    {
        MEM_COMMIT = 0x1000,
        MEM_FREE = 0x10000,
        MEM_RESERVE = 0x2000,
    }
    
    //  https://docs.microsoft.com/en-au/windows/win32/memory/memory-protection-constants
    [Flags]
    public enum MemoryInformationProtection : int
    {
        PAGE_EXECUTE = 0x10,
        PAGE_EXECUTE_READ = 0x20,
        PAGE_EXECUTE_READWRITE = 0x40,
        PAGE_EXECUTE_WRITECOPY = 0x80,
        PAGE_NOACCESS = 0x01,
        PAGE_READONLY = 0x02,
        PAGE_READWRITE = 0x04,
        PAGE_WRITECOPY = 0x08,
        PAGE_TARGETS_INVALID = 0x40000000,
        PAGE_TARGETS_NO_UPDATE = 0x40000000,

        PAGE_GUARD = 0x100,
        PAGE_NOCACHE = 0x200,
        PAGE_WRITECOMBINE = 0x400,
    }

    //  http://www.pinvoke.net/default.aspx/kernel32.virtualqueryex
    //  https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-memory_basic_information
    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_BASIC_INFORMATION64
    {
        public ulong BaseAddress;
        public ulong AllocationBase;
        public int AllocationProtect;
        public int __alignment1;
        public ulong RegionSize;
        public int State;
        public int Protect;
        public int Type;
        public int __alignment2;
    }

    [Flags]
    public enum ProcessAccessFlags : uint
    {
        All = 0x001F0FFF,
        Terminate = 0x00000001,
        CreateThread = 0x00000002,
        VirtualMemoryOperation = 0x00000008,
        VirtualMemoryRead = 0x00000010,
        VirtualMemoryWrite = 0x00000020,
        DuplicateHandle = 0x00000040,
        CreateProcess = 0x000000080,
        SetQuota = 0x00000100,
        SetInformation = 0x00000200,
        QueryInformation = 0x00000400,
        QueryLimitedInformation = 0x00001000,
        Synchronize = 0x00100000
    }

    [DllImport("kernel32.dll")]
    static public extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION64 lpBuffer, uint dwLength);

    [DllImport("kernel32.dll")]
    static public extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public int x;
        public int y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    static public extern bool SetProcessDPIAware();

    [DllImport("user32.dll")]
    static public extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    /*
    https://stackoverflow.com/questions/19867402/how-can-i-use-enumwindows-to-find-windows-with-a-specific-caption-title/20276701#20276701
    https://stackoverflow.com/questions/295996/is-the-order-in-which-handles-are-returned-by-enumwindows-meaningful/296014#296014
    */
    public static System.Collections.Generic.IReadOnlyList<IntPtr> ListWindowHandlesInZOrder()
    {
        IntPtr found = IntPtr.Zero;
        System.Collections.Generic.List<IntPtr> windowHandles = new System.Collections.Generic.List<IntPtr>();

        EnumWindows(delegate (IntPtr wnd, IntPtr param)
        {
            windowHandles.Add(wnd);

            // return true here so that we iterate all windows
            return true;
        }, IntPtr.Zero);

        return windowHandles;
    }

    [DllImport("user32.dll")]
    static public extern IntPtr ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    static public extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

    [DllImport("user32.dll", SetLastError = false)]
    static public extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    static public extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

    [DllImport("user32.dll", SetLastError = true)]
    static public extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
}
