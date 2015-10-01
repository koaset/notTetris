using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotTetris
{
    class Results
    {
        public bool IsSinglerplayer
        {
            get { return isSinglerplayer; }
            set { isSinglerplayer = value; }
        }

        public bool Player1Won
        {
            get { return p1Won; }
            set { p1Won = value; }
        }

        public float Player1Score
        {
            get { return p1Score; }
            set { p1Score = value; }
        }

        public float Player2Score
        {
            get { return p2Score; }
            set { p2Score = value; }
        }

        public TimeSpan Time
        {
            get { return time; }
            set { time = value; }
        }

        public string Difficulty
        {
            get { return difficulty; }
            set { difficulty = value; }
        }

        private bool isSinglerplayer;
        private bool p1Won;
        private float p1Score;
        private float p2Score;
        private TimeSpan time;
        private string difficulty;

        public Results()
        {
            isSinglerplayer = true;
            p1Won = true;
            p1Score = 0;
            p2Score = 0;
            time = new TimeSpan(0, 0, 0);
            difficulty = "Easy";
        }
    }
}
