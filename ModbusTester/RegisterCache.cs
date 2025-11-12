using System;
using System.Collections.Concurrent;

public static class RegisterCache
{
    public static event Action? Changed;

    private static readonly ConcurrentDictionary<ushort, ushort> _regs = new();

    public static void UpdateRange(ushort startAddr, ushort[] values)
    {
        for (int i = 0; i < values.Length; i++)
            _regs[(ushort)(startAddr + i)] = values[i];
        Changed?.Invoke();
    }

    // 추가: 단일 주소 쓰기 (QuickViewPolling이 이걸 리플렉션으로 찾아 호출)
    public static void Set(ushort addr, ushort value)
    {
        _regs[addr] = value;
        Changed?.Invoke();
    }

    public static bool TryGet(ushort addr, out ushort value) => _regs.TryGetValue(addr, out value);
}
