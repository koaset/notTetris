using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NotTetris.Graphics;

namespace NotTetris.GameObjects
{
    /// <summary>
    /// A unit of two blocks the player can control
    /// </summary>
    class Cluster
    {
        public Block FirstBlock { get; set; }
        public Block SecondBlock { get; set; }
        public Orientation Orientation { get; set; }
        public bool IsMoving { get; set; }

        private float blockSize;

        public Cluster(Vector2 position, float blockSize)
        {
            FirstBlock = new Block((BlockType)PuzzleGame.r.Next(0, 6), new Vector2(position.X, position.Y + blockSize / 2), blockSize);
            SecondBlock = new Block((BlockType)PuzzleGame.r.Next(0, 6), new Vector2(position.X, position.Y - blockSize / 2), blockSize);
            this.blockSize = blockSize;
        }

        public void Initialize()
        {
            Orientation = Orientation.Up;
            FirstBlock.Initialize();
            SecondBlock.Initialize();
            IsMoving = true;
        }

        public void Update(GameTime gameTime)
        {
            if (IsMoving)
            {
                FirstBlock.Update(gameTime);
                SecondBlock.Update(gameTime);
            }
        }

        public Block[] Separate()
        {
            Block[] blocks = new Block[2];
            blocks[0] = FirstBlock;
            blocks[1] = SecondBlock;
            IsMoving = false;
            return blocks;
        }

        public void Move(Vector2 newPosition)
        {
            if (FirstBlock.IsMoving && SecondBlock.IsMoving)
            {
                FirstBlock.Position = newPosition;

                if (Orientation == Orientation.Down)
                    SecondBlock.Position = newPosition + new Vector2(0f, blockSize);
                else if (Orientation == Orientation.Left)
                    SecondBlock.Position = newPosition + new Vector2(-blockSize, 0f);
                else if (Orientation == Orientation.Up)
                    SecondBlock.Position = newPosition + new Vector2(0f, -blockSize);
                else if (Orientation == Orientation.Right)
                    SecondBlock.Position = newPosition + new Vector2(blockSize, 0f);
            }
        }

        public void SetPosition(Vector2 firstBlockPosition, Vector2 secondBlockPosition)
        {
            FirstBlock.Position = firstBlockPosition;
            SecondBlock.Position = secondBlockPosition;
        }

        public void Rotate(bool rotateClockwise)
        {
            if (rotateClockwise)
            {
                if (Orientation == Orientation.Down)
                {
                    SecondBlock.Position += - new Vector2(blockSize);
                    Orientation = Orientation.Left;
                }
                else if (Orientation == Orientation.Left)
                {
                    SecondBlock.Position += new Vector2(blockSize, -blockSize);
                    Orientation = Orientation.Up;
                }
                else if (Orientation == Orientation.Up)
                {
                    SecondBlock.Position += new Vector2(blockSize);
                    Orientation = Orientation.Right;
                }
                else if (Orientation == Orientation.Right)
                {
                    SecondBlock.Position += new Vector2(-blockSize, blockSize);
                    Orientation = Orientation.Down;
                }
            }
            else
            {
                if (Orientation == Orientation.Down)
                {
                    SecondBlock.Position += new Vector2(blockSize, -blockSize);
                    Orientation = Orientation.Right;
                }
                else if (Orientation == Orientation.Left)
                {
                    SecondBlock.Position += new Vector2(blockSize);
                    Orientation = Orientation.Down;
                }
                else if (Orientation == Orientation.Up)
                {
                    SecondBlock.Position += new Vector2(-blockSize, blockSize);
                    Orientation = Orientation.Left;
                }
                else if (Orientation == Orientation.Right)
                {
                    SecondBlock.Position += new Vector2(-blockSize);
                    Orientation = Orientation.Up;
                }
            }
        }

        public void Invert()
        {
            if (Orientation == Orientation.Down)
                Orientation = Orientation.Up;
            else if (Orientation == Orientation.Up)
                Orientation = Orientation.Down;

            Vector2 temp = FirstBlock.Position;
            FirstBlock.Position = SecondBlock.Position;
            SecondBlock.Position = temp;
        }

        public void SetDropSpeed(float speed)
        {
            FirstBlock.DropSpeed = speed;
            SecondBlock.DropSpeed = speed;
        }
    }

    #region Orientation enum
    /// <summary>
    /// Determines position of second block relative to first
    /// </summary>
    public enum Orientation
    {
        Down,
        Left,
        Right,
        Up,
    }
    #endregion
}
