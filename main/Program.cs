using read_memory_64_bit;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using WindowsInput;
using static System.Net.WebRequestMethods;
using static utils;
using static WindowsInput.InputSimulator;

public class Program
{
    static Queue<ReadingFromGameClient> readingFromGameHistory = new Queue<ReadingFromGameClient>();

    System.Diagnostics.Process[] GetWindowsProcessesLookingLikeEVEOnlineClient() =>
    System.Diagnostics.Process.GetProcessesByName("exefile");

    public class GameClientProcessSummaryStruct
    {
        public int processId;

        public string mainWindowId;

        public string mainWindowTitle;

        public int mainWindowZIndex;
    }
    public System.Collections.Generic.IReadOnlyList<GameClientProcessSummaryStruct> ListGameClientProcesses()
    {
        var allWindowHandlesInZOrder = WinApi.ListWindowHandlesInZOrder();

        int? zIndexFromWindowHandle(IntPtr windowHandleToSearch) =>
            allWindowHandlesInZOrder
            .Select((windowHandle, index) => (windowHandle, index: (int?)index))
            .FirstOrDefault(handleAndIndex => handleAndIndex.windowHandle == windowHandleToSearch)
            .index;

        var processes =
            GetWindowsProcessesLookingLikeEVEOnlineClient()
            .Select(process =>
            {
                return new GameClientProcessSummaryStruct
                {
                    processId = process.Id,
                    mainWindowId = process.MainWindowHandle.ToInt64().ToString(),
                    mainWindowTitle = process.MainWindowTitle,
                    mainWindowZIndex = zIndexFromWindowHandle(process.MainWindowHandle) ?? 9999,
                };
            })
            .ToList();

        return processes;
    }

    ulong? FindUIRootAddressFromProcessId(int processId)
    {
        var candidatesAddresses =
            read_memory_64_bit.Sanderling.EveOnline64.EnumeratePossibleAddressesForUIRootObjectsFromProcessId(processId);

        using (var memoryReader = new read_memory_64_bit.Sanderling.MemoryReaderFromLiveProcess(processId))
        {
            var uiTrees =
                candidatesAddresses
                .Select(candidateAddress => read_memory_64_bit.Sanderling.EveOnline64.ReadUITreeFromAddress(candidateAddress, memoryReader, 99))
                .ToList();

            return
                uiTrees
                .OrderByDescending(uiTree => uiTree?.EnumerateSelfAndDescendants().Count() ?? -1)
                .FirstOrDefault()
                ?.pythonObjectAddress;
        }
    }

    public static (int, string, string) ListGameClientProcessesRequest()
    {
        var program = new Program();

        var processes = program.ListGameClientProcesses();

        foreach (var process in processes)
        {
            int processId = process.processId;
            var windowId = process.mainWindowId.ToString();
            var windowsTitle = process.mainWindowTitle.ToString();
            Console.WriteLine($"ProcessId: {processId}, MainWindowId: {windowId}, MainWindowTitle: {windowsTitle}, MainWindowZIndex: {process.mainWindowZIndex}");
            return (processId, windowId, windowsTitle);
        }
        return (0, "", "");
    }

    public static ulong SearchUIRootAddress(int processId)
    {
        var candidatesAddresses =
   read_memory_64_bit.Sanderling.EveOnline64.EnumeratePossibleAddressesForUIRootObjectsFromProcessId(processId);
        var memoryReader = new read_memory_64_bit.Sanderling.MemoryReaderFromLiveProcess(processId);
            var uiTrees =
                candidatesAddresses
                .Select(candidateAddress => read_memory_64_bit.Sanderling.EveOnline64.ReadUITreeFromAddress(candidateAddress, memoryReader, 99))
                .ToList();

            var UIRootAddress =
                uiTrees
                .OrderByDescending(uiTree => uiTree?.EnumerateSelfAndDescendants().Count() ?? -1)
                .FirstOrDefault()
                ?.pythonObjectAddress;
            Console.WriteLine($"Address: UIRootAddress");
        return UIRootAddress.Value;
    }

    public static void ReadFromWindow(long windowId,ulong uiRootAddress, utils.GetImageDataFromReadingStructure getImageData,int processId)
    {
        int readingFromGameCount = 0;
        var readingFromGameIndex = System.Threading.Interlocked.Increment(ref readingFromGameCount);
        var generalStopwatch = System.Diagnostics.Stopwatch.StartNew();

        var readingId = readingFromGameIndex.ToString("D6") + "-" + generalStopwatch.ElapsedMilliseconds;

        var windowHandle = new IntPtr(windowId);

        WinApi.GetWindowThreadProcessId(windowHandle, out var processIdUnsigned);

        var windowRect = new WinApi.Rect();
        WinApi.GetWindowRect(windowHandle, ref windowRect);

        var clientRectOffsetFromScreen = new WinApi.Point(0, 0);
        WinApi.ClientToScreen(windowHandle, ref clientRectOffsetFromScreen);

        var windowClientRectOffset =
            new Location2d
            { x = clientRectOffsetFromScreen.x - windowRect.left, y = clientRectOffsetFromScreen.y - windowRect.top };

        string memoryReadingSerialRepresentationJson = null;

        using (var memoryReader = new read_memory_64_bit.Sanderling.MemoryReaderFromLiveProcess(processId))
        {
            var uiTree = read_memory_64_bit.Sanderling.EveOnline64.ReadUITreeFromAddress(uiRootAddress, memoryReader, 99);

            if (uiTree != null)
            {
                memoryReadingSerialRepresentationJson =
                read_memory_64_bit.Sanderling.EveOnline64.SerializeMemoryReadingNodeToJson(
                    uiTree.WithOtherDictEntriesRemoved());
            }
        }

        {
            /*
            Maybe taking screenshots needs the window to be not occluded by other windows.
            We can review this later.
            */
            var setForegroundWindowError = SetForegroundWindowInWindows.TrySetForegroundWindow(windowHandle);

            if (setForegroundWindowError != null)
            {
                Console.WriteLine($"Error setting foreground window: {setForegroundWindowError}");
            }
        }

        var pixels_1x1_R8G8B8 = utils.GetScreenshotOfWindowAsPixelsValues_R8G8B8(windowHandle);

        var historyEntry = new ReadingFromGameClient
        {
            windowHandle = windowHandle,
            readingId = readingId,
            pixels_1x1_R8G8B8 = pixels_1x1_R8G8B8,
        };

        var imageData = utils.CompileImageDataFromReadingResult(getImageData, historyEntry);

        readingFromGameHistory.Enqueue(historyEntry);

        while (4 < readingFromGameHistory.Count)
        {
            readingFromGameHistory.Dequeue();
        }

        processId = processId;
        windowClientRectOffset = windowClientRectOffset;
        memoryReadingSerialRepresentationJson = memoryReadingSerialRepresentationJson;
        readingId = readingId;
        imageData = imageData;
    }

    public void ExecuteEffectOnWindow(bool bringWindowToForeground,long windowId, EffectSequenceElement[] task)
    {
        var windowHandle = new IntPtr(windowId);

        if (bringWindowToForeground)
        {
            var setForegroundWindowError = SetForegroundWindowInWindows.TrySetForegroundWindow(windowHandle);

            if (setForegroundWindowError != null)
            {
                Console.WriteLine();
            }
        }


            foreach (var sequenceElement in task)
            {
                if (sequenceElement?.effect != null)
                    _ExecuteEffectOnWindow(sequenceElement.effect, windowHandle, bringWindowToForeground);

                if (sequenceElement?.delayMilliseconds != null)
                    System.Threading.Thread.Sleep(sequenceElement.delayMilliseconds.Value);
            }
        
    }
    
    public void _ExecuteEffectOnWindow(EffectOnWindowStructure effectOnWindow,IntPtr windowHandle,bool bringWindowToForeground)
    {
        if (bringWindowToForeground)
            WinApi.SetForegroundWindow(windowHandle);

        if (effectOnWindow?.MouseMoveTo != null)
        {
            //  Build motion description based on https://github.com/Arcitectus/Sanderling/blob/ada11c9f8df2367976a6bcc53efbe9917107bfa7/src/Sanderling/Sanderling/Motor/Extension.cs#L24-L131

            var mousePosition = new BotEngine.Vektor2DInt(
                effectOnWindow.MouseMoveTo.location.x,
                effectOnWindow.MouseMoveTo.location.y);

            var mouseButtons = new BotEngine.MouseButtonIdEnum[] { };

            var windowMotor = new Sanderling.Motor.WindowMotor(windowHandle);

            var motionSequence = new BotEngine.Motion[]{
                new BotEngine.Motion(
                    mousePosition: mousePosition,
                    mouseButtonDown: mouseButtons,
                    windowToForeground: bringWindowToForeground),
                new BotEngine.Motion(
                    mousePosition: mousePosition,
                    mouseButtonUp: mouseButtons,
                    windowToForeground: bringWindowToForeground),
            };

            windowMotor.ActSequenceMotion(motionSequence);
        }

        if (effectOnWindow?.KeyDown != null)
        {
            var virtualKeyCode = (WindowsInput.Native.VirtualKeyCode)effectOnWindow.KeyDown.virtualKeyCode;

            (MouseActionForKeyUpOrDown(keyCode: virtualKeyCode, buttonUp: false)
            ??
            (() => new InputSimulator().Keyboard.KeyDown(virtualKeyCode)))();
        }

        if (effectOnWindow?.KeyUp != null)
        {
            var virtualKeyCode = (WindowsInput.Native.VirtualKeyCode)effectOnWindow.KeyUp.virtualKeyCode;

            (MouseActionForKeyUpOrDown(keyCode: virtualKeyCode, buttonUp: true)
            ??
            (() => new InputSimulator().Keyboard.KeyUp(virtualKeyCode)))();
        }
    }

    static System.Action MouseActionForKeyUpOrDown(WindowsInput.Native.VirtualKeyCode keyCode, bool buttonUp)
    {
        IMouseSimulator mouseSimulator() => new WindowsInput.InputSimulator().Mouse;

        var method = keyCode switch
        {
            WindowsInput.Native.VirtualKeyCode.LBUTTON =>
                buttonUp ?
                mouseSimulator().LeftButtonUp
                : mouseSimulator().LeftButtonDown,
            WindowsInput.Native.VirtualKeyCode.RBUTTON =>
                buttonUp ?
                (Func<IMouseSimulator>)mouseSimulator().RightButtonUp
                : mouseSimulator().RightButtonDown,
            _ => null
        };

        if (method != null)
            return () => method();

        return null;
    }


    public static void Main(string[] args)
    {
        var program = new Program();
        (int processId, string windowId, string windowsTitle) = ListGameClientProcessesRequest();
        ulong uiRootAddress = SearchUIRootAddress(processId);
        Console.WriteLine(uiRootAddress);
    }
}