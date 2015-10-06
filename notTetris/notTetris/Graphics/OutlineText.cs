using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NotTetris.Graphics
{
    class OutlineText : Text
    {
        public Color OutlineColor { get; set; }
        public float OutlineSize { get; set; }

        public override void Initialize()
        {
            base.Initialize();
            OutlineColor = Color.White;
            OutlineSize = 1f;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            layer -= 0.01f;
            DrawAtOffset(new Vector2(OutlineSize, OutlineSize));
            DrawAtOffset(new Vector2(OutlineSize, -OutlineSize));
            DrawAtOffset(new Vector2(-OutlineSize, OutlineSize));
            DrawAtOffset(new Vector2(-OutlineSize, -OutlineSize));
            layer += 0.01f;
            
        }

        private void DrawAtOffset(Vector2 offset)
        {
            if (isShowing)
                spriteBatch.DrawString(spriteFont, text, position + offset, OutlineColor, rotation, origin, scale, effects, layer);
        }
    }
}
