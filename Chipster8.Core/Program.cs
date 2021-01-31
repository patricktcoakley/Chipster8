namespace Chipster8.Core
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using var game = new Core.Chipster8Game();
            game.Run();
        }
    }
}