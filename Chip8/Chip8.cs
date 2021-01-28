using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Chip8
{
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
            0xF0, 0x80, 0xF0, 0x80, 0x80 // F
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
            get => Registers[15];
            set => Registers[15] = value;
        }
        public ushort[] Stack = new ushort[16];
        public byte[] Registers = new byte[16];
        public byte[] Memory = new byte[4096];
        public bool[] Keypad = new bool[16];
        public byte[] Video = new byte[2048];
        private byte RandomByte() => (byte) _random.Next(0xFF);
        public void PowerOn() => State = Chip8State.Running;
        public bool ShouldPlaySound { get; set; }
        public bool HasColor(int i) => Video[i] == 0x0;
        public bool IsOn() => PC <= ProgramSize && (State == Chip8State.Running || State == Chip8State.Paused);
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

        public Chip8()
        {
            PC = ProgramStartAddress;
            State = Chip8State.Off;
            _fonts.CopyTo(Memory, 0);
        }

        public void LoadRom(string title)
        {
            var path = Path.Combine("Roms", title);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"{path} is not a valid ROM title!");
            }

            Debug.WriteLine($"Loading {path}");
            using var reader = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
            var bytes = reader.ReadBytes((int) reader.BaseStream.Length);
            bytes.CopyTo(Memory, ProgramStartAddress);
            ProgramSize = bytes.Length + ProgramStartAddress;
            State = Chip8State.Running;
        }

        public void Step()
        {
            Opcode = (ushort) (Memory[PC] << 8 | Memory[PC + 1]);
            PC += 2;
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
                sb.AppendLine($"Register[${i}] -> {Registers[i]}");
            }

            sb.AppendLine($"VF -> {Registers[15]}");

            return sb.ToString();
        }

        public string DumpMemory()
        {
            var sb = new StringBuilder();

            for (var i = 0; i < Memory.Length; ++i)
            {
                sb.AppendLine($"Memory[{i}] -> {Memory[i]}");
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
            var vx = (ushort) ((opcode & 0x0F00) >> 8);
            var vy = (ushort) ((opcode & 0x00F0) >> 4);
            var n = (ushort) (opcode & 0x000F);
            var nn = (ushort) (opcode & 0x00FF);
            var nnn = (ushort) (opcode & 0x0FFF);

            switch (opcode & 0xF000)
            {
                case 0x0000:
                    switch (n)
                    {
                        case 0x0:
                            Array.Clear(Video, 0, Video.Length);
                            ShouldDraw = true;
                            Debug.WriteLine($"0x{opcode:X} -> 00EO: CLS - Clear screen");
                            break;

                        case 0xE:
                            PC = Stack[--SP];
                            Debug.WriteLine($"0x{opcode:X} -> 00EE: RET - Return");
                            break;

                        default:
                            Debug.WriteLine($"0x{opcode:X} -> Unknown 0x00 opcode");
                            break;
                    }

                    break;

                case 0x1000:
                    PC = nnn;
                    Debug.WriteLine($"0x{opcode:X} -> 1NNN: JP addr - Jump to address");
                    break;

                case 0x2000:
                    Stack[SP++] = PC;
                    PC = nnn;
                    Debug.WriteLine($"0x{opcode:X} -> 2NNN: CALL addr - Call subroutine at address");
                    break;

                case 0x3000:
                    if (Registers[vx] == nn)
                    {
                        PC += 2;
                    }

                    Debug.WriteLine($"0x{opcode:X} -> 3XKK: SE Vx, byte - Skip next instruction if Vx = byte");
                    break;

                case 0x4000:
                    if (Registers[vx] != nn)
                    {
                        PC += 2;
                    }

                    Debug.WriteLine($"0x{opcode:X} -> 4XKK: SNE Vx, byte - Skip next instruction if Vx != byte");
                    break;

                case 0x5000:
                    if (Registers[vx] == Registers[vy])
                    {
                        PC += 2;
                    }

                    Debug.WriteLine($"0x{opcode:X} -> 5XY0: SE Vx, Vy - Skip next instruction if Vx = Vy");
                    break;

                case 0x6000:
                    Registers[vx] = (byte) nn;
                    Debug.WriteLine($"0x{opcode:X} -> 6XKK: LD Vx, byte - Set Vx = byte");
                    break;

                case 0x7000:
                    Registers[vx] += (byte) nn;
                    Debug.WriteLine($"0x{opcode:X} -> 7XKK: ADD Vx, byte - Add byte to Vx");
                    break;

                case 0x8000:
                    switch (n)
                    {
                        case 0x0:
                            Registers[vx] = Registers[vy];
                            Debug.WriteLine($"0x{opcode:X} -> 8XY0: LD Vx, Vy - Set Vx = Vy");
                            break;

                        case 0x1:
                            Registers[vx] |= Registers[vy];
                            Debug.WriteLine($"0x{opcode:X} -> 8XY1: OR Vx, Vy - Set Vx = Vx OR Vy");
                            break;

                        case 0x2:
                            Registers[vx] &= Registers[vy];
                            Debug.WriteLine($"0x{opcode:X} -> 8XY2 AND Vx, Vy - Set Vx = Vx AND Vy");
                            break;

                        case 0x3:
                            Registers[vx] ^= Registers[vy];
                            Debug.WriteLine($"0x{opcode:X} -> 8XY3: XOR Vx, Vy - Set Vx = Vx XOR Vy");
                            break;

                        case 0x4:
                            var result = (ushort) (Registers[vx] + Registers[vy]);
                            Registers[vx] = (byte) (result & 0xFF);
                            VF = result > 0xFF ? 1 : 0;
                            Debug.WriteLine($"0x{opcode:X} -> 8XY4: ADD Vx, Vy - Set Vx = Vx + Vy, Set VF = carry");
                            break;

                        case 0x5:
                            VF = Registers[vx] > Registers[vy] ? 1 : 0;
                            Registers[vx] -= Registers[vy];
                            Debug.WriteLine(
                                $"0x{opcode:X} -> 8XY5: SUB Vx, Vy - Set Vx = Vx - Vy, Set VF = not borrow");
                            break;

                        case 0x6:
                            VF = (byte) (Registers[vx] & 1);
                            Registers[vx] >>= 1;
                            Debug.WriteLine($"0x{opcode:X} -> 8XY6: SHR Vx - Set Vx = Vx SHR 1");
                            break;

                        case 0x7:
                            Registers[vx] = (byte) (Registers[vy] - Registers[vx]);
                            VF = Registers[vx] < Registers[vy] ? 1 : 0;
                            Debug.WriteLine(
                                $"0x{opcode:X} -> 8XY7: SUB Vx, Vy - Set Vx = Vy - Vx, Set VF = not borrow");
                            break;

                        case 0xE:
                            VF = (byte) ((Registers[vx] & 0x80) >> 7);
                            Registers[vx] <<= 1;
                            Debug.WriteLine($"0x{opcode:X} -> 8XYE - SHL Vx - Set Vx = Vx SHL 1");
                            break;

                        default:
                            Debug.WriteLine($"Unknown 0x8000 opcode: {opcode:X}.");
                            break;
                    }

                    break;

                case 0x9000:
                    if (Registers[vx] != Registers[vy])
                    {
                        PC += 2;
                    }
                    Debug.WriteLine($"0x{opcode:X} -> 9XY0: SNE Vx, Vy - Skip next instruction if Vx != Vy");
                    break;

                case 0xA000:
                    I = nnn;
                    Debug.WriteLine($"0x{opcode:X} -> ANNN: LD I, addr - Set I = nnn");
                    break;

                case 0xB000:
                    PC = Registers[nnn];
                    Debug.WriteLine($"0x{opcode:X} -> BNNN: JP V0, addr - Jump to address V0 + addr");
                    break;

                case 0xC000:
                    Registers[vx] = (byte) (RandomByte() & nn);
                    Debug.WriteLine($"0x{opcode:X} -> CXKK: RND Vx, byte - Set Vx = random byte AND byte");
                    break;

                case 0xD000:
                    VF = 0;

                    for (var y = 0; y < n; ++y)
                    {
                        var yPos = (Registers[vy] + y) % VideoHeight;
                        for (var x = 0; x < 8; ++x)
                        {
                            if ((Memory[I + y] & 0x80 >> x) != 0)
                            {
                                var xPos = (Registers[vx] + x) % VideoWidth;
                                var spritePos = yPos * VideoWidth + xPos;
                                VF = Video[spritePos];
                                Video[spritePos] ^= 1;
                            }
                        }

                        ShouldDraw = true;
                    }

                    Debug.WriteLine(
                        $"0x{opcode:X} -> DXYN: DRW Vx, Vy, nibble - Display n-byte sprite starting at I to coordinates (Vx, Vy), Set VF = collision");
                    break;

                case 0xE000:
                    switch (nn)
                    {
                        case 0x9E:
                            if (Keypad[Registers[vx]])
                            {
                                PC += 2;
                            }

                            Debug.WriteLine(
                                $"0x{opcode:X} -> EX9E: SKP Vx - Skip next instruction if key with the value of Vx is pressed");
                            break;

                        case 0xA1:
                            if (!Keypad[Registers[vx]])
                            {
                                PC += 2;
                            }

                            Debug.WriteLine(
                                $"0x{opcode:X} -> EXA1: SKNP Vx - Skip next instruction if key with the value of Vx is not pressed");
                            break;

                        default:
                            Debug.WriteLine($"Unknown 0xE000 opcode: {opcode:X}");
                            break;
                    }

                    break;

                case 0xF000:
                    switch (nn)
                    {
                        case 0x07:
                            Registers[vx] = DT;
                            Debug.WriteLine($"0x{opcode:X} -> FX07: LD Vx, DT - Set Vx = delay timer");
                            break;
                        case 0x0A:
                            if (Keypad[0])
                            {
                                Registers[vx] = 0;
                            }
                            else if (Keypad[1])
                            {
                                Registers[vx] = 1;
                            }
                            else if (Keypad[2])
                            {
                                Registers[vx] = 2;
                            }
                            else if (Keypad[3])
                            {
                                Registers[vx] = 3;
                            }
                            else if (Keypad[4])
                            {
                                Registers[vx] = 4;
                            }
                            else if (Keypad[5])
                            {
                                Registers[vx] = 5;
                            }
                            else if (Keypad[6])
                            {
                                Registers[vx] = 6;
                            }
                            else if (Keypad[7])
                            {
                                Registers[vx] = 7;
                            }
                            else if (Keypad[8])
                            {
                                Registers[vx] = 8;
                            }
                            else if (Keypad[9])
                            {
                                Registers[vx] = 9;
                            }
                            else if (Keypad[10])
                            {
                                Registers[vx] = 10;
                            }
                            else if (Keypad[11])
                            {
                                Registers[vx] = 11;
                            }
                            else if (Keypad[12])
                            {
                                Registers[vx] = 12;
                            }
                            else if (Keypad[13])
                            {
                                Registers[vx] = 13;
                            }
                            else if (Keypad[14])
                            {
                                Registers[vx] = 14;
                            }
                            else if (Keypad[15])
                            {
                                Registers[vx] = 15;
                            }
                            else
                            {
                                PC -= 2;
                            }

                            Debug.WriteLine(
                                $"0x{opcode:X} -> FX0A: LD Vx, K - Wait for key press and store the value into Vx");
                            break;
                        case 0x15:
                            DT = Registers[vx];
                            Debug.WriteLine($"0x{opcode:X} -> FX15: LD DT, Vx - Set delay timer = Vx");
                            break;
                        case 0x18:
                            ST = Registers[vx];
                            Debug.WriteLine($"0x{opcode:X} -> FX18: LD ST, Vx - Set sound timer = Vx");
                            break;
                        case 0x1E:
                            I += Registers[vx];
                            Debug.WriteLine($"0x{opcode:X} -> FX1E: Add I, Vx - Set I = I + Vx");
                            break;
                        case 0x29:
                            I = (ushort) (CharSize * Registers[vx]);
                            Debug.WriteLine(
                                $"0x{opcode:X} -> FX29: LD F, Vx - Set I = location of sprite for digit Vx");
                            break;
                        case 0x33:
                        {
                            var result = Registers[vx];
                            Memory[I + 2] = (byte) (result % 10);
                            result /= 10;
                            Memory[I + 1] = (byte) (result % 10);
                            result /= 10;
                            Memory[I] = (byte) (result % 10);
                        }
                            Debug.WriteLine(
                                $"0x{opcode:X} -> FX33: LD B, Vx - Store BCD (Binary-Coded Decimal) representation of Vx in memory locations I, I + 1, and I + 2");
                            break;
                        case 0x55:
                            for (var offset = 0; offset <= vx; ++offset)
                            {
                                Memory[I + offset] = Registers[offset];
                            }

                            Debug.WriteLine(
                                $"0x{opcode:X} -> FX55: LD [I], Vx - Store V0~Vx in memory starting at location I");
                            break;
                        case 0x65:
                            for (var offset = 0; offset <= vx; ++offset)
                            {
                                Registers[offset] = Memory[I + offset];
                            }

                            Debug.WriteLine(
                                $"0x{opcode:X} -> FX65: LD Vx, [I] - Read registers V0~Vx from memory starting at location I");
                            break;
                        default:
                            Debug.WriteLine($"Unknown 0xF000 opcode: {opcode:X}");
                            break;
                    }

                    break;


                default:
                    Debug.WriteLine($"Unknown opcode: {opcode:X & 0xF000}");
                    break;
            }
        }
    }
}