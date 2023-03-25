using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using static utils;
using static WinApi;

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
    public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static public extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")]
    static extern bool GetCursorPos(out POINT lpPoint);
    [DllImport("user32.dll")]
    static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);
    [DllImport("user32.dll")]
    static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);
    [DllImport("user32.dll")]
    static extern int GetDpiForWindow(IntPtr hWnd);

    public static void Main(string[] args)
    {
        var scaleX = PrimaryScreen.PrimaryScreen.ScaleX;

        (int processId, long windowId, string windowsTitle) = Program.ListGameClientProcessesRequest();
        //ulong uiRootAddress = Program.SearchUIRootAddress(processId);
        //Console.WriteLine(uiRootAddress);
        //var getImageData = new GetImageDataFromReadingStructure
        //{
        //    screenshot1x1Rects = new Rect2d[5]
        //};
        //var memoryReadingSerialRepresentationJson = Program.ReadFromWindow(windowId, uiRootAddress, getImageData, processId);
        //File.WriteAllText("111111111111111.json", memoryReadingSerialRepresentationJson);
        //Console.WriteLine("done");

        //var memoryReadingSerialRepresentationJson = File.ReadAllText("1.txt");
        //var memoryReadingSerialRepresentationJson = File.ReadAllText("memory-reading.json");
        //JObject json = JObject.Parse(memoryReadingSerialRepresentationJson);
        //PrintJson(json);
        //Console.WriteLine();

        //var windowHandle = new IntPtr(windowId);
        //// 获取窗口客户端坐标系中的一个点
        //POINT clientPoint = new POINT();
        //clientPoint.X = 283;
        //clientPoint.Y = 270;
        //Console.WriteLine(scaleX);
        //ClientToScreen(windowHandle, ref clientPoint);


        var windowHandle = new IntPtr(windowId);

        // 将客户端坐标转换为屏幕坐标
        POINT clientPoint = new POINT(); // 在客户端坐标系下的坐标
        clientPoint.x = 1507;
        clientPoint.y = 198;
        ClientToScreen(windowHandle, ref clientPoint);
        Console.WriteLine("{0}:{1}", clientPoint.x , clientPoint.y);
        SetCursorPos(clientPoint.x,clientPoint.y);

        //// 获取窗口客户端坐标系中的一个点
        //POINT clientPoint = new POINT();
        //clientPoint.X = 1507;
        //clientPoint.Y = 198;
        ////ClientToScreen(windowHandle, ref clientPoint);
        //ScreenToClient(windowHandle, ref clientPoint);
        //Console.WriteLine("屏幕坐标系中的点：({0}, {1})", clientPoint.X, clientPoint.Y);
        //SetCursorPos(clientPoint.X, clientPoint.Y);
    }
}