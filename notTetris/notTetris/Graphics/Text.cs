using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NotTetris.Graphics
{
    /// <summary>
    /// A drawable text string
    /// </summary>
    class Text : IDrawable
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

        public float Spacing
        {
            get { return spacing; }
            set { spacing = value; }
        }
        #endregion

        protected SpriteFont spriteFont;
        protected SpriteBatch spriteBatch;
        protected Vector2 origin;
        protected string text;
        protected bool isShowing;
        protected bool isCentered;
        protected Vector2 position;
        protected Vector2 scale;
        protected float rotation;
        protected Color color;
        protected SpriteEffects effects;
        protected float layer;
        protected FontNames font;
        protected float spacing;

        public virtual void Initialize()
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

        public Vector2 GetSize()
        {
            return spriteFont.MeasureString(TextValue) * scale;
        }

        public void LoadContent(SpriteBatch spriteBatch)
        {
            this.spriteBatch = spriteBatch;
            spriteFont = GraphicsManager.GetFont(font);
            origin = spriteFont.MeasureString(text);
        }

        public virtual void Draw(GameTime gameTime)
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
