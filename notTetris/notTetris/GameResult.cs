using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotTetris
{
    /// <summary>
    /// Holds info about the outcome of a played game
    /// </summary>
    class GameResult
    {
        public bool IsSingleplayer { get; set; }
        public bool Player1Won { get; set; }
        public float Player1Score { get; set; }
        public float Player2Score { get; set; }
        public TimeSpan Time { get; set; }
        public string Difficulty { get; set; }

        public GameResult()
        {
            IsSingleplayer = true;
            Player1Won = true;
            Player1Score = 0;
            Player2Score = 0;
            Time = new TimeSpan(0, 0, 0);
            Difficulty = "Easy";
        }
    }
}
