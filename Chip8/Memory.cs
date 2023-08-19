using System;

namespace Chip8;

public class Memory
{
    public const ushort ProgramStartAddress = 0x200;
    public const byte CharSize = 0x5;

    private readonly byte[] _fonts =
    {
        0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
        0x20, 0x60, 0x20, 0x20, 0x70, // 1
        0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
        0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
        0x90, 0x90, 0xF0, 0x10, 0x10, // 4
        0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
        0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
        0xF0, 0x10, 0x20, 0x40, 0x40, // 7
        0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
        0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
        0xF0, 0x90, 0xF0, 0x90, 0x90, // A
        0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
        0xF0, 0x80, 0x80, 0x80, 0xF0, // C
        0xE0, 0x90, 0x90, 0x90, 0xE0, // D
        0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
        0xF0, 0x80, 0xF0, 0x80, 0x80 // F
    };

    private readonly Random _random = new();
    public readonly bool[] Keypad = new bool[16];
    public readonly byte[] RAM = new byte[4096];
    public readonly byte[] Registers = new byte[16];
    public readonly ushort[] Stack = new ushort[16];
    public readonly byte[] Video = new byte[2048];

    public Memory()
    {
        PC = ProgramStartAddress;
        _fonts.CopyTo(RAM, 0);
    }

    public ushort I { get; set; }
    public ushort PC { get; set; }
    public byte SP { get; set; }
    public byte DT { get; set; }
    public byte ST { get; set; }
    public long ProgramSize { get; set; }

    public byte VF
    {
        get => Registers[15];
        set => Registers[15] = value;
    }

    public ushort Opcode
    {
        get => (ushort)(RAM[PC] << 8 | RAM[PC + 1]);
    }

    public void LoadRom(byte[] rom)
    {
        rom.CopyTo(RAM, ProgramStartAddress);
        ProgramSize = rom.Length + ProgramStartAddress;
    }

    public byte RandomByte() => (byte)_random.Next(0xFF);
    public bool HasColor(int i) => Video[i] == 0x0;
}
