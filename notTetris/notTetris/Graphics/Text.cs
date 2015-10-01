using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NotTetris.Graphics
{
    class Text
    {
        #region Props
        public string TextValue
        {
            get { return text; }
            set { text = value; }
        }

        public bool IsShowing
        {
            get { return isShowing; }
            set { isShowing = value; }
        }

        public bool IsCentered
        {
            get { return isCentered; }
            set { isCentered = value; }
        }

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Vector2 Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public Color TextColor
        {
            get { return color; }
            set { color = value; }
        }

        public SpriteEffects SpriteEffects
        {
            get { return effects; }
            set { effects = value; }
        }

        public float Layer
        {
            get { return layer; }
            set { layer = value; }
        }

        public FontNames Font
        {
            get { return font; }
            set { font = value; }
        }

        public int LineSpacing
        {
            get { return lineSpacing; }
            set { lineSpacing = value; }
        }

        public float Spacing
        {
            get { return spacing; }
            set { spacing = value; }
        }
        #endregion

        SpriteFont spriteFont;
        SpriteBatch spriteBatch;
        Vector2 origin;
        string text;
        bool isShowing;
        bool isCentered;
        Vector2 position;
        Vector2 scale;
        float rotation;
        Color color;
        SpriteEffects effects;
        float layer;
        FontNames font;
        int lineSpacing;
        float spacing;

        public void Initialize()
        {
            text = "Placeholder";
            isShowing = true;
            isCentered = false;
            position = Vector2.Zero;
            scale = Vector2.One;
            rotation = 0f;
            color = Color.Black;
            effects = SpriteEffects.None;
            layer = 0.5f;
            font = FontNames.Segoe_UI_Mono;
        }

        public void LoadContent(SpriteBatch spriteBatch)
        {
            this.spriteBatch = spriteBatch;
            spriteFont = GraphicsManager.GetFont(font);
            origin = spriteFont.MeasureString(text);
        }

        public void Draw(GameTime gameTime)
        {

            spriteFont.Spacing = spacing;
            if (isCentered)
                origin = spriteFont.MeasureString(text) / 2;
            else
                origin = Vector2.Zero;

            if (isShowing)
                spriteBatch.DrawString(spriteFont, text, position, color, rotation, origin, scale, effects, layer);
        }
    }
}
