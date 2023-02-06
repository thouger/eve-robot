using read_memory_64_bit;
public struct Location2d
{
    public Int64 x, y;
}

static public class SetForegroundWindowInWindows
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
            WinApi.User32.SetForegroundWindow(windowHandle);

            if (WinApi.User32.GetForegroundWindow() == windowHandle)
                return null;

            var windowsInZOrder = WinApi.ListWindowHandlesInZOrder();

            var windowIndex = windowsInZOrder.ToList().IndexOf(windowHandle);

            if (windowIndex < 0)
                return "Did not find window for this handle";

            {
                var simulator = new WindowsInput.InputSimulator();

                simulator.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.MENU);
                WinApi.User32.SetForegroundWindow(windowHandle);
                simulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.MENU);

                System.Threading.Thread.Sleep(AltKeyPlusSetForegroundWindowWaitTimeMilliseconds);

                if (WinApi.User32.GetForegroundWindow() == windowHandle)
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

public class Program
{
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
            Sanderling.EveOnline64.EnumeratePossibleAddressesForUIRootObjectsFromProcessId(processId);

        using (var memoryReader = new Sanderling.MemoryReaderFromLiveProcess(processId))
        {
            var uiTrees =
                candidatesAddresses
                .Select(candidateAddress => Sanderling.EveOnline64.ReadUITreeFromAddress(candidateAddress, memoryReader, 99))
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
   Sanderling.EveOnline64.EnumeratePossibleAddressesForUIRootObjectsFromProcessId(processId);
        var memoryReader = new Sanderling.MemoryReaderFromLiveProcess(processId);
            var uiTrees =
                candidatesAddresses
                .Select(candidateAddress => Sanderling.EveOnline64.ReadUITreeFromAddress(candidateAddress, memoryReader, 99))
                .ToList();

            var UIRootAddress =
                uiTrees
                .OrderByDescending(uiTree => uiTree?.EnumerateSelfAndDescendants().Count() ?? -1)
                .FirstOrDefault()
                ?.pythonObjectAddress;
            Console.WriteLine($"Address: UIRootAddress");
        return UIRootAddress.Value;
    }

public static void Main(string[] args)
    {
        var generalStopwatch = System.Diagnostics.Stopwatch.StartNew();
        int readingFromGameCount = 0;

        var program = new Program();
        (int processId, string windowId, string windowsTitle) = ListGameClientProcessesRequest();
        ulong uiRootAddress = SearchUIRootAddress(processId);


        var readingFromGameIndex = System.Threading.Interlocked.Increment(ref readingFromGameCount);

            var readingId = readingFromGameIndex.ToString("D6") + "-" + generalStopwatch.ElapsedMilliseconds;

            var windowHandle = new IntPtr(long.Parse(windowId));

            WinApi.GetWindowThreadProcessId(windowHandle, out var processIdUnsigned);

            var windowRect = new WinApi.Rect();
            WinApi.GetWindowRect(windowHandle, ref windowRect);

            var clientRectOffsetFromScreen = new WinApi.Point(0, 0);
            WinApi.ClientToScreen(windowHandle, ref clientRectOffsetFromScreen);

            var windowClientRectOffset =
                new Location2d
                { x = clientRectOffsetFromScreen.x - windowRect.left, y = clientRectOffsetFromScreen.y - windowRect.top };

            string memoryReadingSerialRepresentationJson = null;

            using (var memoryReader = new Sanderling.MemoryReaderFromLiveProcess(processId))
            {
                var uiTree = Sanderling.EveOnline64.ReadUITreeFromAddress(uiRootAddress, memoryReader, 99);

                if (uiTree != null)
                {
                    memoryReadingSerialRepresentationJson =
                    Sanderling.EveOnline64.SerializeMemoryReadingNodeToJson(
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
                    return new Response
                    {
                        FailedToBringWindowToFront = setForegroundWindowError,
                    };
                }
            }

            var pixels_1x1_R8G8B8 = GetScreenshotOfWindowAsPixelsValues_R8G8B8(windowHandle);

            var historyEntry = new ReadingFromGameClient
            {
                windowHandle = windowHandle,
                readingId = readingId,
                pixels_1x1_R8G8B8 = pixels_1x1_R8G8B8,
            };

            var imageData = CompileImageDataFromReadingResult(request.ReadFromWindow.getImageData, historyEntry);

            readingFromGameHistory.Enqueue(historyEntry);

            while (4 < readingFromGameHistory.Count)
            {
                readingFromGameHistory.Dequeue();
            }

            return new Response
            {
                ReadFromWindowResult = new Response.ReadFromWindowResultStructure
                {
                    Completed = new Response.ReadFromWindowResultStructure.CompletedStructure
                    {
                        processId = processId,
                        windowClientRectOffset = windowClientRectOffset,
                        memoryReadingSerialRepresentationJson = memoryReadingSerialRepresentationJson,
                        readingId = readingId,
                        imageData = imageData,
                    },
                },
            };

       
    }
}