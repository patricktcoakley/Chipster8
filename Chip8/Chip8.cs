namespace Chip8;

public enum Chip8State
{
    Running,
    Paused,
    Off
}

public class Chip8
{
    public const ushort VideoHeight = 0x20;
    public const ushort VideoWidth = 0x40;
    public Memory Memory { get; } = new();
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
        Memory.LoadRom(rom);
        State = Chip8State.Running;
    }


    public void Step()
    {
        var opcode = Memory.Opcode;
        CPU.SkipNextInstruction(Memory);
        CPU.Execute(opcode, Memory);

        if (Memory.DT > 0)
        {
            --Memory.DT;
        }

        if (Memory.ST > 0)
        {
            ShouldPlaySound = true;
            --Memory.ST;
        }
        else
        {
            ShouldPlaySound = false;
        }
    }
}
