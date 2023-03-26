using System.Diagnostics;
using static utils;
using WindowsInput;

public class Program
{
    static readonly Queue<ReadingFromGameClient> readingFromGameHistory = new();

    static Process[] GetWindowsProcessesLookingLikeEVEOnlineClient() => Process.GetProcessesByName("exefile");

    public class GameClientProcessSummaryStruct
    {
        public int processId;

        public long mainWindowId;

        public string mainWindowTitle;

        public int mainWindowZIndex;
    }

    public IReadOnlyList<GameClientProcessSummaryStruct> ListGameClientProcesses()
    {
        var allWindowHandlesInZOrder = WinApi.ListWindowHandles();

        Dictionary<IntPtr, int> zIndexes = allWindowHandlesInZOrder
            .Select((windowHandle, index) => (windowHandle, index))
            .ToDictionary(x => x.windowHandle, x => x.index);

        var processes = GetWindowsProcessesLookingLikeEVEOnlineClient()
            .Where(process => process.MainWindowHandle != IntPtr.Zero)
            .Select(process =>
            {
                return new GameClientProcessSummaryStruct
                {
                    processId = process.Id,
                    mainWindowId = process.MainWindowHandle.ToInt64(),
                    mainWindowTitle = process.MainWindowTitle,
                    mainWindowZIndex = zIndexes.GetValueOrDefault(process.MainWindowHandle, 9999),
                };
            })
            .ToList();

        return processes;
    }

    public static (int, long, string) ListGameClientProcessesRequest()
    {
        var program = new Program();

        var processes = program.ListGameClientProcesses();

        foreach (var process in processes)
        {
            int processId = process.processId;
            long windowId = process.mainWindowId;
            var windowsTitle = process.mainWindowTitle.ToString();
            Console.WriteLine($"ProcessId: {processId}, MainWindowId: {windowId}, MainWindowTitle: {windowsTitle}, MainWindowZIndex: {process.mainWindowZIndex}");
            return (processId, windowId, windowsTitle);
        }
        return (0, 0, "not found");
    }

    public static ulong SearchUIRootAddress(int processId)
    {
        var candidatesAddresses = read_memory_64_bit.EveOnline64.EnumeratePossibleAddressesForUIRootObjectsFromProcessId(processId);
        var memoryReader = new read_memory_64_bit.MemoryReaderFromLiveProcess(processId);
        var uiTrees =
            candidatesAddresses
            .Select(candidateAddress => read_memory_64_bit.EveOnline64.ReadUITreeFromAddress(candidateAddress, memoryReader, 99))
            .ToList();

        var UIRootAddress =
            uiTrees
            .OrderByDescending(uiTree => uiTree?.EnumerateSelfAndDescendants().Count() ?? -1)
            .FirstOrDefault()
            ?.pythonObjectAddress;
        Console.WriteLine($"Address: UIRootAddress");
        return UIRootAddress.Value;
    }

    public static string ReadFromWindow(long windowId, ulong uiRootAddress, utils.GetImageDataFromReadingStructure getImageData, int processId)
    {
        int readingFromGameCount = 0;
        var readingFromGameIndex = Interlocked.Increment(ref readingFromGameCount);
        var generalStopwatch = Stopwatch.StartNew();

        var readingId = readingFromGameIndex.ToString("D6") + "-" + generalStopwatch.ElapsedMilliseconds;

        var windowHandle = new IntPtr(windowId);

        //WinApi.GetWindowThreadProcessId(windowHandle, out var processIdUnsigned);

        var windowRect = new WinApi.Rect();
        WinApi.GetWindowRect(windowHandle, ref windowRect);

        var clientRectOffsetFromScreen = new WinApi.Point(0, 0);
        WinApi.ClientToScreen(windowHandle, ref clientRectOffsetFromScreen);

        var windowClientRectOffset =
            new Location2d
            { x = clientRectOffsetFromScreen.x - windowRect.left, y = clientRectOffsetFromScreen.y - windowRect.top };

        string memoryReadingSerialRepresentationJson = null;

        using (var memoryReader = new read_memory_64_bit.MemoryReaderFromLiveProcess(processId))
        {
            var uiTree = read_memory_64_bit.EveOnline64.ReadUITreeFromAddress(uiRootAddress, memoryReader, 99);

            if (uiTree != null)
            {
                memoryReadingSerialRepresentationJson =
                read_memory_64_bit.EveOnline64.SerializeMemoryReadingNodeToJson(
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

        //processId = processId;
        //windowClientRectOffset = windowClientRectOffset;
        //memoryReadingSerialRepresentationJson = memoryReadingSerialRepresentationJson;
        //readingId = readingId;
        //imageData = imageData;
        return memoryReadingSerialRepresentationJson;
    }

    public void ExecuteEffectOnWindow(bool bringWindowToForeground, long windowId, EffectSequenceElement[] task)
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

    public void _ExecuteEffectOnWindow(EffectOnWindowStructure effectOnWindow, IntPtr windowHandle, bool bringWindowToForeground)
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

    static Action MouseActionForKeyUpOrDown(WindowsInput.Native.VirtualKeyCode keyCode, bool buttonUp)
    {
        IMouseSimulator mouseSimulator() => new InputSimulator().Mouse;

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
}