using System;

namespace Chip8;

[Flags]
public enum Quirks
{
    VfReset = 0,
    LoadStoreIncrement = 1,
    VBlank = 2,
    Wrap = 4,
    Shift = 8,
    Jump = 16
}

public enum Platform
{
    CosmacVIP,
    Modern,
    Chip48,
    SuperChip,
    XoChip
}

public record Spec
{
    public Platform Platform { get; init; }
    public ushort VideoWidth { get; init; }
    public ushort VideoHeight { get; init; }
    public byte CharSize { get; init; }
    public Quirks Quirks { get; init; }
    public ushort TickRate { get; init; }

    public static Spec CosmacVIP() =>
        new()
        {
            Platform = Platform.CosmacVIP,
            VideoWidth = 64,
            VideoHeight = 32,
            Quirks = Quirks.VfReset | Quirks.VBlank,
            TickRate = 15
        };
}
