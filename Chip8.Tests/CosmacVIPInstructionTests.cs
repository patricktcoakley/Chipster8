namespace Chip8.Tests;

public class CosmacVIPInstructionTests
{
    private readonly VirtualMachine _virtualMachine = new(Spec.CosmacVIP());

    [Fact]
    public void Test_00E0()
    {
        Array.Fill(_virtualMachine.Video, (byte)1);

        Instruction.Execute(0x00E0, _virtualMachine);

        Assert.All(_virtualMachine.Video, x => Assert.Equal(0, x));
    }

    [Fact]
    public void Test_00EE()
    {
        _virtualMachine.SP = 0xE;
        _virtualMachine.Stack[_virtualMachine.SP] = 0x5;
        var expectedSP = _virtualMachine.SP - 1;

        Instruction.Execute(0x00EE, _virtualMachine);

        Assert.Equal(expectedSP, _virtualMachine.SP);
        Assert.Equal(_virtualMachine.PC - 2, _virtualMachine.Stack[_virtualMachine.SP]);
    }

    [Fact]
    public void Test_1NNN()
    {
        Instruction.Execute(0x1222, _virtualMachine);

        Assert.Equal(0x222, _virtualMachine.PC);
    }

    [Fact]
    public void Test_2NNN()
    {
        _virtualMachine.SP = 0xE;

        Instruction.Execute(0x2111, _virtualMachine);

        Assert.Equal(0xF, _virtualMachine.SP);
        Assert.Equal(VirtualMachine.ProgramStartAddress, _virtualMachine.Stack[_virtualMachine.SP - 1]);
        Assert.Equal(0x111, _virtualMachine.PC);
    }

    [Fact]
    public void Test_3XKK()
    {
        const byte x = 0xE;
        const byte nn = 0xFF;
        var startingPC = _virtualMachine.PC;
        _virtualMachine.Registers[x] = nn;

        Instruction.Execute(0x3EFF, _virtualMachine);

        Assert.Equal(nn, _virtualMachine.Registers[x]);
        Assert.Equal(startingPC + 4, _virtualMachine.PC);
    }

    [Fact]
    public void Test_4XKK()
    {
        const byte x = 0xE;
        const byte nn = 0xFF;
        var startingPC = _virtualMachine.PC;
        _virtualMachine.Registers[x] = nn - 1;

        Instruction.Execute(0x4EFF, _virtualMachine);

        Assert.NotEqual(nn, _virtualMachine.Registers[x]);
        Assert.Equal(startingPC + 4, _virtualMachine.PC);
    }

    [Fact]
    public void Test_5XY0()
    {
        const int x = 0xD;
        const int y = 0xE;
        const byte nn = 0xFF;
        var startingPC = _virtualMachine.PC;
        _virtualMachine.Registers[x] = nn;
        _virtualMachine.Registers[y] = nn;

        Instruction.Execute(0x5DE0, _virtualMachine);

        Assert.Equal(_virtualMachine.Registers[x], _virtualMachine.Registers[y]);
        Assert.Equal(startingPC + 4, _virtualMachine.PC);
    }

    [Fact]
    public void Test_6XKK()
    {
        const int x = 0xD;
        const byte nn = 0xFF;
        _virtualMachine.Registers[x] = nn;

        Instruction.Execute(0x6DFF, _virtualMachine);

        Assert.Equal(nn, _virtualMachine.Registers[x]);
    }

    [Fact]
    public void Test_7XKK()
    {
        const int x = 0xD;
        const byte nn = 0xA;
        _virtualMachine.Registers[x] = 0xE2;
        var expected = (byte)(_virtualMachine.Registers[x] + nn);

        Instruction.Execute(0x7D0A, _virtualMachine);

        Assert.Equal(expected, _virtualMachine.Registers[x]);
    }

    [Fact]
    public void Test_8XY0()
    {
        const int x = 0xD;
        const int y = 0xE;
        _virtualMachine.Registers[y] = 0x1;

        Instruction.Execute(0x8DE0, _virtualMachine);

        Assert.Equal(_virtualMachine.Registers[x], _virtualMachine.Registers[y]);
    }

    [Fact]
    public void Test_8XY1()
    {
        const int x = 0xD;
        const int y = 0xE;
        _virtualMachine.Registers[x] = 0x8;
        _virtualMachine.Registers[y] = 0xA;
        var expected = _virtualMachine.Registers[x] | _virtualMachine.Registers[y];

        Instruction.Execute(0x8DE1, _virtualMachine);

        Assert.Equal(expected, _virtualMachine.Registers[x]);
    }

    [Fact]
    public void Test_8XY2()
    {
        const int x = 0xD;
        const int y = 0xE;
        _virtualMachine.Registers[x] = 0x8;
        _virtualMachine.Registers[y] = 0xA;
        var expected = _virtualMachine.Registers[x] & _virtualMachine.Registers[y];

        Instruction.Execute(0x8DE2, _virtualMachine);

        Assert.Equal(expected, _virtualMachine.Registers[x]);
    }

    [Fact]
    public void Test_8XY3()
    {
        const int x = 0xD;
        const int y = 0xE;
        _virtualMachine.Registers[x] = 0x8;
        _virtualMachine.Registers[y] = 0xA;
        var expected = _virtualMachine.Registers[x] ^ _virtualMachine.Registers[y];

        Instruction.Execute(0x8DE3, _virtualMachine);

        Assert.Equal(expected, _virtualMachine.Registers[x]);
    }

    [Fact]
    public void Test_8XY4()
    {
        const int x = 0xD;
        const int y = 0xE;
        _virtualMachine.Registers[x] = 0xF;
        _virtualMachine.Registers[y] = 0xF;
        var expectedResult = _virtualMachine.Registers[x] + _virtualMachine.Registers[y];
        var expectedVF = 0;

        Instruction.Execute(0x8DE4, _virtualMachine);

        Assert.Equal(expectedResult, _virtualMachine.Registers[x]);
        Assert.Equal(expectedVF, _virtualMachine.VF);
    }

    [Fact]
    public void Test_8XY5()
    {
        const int x = 0xD;
        const int y = 0xE;
        _virtualMachine.Registers[x] = 0xF;
        _virtualMachine.Registers[y] = 0xA;
        var expectedResult = _virtualMachine.Registers[x] - _virtualMachine.Registers[y];
        var expectedVF = 1;

        Instruction.Execute(0x8DE5, _virtualMachine);

        Assert.Equal(expectedResult, _virtualMachine.Registers[x]);
        Assert.Equal(expectedVF, _virtualMachine.VF);
    }

    [Fact]
    public void Test_8XY6()
    {
        const int x = 0xD;
        _virtualMachine.Registers[x] = 0xF;
        var expectedResult = _virtualMachine.Registers[x] >> 1;
        var expectedVF = _virtualMachine.Registers[x] & 1;

        Instruction.Execute(0x8DE6, _virtualMachine);

        Assert.Equal(expectedResult, _virtualMachine.Registers[x]);
        Assert.Equal(expectedVF, _virtualMachine.VF);
    }

    [Fact]
    public void Test_8XY7()
    {
        const int x = 0xD;
        const int y = 0xE;
        _virtualMachine.Registers[x] = 0xA;
        _virtualMachine.Registers[y] = 0xF;
        var expectedResult = _virtualMachine.Registers[y] - _virtualMachine.Registers[x];
        var expectedVF = 1;

        Instruction.Execute(0x8DE7, _virtualMachine);

        Assert.Equal(expectedResult, _virtualMachine.Registers[x]);
        Assert.Equal(expectedVF, _virtualMachine.VF);
    }

    [Fact]
    public void Test_8XYE()
    {
        const int x = 0xD;
        _virtualMachine.Registers[x] = 0xF;
        var expectedResult = _virtualMachine.Registers[x] << 1;
        var expectedVF = (byte)((_virtualMachine.Registers[x] & 0x80) >> 7);

        Instruction.Execute(0x8DEE, _virtualMachine);

        Assert.Equal(expectedResult, _virtualMachine.Registers[x]);
        Assert.Equal(expectedVF, _virtualMachine.VF);
    }

    [Fact]
    public void Test_9XY0()
    {
        const int x = 0xD;
        const int y = 0xE;
        var startingPC = _virtualMachine.PC;
        _virtualMachine.Registers[x] = 0xA;
        _virtualMachine.Registers[y] = 0xB;

        Instruction.Execute(0x9DE0, _virtualMachine);

        Assert.NotEqual(_virtualMachine.Registers[x], _virtualMachine.Registers[y]);
        Assert.Equal(startingPC + 4, _virtualMachine.PC);
    }

    [Fact]
    public void Test_ANNN()
    {
        const ushort nnn = 0x100;

        Instruction.Execute(0xA100, _virtualMachine);

        Assert.Equal(nnn, _virtualMachine.I);
    }

    [Fact]
    public void Test_BNNN()
    {
        const ushort nnn = 0xE;

        Instruction.Execute(0xB00E, _virtualMachine);

        Assert.Equal(nnn + 2, _virtualMachine.PC);
    }

    [Fact]
    public void Test_CXKK()
    {
        const byte x = 0xD;
        const byte nn = 0xE;
        _virtualMachine.Registers[x] = nn;

        Instruction.Execute(0xCD0E, _virtualMachine);

        Assert.NotEqual(nn, _virtualMachine.Registers[x]);
    }

    [Fact]
    public void Test_OpDXYN()
    {
        _virtualMachine.I = 0x200;
        _virtualMachine.RAM[_virtualMachine.I] = 0x1;
        _virtualMachine.Video[0x7] = 0x1;

        Instruction.Execute(0xD001, _virtualMachine);

        Assert.Equal(0x0, _virtualMachine.Video[0x7]);
        Assert.Equal(0x1, _virtualMachine.VF);
    }

    [Fact]
    public void Test_EX9E()
    {
        const byte x = 0xD;
        _virtualMachine.Registers[x] = 0x1;
        _virtualMachine.Keypad[_virtualMachine.Registers[x]] = true;
        var startingPC = _virtualMachine.PC;

        Instruction.Execute(0xED9E, _virtualMachine);

        Assert.True(_virtualMachine.Keypad[_virtualMachine.Registers[x]]);
        Assert.Equal(startingPC + 4, _virtualMachine.PC);
    }

    [Fact]
    public void Test_EXA1()
    {
        const byte x = 0xD;
        _virtualMachine.Registers[x] = 0x1;
        _virtualMachine.Keypad[_virtualMachine.Registers[x]] = false;
        var startingPC = _virtualMachine.PC;

        Instruction.Execute(0xEDA1, _virtualMachine);

        Assert.False(_virtualMachine.Keypad[_virtualMachine.Registers[x]]);
        Assert.Equal(startingPC + 4, _virtualMachine.PC);
    }

    [Fact]
    public void Test_FX07()
    {
        const byte x = 0xD;
        _virtualMachine.Registers[x] = 0x1;
        _virtualMachine.DT = 0xF;

        Instruction.Execute(0xFD07, _virtualMachine);

        Assert.Equal(_virtualMachine.Registers[x], _virtualMachine.DT);
    }

    [Fact]
    public void Test_FX0A()
    {
        const byte x = 0xD;
        const byte activeKeypad = 0x5;
        _virtualMachine.Keypad[activeKeypad] = true;
        var startingPC = _virtualMachine.PC;

        Instruction.Execute(0xFD0A, _virtualMachine);

        Assert.Equal(activeKeypad, _virtualMachine.Registers[x]);
        Assert.Equal(startingPC + 2, _virtualMachine.PC);
    }

    [Fact]
    public void Test_FX15()
    {
        const byte x = 0xD;
        _virtualMachine.Registers[x] = 0x11;
        _virtualMachine.DT = 0x12;

        Instruction.Execute(0xFD15, _virtualMachine);

        Assert.Equal(_virtualMachine.DT, _virtualMachine.Registers[x]);
    }

    [Fact]
    public void Test_FX18()
    {
        const byte x = 0xD;
        _virtualMachine.Registers[x] = 0x11;
        _virtualMachine.ST = 0x12;

        Instruction.Execute(0xFD18, _virtualMachine);

        Assert.Equal(_virtualMachine.ST, _virtualMachine.Registers[x]);
    }

    [Fact]
    public void Test_FX1E()
    {
        const byte x = 0xD;
        _virtualMachine.Registers[x] = 0x11;
        _virtualMachine.I = 0x12;
        var expected = _virtualMachine.Registers[x] + _virtualMachine.I;

        Instruction.Execute(0xFD1E, _virtualMachine);

        Assert.Equal(expected, _virtualMachine.I);
    }

    [Fact]
    public void Test_FX29()
    {
        const byte x = 0xD;
        _virtualMachine.Registers[x] = 0x11;
        _virtualMachine.I = 0x12;
        var expected = _virtualMachine.Registers[x] * _virtualMachine.CharSize;

        Instruction.Execute(0xFD29, _virtualMachine);

        Assert.Equal(expected, _virtualMachine.I);
    }

    [Fact]
    public void Test_FX33()
    {
        const byte x = 0xD;
        _virtualMachine.Registers[x] = 0xFF;
        _virtualMachine.I = 0xFF;
        var result = _virtualMachine.Registers[x];
        var expected1 = result % 10;
        result /= 10;
        var expected2 = result % 10;
        result /= 10;
        var expected3 = result % 10;

        Instruction.Execute(0xFD33, _virtualMachine);

        Assert.Equal(expected1, _virtualMachine.RAM[_virtualMachine.I + 2]);
        Assert.Equal(expected2, _virtualMachine.RAM[_virtualMachine.I + 1]);
        Assert.Equal(expected3, _virtualMachine.RAM[_virtualMachine.I]);
    }

    [Fact]
    public void Test_FX55()
    {
        const byte x = 0x2;
        const byte expected = 0xEF;
        for (var offset = 0; offset <= x; ++offset)
        {
            _virtualMachine.Registers[offset] = expected;
        }

        Instruction.Execute(0xFD55, _virtualMachine);

        Assert.Equal(expected, _virtualMachine.RAM[_virtualMachine.I]);
        Assert.Equal(expected, _virtualMachine.RAM[_virtualMachine.I + 1]);
        Assert.Equal(expected, _virtualMachine.RAM[_virtualMachine.I + 2]);
    }

    [Fact]
    public void Test_FX65()
    {
        const byte x = 0x2;
        const byte expected = 0xEF;
        for (var offset = 0; offset <= x; ++offset)
        {
            _virtualMachine.RAM[_virtualMachine.I + offset] = expected;
        }

        Instruction.Execute(0xFD65, _virtualMachine);

        Assert.Equal(expected, _virtualMachine.Registers[0]);
        Assert.Equal(expected, _virtualMachine.Registers[1]);
        Assert.Equal(expected, _virtualMachine.Registers[2]);
    }
}
