using read_memory_64_bit;
using System.Runtime.InteropServices;
using static utils;

namespace main;
public class main
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static public extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")]
    static extern bool GetCursorPos(out POINT lpPoint);
    [DllImport("user32.dll")]
    static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

    static void Main(string[] args)
    {
        var scaleX = PrimaryScreen.PrimaryScreen.ScaleX;
        (int processId, long windowId, string windowsTitle) = Program.ListGameClientProcessesRequest();

        ulong uiRootAddress = Program.SearchUIRootAddress(processId);
        var memoryReadingSerialRepresentationJson = Program.ReadFromWindow(windowId, uiRootAddress, processId);
        File.WriteAllText("111111111111111.json", memoryReadingSerialRepresentationJson);
        Console.WriteLine("done");

        //var memoryReadingSerialRepresentationJson = File.ReadAllText("1.txt");
        //var memoryReadingSerialRepresentationJson = File.ReadAllText("memory-reading.json");
        //JObject json = JObject.Parse(memoryReadingSerialRepresentationJson);
        //PrintJson(json);
        //Console.WriteLine();

        var windowHandle = new IntPtr(windowId);

        // 将客户端坐标转换为屏幕坐标
        POINT clientPoint = new(); // 在客户端坐标系下的坐标
        clientPoint.x = 1507;
        clientPoint.y = 198;
        ClientToScreen(windowHandle, ref clientPoint);
        //clientPoint.x = (int)(clientPoint.x * 1.5);
        //clientPoint.y = (int)(clientPoint.y * 1.5);

        Console.WriteLine("{0}:{1}", clientPoint.x , clientPoint.y);
        SetCursorPos(clientPoint.x,clientPoint.y);
    }
}