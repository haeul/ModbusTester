using System;
using System.Collections.Concurrent;

public static class RegisterCache
{
    public static event Action? Changed;

    // Holding/Input 구분이 필요하면 사전 두 개로 나눠도 됨
    private static readonly ConcurrentDictionary<ushort, ushort> _regs = new();

    public static void UpdateRange(ushort startAddr, ushort[] values)
    {
        for (int i = 0; i < values.Length; i++)
            _regs[(ushort)(startAddr + i)] = values[i];
        Changed?.Invoke();
    }

    public static bool TryGet(ushort addr, out ushort value) => _regs.TryGetValue(addr, out value);
}
