using System;
using System.Diagnostics;
using System.Text;

namespace Chip8;

public enum Chip8State
{
    Running,
    Paused,
    Off
}

public class Chip8
{
    public const byte VideoHeight = 0x20;
    public const byte VideoWidth = 0x40;
    private const ushort ProgramStartAddress = 0x200;
    private const byte CharSize = 0x5;

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
        0xF0, 0x80, 0xF0, 0x80, 0x80  // F
    };

    private readonly Random _random = new();

    public Chip8State State { get; set; }
    public bool ShouldDraw { get; set; }
    private ushort I { get; set; }
    private ushort PC { get; set; }
    private byte SP { get; set; }
    private byte DT { get; set; }
    private byte ST { get; set; }
    private ushort Opcode { get; set; }
    private long ProgramSize { get; set; }

    private byte VF
    {
        set => _registers[15] = value;
    }

    private readonly byte[] _registers = new byte[16];
    private readonly byte[] _memory = new byte[4096];
    private readonly ushort[] _stack = new ushort[16];
    private byte RandomByte() => (byte)_random.Next(0xFF);
    public readonly bool[] Keypad = new bool[16];
    public readonly byte[] Video = new byte[2048];
    public bool ShouldPlaySound { get; set; }
    public bool HasColor(int i) => Video[i] == 0x0;
    public bool IsOn() => PC <= ProgramSize && State is Chip8State.Running or Chip8State.Paused;
    public bool IsRunning() => State == Chip8State.Running;
    public bool IsPaused() => State == Chip8State.Paused;
    public bool IsOff() => State == Chip8State.Off;

    public void Pause() =>
        State = State switch
        {
            Chip8State.Paused => Chip8State.Running,
            Chip8State.Running => Chip8State.Paused,
            _ => State
        };

    public void PowerOn() => State = Chip8State.Running;
    public void PowerOff() => State = Chip8State.Off;

    public Chip8()
    {
        PC = ProgramStartAddress;
        State = Chip8State.Off;
        _fonts.CopyTo(_memory, 0);
    }

    public void LoadRom(byte[] rom)
    {
        rom.CopyTo(_memory, ProgramStartAddress);
        ProgramSize = rom.Length + ProgramStartAddress;
    }

    public void Step()
    {
        Opcode = (ushort)(_memory[PC] << 8 | _memory[PC + 1]);
        SkipNextInstruction();
        Execute(Opcode);

        if (DT > 0)
        {
            --DT;
        }

        if (ST > 0)
        {
            ShouldPlaySound = true;
            --ST;
        }
        else
        {
            ShouldPlaySound = false;
        }
    }

    public string DumpRegisters()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Opcode -> {Opcode}");
        sb.AppendLine($"PC -> {PC}");
        sb.AppendLine($"I -> {I}");
        sb.AppendLine($"SP -> {SP}");
        for (var i = 0; i < 15; ++i)
        {
            sb.AppendLine($"Register[${i}] -> {_registers[i]}");
        }

        sb.AppendLine($"VF -> {_registers[15]}");

        return sb.ToString();
    }

    public string DumpMemory()
    {
        var sb = new StringBuilder();

        for (var i = 0; i < _memory.Length; ++i)
        {
            sb.AppendLine($"Memory[{i}] -> {_memory[i]}");
        }

        return sb.ToString();
    }

    public string DumpVideo()
    {
        var sb = new StringBuilder();

        for (var i = 0; i < Video.Length; ++i)
        {
            sb.AppendLine($"Video[{i}] -> {Video[i]}");
        }

        return sb.ToString();
    }

    private void Execute(ushort opcode)
    {
        var c = (ushort)(opcode & 0xF000) >> 12;
        var x = (ushort)(opcode & 0x0F00) >> 8;
        var y = (ushort)(opcode & 0x00F0) >> 4;
        var d = (ushort)(opcode & 0x000F);

        var nn = (ushort)(opcode & 0x00FF);
        var nnn = (ushort)(opcode & 0x0FFF);

        ushort result;
        byte updatedVF;
        switch (c, vx: x, vy: y, d)
        {
            case (0x0, 0x0, 0xE, 0x0):
                Debug.WriteLine($"0x{opcode:X} -> 00E0: CLS - Clear screen");
                Array.Clear(Video, 0, Video.Length);
                ShouldDraw = true;
                break;

            case (0x0, 0x0, 0xE, 0xE):
                Debug.WriteLine($"0x{opcode:X} -> 00EE: RET - Return");
                PC = _stack[--SP];
                break;

            case (0x1, _, _, _):
                Debug.WriteLine($"0x{opcode:X} -> 1NNN: JP addr - Jump to address");
                PC = nnn;
                break;

            case (0x2, _, _, _):
                Debug.WriteLine($"0x{opcode:X} -> 2NNN: CALL addr - Call subroutine at address");
                _stack[SP++] = PC;
                PC = nnn;
                break;

            case (0x3, _, _, _):
                Debug.WriteLine($"0x{opcode:X} -> 3XKK: SE Vx, byte - Skip next instruction if Vx = byte");
                if (_registers[x] == nn)
                {
                    SkipNextInstruction();
                }

                break;

            case (0x4, _, _, _):
                Debug.WriteLine($"0x{opcode:X} -> 4XKK: SNE Vx, byte - Skip next instruction if Vx != byte");
                if (_registers[x] != nn)
                {
                    SkipNextInstruction();
                }

                break;

            case (0x5, _, _, _):
                Debug.WriteLine($"0x{opcode:X} -> 5XY0: SE Vx, Vy - Skip next instruction if Vx = Vy");
                if (_registers[x] == _registers[y])
                {
                    SkipNextInstruction();
                }

                break;
            case (0x6, _, _, _):
                Debug.WriteLine($"0x{opcode:X} -> 6XKK: LD Vx, byte - Set Vx = byte");
                _registers[x] = (byte)nn;
                break;

            case (0x7, _, _, _):
                Debug.WriteLine($"0x{opcode:X} -> 7XKK: ADD Vx, byte - Add byte to Vx");
                _registers[x] += (byte)nn;
                break;

            case (0x8, _, _, 0x0):
                Debug.WriteLine($"0x{opcode:X} -> 8XY0: LD Vx, Vy - Set Vx = Vy");
                _registers[x] = _registers[y];
                break;
            case (0x8, _, _, 0x1):
                Debug.WriteLine($"0x{opcode:X} -> 8XY1: OR Vx, Vy - Set Vx = Vx OR Vy");
                _registers[x] |= _registers[y];
                break;

            case (0x8, _, _, 0x2):
                Debug.WriteLine($"0x{opcode:X} -> 8XY2 AND Vx, Vy - Set Vx = Vx AND Vy");
                _registers[x] &= _registers[y];
                break;

            case (0x8, _, _, 0x3):
                Debug.WriteLine($"0x{opcode:X} -> 8XY3: XOR Vx, Vy - Set Vx = Vx XOR Vy");
                _registers[x] ^= _registers[y];
                break;

            case (0x8, _, _, 0x4):
                Debug.WriteLine($"0x{opcode:X} -> 8XY4: ADD Vx, Vy - Set Vx = Vx + Vy, Set VF = carry");
                result = (ushort)(_registers[x] + _registers[y]);
                _registers[x] = (byte)(result & 0xFF);
                VF = (byte)(result > 0xFF ? 1 : 0);
                break;

            case (0x8, _, _, 0x5):
                Debug.WriteLine($"0x{opcode:X} -> 8XY5: SUB Vx, Vy - Set Vx = Vx - Vy, Set VF = not borrow");
                updatedVF = (byte)(_registers[x] > _registers[y] ? 1 : 0);
                _registers[x] -= _registers[y];
                VF = updatedVF;
                break;

            case (0x8, _, _, 0x6):
                Debug.WriteLine($"0x{opcode:X} -> 8XY6: SHR Vx - Set Vx = Vx SHR 1");
                updatedVF = (byte)(_registers[x] & 1);
                _registers[x] >>= 1;
                VF = updatedVF;
                break;

            case (0x8, _, _, 0x7):
                Debug.WriteLine($"0x{opcode:X} -> 8XY7: SUB Vx, Vy - Set Vx = Vy - Vx, Set VF = not borrow");
                updatedVF = (byte)(_registers[x] < _registers[y] ? 1 : 0);
                _registers[x] = (byte)(_registers[y] - _registers[x]);
                VF = updatedVF;
                break;

            case (0x8, _, _, 0xE):
                Debug.WriteLine($"0x{opcode:X} -> 8XYE - SHL Vx - Set Vx = Vx SHL 1");
                updatedVF = (byte)((_registers[x] & 0x80) >> 7);
                _registers[x] <<= 1;
                VF = updatedVF;
                break;

            case (0x9, _, _, 0x0):
                Debug.WriteLine($"0x{opcode:X} -> 9XY0: SNE Vx, Vy - Skip next instruction if Vx != Vy");
                if (_registers[x] != _registers[y])
                {
                    SkipNextInstruction();
                }

                break;

            case (0xA, _, _, _):
                Debug.WriteLine($"0x{opcode:X} -> ANNN: LD I, addr - Set I = nnn");
                I = nnn;
                break;

            case (0xB, _, _, _):
                Debug.WriteLine($"0x{opcode:X} -> BNNN: JP V0, addr - Jump to address V0 + addr");
                PC = _registers[nnn];
                break;

            case (0xC, _, _, _):
                Debug.WriteLine($"0x{opcode:X} -> CXKK: RND Vx, byte - Set Vx = random byte AND byte");
                _registers[x] = (byte)(RandomByte() & nn);
                break;

            case (0xD, _, _, _):
                Debug.WriteLine(
                    $"0x{opcode:X} -> DXYN: DRW Vx, Vy, nibble - Display n-byte sprite starting at I to coordinates (Vx, Vy), Set VF = collision");

                VF = 0;
                for (var displayY = 0; displayY < d; ++displayY)
                {
                    var yPos = (_registers[y] + displayY) % VideoHeight;
                    for (var displayX = 0; displayX < 8; ++displayX)
                    {
                        if ((_memory[I + displayY] & 0x80 >> displayX) != 0)
                        {
                            var xPos = (_registers[x] + displayX) % VideoWidth;
                            var spritePos = yPos * VideoWidth + xPos;
                            VF = Video[spritePos];
                            Video[spritePos] ^= 1;
                        }
                    }
                }

                ShouldDraw = true;
                break;

            case (0xE, _, 0x9, 0xE):
                Debug.WriteLine(
                    $"0x{opcode:X} -> EX9E: SKP Vx - Skip next instruction if key with the value of Vx is pressed");

                if (Keypad[_registers[x]])
                {
                    SkipNextInstruction();
                }

                break;

            case (0xE, _, 0xA, 0x1):
                Debug.WriteLine(
                    $"0x{opcode:X} -> EXA1: SKNP Vx - Skip next instruction if key with the value of Vx is not pressed");

                if (!Keypad[_registers[x]])
                {
                    SkipNextInstruction();
                }
                break;

            case (0xF, _, 0x0, 0x7):
                Debug.WriteLine($"0x{opcode:X} -> FX07: LD Vx, DT - Set Vx = delay timer");
                _registers[x] = DT;
                break;

            case (0xF, _, 0x0, 0xA):
                Debug.WriteLine($"0x{opcode:X} -> FX0A: LD Vx, K - Wait for key press and store the value into Vx");
                if (Keypad[0])
                {
                    _registers[x] = 0;
                }
                else if (Keypad[1])
                {
                    _registers[x] = 1;
                }
                else if (Keypad[2])
                {
                    _registers[x] = 2;
                }
                else if (Keypad[3])
                {
                    _registers[x] = 3;
                }
                else if (Keypad[4])
                {
                    _registers[x] = 4;
                }
                else if (Keypad[5])
                {
                    _registers[x] = 5;
                }
                else if (Keypad[6])
                {
                    _registers[x] = 6;
                }
                else if (Keypad[7])
                {
                    _registers[x] = 7;
                }
                else if (Keypad[8])
                {
                    _registers[x] = 8;
                }
                else if (Keypad[9])
                {
                    _registers[x] = 9;
                }
                else if (Keypad[10])
                {
                    _registers[x] = 10;
                }
                else if (Keypad[11])
                {
                    _registers[x] = 11;
                }
                else if (Keypad[12])
                {
                    _registers[x] = 12;
                }
                else if (Keypad[13])
                {
                    _registers[x] = 13;
                }
                else if (Keypad[14])
                {
                    _registers[x] = 14;
                }
                else if (Keypad[15])
                {
                    _registers[x] = 15;
                }
                else
                {
                    PC -= 2;
                }

                break;

            case (0xF, _, 0x1, 0x5):
                Debug.WriteLine($"0x{opcode:X} -> FX15: LD DT, Vx - Set delay timer = Vx");
                DT = _registers[x];
                break;

            case (0xF, _, 0x1, 0x8):
                Debug.WriteLine($"0x{opcode:X} -> FX18: LD ST, Vx - Set sound timer = Vx");
                ST = _registers[x];
                break;

            case (0xF, _, 0x1, 0xE):
                Debug.WriteLine($"0x{opcode:X} -> FX1E: Add I, Vx - Set I = I + Vx");
                I += _registers[x];
                break;

            case (0xF, _, 0x2, 0x9):
                Debug.WriteLine(
                    $"0x{opcode:X} -> FX29: LD F, Vx - Set I = location of sprite for digit Vx");

                I = (ushort)(CharSize * _registers[x]);
                break;

            case (0xF, _, 0x3, 0x3):
                Debug.WriteLine(
                    $"0x{opcode:X} -> FX33: LD B, Vx - Store BCD (Binary-Coded Decimal) representation of Vx in memory locations I, I + 1, and I + 2");

                result = _registers[x];
                _memory[I + 2] = (byte)(result % 10);
                result /= 10;
                _memory[I + 1] = (byte)(result % 10);
                result /= 10;
                _memory[I] = (byte)(result % 10);
                break;

            case (0xF, _, 0x5, 0x5):
                Debug.WriteLine($"0x{opcode:X} -> FX55: LD [I], Vx - Store V0~Vx in memory starting at location I");
                for (var offset = 0; offset <= x; ++offset)
                {
                    _memory[I + offset] = _registers[offset];
                }

                break;

            case (0xF, _, 0x6, 0x5):
                Debug.WriteLine(
                    $"0x{opcode:X} -> FX65: LD Vx, [I] - Read registers V0~Vx from memory starting at location I");

                for (var offset = 0; offset <= x; ++offset)
                {
                    _registers[offset] = _memory[I + offset];
                }

                break;

            default:
                Debug.WriteLine($"Unknown opcode: {opcode:X}");
                break;
        }
    }

    private void SkipNextInstruction() => PC += 2;
}
