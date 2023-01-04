module Chip8.FSharp.Chip8

open System

type Word = byte
let inline Word n : Word = byte n
type DWord = uint16
let inline DWord n : DWord = uint16 n
type Instruction = DWord

type State =
    | Running
    | Paused
    | Off

type Registers = Word [] // 16
type Memory = Word [] // 4096
type Stack = DWord [] // 16
type Keypad = bool [] // 16
type Video = Word [] // 2048

[<Struct>]
type Machine =
    { Index: DWord
      ProgramCounter: DWord
      StackPointer: int
      DelayTimer: Word
      SoundTimer: Word
      Registers: Registers
      Memory: Memory
      Keypad: Keypad
      Stack: Stack
      Video: Video
      State: State
      ShouldDraw: bool }

let defaultMachine =
    { Index = DWord 0
      ProgramCounter = DWord 0
      StackPointer = 0
      DelayTimer = Word 0
      SoundTimer = Word 0
      Registers = Array.zeroCreate<Word> (16)
      Memory = Array.zeroCreate<Word> (4096)
      Keypad = Array.zeroCreate<bool> (16)
      Stack = Array.zeroCreate<DWord> (16)
      Video = Array.zeroCreate<Word> (2048)
      State = Off
      ShouldDraw = false }

let videoHeight: int = 32
let videoWidth: int = 64
let programStartAddress: DWord = 0x200us
let charSize: Word = 0x5uy

let toNibbles (instr: Instruction) : Word * Word * Word * Word =
    ((Word(instr &&& 0xF000us) >>> 12),
     (Word(instr &&& 0x0F00us) >>> 8),
     (Word(instr &&& 0x00F0us) >>> 4),
     (Word instr &&& 0x000Fuy))

let toN (instr: Instruction) : Word = Word(instr &&& 0x000Fus)
let toNN (instr: Instruction) : Word = Word(instr &&& 0x00FFus)
let toNNN (instr: Instruction) : DWord = DWord(instr &&& 0x0FFFus)
let toVx (instr: Instruction) : int = int ((instr &&& 0x0F00us) >>> 8)
let toVy (instr: Instruction) : int = int ((instr &&& 0x00F0us) >>> 8)

// let fonts: byte[] = [|
//             0xF0uy; 0x90uy; 0x90uy; 0x90uy; 0xF0uy; // 0
//             0x20uy; 0x60uy; 0x20uy; 0x20uy; 0x70uy; // 1
//             0xF0uy; 0x10uy; 0xF0uy; 0x80uy; 0xF0uy; // 2
//             0xF0uy; 0x10uy; 0xF0uy; 0x10uy; 0xF0uy; // 3
//             0x90uy; 0x90uy; 0xF0uy; 0x10uy; 0x10uy; // 4
//             0xF0uy; 0x80uy; 0xF0uy; 0x10uy; 0xF0uy; // 5
//             0xF0uy; 0x80uy; 0xF0uy; 0x90uy; 0xF0uy; // 6
//             0xF0uy; 0x10uy; 0x20uy; 0x40uy; 0x40uy; // 7
//             0xF0uy; 0x90uy; 0xF0uy; 0x90uy; 0xF0uy; // 8
//             0xF0uy; 0x90uy; 0xF0uy; 0x10uy; 0xF0uy; // 9
//             0xF0uy; 0x90uy; 0xF0uy; 0x90uy; 0x90uy; // A
//             0xE0uy; 0x90uy; 0xE0uy; 0x90uy; 0xE0uy; // B
//             0xF0uy; 0x80uy; 0x80uy; 0x80uy; 0xF0uy; // C
//             0xE0uy; 0x90uy; 0x90uy; 0x90uy; 0xE0uy; // D
//             0xF0uy; 0x80uy; 0xF0uy; 0x80uy; 0xF0uy; // E
//             0xF0uy; 0x80uy; 0xF0uy; 0x80uy; 0x80uy // F
// |]

//00E0 - CLS
let clearScreen (machine: Machine) : Machine =
    { machine with Video = Array.zeroCreate 2048 }

//00EE - RET
let returnFromSubroutine (machine: Machine) : Machine =
    { machine with
        ProgramCounter = DWord machine.Stack[machine.StackPointer]
        StackPointer = machine.StackPointer - 1 }

//1nnn - JP addr
let jump (nnn: DWord, machine: Machine) = { machine with ProgramCounter = nnn }

//2nnn - CALL nnn
let call (nnn: DWord, machine: Machine) =
    { machine with
        StackPointer = machine.StackPointer + 1
        Stack =
            machine.Stack
            |> Array.updateAt machine.StackPointer machine.ProgramCounter
        ProgramCounter = nnn }

//3xkk - SE Vx, byte
let skipEqualValue (vx: int, nn: Word, machine: Machine) : Machine =
    if machine.Registers[vx] = nn then
        { machine with ProgramCounter = machine.ProgramCounter + 2us }
    else
        machine

//4xkk - SNE Vx, byte
let skipNotEqualValue (vx: int, nn: Word, machine: Machine) : Machine =
    if machine.Registers[vx] <> nn then
        { machine with ProgramCounter = machine.ProgramCounter + 2us }
    else
        machine

//5xy0 - SE Vx, Vy
let skipEqualRegister (vx: int, vy: int, machine: Machine) : Machine =
    if machine.Registers[vx] = machine.Registers[vy] then
        { machine with ProgramCounter = machine.ProgramCounter + 2us }
    else
        machine

//6xkk - LD Vx, byte
let loadValue (vx: int, nn: Word, machine: Machine) : Machine =
    { defaultMachine with Registers = machine.Registers |> Array.updateAt vx nn }

//7xkk - ADD Vx, byte
let addValue (vx: int, nn: Word, machine: Machine) : Machine =
    loadValue (vx, nn + machine.Registers[vx], machine)

//8xy0 - LD Vx, Vy
let loadRegister (vx: int, vy: int, machine: Machine) : Machine =
    loadValue (vx, machine.Registers[vy], machine)

//8xy1 - OR Vx, Vy
let orRegister (vx: int, vy: int, machine: Machine) : Machine =
    loadValue (vx, machine.Registers[vx] ||| machine.Registers[vy], machine)

//8xy2 - AND Vx, Vy
let andRegister (vx: int, vy: int, machine: Machine) : Machine =
    loadValue (vx, machine.Registers[vx] &&& machine.Registers[vy], machine)

//8xy3 - XOR Vx, Vy
let xorRegister (vx: int, vy: int, machine: Machine) : Machine =
    loadValue (vx, machine.Registers[vx] ^^^ machine.Registers[vy], machine)

//8xy4 - ADD Vx, Vy
let addRegister (vx: int, vy: int, machine: Machine) : Machine =
    let result =
        DWord machine.Registers[vx]
        + DWord machine.Registers[vy]

    let updatedMachine =
        loadValue (vx, (Word result &&& 0xFFuy), machine)

    let updatedVf =
        if result > 0xFFus then 1uy else 0uy

    { updatedMachine with
        Registers =
            updatedMachine.Registers
            |> Array.updateAt 15 updatedVf }

//8xy5 - SUB Vx, Vy
let subRegister (vx: int, vy: int, machine: Machine) : Machine =
    let result =
        machine.Registers[vx] - machine.Registers[vy]

    let updatedVf =
        if machine.Registers[vx] > machine.Registers[vy] then
            1uy
        else
            0uy

    let updatedMachine =
        loadValue (vx, result, machine)

    { updatedMachine with
        Registers =
            updatedMachine.Registers
            |> Array.updateAt 15 updatedVf }

//8xy6 - SHR Vx {, Vy}
let shiftRegisterRight (vx: int, machine: Machine): Machine =
    { machine with
        Registers =
            machine.Registers
            |> Array.updateAt vx (machine.Registers[vx] >>> 1)
            |> Array.updateAt 15 (machine.Registers[vx] &&& 1uy) }

//8xy7 - SUBN Vx, Vy
let subRegister'(vx: int, vy: int, machine: Machine): Machine =
    let result = machine.Registers[vx] - machine.Registers[vy]
    let updatedVf = if result < machine.Registers[vy] then 1uy else 0uy
    { machine with Registers = machine.Registers |> Array.updateAt vx result |> Array.updateAt 15 updatedVf }

//8xyE - SHL Vx {, Vy}
let shiftRegisterLeft(vx: int, machine: Machine): Machine =
    let updatedVf = (machine.Registers[vx] &&& 0x80uy) >>> 7
    {machine with Registers = machine.Registers |> Array.updateAt vx (machine.Registers[vx] <<< 1) |> Array.updateAt 15 updatedVf}

//9xy0 - SNE Vx, Vy
let skipRegisterNotEqual(vx: int, vy: int, machine: Machine): Machine =
    if machine.Registers[vx] <> machine.Registers[vy] then
        { machine with ProgramCounter = machine.ProgramCounter + 2us }
    else
        machine
        
//Annn - LD I, addr
let loadIndex (nnn: DWord, machine: Machine): Machine =
    { machine with Index = nnn }

//Bnnn - JP V0, addr
let loadProgramCounter (nnn: DWord, machine: Machine): Machine =
    { machine with ProgramCounter = nnn }

//Cxkk - RND Vx, byte
let loadRandomValue(vx: int, nn: Word,  randomInt: int, machine: Machine): Machine =
    let value: Word = Word randomInt &&& nn
    { machine with Registers = machine.Registers |> Array.updateAt vx value }

//Dxyn - DRW Vx, Vy, nibble
let draw(vx: int, vy: int, machine: Machine): Machine =
    let mutable originalVideo = machine.Video
    let mutable vf = Word 0
    for i in 0..8 do
        let y = int (machine.Registers[vy] + Word i) % videoHeight
        for j in 0..8 do
            let x: int = int (machine.Registers[vx] + Word j) % videoWidth
            let spritePos: int = y * videoWidth + x
            vf <- 0uy
            originalVideo[spritePos] <- originalVideo[spritePos] ^^^ 1uy
    { machine with Video = originalVideo; Registers = machine.Registers |> Array.updateAt 15 vf; ShouldDraw = true }
            
//Ex9E - SKP Vx
let skipIfKeypadPressed(vx: int, machine: Machine): Machine =
    if machine.Keypad[int machine.Registers[vx]] then
        { machine with ProgramCounter = machine.ProgramCounter + 2us }
    else
        machine

//ExA1 - SKNP Vx
let skipIfKeypadNotPressed(vx: int, machine: Machine): Machine =
    if not machine.Keypad[int machine.Registers[vx]] then
        { machine with ProgramCounter = machine.ProgramCounter + 2us }
    else
        machine

//Fx07 - LD Vx, DT
let loadDelayTimerRegister(vx: int, machine: Machine): Machine =
    {machine with Registers = machine.Registers |> Array.updateAt vx machine.DelayTimer}

//Fx0A - LD Vx, K
let loadKeypad (vx: int, machine: Machine): Machine =
    let active = machine.Keypad |> Array.indexed |> Array.filter (fun (i,v) -> v = true)
    if active.Length > 0 then
        let (i, _) = active[0]
        { machine with Registers = machine.Registers |> Array.updateAt vx (Word i) }
    else   
        {machine with ProgramCounter = machine.ProgramCounter + 2us}
        
//Fx15 - LD DT, Vx
let loadRegisterDelayTimer(vx: int, machine: Machine): Machine =
    { machine with DelayTimer = machine.Registers[vx] }

//Fx18 - LD ST, Vx
let loadRegisterSoundTimer(vx: int, machine: Machine): Machine =
    { machine with SoundTimer = machine.Registers[vx] }
    
//Fx1E - ADD I, Vx
let addIndexRegister (vx: int, machine: Machine): Machine =
    { machine with Index = DWord machine.Registers[vx] + machine.Index }
   
//Fx29 - LD F, Vx 
let loadSpriteLocation (vx: int, machine: Machine): Machine =
    { machine with Index = DWord machine.Registers[vx] * DWord charSize }
    
//Fx33 - LD B, Vx
let loadBCD (vx: int, machine: Machine): Machine =
    let result: Word = machine.Registers[vx]
    let result2: Word = result % 10uy
    let result3: Word = result2 % 10uy
    let result4: Word = result3 % 10uy
    let index = int machine.Index
    let updatedMemory = machine.Memory |> Array.updateAt index+2 result2
                                       |> Array.updateAt index+1 result3
                                       |> Array.updateAt index result4
    { machine with Memory = updatedMemory }

//Fx55 - LD [I], Vx
let loadMemoryIndex (vx: int, machine: Machine): Machine =
  { machine with Memory = [| for i in int machine.Index .. vx do  machine.Registers[int i]  |] }

//Fx65 - LD Vx, [I]
let loadRegistersMemory (vx: int, machine: Machine): Machine =
  { machine with Registers = [| for i in int machine.Index .. vx do  machine.Memory[int (i + int machine.Index)]  |] }

let decode instr machine =
    match toNibbles instr with
    | 0x0uy, 0x0uy, 0xeuy, 0x0uy -> clearScreen machine
    | 0x0uy, 0x0uy, 0xeuy, 0xeuy -> machine
    | _ -> machine
