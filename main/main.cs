//using static utils;
//namespace main;
//public class main
//{
//    public static void Main(string[] args)
//    {
//        (int processId, long windowId, string windowsTitle) = Program.ListGameClientProcessesRequest();
//        ulong uiRootAddress = Program.SearchUIRootAddress(processId);
//        Console.WriteLine(uiRootAddress);
//        var getImageData =  new GetImageDataFromReadingStructure
//        {
//            screenshot1x1Rects = new Rect2d[5]
//        };
//        Program.ReadFromWindow(windowId, uiRootAddress, getImageData, processId);
//    }
//}


using System.Runtime.InteropServices;

namespace ClickWindowExample
{
    class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        static void Main(string[] args)
        {
            IntPtr windowHandle = 8982308; // 得到窗口句柄的 ID
            int x = 74; // 相对窗口的 X 坐标
            int y = 346; // 相对窗口的 Y 坐标

            // 发送鼠标左键单击消息
            SendMessage(windowHandle, 0x201, IntPtr.Zero, MakeLParam(x, y));

            // 发送鼠标左键按下消息
            SendMessage(windowHandle, 0x202, IntPtr.Zero, MakeLParam(x, y));

            // 发送鼠标左键弹起消息
            SendMessage(windowHandle, 0x203, IntPtr.Zero, MakeLParam(x, y));
        }

        private static IntPtr MakeLParam(int LoWord, int HiWord)
        {
            return (IntPtr)((HiWord << 16) | (LoWord & 0xFFFF));
        }
    }
}
