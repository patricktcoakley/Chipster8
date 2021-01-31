using System;
using Chipster8.Core;

namespace Chipster8.OpenGL
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