using System;
using System.Runtime.InteropServices;
using WindowsInput;
using static WinApi;

public class SetForegroundWindowInWindows
{


    static public int AltKeyPlusSetForegroundWindowWaitTimeMilliseconds = 60;

    /// <summary>
    /// </summary>
    /// <param name="windowHandle"></param>
    /// <returns>null in case of success</returns>
    static public string TrySetForegroundWindow(IntPtr windowHandle)
    {
        try
        {
            /*
            * For the conditions for `SetForegroundWindow` to work, see https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setforegroundwindow
            * */
            WinApi.SetForegroundWindow(windowHandle);

            if (WinApi.GetForegroundWindow() == windowHandle)
                return null;

            var windowsInZOrder = WinApi.ListWindowHandlesInZOrder();

            var windowIndex = windowsInZOrder.ToList().IndexOf(windowHandle);

            if (windowIndex < 0)
                return "Did not find window for this handle";

            {
                //Using names from Windows API and < https://www.nuget.org/packages/InputSimulator/>
                var inputSimulator = new InputSimulator();
                var simulator = new WindowsInput.InputSimulator();

                simulator.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.MENU);
                WinApi.SetForegroundWindow(windowHandle);
                simulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.MENU);

                System.Threading.Thread.Sleep(AltKeyPlusSetForegroundWindowWaitTimeMilliseconds);

                if (WinApi.GetForegroundWindow() == windowHandle)
                    return null;

                return "Alt key plus SetForegroundWindow approach was not successful.";
            }
        }
        catch (Exception e)
        {
            return "Exception: " + e.ToString();
        }
    }
}


public class utils
{
    public class EffectSequenceElement
    {
        public EffectOnWindowStructure effect;

        public int? delayMilliseconds;
    }

    public class EffectOnWindowStructure
    {
        public MouseMoveToStructure MouseMoveTo;

        public KeyboardKey KeyDown;

        public KeyboardKey KeyUp;
    }

    public class KeyboardKey
    {
        public int virtualKeyCode;
    }

    public class MouseMoveToStructure
    {
        public Location2d location;
    }

    public enum MouseButton
    {
        left, right,
    }

    public struct Rect2d
    {
        public int x, y, width, height;
    }

    public struct GetImageDataFromReadingStructure
    {
        public Rect2d[] screenshot1x1Rects;
    }
    
    public struct ImageCrop
    {
        public Location2d offset;

        public int[][] pixels_R8G8B8;
    }

    public struct GetImageDataFromReadingResultStructure
    {
        public ImageCrop[] screenshot1x1Rects;
    }

    public static int[][] CopyRectangularCrop(int[][] original, Rect2d rect)
    {
        return
            original
            .Skip(rect.y)
            .Take(rect.height)
            .Select(rowPixels =>
            {
                if (rect.x == 0 && rect.width == rowPixels.Length)
                    return rowPixels;

                var cropRowPixels = new int[rect.width];

                System.Buffer.BlockCopy(rowPixels, rect.x * 4, cropRowPixels, 0, rect.width * 4);

                return cropRowPixels;
            })
            .ToArray();
    }

    public static GetImageDataFromReadingResultStructure CompileImageDataFromReadingResult(
    GetImageDataFromReadingStructure request,
    ReadingFromGameClient historyEntry)
    {
        ImageCrop[] screenshot1x1Rects = null;

        if (historyEntry.pixels_1x1_R8G8B8 != null)
        {
            screenshot1x1Rects =
                request.screenshot1x1Rects
                .Select(rect =>
                {
                    var cropPixels = CopyRectangularCrop(historyEntry.pixels_1x1_R8G8B8, rect);

                    return new ImageCrop
                    {
                        pixels_R8G8B8 = cropPixels,
                        offset = new Location2d { x = rect.x, y = rect.y },
                    };
                }).ToArray();
        }

        return new GetImageDataFromReadingResultStructure
        {
            screenshot1x1Rects = screenshot1x1Rects,
        };
    }
    
    public struct ReadingFromGameClient
    {
        public IntPtr windowHandle;

        public string readingId;

        public int[][] pixels_1x1_R8G8B8;
    }

    public static System.Drawing.Bitmap GetScreenshotOfWindowAsBitmap(IntPtr windowHandle)
    {
        SetProcessDPIAware();

        var windowRect = new WinApi.Rect();
        if (WinApi.GetWindowRect(windowHandle, ref windowRect) == IntPtr.Zero)
            return null;

        int width = windowRect.right - windowRect.left;
        int height = windowRect.bottom - windowRect.top;

        var asBitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

        System.Drawing.Graphics.FromImage(asBitmap).CopyFromScreen(
            windowRect.left,
            windowRect.top,
            0,
            0,
            new System.Drawing.Size(width, height),
            System.Drawing.CopyPixelOperation.SourceCopy);

        return asBitmap;
    }

    public static void SetProcessDPIAware()
    {
        //  https://www.google.com/search?q=GetWindowRect+dpi
        //  https://github.com/dotnet/wpf/issues/859
        //  https://github.com/dotnet/winforms/issues/135
        WinApi.SetProcessDPIAware();
    }

    public static int[][] GetScreenshotOfWindowAsPixelsValues_R8G8B8(IntPtr windowHandle)
    {
        var screenshotAsBitmap = GetScreenshotOfWindowAsBitmap(windowHandle);

        if (screenshotAsBitmap == null)
            return null;

        var bitmapData = screenshotAsBitmap.LockBits(
            new System.Drawing.Rectangle(0, 0, screenshotAsBitmap.Width, screenshotAsBitmap.Height),
            System.Drawing.Imaging.ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format24bppRgb);

        int byteCount = bitmapData.Stride * screenshotAsBitmap.Height;
        byte[] pixelsArray = new byte[byteCount];
        IntPtr ptrFirstPixel = bitmapData.Scan0;
        Marshal.Copy(ptrFirstPixel, pixelsArray, 0, pixelsArray.Length);

        screenshotAsBitmap.UnlockBits(bitmapData);

        var pixels = new int[screenshotAsBitmap.Height][];

        for (var rowIndex = 0; rowIndex < screenshotAsBitmap.Height; ++rowIndex)
        {
            var rowPixelValues = new int[screenshotAsBitmap.Width];

            for (var columnIndex = 0; columnIndex < screenshotAsBitmap.Width; ++columnIndex)
            {
                var pixelBeginInArray = bitmapData.Stride * rowIndex + columnIndex * 3;

                var red = pixelsArray[pixelBeginInArray + 2];
                var green = pixelsArray[pixelBeginInArray + 1];
                var blue = pixelsArray[pixelBeginInArray + 0];

                rowPixelValues[columnIndex] = (red << 16) | (green << 8) | blue;
            }

            pixels[rowIndex] = rowPixelValues;
        }

        return pixels;
    }

    public struct Location2d
    {
        public Int64 x, y;
    }

}
