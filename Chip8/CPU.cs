using System;
using System.Diagnostics;

namespace Chip8;

internal static class CPU
{
    internal static void SkipNextInstruction(Memory memory) => memory.PC += 2;

    internal static void Execute(ushort opcode, Memory memory)
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
                Array.Clear(memory.Video, 0, memory.Video.Length);
                break;

            case (0x0, 0x0, 0xE, 0xE):
                Debug.WriteLine($"0x{opcode:X} -> 00EE: RET - Return");
                memory.PC = memory.Stack[--memory.SP];
                break;

            case (0x1, _, _, _):
                Debug.WriteLine($"0x{opcode:X} -> 1NNN: JP addr - Jump to address");
                memory.PC = nnn;
                break;

            case (0x2, _, _, _):
                Debug.WriteLine($"0x{opcode:X} -> 2NNN: CALL addr - Call subroutine at address");
                memory.Stack[memory.SP++] = memory.PC;
                memory.PC = nnn;
                break;

            case (0x3, _, _, _):
                Debug.WriteLine($"0x{opcode:X} -> 3XKK: SE Vx, byte - Skip next instruction if Vx = byte");
                if (memory.Registers[x] == nn)
                {
                    SkipNextInstruction(memory);
                }

                break;

            case (0x4, _, _, _):
                Debug.WriteLine($"0x{opcode:X} -> 4XKK: SNE Vx, byte - Skip next instruction if Vx != byte");
                if (memory.Registers[x] != nn)
                {
                    SkipNextInstruction(memory);
                }

                break;

            case (0x5, _, _, _):
                Debug.WriteLine($"0x{opcode:X} -> 5XY0: SE Vx, Vy - Skip next instruction if Vx = Vy");
                if (memory.Registers[x] == memory.Registers[y])
                {
                    SkipNextInstruction(memory);
                }

                break;
            case (0x6, _, _, _):
                Debug.WriteLine($"0x{opcode:X} -> 6XKK: LD Vx, byte - Set Vx = byte");
                memory.Registers[x] = (byte)nn;
                break;

            case (0x7, _, _, _):
                Debug.WriteLine($"0x{opcode:X} -> 7XKK: ADD Vx, byte - Add byte to Vx");
                memory.Registers[x] += (byte)nn;
                break;

            case (0x8, _, _, 0x0):
                Debug.WriteLine($"0x{opcode:X} -> 8XY0: LD Vx, Vy - Set Vx = Vy");
                memory.Registers[x] = memory.Registers[y];
                break;

            case (0x8, _, _, 0x1):
                Debug.WriteLine($"0x{opcode:X} -> 8XY1: OR Vx, Vy - Set Vx = Vx OR Vy");
                memory.Registers[x] |= memory.Registers[y];
                break;

            case (0x8, _, _, 0x2):
                Debug.WriteLine($"0x{opcode:X} -> 8XY2 AND Vx, Vy - Set Vx = Vx AND Vy");
                memory.Registers[x] &= memory.Registers[y];
                break;

            case (0x8, _, _, 0x3):
                Debug.WriteLine($"0x{opcode:X} -> 8XY3: XOR Vx, Vy - Set Vx = Vx XOR Vy");
                memory.Registers[x] ^= memory.Registers[y];
                break;

            case (0x8, _, _, 0x4):
                Debug.WriteLine($"0x{opcode:X} -> 8XY4: ADD Vx, Vy - Set Vx = Vx + Vy, Set VF = carry");
                result = (ushort)(memory.Registers[x] + memory.Registers[y]);
                memory.Registers[x] = (byte)(result & 0xFF);
                memory.VF = (byte)(result > 0xFF ? 1 : 0);
                break;

            case (0x8, _, _, 0x5):
                Debug.WriteLine($"0x{opcode:X} -> 8XY5: SUB Vx, Vy - Set Vx = Vx - Vy, Set VF = not borrow");
                updatedVF = (byte)(memory.Registers[x] > memory.Registers[y] ? 1 : 0);
                memory.Registers[x] -= memory.Registers[y];
                memory.VF = updatedVF;
                break;

            case (0x8, _, _, 0x6):
                Debug.WriteLine($"0x{opcode:X} -> 8XY6: SHR Vx - Set Vx = Vx SHR 1");
                updatedVF = (byte)(memory.Registers[x] & 1);
                memory.Registers[x] >>= 1;
                memory.VF = updatedVF;
                break;

            case (0x8, _, _, 0x7):
                Debug.WriteLine($"0x{opcode:X} -> 8XY7: SUB Vx, Vy - Set Vx = Vy - Vx, Set VF = not borrow");
                updatedVF = (byte)(memory.Registers[x] < memory.Registers[y] ? 1 : 0);
                memory.Registers[x] = (byte)(memory.Registers[y] - memory.Registers[x]);
                memory.VF = updatedVF;
                break;

            case (0x8, _, _, 0xE):
                Debug.WriteLine($"0x{opcode:X} -> 8XYE - SHL Vx - Set Vx = Vx SHL 1");
                updatedVF = (byte)((memory.Registers[x] & 0x80) >> 7);
                memory.Registers[x] <<= 1;
                memory.VF = updatedVF;
                break;

            case (0x9, _, _, 0x0):
                Debug.WriteLine($"0x{opcode:X} -> 9XY0: SNE Vx, Vy - Skip next instruction if Vx != Vy");
                if (memory.Registers[x] != memory.Registers[y])
                {
                    SkipNextInstruction(memory);
                }

                break;

            case (0xA, _, _, _):
                Debug.WriteLine($"0x{opcode:X} -> ANNN: LD I, addr - Set I = nnn");
                memory.I = nnn;
                break;

            case (0xB, _, _, _):
                Debug.WriteLine($"0x{opcode:X} -> BNNN: JP V0, addr - Jump to address V0 + addr");
                memory.PC = memory.Registers[nnn];
                break;

            case (0xC, _, _, _):
                Debug.WriteLine($"0x{opcode:X} -> CXKK: RND Vx, byte - Set Vx = random byte AND byte");
                memory.Registers[x] = (byte)(memory.RandomByte() & nn);
                break;

            case (0xD, _, _, _):
                Debug.WriteLine(
                    $"0x{opcode:X} -> DXYN: DRW Vx, Vy, nibble - Display n-byte sprite starting at I to coordinates (Vx, Vy), Set VF = collision");

                memory.VF = 0;
                for (var displayY = 0; displayY < d; ++displayY)
                {
                    var yPos = (memory.Registers[y] + displayY) % Chip8.VideoHeight;
                    for (var displayX = 0; displayX < 8; ++displayX)
                    {
                        if ((memory.RAM[memory.I + displayY] & 0x80 >> displayX) != 0)
                        {
                            var xPos = (memory.Registers[x] + displayX) % Chip8.VideoWidth;
                            var spritePos = yPos * Chip8.VideoWidth + xPos;
                            memory.VF = memory.Video[spritePos];
                            memory.Video[spritePos] ^= 1;
                        }
                    }
                }

                break;

            case (0xE, _, 0x9, 0xE):
                Debug.WriteLine(
                    $"0x{opcode:X} -> EX9E: SKP Vx - Skip next instruction if key with the value of Vx is pressed");

                if (memory.Keypad[memory.Registers[x]])
                {
                    SkipNextInstruction(memory);
                }

                break;

            case (0xE, _, 0xA, 0x1):
                Debug.WriteLine(
                    $"0x{opcode:X} -> EXA1: SKNP Vx - Skip next instruction if key with the value of Vx is not pressed");

                if (!memory.Keypad[memory.Registers[x]])
                {
                    SkipNextInstruction(memory);
                }

                break;

            case (0xF, _, 0x0, 0x7):
                Debug.WriteLine($"0x{opcode:X} -> FX07: LD Vx, memory.DT - Set Vx = delay timer");
                memory.Registers[x] = memory.DT;
                break;

            case (0xF, _, 0x0, 0xA):
                Debug.WriteLine($"0x{opcode:X} -> FX0A: LD Vx, K - Wait for key press and store the value into Vx");
                if (memory.Keypad[0])
                {
                    memory.Registers[x] = 0;
                }
                else if (memory.Keypad[1])
                {
                    memory.Registers[x] = 1;
                }
                else if (memory.Keypad[2])
                {
                    memory.Registers[x] = 2;
                }
                else if (memory.Keypad[3])
                {
                    memory.Registers[x] = 3;
                }
                else if (memory.Keypad[4])
                {
                    memory.Registers[x] = 4;
                }
                else if (memory.Keypad[5])
                {
                    memory.Registers[x] = 5;
                }
                else if (memory.Keypad[6])
                {
                    memory.Registers[x] = 6;
                }
                else if (memory.Keypad[7])
                {
                    memory.Registers[x] = 7;
                }
                else if (memory.Keypad[8])
                {
                    memory.Registers[x] = 8;
                }
                else if (memory.Keypad[9])
                {
                    memory.Registers[x] = 9;
                }
                else if (memory.Keypad[10])
                {
                    memory.Registers[x] = 10;
                }
                else if (memory.Keypad[11])
                {
                    memory.Registers[x] = 11;
                }
                else if (memory.Keypad[12])
                {
                    memory.Registers[x] = 12;
                }
                else if (memory.Keypad[13])
                {
                    memory.Registers[x] = 13;
                }
                else if (memory.Keypad[14])
                {
                    memory.Registers[x] = 14;
                }
                else if (memory.Keypad[15])
                {
                    memory.Registers[x] = 15;
                }
                else
                {
                    memory.PC -= 2;
                }

                break;

            case (0xF, _, 0x1, 0x5):
                Debug.WriteLine($"0x{opcode:X} -> FX15: LD memory.DT, Vx - Set delay timer = Vx");
                memory.DT = memory.Registers[x];
                break;

            case (0xF, _, 0x1, 0x8):
                Debug.WriteLine($"0x{opcode:X} -> FX18: LD memory.ST, Vx - Set sound timer = Vx");
                memory.ST = memory.Registers[x];
                break;

            case (0xF, _, 0x1, 0xE):
                Debug.WriteLine($"0x{opcode:X} -> FX1E: Add I, Vx - Set I = I + Vx");
                memory.I += memory.Registers[x];
                break;

            case (0xF, _, 0x2, 0x9):
                Debug.WriteLine(
                    $"0x{opcode:X} -> FX29: LD F, Vx - Set I = location of sprite for digit Vx");

                memory.I = (ushort)(Memory.CharSize * memory.Registers[x]);
                break;

            case (0xF, _, 0x3, 0x3):
                Debug.WriteLine(
                    $"0x{opcode:X} -> FX33: LD B, Vx - Store BCD (Binary-Coded Decimal) representation of Vx in memory locations I, I + 1, and I + 2");

                result = memory.Registers[x];
                memory.RAM[memory.I + 2] = (byte)(result % 10);
                result /= 10;
                memory.RAM[memory.I + 1] = (byte)(result % 10);
                result /= 10;
                memory.RAM[memory.I] = (byte)(result % 10);
                break;

            case (0xF, _, 0x5, 0x5):
                Debug.WriteLine(
                    $"0x{opcode:X} -> FX55: LD [memory.I], Vx - Store V0~Vx in memory starting at location I");

                for (var offset = 0; offset <= x; ++offset)
                {
                    memory.RAM[memory.I + offset] = memory.Registers[offset];
                }

                break;

            case (0xF, _, 0x6, 0x5):
                Debug.WriteLine(
                    $"0x{opcode:X} -> FX65: LD Vx, [memory.I] - Read registers V0~Vx from memory starting at location I");

                for (var offset = 0; offset <= x; ++offset)
                {
                    memory.Registers[offset] = memory.RAM[memory.I + offset];
                }

                break;

            default:
                Debug.WriteLine($"Unknown opcode: {opcode:X}");
                break;
        }
    }
}
