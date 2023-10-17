using System;
using System.Diagnostics;

namespace Chip8;

static public class CPU
{
    internal static void SkipNextInstruction(Memory memory) => memory.PC += 2;

    static public void Execute(ushort opcode, Memory memory)
    {
        var c = (ushort)((opcode & 0xF000) >> 12);
        var x = (ushort)((opcode & 0x0F00) >> 8);
        var y = (ushort)((opcode & 0x00F0) >> 4);
        var d = (ushort)(opcode & 0x000F);

        var nn = (ushort)(opcode & 0x00FF);
        var nnn = (ushort)(opcode & 0x0FFF);

        switch (c, x, y, d)
        {
            case (0x0, 0x0, 0xE, 0x0):
                Op00E0(opcode, memory);
                break;
            case (0x0, 0x0, 0xE, 0xE):
                Op00EE(opcode, memory);
                break;
            case (0x1, _, _, _):
                Op1NNN(opcode, memory, nnn);
                break;
            case (0x2, _, _, _):
                Op2NNN(opcode, memory, nnn);
                break;
            case (0x3, _, _, _):
                Op3XKK(opcode, memory, x, nn);
                break;
            case (0x4, _, _, _):
                Op4XKK(opcode, memory, x, nn);
                break;
            case (0x5, _, _, _):
                Op5XY0(opcode, memory, x, y);
                break;
            case (0x6, _, _, _):
                Op6XKK(opcode, memory, x, nn);
                break;
            case (0x7, _, _, _):
                Op7XKK(opcode, memory, x, nn);
                break;
            case (0x8, _, _, 0x0):
                Op8XY0(opcode, memory, x, y);
                break;
            case (0x8, _, _, 0x1):
                Op8XY1(opcode, memory, x, y);
                break;
            case (0x8, _, _, 0x2):
                Op8XY2(opcode, memory, x, y);
                break;
            case (0x8, _, _, 0x3):
                Op8XY3(opcode, memory, x, y);
                break;
            case (0x8, _, _, 0x4):
                Op8XY4(opcode, memory, x, y);
                break;
            case (0x8, _, _, 0x5):
                Op8XY5(opcode, memory, x, y);
                break;
            case (0x8, _, _, 0x6):
                Op8XY6(opcode, memory, x);
                break;
            case (0x8, _, _, 0x7):
                Op8XY7(opcode, memory, x, y);
                break;
            case (0x8, _, _, 0xE):
                Op8XYE(opcode, memory, x);
                break;
            case (0x9, _, _, 0x0):
                Op9XY0(opcode, memory, x, y);
                break;
            case (0xA, _, _, _):
                OpANNN(opcode, memory, nnn);
                break;
            case (0xB, _, _, _):
                OpBNNN(opcode, memory, nnn);
                break;
            case (0xC, _, _, _):
                OpCXKK(opcode, memory, x, nn);
                break;
            case (0xD, _, _, _):
                OpDXYN(opcode, memory, d, x, y);
                break;
            case (0xE, _, 0x9, 0xE):
                OpEX9E(opcode, memory, x);
                break;
            case (0xE, _, 0xA, 0x1):
                OpEXA1(opcode, memory, x);
                break;
            case (0xF, _, 0x0, 0x7):
                OpFX07(opcode, memory, x);
                break;
            case (0xF, _, 0x0, 0xA):
                OpFX0A(opcode, memory, x);
                break;
            case (0xF, _, 0x1, 0x5):
                OpFX15(opcode, memory, x);
                break;
            case (0xF, _, 0x1, 0x8):
                OpFX18(opcode, memory, x);
                break;
            case (0xF, _, 0x1, 0xE):
                OpFX1E(opcode, memory, x);
                break;
            case (0xF, _, 0x2, 0x9):
                OpFX29(opcode, memory, x);
                break;
            case (0xF, _, 0x3, 0x3):
                OpFX33(opcode, memory, x);
                break;
            case (0xF, _, 0x5, 0x5):
                OpFX55(opcode, memory, x);
                break;
            case (0xF, _, 0x6, 0x5):
                OpFX65(opcode, memory, x);
                break;
            default:
                Debug.WriteLine($"Unknown opcode: {opcode:X}");
                break;
        }
    }

    private static void Op00E0(ushort opcode, Memory memory)
    {
        Debug.WriteLine($"0x{opcode:X} -> 00E0: CLS - Clear screen");
        Array.Clear(memory.Video, 0, memory.Video.Length);
    }

    private static void Op00EE(ushort opcode, Memory memory)
    {
        Debug.WriteLine($"0x{opcode:X} -> 00EE: RET - Return");
        memory.PC = memory.Stack[--memory.SP];
    }

    private static void Op1NNN(ushort opcode, Memory memory, ushort nnn)
    {
        Debug.WriteLine($"0x{opcode:X} -> 1NNN: JP addr - Jump to address");
        memory.PC = nnn;
    }

    private static void Op2NNN(ushort opcode, Memory memory, ushort nnn)
    {
        Debug.WriteLine($"0x{opcode:X} -> 2NNN: CALL addr - Call subroutine at address");
        memory.Stack[memory.SP++] = memory.PC;
        memory.PC = nnn;
    }

    private static void Op3XKK(ushort opcode, Memory memory, ushort x, ushort nn)
    {
        Debug.WriteLine($"0x{opcode:X} -> 3XKK: SE Vx, byte - Skip next instruction if Vx = byte");
        if (memory.Registers[x] == nn)
        {
            SkipNextInstruction(memory);
        }
    }

    private static void Op4XKK(ushort opcode, Memory memory, ushort x, ushort nn)
    {
        Debug.WriteLine($"0x{opcode:X} -> 4XKK: SNE Vx, byte - Skip next instruction if Vx != byte");
        if (memory.Registers[x] != nn)
        {
            SkipNextInstruction(memory);
        }
    }

    private static void Op5XY0(ushort opcode, Memory memory, ushort x, ushort y)
    {
        Debug.WriteLine($"0x{opcode:X} -> 5XY0: SE Vx, Vy - Skip next instruction if Vx = Vy");
        if (memory.Registers[x] == memory.Registers[y])
        {
            SkipNextInstruction(memory);
        }
    }

    private static void Op6XKK(ushort opcode, Memory memory, ushort x, ushort nn)
    {
        Debug.WriteLine($"0x{opcode:X} -> 6XKK: LD Vx, byte - Set Vx = byte");
        memory.Registers[x] = (byte)nn;
    }

    private static void Op7XKK(ushort opcode, Memory memory, ushort x, ushort nn)
    {
        Debug.WriteLine($"0x{opcode:X} -> 7XKK: ADD Vx, byte - Add byte to Vx");
        memory.Registers[x] += (byte)nn;
    }

    private static void Op8XY0(ushort opcode, Memory memory, ushort x, ushort y)
    {
        Debug.WriteLine($"0x{opcode:X} -> 8XY0: LD Vx, Vy - Set Vx = Vy");
        memory.Registers[x] = memory.Registers[y];
    }

    private static void Op8XY1(ushort opcode, Memory memory, ushort x, ushort y)
    {
        Debug.WriteLine($"0x{opcode:X} -> 8XY1: OR Vx, Vy - Set Vx = Vx OR Vy");
        memory.Registers[x] |= memory.Registers[y];
    }

    private static void Op8XY2(ushort opcode, Memory memory, ushort x, ushort y)
    {
        Debug.WriteLine($"0x{opcode:X} -> 8XY2 AND Vx, Vy - Set Vx = Vx AND Vy");
        memory.Registers[x] &= memory.Registers[y];
    }

    private static void Op8XY3(ushort opcode, Memory memory, ushort x, ushort y)
    {
        Debug.WriteLine($"0x{opcode:X} -> 8XY3: XOR Vx, Vy - Set Vx = Vx XOR Vy");
        memory.Registers[x] ^= memory.Registers[y];
    }

    private static void Op8XY4(ushort opcode, Memory memory, ushort x, ushort y)
    {
        Debug.WriteLine($"0x{opcode:X} -> 8XY4: ADD Vx, Vy - Set Vx = Vx + Vy, Set VF = carry");
        var result = memory.Registers[x] + memory.Registers[y];
        memory.Registers[x] = (byte)(result & 0xFF);
        memory.VF = (byte)(result > 0xFF ? 1 : 0);
    }

    private static void Op8XY5(ushort opcode, Memory memory, ushort x, ushort y)
    {
        Debug.WriteLine($"0x{opcode:X} -> 8XY5: SUB Vx, Vy - Set Vx = Vx - Vy, Set VF = not borrow");
        var updatedVF = (byte)(memory.Registers[x] > memory.Registers[y] ? 1 : 0);
        memory.Registers[x] -= memory.Registers[y];
        memory.VF = updatedVF;
    }

    private static void Op8XY6(ushort opcode, Memory memory, ushort x)
    {
        Debug.WriteLine($"0x{opcode:X} -> 8XY6: SHR Vx - Set Vx = Vx SHR 1");
        var updatedVF = (byte)(memory.Registers[x] & 1);
        memory.Registers[x] >>= 1;
        memory.VF = updatedVF;
    }

    private static void Op8XY7(ushort opcode, Memory memory, ushort x, ushort y)
    {
        Debug.WriteLine($"0x{opcode:X} -> 8XY7: SUB Vx, Vy - Set Vx = Vy - Vx, Set VF = not borrow");
        var updatedVF = (byte)(memory.Registers[x] < memory.Registers[y] ? 1 : 0);
        memory.Registers[x] = (byte)(memory.Registers[y] - memory.Registers[x]);
        memory.VF = updatedVF;
    }

    private static void Op8XYE(ushort opcode, Memory memory, ushort x)
    {
        Debug.WriteLine($"0x{opcode:X} -> 8XYE - SHL Vx - Set Vx = Vx SHL 1");
        var updatedVF = (byte)((memory.Registers[x] & 0x80) >> 7);
        memory.Registers[x] <<= 1;
        memory.VF = updatedVF;
    }

    private static void Op9XY0(ushort opcode, Memory memory, ushort x, ushort y)
    {
        Debug.WriteLine($"0x{opcode:X} -> 9XY0: SNE Vx, Vy - Skip next instruction if Vx != Vy");
        if (memory.Registers[x] != memory.Registers[y])
        {
            SkipNextInstruction(memory);
        }
    }

    private static void OpANNN(ushort opcode, Memory memory, ushort nnn)
    {
        Debug.WriteLine($"0x{opcode:X} -> ANNN: LD I, addr - Set I = nnn");
        memory.I = nnn;
    }

    private static void OpBNNN(ushort opcode, Memory memory, ushort nnn)
    {
        Debug.WriteLine($"0x{opcode:X} -> BNNN: JP V0, addr - Jump to address V0 + addr");
        memory.PC = memory.Registers[nnn];
    }

    private static void OpCXKK(ushort opcode, Memory memory, ushort x, ushort nn)
    {
        Debug.WriteLine($"0x{opcode:X} -> CXKK: RND Vx, byte - Set Vx = random byte AND byte");
        memory.Registers[x] = (byte)(memory.RandomByte() & nn);
    }

    private static void OpDXYN(ushort opcode, Memory memory, ushort d, ushort x, ushort y)
    {
        Debug.WriteLine(
            $"0x{opcode:X} -> DXYN: DRW Vx, Vy, nibble - Display n-byte sprite starting at I to coordinates (Vx, Vy), Set VF = collision");

        byte collision = 0;
        for (var displayY = 0; displayY < d; ++displayY)
        {
            var pixel = memory.RAM[memory.I + displayY];
            for (var displayX = 0; displayX < 8; ++displayX)
            {
                if ((pixel & 0x80 >> displayX) != 0)
                {
                    var xPos = (memory.Registers[x] + displayX) % Chip8.VideoWidth;
                    var yPos = (memory.Registers[y] + displayY) % Chip8.VideoHeight;
                    var pixelPos = yPos * Chip8.VideoWidth + xPos;
                    if (memory.Video[pixelPos] == 0x1)
                    {
                        collision = 1;
                    }
                    memory.Video[pixelPos] ^= 0x1;
                }
            }
        }
        memory.VF = collision;
    }

    private static void OpEX9E(ushort opcode, Memory memory, ushort x)
    {
        Debug.WriteLine(
            $"0x{opcode:X} -> EX9E: SKP Vx - Skip next instruction if key with the value of Vx is pressed");

        if (memory.Keypad[memory.Registers[x]])
        {
            SkipNextInstruction(memory);
        }
    }

    private static void OpEXA1(ushort opcode, Memory memory, ushort x)
    {
        Debug.WriteLine(
            $"0x{opcode:X} -> EXA1: SKNP Vx - Skip next instruction if key with the value of Vx is not pressed");

        if (!memory.Keypad[memory.Registers[x]])
        {
            SkipNextInstruction(memory);
        }
    }

    private static void OpFX07(ushort opcode, Memory memory, ushort x)
    {
        Debug.WriteLine($"0x{opcode:X} -> FX07: LD Vx, memory.DT - Set Vx = delay timer");
        memory.Registers[x] = memory.DT;
    }

    private static void OpFX0A(ushort opcode, Memory memory, ushort x)
    {
        Debug.WriteLine($"0x{opcode:X} -> FX0A: LD Vx, K - Wait for key press and store the value into Vx");
        var activeKeypad = Array.FindIndex(memory.Keypad, keypad => keypad);
        if (activeKeypad >= 0)
        {
            memory.Registers[x] = (byte)activeKeypad;
        }
        else
        {
            memory.PC -= 2;
        }
    }

    private static void OpFX15(ushort opcode, Memory memory, ushort x)
    {
        Debug.WriteLine($"0x{opcode:X} -> FX15: LD memory.DT, Vx - Set delay timer = Vx");
        memory.DT = memory.Registers[x];
    }

    private static void OpFX18(ushort opcode, Memory memory, ushort x)
    {
        Debug.WriteLine($"0x{opcode:X} -> FX18: LD memory.ST, Vx - Set sound timer = Vx");
        memory.ST = memory.Registers[x];
    }

    private static void OpFX1E(ushort opcode, Memory memory, ushort x)
    {
        Debug.WriteLine($"0x{opcode:X} -> FX1E: Add I, Vx - Set I = I + Vx");
        memory.I += memory.Registers[x];
    }

    private static void OpFX29(ushort opcode, Memory memory, ushort x)
    {
        Debug.WriteLine(
            $"0x{opcode:X} -> FX29: LD F, Vx - Set I = location of sprite for digit Vx");

        memory.I = (ushort)(Memory.CharSize * memory.Registers[x]);
    }

    private static void OpFX33(ushort opcode, Memory memory, ushort x)
    {
        Debug.WriteLine(
            $"0x{opcode:X} -> FX33: LD B, Vx - Store BCD (Binary-Coded Decimal) representation of Vx in memory locations I, I + 1, and I + 2");

        ushort result = memory.Registers[x];

        for (var offset = 2; offset >= 0; offset--)
        {
            memory.RAM[memory.I + offset] = (byte)(result % 10);
            result /= 10;
        }
    }

    private static void OpFX55(ushort opcode, Memory memory, ushort x)
    {
        Debug.WriteLine(
            $"0x{opcode:X} -> FX55: LD [memory.I], Vx - Store V0~Vx in memory starting at location I");

        for (var offset = 0; offset <= x; ++offset)
        {
            memory.RAM[memory.I + offset] = memory.Registers[offset];
        }
    }

    private static void OpFX65(ushort opcode, Memory memory, ushort x)
    {
        Debug.WriteLine(
            $"0x{opcode:X} -> FX65: LD Vx, [memory.I] - Read registers V0~Vx from memory starting at location I");

        for (var offset = 0; offset <= x; ++offset)
        {
            memory.Registers[offset] = memory.RAM[memory.I + offset];
        }
    }
}
