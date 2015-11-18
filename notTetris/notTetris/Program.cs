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
            
            if (args.Length > 0 && args[0] == "-nolog")
            {
                Run();
            }
            else
            {
                try
                {
                    Run();
                }
                catch (Exception e)
                {
                    using (var errorLogger = new ErrorLogger(e))
                    {
                        errorLogger.Run();
                    }
                }
            }
        }

        static void Run()
        {
            using (PuzzleGame game = new PuzzleGame())
            {
                game.Run();
            }
        }
    }
#endif
}

