using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NotTetris.Graphics;

namespace NotTetris.GameObjects
{
    #region Blocktype enum
    enum BlockType
    {
        red,
        blue,
        yellow,
        green,
        orange,
        black,
    }
    #endregion

    class Block
    {
        #region Props
        public float Size
        {
            get { return size; }
            set { size = value; }
        }

        public float DropSpeed
        {
            get { return dropSpeed; }
            set { dropSpeed = value; }
        }

        public bool IsExploding
        {
            get { return isExploding; }
            set { isExploding = value; }
        }

        public bool IsMoving
        {
            get { return isMoving; }
            set { isMoving = value; }
        }

        public BlockType BlockType
        {
            get { return type; }
        }

        public bool IsCleared
        {
            get { return isCleared; }
            set { isCleared = value; }
        }

        public bool IsDisposed
        {
            get { return isDisposed; }
            set { isDisposed = value; }
        }

        public bool IsDisposing
        {
            get { return isDisposing; }
            set { isDisposing = value; }
        }

        public bool WillBeChecked
        {
            get { return willBeChecked; }
            set { willBeChecked = value; }
        }

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public bool ShouldDrop { get; set; }
        #endregion

        public const float STANDARDSIZE = 50f;
        float size;

        private float dropSpeed;
        bool isExploding;
        bool isMoving;
        BlockType type;
        private bool isCleared;
        private bool isDisposed;
        private bool isDisposing;
        private bool willBeChecked;
        Vector2 position;
        Vector2 scale;

        public Block(BlockType blockType, Vector2 position, float size)
        {
            this.position = position;
            this.size = size;
            type = blockType;
        }

        public void Initialize()
        {
            scale = new Vector2(size / STANDARDSIZE);
            isMoving = true;
            dropSpeed = 1f;
            willBeChecked = false;
            ShouldDrop = false;
        }

        public void Update(GameTime gameTime)
        {
            if (isMoving)
                position.Y += dropSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Attach(Vector2 position)
        {
            this.position = position;
            isMoving = false;
            willBeChecked = true;
        }

        public void Detatch()
        {
            isMoving = true;
        }

        public void Explode()
        {
            isExploding = true;
            isCleared = true;
        }

        public void Dispose()
        {
            isExploding = false;
            isCleared = false;
            isMoving = false;
            position = Vector2.Zero;
            type = 0;
            isDisposing = true;
        }

        public void Draw(GameTime gameTime, Image image)
        {
            image.Position = Position;
            image.Draw(gameTime);
        }
    }
}
