

//using read_memory_64_bit;
//using System.Collections.Immutable;
//using System.Runtime.InteropServices;

//public static partial class read_memory
//{
//    static public IImmutableList<ulong> EnumeratePossibleAddressesForUIRootObjectsFromProcessId(int processId)
//    {
//        var memoryReader = new MemoryReaderFromLiveProcess(processId);

//        var (committedMemoryRegions, _) = ReadCommittedMemoryRegionsWithoutContentFromProcessId(processId);

//        return EnumeratePossibleAddressesForUIRootObjects(committedMemoryRegions, memoryReader);
//    }

//    static public (IImmutableList<(ulong baseAddress, int length)> memoryRegions, IImmutableList<string> logEntries) ReadCommittedMemoryRegionsWithoutContentFromProcessId(int processId)
//    {
//        var genericResult = ReadCommittedMemoryRegionsFromProcessId(processId, readContent: false);

//        var memoryRegions =
//            genericResult.memoryRegions
//            .Select(memoryRegion => (baseAddress: memoryRegion.baseAddress, length: (int)memoryRegion.length))
//            .ToImmutableList();

//        return (memoryRegions, genericResult.logEntries);
//    }

//    static public IImmutableList<ulong> EnumeratePossibleAddressesForUIRootObjects(
//            IEnumerable<(ulong baseAddress, int length)> memoryRegions,
//            IMemoryReader memoryReader)
//    {
//        var memoryRegionsOrderedByAddress =
//            memoryRegions
//            .OrderBy(memoryRegion => memoryRegion.baseAddress)
//            .ToImmutableArray();

//        string ReadNullTerminatedAsciiStringFromAddressUpTo255(ulong address)
//        {
//            var asMemory = memoryReader.ReadBytes(address, 0x100);

//            if (asMemory == null)
//                return null;

//            var asSpan = asMemory.Value.Span;

//            var length = 0;

//            for (var i = 0; i < asSpan.Length; ++i)
//            {
//                length = i;

//                if (asSpan[i] == 0)
//                    break;
//            }

//            return System.Text.Encoding.ASCII.GetString(asSpan[..length]);
//        }

//        ReadOnlyMemory<ulong>? ReadMemoryRegionContentAsULongArray((ulong baseAddress, int length) memoryRegion)
//        {
//            var asByteArray = memoryReader.ReadBytes(memoryRegion.baseAddress, memoryRegion.length);

//            if (asByteArray == null)
//                return null;

//            return TransformMemoryContent.AsULongMemory(asByteArray.Value);
//        }

//        IEnumerable<ulong> EnumerateCandidatesForPythonTypeObjectType()
//        {
//            IEnumerable<ulong> EnumerateCandidatesForPythonTypeObjectTypeInMemoryRegion((ulong baseAddress, int length) memoryRegion)
//            {
//                var memoryRegionContentAsULongArray = ReadMemoryRegionContentAsULongArray(memoryRegion);

//                if (memoryRegionContentAsULongArray == null)
//                    yield break;

//                for (var candidateAddressIndex = 0; candidateAddressIndex < memoryRegionContentAsULongArray.Value.Length - 4; ++candidateAddressIndex)
//                {
//                    var candidateAddressInProcess = memoryRegion.baseAddress + (ulong)candidateAddressIndex * 8;

//                    var candidate_ob_type = memoryRegionContentAsULongArray.Value.Span[candidateAddressIndex + 1];

//                    if (candidate_ob_type != candidateAddressInProcess)
//                        continue;

//                    var candidate_tp_name =
//                        ReadNullTerminatedAsciiStringFromAddressUpTo255(
//                            memoryRegionContentAsULongArray.Value.Span[candidateAddressIndex + 3]);

//                    if (candidate_tp_name != "type")
//                        continue;

//                    yield return candidateAddressInProcess;
//                }
//            }

//            return
//                memoryRegionsOrderedByAddress
//                .AsParallel()
//                .WithDegreeOfParallelism(2)
//                .SelectMany(EnumerateCandidatesForPythonTypeObjectTypeInMemoryRegion)
//                .ToImmutableArray();
//        }

//        IEnumerable<(ulong address, string tp_name)> EnumerateCandidatesForPythonTypeObjects(
//            IImmutableList<ulong> typeObjectCandidatesAddresses)
//        {
//            if (typeObjectCandidatesAddresses.Count < 1)
//                yield break;

//            var typeAddressMin = typeObjectCandidatesAddresses.Min();
//            var typeAddressMax = typeObjectCandidatesAddresses.Max();

//            foreach (var memoryRegion in memoryRegionsOrderedByAddress)
//            {
//                var memoryRegionContentAsULongArray = ReadMemoryRegionContentAsULongArray(memoryRegion);

//                if (memoryRegionContentAsULongArray == null)
//                    continue;

//                for (var candidateAddressIndex = 0; candidateAddressIndex < memoryRegionContentAsULongArray.Value.Length - 4; ++candidateAddressIndex)
//                {
//                    var candidateAddressInProcess = memoryRegion.baseAddress + (ulong)candidateAddressIndex * 8;

//                    var candidate_ob_type = memoryRegionContentAsULongArray.Value.Span[candidateAddressIndex + 1];

//                    {
//                        //  This check is redundant with the following one. It just implements a specialization to optimize runtime expenses.
//                        if (candidate_ob_type < typeAddressMin || typeAddressMax < candidate_ob_type)
//                            continue;
//                    }

//                    if (!typeObjectCandidatesAddresses.Contains(candidate_ob_type))
//                        continue;

//                    var candidate_tp_name =
//                        ReadNullTerminatedAsciiStringFromAddressUpTo255(
//                            memoryRegionContentAsULongArray.Value.Span[candidateAddressIndex + 3]);

//                    if (candidate_tp_name == null)
//                        continue;

//                    yield return (candidateAddressInProcess, candidate_tp_name);
//                }
//            }
//        }

//        IEnumerable<ulong> EnumerateCandidatesForInstancesOfPythonType(
//            IImmutableList<ulong> typeObjectCandidatesAddresses)
//        {
//            if (typeObjectCandidatesAddresses.Count < 1)
//                yield break;

//            var typeAddressMin = typeObjectCandidatesAddresses.Min();
//            var typeAddressMax = typeObjectCandidatesAddresses.Max();

//            foreach (var memoryRegion in memoryRegionsOrderedByAddress)
//            {
//                var memoryRegionContentAsULongArray = ReadMemoryRegionContentAsULongArray(memoryRegion);

//                if (memoryRegionContentAsULongArray == null)
//                    continue;

//                for (var candidateAddressIndex = 0; candidateAddressIndex < memoryRegionContentAsULongArray.Value.Length - 4; ++candidateAddressIndex)
//                {
//                    var candidateAddressInProcess = memoryRegion.baseAddress + (ulong)candidateAddressIndex * 8;

//                    var candidate_ob_type = memoryRegionContentAsULongArray.Value.Span[candidateAddressIndex + 1];

//                    {
//                        //  This check is redundant with the following one. It just implements a specialization to reduce processing time.
//                        if (candidate_ob_type < typeAddressMin || typeAddressMax < candidate_ob_type)
//                            continue;
//                    }

//                    if (!typeObjectCandidatesAddresses.Contains(candidate_ob_type))
//                        continue;

//                    yield return candidateAddressInProcess;
//                }
//            }
//        }

//        var uiRootTypeObjectCandidatesAddresses =
//            EnumerateCandidatesForPythonTypeObjects(EnumerateCandidatesForPythonTypeObjectType().ToImmutableList())
//            .Where(typeObject => typeObject.tp_name == "UIRoot")
//            .Select(typeObject => typeObject.address)
//            .ToImmutableList();

//        return
//            EnumerateCandidatesForInstancesOfPythonType(uiRootTypeObjectCandidatesAddresses)
//            .ToImmutableList();
//    }

//    static public (IImmutableList<SampleMemoryRegion> memoryRegions, IImmutableList<string> logEntries) ReadCommittedMemoryRegionsFromProcessId(
//            int processId,
//            bool readContent)
//    {
//        var logEntries = new List<string>();

//        void logLine(string lineText)
//        {
//            logEntries.Add(lineText);
//            //  Console.WriteLine(lineText);
//        }

//        logLine("Reading from process " + processId + ".");

//        var processHandle = WinApi.OpenProcess(
//            (int)(WinApi.ProcessAccessFlags.QueryInformation | WinApi.ProcessAccessFlags.VirtualMemoryRead), false, processId);

//        long address = 0;

//        var committedRegions = new List<SampleMemoryRegion>();

//        do
//        {
//            int result = WinApi.VirtualQueryEx(
//                processHandle,
//                (IntPtr)address,
//                out WinApi.MEMORY_BASIC_INFORMATION64 m,
//                (uint)Marshal.SizeOf(typeof(WinApi.MEMORY_BASIC_INFORMATION64)));

//            var regionProtection = (WinApi.MemoryInformationProtection)m.Protect;

//            logLine($"{m.BaseAddress}-{(uint)m.BaseAddress + (uint)m.RegionSize - 1} : {m.RegionSize} bytes result={result}, state={(WinApi.MemoryInformationState)m.State}, type={(WinApi.MemoryInformationType)m.Type}, protection={regionProtection}");

//            if (address == (long)m.BaseAddress + (long)m.RegionSize)
//                break;

//            address = (long)m.BaseAddress + (long)m.RegionSize;

//            if (m.State != (int)WinApi.MemoryInformationState.MEM_COMMIT)
//                continue;

//            var protectionFlagsToSkip = WinApi.MemoryInformationProtection.PAGE_GUARD | WinApi.MemoryInformationProtection.PAGE_NOACCESS;

//            var matchingFlagsToSkip = protectionFlagsToSkip & regionProtection;

//            if (matchingFlagsToSkip != 0)
//            {
//                logLine($"Skipping region beginning at {m.BaseAddress:X} as it has flags {matchingFlagsToSkip}.");
//                continue;
//            }

//            var regionBaseAddress = m.BaseAddress;

//            byte[] regionContent = null;

//            if (readContent)
//            {
//                UIntPtr bytesRead = UIntPtr.Zero;
//                var regionContentBuffer = new byte[(long)m.RegionSize];

//                WinApi.ReadProcessMemory(processHandle, regionBaseAddress, regionContentBuffer, (UIntPtr)regionContentBuffer.LongLength, ref bytesRead);

//                if (bytesRead.ToUInt64() != (ulong)regionContentBuffer.LongLength)
//                    throw new Exception($"Failed to ReadProcessMemory at 0x{regionBaseAddress:X}: Only read " + bytesRead + " bytes.");

//                regionContent = regionContentBuffer;
//            }

//            committedRegions.Add(new SampleMemoryRegion(
//                BaseAddress: regionBaseAddress,
//                Length: m.RegionSize,
//                Content: regionContent));

//        } while (true);

//        logLine($"Found {committedRegions.Count} committed regions with a total size of {committedRegions.Select(region => (long)region.length).Sum()}.");

//        return (committedRegions.ToImmutableList(), logEntries.ToImmutableList());
//    }
//}
