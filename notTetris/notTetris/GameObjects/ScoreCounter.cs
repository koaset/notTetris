using System;
using System.Collections.Generic;
using System.Text;
using NotTetris.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NotTetris.GameObjects
{
    class ScoreCounter
    {
        #region Props
        public float Score
        {
            get { return score; }
            set
            {
                score = value;
                text.TextValue = "Score: " + score.ToString("F0");
            }
        }

        public Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
                text.Position = position;
            }
        }
        #endregion

        Text text;
        private float score;
        private Vector2 position;

        public ScoreCounter(Vector2 position)
        {
            text = new Text();
            this.position = position;
        }

        public void Initialize()
        {
            score = 0;
            text.Initialize();
            text.IsCentered = false;
            text.IsShowing = true;
            text.Font = FontNames.Segoe_UI_Mono;
            text.Layer = 0.5f;
            text.TextColor = Color.MediumSlateBlue;
            text.TextValue = "Score: " + score.ToString("F0");
            text.Position = position;
        }

        public void LoadContent(SpriteBatch spriteBatch)
        {
            text.LoadContent(spriteBatch);
        }

        public void Draw(GameTime gameTime)
        {
            text.Draw(gameTime);
        }
    }
}
