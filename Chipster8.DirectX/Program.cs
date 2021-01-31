using System;
using Chipster8.Core;

namespace Chipster8.DirectX
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using var game = new Chipster8Game();
            game.Run();
        }
    }
}