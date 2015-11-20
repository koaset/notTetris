using System;
using System.Collections.Generic;
using System.Text;
using NotTetris.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NotTetris.GameObjects
{
    /// <summary>
    /// Displays score during play
    /// </summary>
    class ScoreCounter : NotTetris.Graphics.IDrawable
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
            get { return text.Position; }
            set { text.Position = value; }
        }
        #endregion

        Text text;
        private float score;

        public ScoreCounter()
        {
            text = new Text();
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
            text.Position = Vector2.Zero;
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
