using System;

namespace Chip8;

public enum Chip8State
{
    Running,
    Paused,
    Finished,
    Off
}

public class Chip8
{
    private Chip8(Spec spec)
    {
        ArgumentNullException.ThrowIfNull(spec, nameof(spec));
        VirtualMachine = new VirtualMachine(spec);
    }

    public Chip8() : this(Spec.CosmacVIP())
    {
    }

    public ushort VideoHeight => VirtualMachine.VideoHeight;
    public ushort VideoWidth => VirtualMachine.VideoWidth;

    public VirtualMachine VirtualMachine { get; init; }
    public Chip8State State { get; set; } = Chip8State.Off;
    public bool ShouldPlaySound { get; set; }

    public void Pause() =>
        State = State switch
        {
            Chip8State.Paused => Chip8State.Running,
            Chip8State.Running => Chip8State.Paused,
            _ => State
        };

    public void Run(byte[] rom)
    {
        VirtualMachine.LoadRom(rom);
        State = Chip8State.Running;
    }

    public void Step()
    {
        if (VirtualMachine.PC >= VirtualMachine.ProgramStartAddress + VirtualMachine.ProgramSize)
        {
            State = Chip8State.Finished;
            return;
        }

        var opcode = VirtualMachine.Opcode;
        Instruction.Execute(opcode, VirtualMachine);

        if (VirtualMachine.DT > 0)
        {
            --VirtualMachine.DT;
        }

        if (VirtualMachine.ST > 0)
        {
            ShouldPlaySound = true;
            --VirtualMachine.ST;
        }
        else
        {
            ShouldPlaySound = false;
        }
    }
}
