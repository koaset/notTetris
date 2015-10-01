using System;

namespace NotTetris
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (PuzzleGame game = new PuzzleGame())
            {
                game.Run();
            }
        }
    }
#endif
}

