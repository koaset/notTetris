using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NotTetris.Graphics;

namespace NotTetris.GameObjects
{
    #region Orientation enum
    public enum Orientation
    {
        Down,
        Left,
        Right,
        Up,
    }
    #endregion

    class Cluster
    {
        #region Props
        public Block FirstBlock
        {
            get { return firstBlock; }
            set { firstBlock = value; }
        }

        public Block SecondBlock
        {
            get { return secondBlock; }
        }

        public Orientation Orientation
        {
            get { return orientation; }
            set { orientation = value; }
        }

        public bool IsMoving
        {
            get { return isMoving; }
            set { isMoving = value; }
        }
        #endregion

        

        Block firstBlock;
        Block secondBlock;
        private Orientation orientation;
        private bool isMoving;
        float blockSize;

        public Cluster(Vector2 position, float blockSize)
        {
            firstBlock = new Block((BlockType)PuzzleGame.r.Next(0, 6), new Vector2(position.X, position.Y - blockSize / 2), blockSize);
            secondBlock = new Block((BlockType)PuzzleGame.r.Next(0, 6), new Vector2(position.X, position.Y + blockSize / 2), blockSize);
            this.blockSize = blockSize;
        }

        public void Initialize()
        {
            orientation = Orientation.Down;
            firstBlock.Initialize();
            secondBlock.Initialize();
            isMoving = true;
        }

        public void Update(GameTime gameTime)
        {
            if (isMoving)
            {
                firstBlock.Update(gameTime);
                secondBlock.Update(gameTime);
            }
        }

        public Block[] Separate()
        {
            Block[] blocks = new Block[2];
            blocks[0] = firstBlock;
            blocks[1] = secondBlock;
            isMoving = false;
            return blocks;
        }

        public void Move(Vector2 newPosition)
        {
            if (firstBlock.IsMoving && secondBlock.IsMoving)
            {
                firstBlock.Position = newPosition;

                if (orientation == Orientation.Down)
                    secondBlock.Position = newPosition + new Vector2(0f, blockSize);
                else if (orientation == Orientation.Left)
                    secondBlock.Position = newPosition + new Vector2(-blockSize, 0f);
                else if (orientation == Orientation.Up)
                    secondBlock.Position = newPosition + new Vector2(0f, -blockSize);
                else if (orientation == Orientation.Right)
                    secondBlock.Position = newPosition + new Vector2(blockSize, 0f);
            }
        }

        public void Rotate(bool rotateClockwise)
        {
            if (rotateClockwise)
            {
                if (orientation == Orientation.Down)
                {
                    secondBlock.Position += - new Vector2(blockSize);
                    orientation = Orientation.Left;
                }
                else if (orientation == Orientation.Left)
                {
                    secondBlock.Position += new Vector2(blockSize, -blockSize);
                    orientation = Orientation.Up;
                }
                else if (orientation == Orientation.Up)
                {
                    secondBlock.Position += new Vector2(blockSize);
                    orientation = Orientation.Right;
                }
                else if (orientation == Orientation.Right)
                {
                    secondBlock.Position += new Vector2(-blockSize, blockSize);
                    orientation = Orientation.Down;
                }
            }
            else
            {
                if (orientation == Orientation.Down)
                {
                    secondBlock.Position += new Vector2(blockSize, -blockSize);
                    orientation = Orientation.Right;
                }
                else if (orientation == Orientation.Up)
                {
                    secondBlock.Position += new Vector2(-blockSize, blockSize);
                    orientation = Orientation.Right;
                }
            }
        }

        public void Invert()
        {
            if (orientation == Orientation.Down)
                orientation = Orientation.Up;
            else if (orientation == Orientation.Up)
                orientation = Orientation.Down;

            Vector2 temp = firstBlock.Position;
            firstBlock.Position = secondBlock.Position;
            secondBlock.Position = temp;
        }

        public void SetDropSpeed(float speed)
        {
            firstBlock.DropSpeed = speed;
            secondBlock.DropSpeed = speed;
        }

        public void SetBlockTypes(BlockType firstBlock, BlockType secondBlock)
        {

        }
    }
}
