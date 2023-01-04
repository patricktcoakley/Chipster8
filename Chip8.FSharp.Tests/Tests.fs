module Tests

open Chip8.FSharp
open Chip8.FSharp.Chip8
open Microsoft.FSharp.Collections
open Xunit

[<Fact>]
let ``clearScreen returns a copy of machine state with empty video buffer`` () =
    let machine =
        { defaultMachine with Video = defaultMachine.Video |> Array.updateAt 0 0xFFuy }

    let expected =
        { defaultMachine with Video = Array.zeroCreate<Word> (2048) }

    let actual = clearScreen machine
    Assert.Equal(expected, actual)

[<Fact>]
let ``returnFromSubroutine returns a copy of machine state with updated program counter and decremented stack pointer``
    ()
    =
    let machine =
        { defaultMachine with
            Stack = defaultMachine.Stack |> Array.updateAt 15 8us
            StackPointer = 15 }

    let expected =
        { machine with
            ProgramCounter = 8us
            StackPointer = machine.StackPointer - 1 }

    let actual = returnFromSubroutine machine
    Assert.Equal(expected, actual)

[<Fact>]
let ``jump returns a copy of machine state with program counter point to address`` () =
    let expected =
        { defaultMachine with ProgramCounter = 0xFFus }

    let actual = jump (0xFFus, defaultMachine)
    Assert.Equal(expected, actual)

[<Fact>]
let ``call returns a copy of machine state with program counter incremented and stack at stack pointer updated with program counter``
    ()
    =
    let machine =
        { defaultMachine with
            StackPointer = 10
            ProgramCounter = 0x0Fus }

    let expected =
        { defaultMachine with
            StackPointer = 11
            Stack = defaultMachine.Stack |> Array.updateAt 10 0x0Fus
            ProgramCounter = 0x0Fus }

    let actual = call (0x0Fus, machine)
    Assert.Equal(expected, actual)

[<Fact>]
let ``skipEqualValue returns a copy of machine state with program counter incremented by 2 if register vx  = nn`` () =
    let machine =
        { defaultMachine with
            Registers =
                defaultMachine.Registers
                |> Array.updateAt 1 0x02uy }

    let expected =
        { machine with ProgramCounter = machine.ProgramCounter + 2us }

    let actual =
        skipEqualValue (1, 0x02uy, machine)

    Assert.Equal(expected, actual)

[<Fact>]
let ``skipEqualValue returns original machine state if register vx <> nn`` () =
    let expected = defaultMachine

    let actual =
        skipEqualValue (1, 0x02uy, defaultMachine)

    Assert.Equal(expected, actual)

[<Fact>]
let ``skipNotEqualValue returns a copy of machine state with program counter incremented by 2 if register vx <> nn``
    ()
    =
    let expected =
        { defaultMachine with ProgramCounter = defaultMachine.ProgramCounter + 2us }

    let actual =
        skipNotEqualValue (1, 0x02uy, defaultMachine)

    Assert.Equal(expected, actual)

[<Fact>]
let ``skipNotEqualValue returns original machine state if register vx = nn`` () =
    let machine =
        { defaultMachine with
            Registers =
                defaultMachine.Registers
                |> Array.updateAt 1 0x02uy }

    let expected = machine

    let actual =
        skipNotEqualValue (1, 0x02uy, machine)

    Assert.Equal(expected, actual)

[<Fact>]
let ``skipEqualRegister returns a copy of machine state with program counter incremented by 2 if register vx = register vy``
    ()
    =
    let machine =
        { defaultMachine with
            Registers =
                defaultMachine.Registers
                |> Array.updateAt 1 0x02uy
                |> Array.updateAt 10 0x02uy }

    let expected =
        { machine with ProgramCounter = defaultMachine.ProgramCounter + 2us }

    let actual =
        skipEqualRegister (1, 10, machine)

    Assert.Equal(expected, actual)

[<Fact>]
let ``skipEqualRegister returns original machine state if register vx <> register vy`` () =
    let machine =
        { defaultMachine with
            Registers =
                defaultMachine.Registers
                |> Array.updateAt 1 0x02uy
                |> Array.updateAt 10 0x09uy }

    let expected = machine

    let actual =
        skipEqualRegister (1, 10, machine)

    Assert.Equal(expected, actual)

[<Fact>]
let ``addValue returns a copy of machine state with register vx updated to nn + register vx`` () =
    let vx = 5
    let nn = 0x0Fuy

    let machine =
        { defaultMachine with
            Registers =
                defaultMachine.Registers
                |> Array.updateAt vx 0x0Fuy }

    let expected =
        { machine with
            Registers =
                defaultMachine.Registers
                |> Array.updateAt vx 0x1Euy }

    let actual = addValue (vx, nn, machine)
    Assert.Equal(expected, actual)

[<Fact>]
let ``loadRegister returns a copy of machine state with register vx updated to register vy`` () =
    let vx = 5
    let vy = 12

    let machine =
        { defaultMachine with
            Registers =
                defaultMachine.Registers
                |> Array.updateAt vy 0x0Fuy }

    let expected =
        { machine with Registers = machine.Registers |> Array.updateAt vx 0x0Fuy }

    let actual = loadRegister (vx, vy, machine)
    Assert.Equal(expected, actual)

[<Fact>]
let ``orRegister returns a copy of machine state with register vx updated to register vx OR register vy`` () =
    let vx = 5
    let vy = 12

    let machine =
        { defaultMachine with
            Registers =
                defaultMachine.Registers
                |> Array.updateAt vx 0x01uy
                |> Array.updateAt vy 0x08uy }

    let expected =
        { machine with Registers = machine.Registers |> Array.updateAt vx 0x09uy }

    let actual = orRegister (vx, vy, machine)
    Assert.Equal(expected, actual)

[<Fact>]
let ``andRegister returns a copy of machine state with register vx updated to register vx AND register vy`` () =
    let vx = 5
    let vy = 12

    let machine =
        { defaultMachine with
            Registers =
                defaultMachine.Registers
                |> Array.updateAt vx 0x03uy
                |> Array.updateAt vy 0x07uy }

    let expected =
        { machine with Registers = machine.Registers |> Array.updateAt vx 0x03uy }

    let actual = andRegister (vx, vy, machine)
    Assert.Equal(expected, actual)


[<Fact>]
let ``xorRegister returns a copy of machine state with register vx updated to register vx XOR register vy`` () =
    let vx = 5
    let vy = 12

    let machine =
        { defaultMachine with
            Registers =
                defaultMachine.Registers
                |> Array.updateAt vx 0x01uy
                |> Array.updateAt vy 0x03uy }

    let expected =
        { machine with Registers = machine.Registers |> Array.updateAt vx 0x02uy }

    let actual = xorRegister (vx, vy, machine)
    Assert.Equal(expected, actual)

[<Fact>]
let ``addRegister returns a copy of machine state with register vx updated to register vx + register vy with register vf set to 1 for overflow`` () =
    let vx = 5
    let vy = 12

    let machine =
        { defaultMachine with
            Registers =
                defaultMachine.Registers
                |> Array.updateAt vx 0xFFuy
                |> Array.updateAt vy 0x10uy }

    let expected =
        { machine with Registers = machine.Registers |> Array.updateAt vx 0x0Fuy |> Array.updateAt 15 1uy }

    let actual = addRegister (vx, vy, machine)
    Assert.Equal(expected, actual)

[<Fact>]
let ``addRegister returns a copy of machine state with register vx updated to register vx + register vy with register vf set to 0 for no overflow`` () =
    let vx = 5
    let vy = 12

    let machine =
        { defaultMachine with
            Registers =
                defaultMachine.Registers
                |> Array.updateAt vx 0x04uy
                |> Array.updateAt vy 0x06uy }

    let expected =
        { machine with Registers = machine.Registers |> Array.updateAt vx 0x0Auy |> Array.updateAt 15 0uy }

    let actual = addRegister (vx, vy, machine)
    Assert.Equal(expected, actual)
[<Fact>]
let ``subRegister returns a copy of machine state with register vx updated to register vx + register vy with register vf set to 1 for underflow`` () =
    let vx = 5
    let vy = 12

    let machine =
        { defaultMachine with
            Registers =
                defaultMachine.Registers
                |> Array.updateAt vx 0x20uy
                |> Array.updateAt vy 0x10uy }

    let expected =
        { machine with Registers = machine.Registers |> Array.updateAt vx 0x10uy |> Array.updateAt 15 1uy }

    let actual = subRegister (vx, vy, machine)
    Assert.Equal(expected, actual)

[<Fact>]
let ``subRegister returns a copy of machine state with register vx updated to register vx + register vy with register vf set to 0 for no underflow`` () =
    let vx = 5
    let vy = 12

    let machine =
        { defaultMachine with
            Registers =
                defaultMachine.Registers
                |> Array.updateAt vx 0x04uy
                |> Array.updateAt vy 0x06uy }

    let expected =
        { machine with Registers = machine.Registers |> Array.updateAt vx 0xFEuy |> Array.updateAt 15 0uy }

    let actual = subRegister (vx, vy, machine)
    Assert.Equal(expected, actual)

[<Fact>]
let ``shiftRegisterRight returns a copy of machine state with register vx >> 1 and register vf to register vx & 1`` () =
    let vx = 4
    let machine =
        { defaultMachine with
            Registers =
                defaultMachine.Registers
                |> Array.updateAt vx 0x04uy }
    let expected = { machine with Registers = machine.Registers |> Array.updateAt vx 0x02uy |> Array.updateAt 15 0uy }
    let actual = shiftRegisterRight(vx, machine)
    Assert.Equal(expected, actual)