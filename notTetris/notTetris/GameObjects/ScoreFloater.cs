using System;
using NotTetris.Graphics;
using Microsoft.Xna.Framework;

namespace NotTetris.GameObjects
{
    /// <summary>
    /// Displays score earned when blocks explode
    /// </summary>
    class ScoreFloater : OutlineText
    {
        public Vector2 Velocity { get; set; }
        public float Interval { get; set; }
        float time;
        Color baseColor;

        public override void Initialize()
        {
            base.Initialize();
            isShowing = false;
            IsCentered = true;
            layer = 0.9f;
            scale = new Vector2(1.5f);
            baseColor = Color.Yellow;
            OutlineColor = Color.Black;
            base.color = baseColor;
            Velocity = new Vector2(0.0f, -25f);
            time = 0.0f;
            Interval = 2.0f;
        }

        public void Start(float score, Vector2 position)
        {
            base.position = position + new Vector2(0.0f, -50f);
            TextValue = System.Convert.ToString((int)score);
            time = 0.0f;
            isShowing = true;
            color = baseColor;
            OutlineColor = Color.Black;
        }

        public void Stop()
        {
            isShowing = false;
        }

        public void Update(GameTime gameTime)
        {
            if (isShowing)
            {
                color = baseColor * (float)((1 - time) / Interval);
                OutlineColor = Color.Black * (float)((1 - time) / Interval);
                
                position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                time += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }
    }
}
