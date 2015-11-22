using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NotTetris.Graphics;
using System;

namespace NotTetris.GameObjects
{
    /// <summary>
    /// Defines a block used in gameplay
    /// </summary>
    class Block
    {
        public const float STANDARDSIZE = 50f;

        public bool IsMoving { get; set; }
        public BlockType BlockType { get; set; }
        public bool IsCleared { get; set; }
        public bool IsDisposing { get; set; }
        public bool ShouldBeChecked { get; set; }
        public float Size { get; set; }
        public float DropSpeed { get; set; }
        public bool IsExploding { get; set; }
        public float Rotation { get; set; }
        public bool ShouldDrop { get; set; }

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        private Vector2 position;
        private Vector2 scale;

        public Block(BlockType blockType, Vector2 position, float size)
        {
            this.position = position;
            this.Size = size;
            BlockType = blockType;
        }

        public void Initialize()
        {
            scale = new Vector2(Size / STANDARDSIZE);
            IsMoving = true;
            DropSpeed = 1f;
            ShouldBeChecked = false;
            ShouldDrop = false;
            Rotation = 0.0f;
        }

        public void Update(GameTime gameTime)
        {
            if (IsMoving)
                position.Y += DropSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        

        public void Attach(Vector2 position)
        {
            this.position = position;
            IsMoving = false;
            ShouldBeChecked = true;
        }

        public void Detatch()
        {
            IsMoving = true;
        }

        public void Explode()
        {
            IsExploding = true;
            IsCleared = true;
            Rotation = (float)(PuzzleGame.r.NextDouble() * 2.0f * Math.PI);
        }

        public void Dispose()
        {
            IsDisposing = true;
            IsExploding = false;
            IsCleared = false;
            IsMoving = false;
            position = Vector2.Zero;
            BlockType = 0;
        }

        public void Draw(GameTime gameTime, Image image)
        {
            image.Rotation = Rotation;
            image.Position = Position;
            image.Draw(gameTime);
        }
    }

    #region Blocktype enum
    enum BlockType
    {
        Red,
        Blue,
        Yellow,
        Green,
        Orange,
        Purple,
        Black,
    }
    #endregion
}
