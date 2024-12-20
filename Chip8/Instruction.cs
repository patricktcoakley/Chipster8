using System;
using System.Diagnostics;

namespace Chip8;

public static class Instruction
{
    private static void SkipNextInstruction(VirtualMachine virtualMachine) => virtualMachine.PC += 2;

    public static void Execute(ushort opcode, VirtualMachine virtualMachine)
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
                Op00E0(opcode, virtualMachine);
                break;
            case (0x0, 0x0, 0xE, 0xE):
                Op00EE(opcode, virtualMachine);
                break;
            case (0x1, _, _, _):
                Op1NNN(opcode, virtualMachine, nnn);
                break;
            case (0x2, _, _, _):
                Op2NNN(opcode, virtualMachine, nnn);
                break;
            case (0x3, _, _, _):
                Op3XKK(opcode, virtualMachine, x, nn);
                break;
            case (0x4, _, _, _):
                Op4XKK(opcode, virtualMachine, x, nn);
                break;
            case (0x5, _, _, _):
                Op5XY0(opcode, virtualMachine, x, y);
                break;
            case (0x6, _, _, _):
                Op6XKK(opcode, virtualMachine, x, nn);
                break;
            case (0x7, _, _, _):
                Op7XKK(opcode, virtualMachine, x, nn);
                break;
            case (0x8, _, _, 0x0):
                Op8XY0(opcode, virtualMachine, x, y);
                break;
            case (0x8, _, _, 0x1):
                Op8XY1(opcode, virtualMachine, x, y);
                break;
            case (0x8, _, _, 0x2):
                Op8XY2(opcode, virtualMachine, x, y);
                break;
            case (0x8, _, _, 0x3):
                Op8XY3(opcode, virtualMachine, x, y);
                break;
            case (0x8, _, _, 0x4):
                Op8XY4(opcode, virtualMachine, x, y);
                break;
            case (0x8, _, _, 0x5):
                Op8XY5(opcode, virtualMachine, x, y);
                break;
            case (0x8, _, _, 0x6):
                Op8XY6(opcode, virtualMachine, x);
                break;
            case (0x8, _, _, 0x7):
                Op8XY7(opcode, virtualMachine, x, y);
                break;
            case (0x8, _, _, 0xE):
                Op8XYE(opcode, virtualMachine, x);
                break;
            case (0x9, _, _, 0x0):
                Op9XY0(opcode, virtualMachine, x, y);
                break;
            case (0xA, _, _, _):
                OpANNN(opcode, virtualMachine, nnn);
                break;
            case (0xB, _, _, _):
                OpBNNN(opcode, virtualMachine, nnn);
                break;
            case (0xC, _, _, _):
                OpCXKK(opcode, virtualMachine, x, nn);
                break;
            case (0xD, _, _, _):
                OpDXYN(opcode, virtualMachine, d, x, y);
                break;
            case (0xE, _, 0x9, 0xE):
                OpEX9E(opcode, virtualMachine, x);
                break;
            case (0xE, _, 0xA, 0x1):
                OpEXA1(opcode, virtualMachine, x);
                break;
            case (0xF, _, 0x0, 0x7):
                OpFX07(opcode, virtualMachine, x);
                break;
            case (0xF, _, 0x0, 0xA):
                OpFX0A(opcode, virtualMachine, x);
                break;
            case (0xF, _, 0x1, 0x5):
                OpFX15(opcode, virtualMachine, x);
                break;
            case (0xF, _, 0x1, 0x8):
                OpFX18(opcode, virtualMachine, x);
                break;
            case (0xF, _, 0x1, 0xE):
                OpFX1E(opcode, virtualMachine, x);
                break;
            case (0xF, _, 0x2, 0x9):
                OpFX29(opcode, virtualMachine, x);
                break;
            case (0xF, _, 0x3, 0x3):
                OpFX33(opcode, virtualMachine, x);
                break;
            case (0xF, _, 0x5, 0x5):
                OpFX55(opcode, virtualMachine, x);
                break;
            case (0xF, _, 0x6, 0x5):
                OpFX65(opcode, virtualMachine, x);
                break;
            default:
                Debug.WriteLine($"Unknown opcode: {opcode:X}");
                break;
        }
    }

    private static void Op00E0(ushort opcode, VirtualMachine virtualMachine)
    {
        Debug.WriteLine($"0x{opcode:X} -> 00E0: CLS - Clear screen");
        Array.Clear(virtualMachine.Video, 0, virtualMachine.Video.Length);

        SkipNextInstruction(virtualMachine);
    }

    private static void Op00EE(ushort opcode, VirtualMachine virtualMachine)
    {
        Debug.WriteLine($"0x{opcode:X} -> 00EE: RET - Return");
        virtualMachine.SP -= 1;
        virtualMachine.PC = virtualMachine.Stack[virtualMachine.SP];

        SkipNextInstruction(virtualMachine);
    }

    private static void Op1NNN(ushort opcode, VirtualMachine virtualMachine, ushort nnn)
    {
        Debug.WriteLine($"0x{opcode:X} -> 1NNN: JP addr - Jump to address");
        virtualMachine.PC = nnn;
    }

    private static void Op2NNN(ushort opcode, VirtualMachine virtualMachine, ushort nnn)
    {
        Debug.WriteLine($"0x{opcode:X} -> 2NNN: CALL addr - Call subroutine at address");
        virtualMachine.Stack[virtualMachine.SP++] = virtualMachine.PC;
        virtualMachine.PC = nnn;
    }

    private static void Op3XKK(ushort opcode, VirtualMachine virtualMachine, ushort x, ushort nn)
    {
        Debug.WriteLine($"0x{opcode:X} -> 3XKK: SE Vx, byte - Skip next instruction if Vx = byte");
        if (virtualMachine.Registers[x] == nn)
        {
            SkipNextInstruction(virtualMachine);
        }

        SkipNextInstruction(virtualMachine);
    }

    private static void Op4XKK(ushort opcode, VirtualMachine virtualMachine, ushort x, ushort nn)
    {
        Debug.WriteLine($"0x{opcode:X} -> 4XKK: SNE Vx, byte - Skip next instruction if Vx != byte");
        if (virtualMachine.Registers[x] != nn)
        {
            SkipNextInstruction(virtualMachine);
        }

        SkipNextInstruction(virtualMachine);
    }

    private static void Op5XY0(ushort opcode, VirtualMachine virtualMachine, ushort x, ushort y)
    {
        Debug.WriteLine($"0x{opcode:X} -> 5XY0: SE Vx, Vy - Skip next instruction if Vx = Vy");
        if (virtualMachine.Registers[x] == virtualMachine.Registers[y])
        {
            SkipNextInstruction(virtualMachine);
        }

        SkipNextInstruction(virtualMachine);
    }

    private static void Op6XKK(ushort opcode, VirtualMachine virtualMachine, ushort x, ushort nn)
    {
        Debug.WriteLine($"0x{opcode:X} -> 6XKK: LD Vx, byte - Set Vx = byte");
        virtualMachine.Registers[x] = (byte)nn;

        SkipNextInstruction(virtualMachine);
    }

    private static void Op7XKK(ushort opcode, VirtualMachine virtualMachine, ushort x, ushort nn)
    {
        Debug.WriteLine($"0x{opcode:X} -> 7XKK: ADD Vx, byte - Add byte to Vx");
        virtualMachine.Registers[x] += (byte)nn;

        SkipNextInstruction(virtualMachine);
    }

    private static void Op8XY0(ushort opcode, VirtualMachine virtualMachine, ushort x, ushort y)
    {
        Debug.WriteLine($"0x{opcode:X} -> 8XY0: LD Vx, Vy - Set Vx = Vy");
        virtualMachine.Registers[x] = virtualMachine.Registers[y];

        SkipNextInstruction(virtualMachine);
    }

    private static void Op8XY1(ushort opcode, VirtualMachine virtualMachine, ushort x, ushort y)
    {
        Debug.WriteLine($"0x{opcode:X} -> 8XY1: OR Vx, Vy - Set Vx = Vx OR Vy");
        virtualMachine.Registers[x] |= virtualMachine.Registers[y];

        SkipNextInstruction(virtualMachine);
    }

    private static void Op8XY2(ushort opcode, VirtualMachine virtualMachine, ushort x, ushort y)
    {
        Debug.WriteLine($"0x{opcode:X} -> 8XY2 AND Vx, Vy - Set Vx = Vx AND Vy");
        virtualMachine.Registers[x] &= virtualMachine.Registers[y];

        SkipNextInstruction(virtualMachine);
    }

    private static void Op8XY3(ushort opcode, VirtualMachine virtualMachine, ushort x, ushort y)
    {
        Debug.WriteLine($"0x{opcode:X} -> 8XY3: XOR Vx, Vy - Set Vx = Vx XOR Vy");
        virtualMachine.Registers[x] ^= virtualMachine.Registers[y];

        SkipNextInstruction(virtualMachine);
    }

    private static void Op8XY4(ushort opcode, VirtualMachine virtualMachine, ushort x, ushort y)
    {
        Debug.WriteLine($"0x{opcode:X} -> 8XY4: ADD Vx, Vy - Set Vx = Vx + Vy, Set VF = carry");
        var result = virtualMachine.Registers[x] + virtualMachine.Registers[y];
        virtualMachine.Registers[x] = (byte)(result & 0xFF);
        virtualMachine.VF = (byte)(result > 0xFF ? 1 : 0);

        SkipNextInstruction(virtualMachine);
    }

    private static void Op8XY5(ushort opcode, VirtualMachine virtualMachine, ushort x, ushort y)
    {
        Debug.WriteLine($"0x{opcode:X} -> 8XY5: SUB Vx, Vy - Set Vx = Vx - Vy, Set VF = not borrow");
        var updatedVF = (byte)(virtualMachine.Registers[x] > virtualMachine.Registers[y] ? 1 : 0);
        virtualMachine.Registers[x] -= virtualMachine.Registers[y];
        virtualMachine.VF = updatedVF;

        SkipNextInstruction(virtualMachine);
    }

    private static void Op8XY6(ushort opcode, VirtualMachine virtualMachine, ushort x)
    {
        Debug.WriteLine($"0x{opcode:X} -> 8XY6: SHR Vx - Set Vx = Vx SHR 1");
        var updatedVF = (byte)(virtualMachine.Registers[x] & 1);
        virtualMachine.Registers[x] >>= 1;
        virtualMachine.VF = updatedVF;

        SkipNextInstruction(virtualMachine);
    }

    private static void Op8XY7(ushort opcode, VirtualMachine virtualMachine, ushort x, ushort y)
    {
        Debug.WriteLine($"0x{opcode:X} -> 8XY7: SUB Vx, Vy - Set Vx = Vy - Vx, Set VF = not borrow");
        var updatedVF = (byte)(virtualMachine.Registers[x] < virtualMachine.Registers[y] ? 1 : 0);
        virtualMachine.Registers[x] = (byte)(virtualMachine.Registers[y] - virtualMachine.Registers[x]);
        virtualMachine.VF = updatedVF;

        SkipNextInstruction(virtualMachine);
    }

    private static void Op8XYE(ushort opcode, VirtualMachine virtualMachine, ushort x)
    {
        Debug.WriteLine($"0x{opcode:X} -> 8XYE - SHL Vx - Set Vx = Vx SHL 1");
        var updatedVF = (byte)((virtualMachine.Registers[x] & 0x80) >> 7);
        virtualMachine.Registers[x] <<= 1;
        virtualMachine.VF = updatedVF;

        SkipNextInstruction(virtualMachine);
    }

    private static void Op9XY0(ushort opcode, VirtualMachine virtualMachine, ushort x, ushort y)
    {
        Debug.WriteLine($"0x{opcode:X} -> 9XY0: SNE Vx, Vy - Skip next instruction if Vx != Vy");
        if (virtualMachine.Registers[x] != virtualMachine.Registers[y])
        {
            SkipNextInstruction(virtualMachine);
        }

        SkipNextInstruction(virtualMachine);
    }

    private static void OpANNN(ushort opcode, VirtualMachine virtualMachine, ushort nnn)
    {
        Debug.WriteLine($"0x{opcode:X} -> ANNN: LD I, addr - Set I = nnn");
        virtualMachine.I = nnn;

        SkipNextInstruction(virtualMachine);
    }

    private static void OpBNNN(ushort opcode, VirtualMachine virtualMachine, ushort nnn)
    {
        Debug.WriteLine($"0x{opcode:X} -> BNNN: JP V0, addr - Jump to address V0 + addr");
        virtualMachine.PC = (ushort)(virtualMachine.Registers[0] + nnn);

        SkipNextInstruction(virtualMachine);
    }

    private static void OpCXKK(ushort opcode, VirtualMachine virtualMachine, ushort x, ushort nn)
    {
        Debug.WriteLine($"0x{opcode:X} -> CXKK: RND Vx, byte - Set Vx = random byte AND byte");
        virtualMachine.Registers[x] = (byte)(virtualMachine.RandomByte() & nn);

        SkipNextInstruction(virtualMachine);
    }

    private static void OpDXYN(ushort opcode, VirtualMachine virtualMachine, ushort d, ushort x, ushort y)
    {
        Debug.WriteLine(
            $"0x{opcode:X} -> DXYN: DRW Vx, Vy, nibble - Display n-byte sprite starting at I to coordinates (Vx, Vy), Set VF = collision");

        byte collision = 0;
        for (var displayY = 0; displayY < d; ++displayY)
        {
            var pixel = virtualMachine.RAM[virtualMachine.I + displayY];
            for (var displayX = 0; displayX < 8; ++displayX)
            {
                if ((pixel & 0x80 >> displayX) != 0)
                {
                    var xPos = (virtualMachine.Registers[x] + displayX) % virtualMachine.VideoWidth;
                    var yPos = (virtualMachine.Registers[y] + displayY) % virtualMachine.VideoHeight;
                    var pixelPos = yPos * virtualMachine.VideoWidth + xPos;
                    if (virtualMachine.Video[pixelPos] == 0x1)
                    {
                        collision = 1;
                    }

                    virtualMachine.Video[pixelPos] ^= 0x1;
                }
            }
        }

        virtualMachine.VF = collision;

        SkipNextInstruction(virtualMachine);
    }

    private static void OpEX9E(ushort opcode, VirtualMachine virtualMachine, ushort x)
    {
        Debug.WriteLine(
            $"0x{opcode:X} -> EX9E: SKP Vx - Skip next instruction if key with the value of Vx is pressed");

        if (virtualMachine.Keypad[virtualMachine.Registers[x]])
        {
            SkipNextInstruction(virtualMachine);
        }

        SkipNextInstruction(virtualMachine);
    }

    private static void OpEXA1(ushort opcode, VirtualMachine virtualMachine, ushort x)
    {
        Debug.WriteLine(
            $"0x{opcode:X} -> EXA1: SKNP Vx - Skip next instruction if key with the value of Vx is not pressed");

        if (!virtualMachine.Keypad[virtualMachine.Registers[x]])
        {
            SkipNextInstruction(virtualMachine);
        }

        SkipNextInstruction(virtualMachine);
    }

    private static void OpFX07(ushort opcode, VirtualMachine virtualMachine, ushort x)
    {
        Debug.WriteLine($"0x{opcode:X} -> FX07: LD Vx, memory.DT - Set Vx = delay timer");
        virtualMachine.Registers[x] = virtualMachine.DT;

        SkipNextInstruction(virtualMachine);
    }

    private static void OpFX0A(ushort opcode, VirtualMachine virtualMachine, ushort x)
    {
        Debug.WriteLine($"0x{opcode:X} -> FX0A: LD Vx, K - Wait for key press and store the value into Vx");
        var activeKeypad = Array.FindIndex(virtualMachine.Keypad, keypad => keypad);
        if (activeKeypad < 0)
        {
            return;
        }

        virtualMachine.Registers[x] = (byte)activeKeypad;
        SkipNextInstruction(virtualMachine);
    }

    private static void OpFX15(ushort opcode, VirtualMachine virtualMachine, ushort x)
    {
        Debug.WriteLine($"0x{opcode:X} -> FX15: LD memory.DT, Vx - Set delay timer = Vx");
        virtualMachine.DT = virtualMachine.Registers[x];

        SkipNextInstruction(virtualMachine);
    }

    private static void OpFX18(ushort opcode, VirtualMachine virtualMachine, ushort x)
    {
        Debug.WriteLine($"0x{opcode:X} -> FX18: LD memory.ST, Vx - Set sound timer = Vx");
        virtualMachine.ST = virtualMachine.Registers[x];

        SkipNextInstruction(virtualMachine);
    }

    private static void OpFX1E(ushort opcode, VirtualMachine virtualMachine, ushort x)
    {
        Debug.WriteLine($"0x{opcode:X} -> FX1E: Add I, Vx - Set I = I + Vx");
        virtualMachine.I += virtualMachine.Registers[x];

        SkipNextInstruction(virtualMachine);
    }

    private static void OpFX29(ushort opcode, VirtualMachine virtualMachine, ushort x)
    {
        Debug.WriteLine(
            $"0x{opcode:X} -> FX29: LD F, Vx - Set I = location of sprite for digit Vx");

        virtualMachine.I = (ushort)(virtualMachine.CharSize * virtualMachine.Registers[x]);

        SkipNextInstruction(virtualMachine);
    }

    private static void OpFX33(ushort opcode, VirtualMachine virtualMachine, ushort x)
    {
        Debug.WriteLine(
            $"0x{opcode:X} -> FX33: LD B, Vx - Store BCD (Binary-Coded Decimal) representation of Vx in memory locations I, I + 1, and I + 2");

        ushort result = virtualMachine.Registers[x];

        for (var offset = 2; offset >= 0; offset--)
        {
            virtualMachine.RAM[virtualMachine.I + offset] = (byte)(result % 10);
            result /= 10;
        }

        SkipNextInstruction(virtualMachine);
    }

    private static void OpFX55(ushort opcode, VirtualMachine virtualMachine, ushort x)
    {
        Debug.WriteLine(
            $"0x{opcode:X} -> FX55: LD [memory.I], Vx - Store V0~Vx in memory starting at location I");

        for (var offset = 0; offset <= x; ++offset)
        {
            virtualMachine.RAM[virtualMachine.I + offset] = virtualMachine.Registers[offset];
        }

        SkipNextInstruction(virtualMachine);
    }

    private static void OpFX65(ushort opcode, VirtualMachine virtualMachine, ushort x)
    {
        Debug.WriteLine(
            $"0x{opcode:X} -> FX65: LD Vx, [memory.I] - Read registers V0~Vx from memory starting at location I");

        for (var offset = 0; offset <= x; ++offset)
        {
            virtualMachine.Registers[offset] = virtualMachine.RAM[virtualMachine.I + offset];
        }

        SkipNextInstruction(virtualMachine);
    }
}
