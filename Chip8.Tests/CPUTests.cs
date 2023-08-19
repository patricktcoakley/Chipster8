namespace Chip8.Tests;

public class CPUTests
{
    private readonly Memory _memory = new();

    [Fact]
    public void Test_00E0()
    {
        Array.Fill(_memory.Video, (byte)1);

        CPU.Execute(0x00E0, _memory);

        Assert.All(_memory.Video, x => Assert.Equal(0, x));
    }

    [Fact]
    public void Test_00EE()
    {
        _memory.SP = 0xF;
        _memory.Stack[_memory.SP] = 0x5;
        var expectedSP = _memory.SP - 1;

        CPU.Execute(0x00EE, _memory);

        Assert.Equal(expectedSP, _memory.SP);
        Assert.Equal(_memory.PC, _memory.Stack[_memory.SP]);
    }

    [Fact]
    public void Test_1NNN()
    {
        CPU.Execute(0x1222, _memory);

        Assert.Equal(0x222, _memory.PC);
    }

    [Fact]
    public void Test_2NNN()
    {
        _memory.SP = 0xE;

        CPU.Execute(0x2111, _memory);

        Assert.Equal(0xF, _memory.SP);
        Assert.Equal(Memory.ProgramStartAddress, _memory.Stack[_memory.SP - 1]);
        Assert.Equal(0x111, _memory.PC);
    }

    [Fact]
    public void Test_3XKK()
    {
        const byte x = 0xE;
        const byte nn = 0xFF;
        var startingPC = _memory.PC;
        _memory.Registers[x] = nn;

        CPU.Execute(0x3EFF, _memory);

        Assert.Equal(nn, _memory.Registers[x]);
        Assert.Equal(startingPC + 2, _memory.PC);
    }

    [Fact]
    public void Test_4XKK()
    {
        const byte x = 0xE;
        const byte nn = 0xFF;
        var startingPC = _memory.PC;
        _memory.Registers[x] = nn - 1;

        CPU.Execute(0x4EFF, _memory);

        Assert.NotEqual(nn, _memory.Registers[x]);
        Assert.Equal(startingPC + 2, _memory.PC);
    }

    [Fact]
    public void Test_5XY0()
    {
        const int x = 0xD;
        const int y = 0xE;
        const byte nn = 0xFF;
        var startingPC = _memory.PC;
        _memory.Registers[x] = nn;
        _memory.Registers[y] = nn;

        CPU.Execute(0x5DE0, _memory);

        Assert.Equal(_memory.Registers[x], _memory.Registers[y]);
        Assert.Equal(startingPC + 2, _memory.PC);
    }

    [Fact]
    public void Test_6XKK()
    {
        const int x = 0xD;
        const byte nn = 0xFF;
        _memory.Registers[x] = nn;

        CPU.Execute(0x6DFF, _memory);

        Assert.Equal(nn, _memory.Registers[x]);
    }

    [Fact]
    public void Test_7XKK()
    {
        const int x = 0xD;
        const byte nn = 0xA;
        _memory.Registers[x] = 0xE2;
        var expected = (byte)(_memory.Registers[x] + nn);

        CPU.Execute(0x7D0A, _memory);

        Assert.Equal(expected, _memory.Registers[x]);
    }

    [Fact]
    public void Test_8XY0()
    {
        const int x = 0xD;
        const int y = 0xE;
        _memory.Registers[y] = 0x1;

        CPU.Execute(0x8DE0, _memory);

        Assert.Equal(_memory.Registers[x], _memory.Registers[y]);
    }

    [Fact]
    public void Test_8XY1()
    {
        const int x = 0xD;
        const int y = 0xE;
        _memory.Registers[x] = 0x8;
        _memory.Registers[y] = 0xA;
        var expected = _memory.Registers[x] | _memory.Registers[y];

        CPU.Execute(0x8DE1, _memory);

        Assert.Equal(expected, _memory.Registers[x]);
    }

    [Fact]
    public void Test_8XY2()
    {
        const int x = 0xD;
        const int y = 0xE;
        _memory.Registers[x] = 0x8;
        _memory.Registers[y] = 0xA;
        var expected = _memory.Registers[x] & _memory.Registers[y];

        CPU.Execute(0x8DE2, _memory);

        Assert.Equal(expected, _memory.Registers[x]);
    }

    [Fact]
    public void Test_8XY3()
    {
        const int x = 0xD;
        const int y = 0xE;
        _memory.Registers[x] = 0x8;
        _memory.Registers[y] = 0xA;
        var expected = _memory.Registers[x] ^ _memory.Registers[y];

        CPU.Execute(0x8DE3, _memory);

        Assert.Equal(expected, _memory.Registers[x]);
    }

    [Fact]
    public void Test_8XY4()
    {
        const int x = 0xD;
        const int y = 0xE;
        _memory.Registers[x] = 0xF;
        _memory.Registers[y] = 0xF;
        var expectedResult = _memory.Registers[x] + _memory.Registers[y];
        var expectedVF = 0;

        CPU.Execute(0x8DE4, _memory);

        Assert.Equal(expectedResult, _memory.Registers[x]);
        Assert.Equal(expectedVF, _memory.VF);
    }

    [Fact]
    public void Test_8XY5()
    {
        const int x = 0xD;
        const int y = 0xE;
        _memory.Registers[x] = 0xF;
        _memory.Registers[y] = 0xA;
        var expectedResult = _memory.Registers[x] - _memory.Registers[y];
        var expectedVF = 1;

        CPU.Execute(0x8DE5, _memory);

        Assert.Equal(expectedResult, _memory.Registers[x]);
        Assert.Equal(expectedVF, _memory.VF);
    }

    [Fact]
    public void Test_8XY6()
    {
        const int x = 0xD;
        _memory.Registers[x] = 0xF;
        var expectedResult = _memory.Registers[x] >> 1;
        var expectedVF = _memory.Registers[x] & 1;

        CPU.Execute(0x8DE6, _memory);

        Assert.Equal(expectedResult, _memory.Registers[x]);
        Assert.Equal(expectedVF, _memory.VF);
    }

    [Fact]
    public void Test_8XY7()
    {
        const int x = 0xD;
        const int y = 0xE;
        _memory.Registers[x] = 0xA;
        _memory.Registers[y] = 0xF;
        var expectedResult = _memory.Registers[y] - _memory.Registers[x];
        var expectedVF = 1;

        CPU.Execute(0x8DE7, _memory);

        Assert.Equal(expectedResult, _memory.Registers[x]);
        Assert.Equal(expectedVF, _memory.VF);
    }

    [Fact]
    public void Test_8XYE()
    {
        const int x = 0xD;
        _memory.Registers[x] = 0xF;
        var expectedResult = _memory.Registers[x] << 1;
        var expectedVF = (byte)((_memory.Registers[x] & 0x80) >> 7);

        CPU.Execute(0x8DEE, _memory);

        Assert.Equal(expectedResult, _memory.Registers[x]);
        Assert.Equal(expectedVF, _memory.VF);
    }

    [Fact]
    public void Test_9XY0()
    {
        const int x = 0xD;
        const int y = 0xE;
        var startingPC = _memory.PC;
        _memory.Registers[x] = 0xA;
        _memory.Registers[y] = 0xB;

        CPU.Execute(0x9DE0, _memory);

        Assert.NotEqual(_memory.Registers[x], _memory.Registers[y]);
        Assert.Equal(startingPC + 2, _memory.PC);
    }

    [Fact]
    public void Test_ANNN()
    {
        const ushort nnn = 0x100;

        CPU.Execute(0xA100, _memory);

        Assert.Equal(nnn, _memory.I);
    }

    [Fact]
    public void Test_BNNN()
    {
        const ushort nnn = 0xE;

        CPU.Execute(0xB00E, _memory);

        Assert.Equal(_memory.PC, _memory.Registers[nnn]);
    }

    [Fact]
    public void Test_CXKK()
    {
        const byte x = 0xD;
        const byte nn = 0xE;
        _memory.Registers[x] = nn;

        CPU.Execute(0xCD0E, _memory);

        Assert.NotEqual(nn, _memory.Registers[x]);
    }

    [Fact]
    public void Test_DXYN()
    {
        // TODO
        Assert.True(true);
    }

    [Fact]
    public void Test_EX9E()
    {
        const byte x = 0xD;
        _memory.Registers[x] = 0x1;
        _memory.Keypad[_memory.Registers[x]] = true;
        var startingPC = _memory.PC;

        CPU.Execute(0xED9E, _memory);

        Assert.True(_memory.Keypad[_memory.Registers[x]]);
        Assert.Equal(startingPC + 2, _memory.PC);
    }

    [Fact]
    public void Test_EXA1()
    {
        const byte x = 0xD;
        _memory.Registers[x] = 0x1;
        _memory.Keypad[_memory.Registers[x]] = false;
        var startingPC = _memory.PC;

        CPU.Execute(0xEDA1, _memory);

        Assert.False(_memory.Keypad[_memory.Registers[x]]);
        Assert.Equal(startingPC + 2, _memory.PC);
    }

    [Fact]
    public void Test_FX07()
    {
        const byte x = 0xD;
        _memory.Registers[x] = 0x1;
        _memory.DT = 0xF;

        CPU.Execute(0xFD07, _memory);

        Assert.Equal(_memory.Registers[x], _memory.DT);
    }

    [Fact]
    public void Test_FX0A()
    {
        const byte x = 0xD;
        const byte activeKeypad = 0x5;
        _memory.Keypad[activeKeypad] = true;
        var startingPC = _memory.PC;

        CPU.Execute(0xFD0A, _memory);

        Assert.Equal(activeKeypad, _memory.Registers[x]);
        Assert.Equal(startingPC, _memory.PC);
    }

    [Fact]
    public void Test_FX15()
    {
        const byte x = 0xD;
        _memory.Registers[x] = 0x11;
        _memory.DT = 0x12;

        CPU.Execute(0xFD15, _memory);

        Assert.Equal(_memory.DT, _memory.Registers[x]);
    }

    [Fact]
    public void Test_FX18()
    {
        const byte x = 0xD;
        _memory.Registers[x] = 0x11;
        _memory.ST = 0x12;

        CPU.Execute(0xFD18, _memory);

        Assert.Equal(_memory.ST, _memory.Registers[x]);
    }

    [Fact]
    public void Test_FX1E()
    {
        const byte x = 0xD;
        _memory.Registers[x] = 0x11;
        _memory.I = 0x12;
        var expected = _memory.Registers[x] + _memory.I;

        CPU.Execute(0xFD1E, _memory);

        Assert.Equal(expected, _memory.I);
    }

    [Fact]
    public void Test_FX29()
    {
        const byte x = 0xD;
        _memory.Registers[x] = 0x11;
        _memory.I = 0x12;
        var expected = _memory.Registers[x] * Memory.CharSize;

        CPU.Execute(0xFD29, _memory);

        Assert.Equal(expected, _memory.I);
    }

    [Fact]
    public void Test_FX33()
    {
        const byte x = 0xD;
        _memory.Registers[x] = 0xFF;
        _memory.I = 0xFF;
        var result = _memory.Registers[x];
        var expected1 = result % 10;
        result /= 10;
        var expected2 = result % 10;
        result /= 10;
        var expected3 = result % 10;

        CPU.Execute(0xFD33, _memory);

        Assert.Equal(expected1, _memory.RAM[_memory.I + 2]);
        Assert.Equal(expected2, _memory.RAM[_memory.I + 1]);
        Assert.Equal(expected3, _memory.RAM[_memory.I]);
    }

    [Fact]
    public void Test_FX55()
    {
        const byte x = 0x2;
        const byte expected = 0xEF;
        for (var offset = 0; offset <= x; ++offset)
        {
            _memory.Registers[offset] = expected;
        }

        CPU.Execute(0xFD55, _memory);

        Assert.Equal(expected, _memory.RAM[_memory.I]);
        Assert.Equal(expected, _memory.RAM[_memory.I + 1]);
        Assert.Equal(expected, _memory.RAM[_memory.I + 2]);
    }

    [Fact]
    public void Test_FX65()
    {
        const byte x = 0x2;
        const byte expected = 0xEF;
        for (var offset = 0; offset <= x; ++offset)
        {
            _memory.RAM[_memory.I + offset] = expected;
        }

        CPU.Execute(0xFD65, _memory);

        Assert.Equal(expected, _memory.Registers[0]);
        Assert.Equal(expected, _memory.Registers[1]);
        Assert.Equal(expected, _memory.Registers[2]);
    }
}
