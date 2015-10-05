using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NotTetris.Graphics
{
    class Image
    {
        #region Props
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Vector2 Size
        {
            get { return size; }
            set { size = value; }
        }

        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public float Layer
        {
            get { return this.layer; }
            set { layer = value; }
        }

        public bool IsShowing
        {
            get { return isShowing; }
            set { isShowing = value; }
        }

        public SpriteEffects Effects
        {
            get { return effects; }
            set { effects = value; }
        }

        public TextureNames TextureName
        {
            get { return textureName; }
            set { textureName = value; }
        }

        public Vector2 Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        public Vector2 TextureSize
        {
            get { return new Vector2(texture.Width, texture.Height); }
        }
        #endregion

        private TextureNames textureName;
        protected SpriteBatch spriteBatch;
        protected Texture2D texture;
        protected Rectangle source;
        protected Color color;
        protected float rotation;
        protected Vector2 position;
        protected Vector2 origin;
        protected Vector2 scale;
        protected SpriteEffects effects;
        protected float layer;
        protected Vector2 size;
        protected bool isShowing;

        public virtual void Initialize()
        {
            this.isShowing = true;
            this.color = Color.White;
            this.rotation = 0f;
            this.scale = Vector2.One;
            this.effects = SpriteEffects.None;
            this.layer = 0.5f;
            this.size = Vector2.Zero;
        }

        public virtual void LoadContent(SpriteBatch spriteBatch)
        {
            this.spriteBatch = spriteBatch;
            this.texture = GraphicsManager.GetTexture(textureName);

            this.source = CalculateSource(0);
            this.origin = new Vector2(source.Width / 2, source.Height / 2);
        }

        public virtual void Update(GameTime gameTime){}

        public virtual void Draw(GameTime gameTime)
        {
            if (isShowing)
                this.spriteBatch.Draw(texture, position, source, color, rotation, origin, scale, effects, layer);
        }

        protected Rectangle CalculateSource(int index)
        {
            if (index <= 0)
            {
                return new Rectangle(0, 0, (int)this.size.X, (int)this.size.Y);
            }
            else
            {
                int frameRow = index;
                int frameColumn = 1;
                int frameWidth = (int)this.size.X;
                int frameHeight = (int)this.size.Y;
                int totalColumns = this.texture.Width / frameWidth;
                int totalRows = this.texture.Height / frameHeight;

                while (frameRow > totalRows)
                {
                    frameColumn++;
                    frameRow -= totalRows;
                }

                if (index > totalRows * totalColumns)
                {
                    frameColumn = totalColumns;
                    frameRow = totalRows;
                }

                return new Rectangle(
                    (frameColumn - 1) * frameWidth,
                    (frameRow - 1) * frameHeight,
                    frameWidth,
                    frameHeight);
            }
        }

    }
}
