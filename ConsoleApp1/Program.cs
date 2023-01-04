global::Chip8.Chip8 gChip8;

void LoadSelection()
{
    var path = "/Users/pt/Downloads/test_opcode.ch8";
    using var reader = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
    var bytes = reader.ReadBytes((int) reader.BaseStream.Length);
    gChip8 = new global::Chip8.Chip8();
    gChip8.LoadRom(bytes);
    gChip8.PowerOn();
}


LoadSelection();

while (gChip8.PC < gChip8.ProgramSize - 2)
{
    gChip8.Step();
}


for (var i = 0; i < 32; i++)
{
    for (var j = 0; j < 64; j++)
    {
        var idx = i * 64 + j;
        
        var c = gChip8.Video[idx] == 0 ? ' ' : '█';
        Console.Write($"{c}");
    }
    Console.WriteLine();
}
