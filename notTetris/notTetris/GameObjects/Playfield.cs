using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NotTetris.Graphics;

namespace NotTetris.GameObjects
{
    public delegate void GameOverEventHandler(object o, EventArgs e);

    public enum GameType
    {
        Normal,
        Time_match,
    }

    class Playfield
    {
        public event GameOverEventHandler GameOver;

        const float YPOSITION = 325f;
        const float YOFFSET = 100f;

        Vector2 position;
        SpriteBatch spriteBatch;
        Image backgroundImage;
        Image cutoffLine;
        ScoreCounter scoreCounter;
        float blockSize;
        Vector2 scale;
        List<Block> blocks;
        List<Block> connectedBlocks;
        List<Block> explodingBlocks;
        Cluster currentCluster;
        Cluster nextCluster;
        Block[,] staticBlocks;
        int startingColumn;
        int scoreMultiplier;
        int maxBlocks;
        string difficulty;
        Image[] blockImages;
        Animation explosionAnimation;

        public float SpeedMultiplier { get; set; }
        public float BaseDropSpeed { get; set; }
        public static float Width { get { return 350f; } }
        public static float Height { get { return 500f; } }
        public bool ControlsLocked { get; set; }
        public bool IsPaused { get; set; }
        public bool IsShowing { get; set; }
        public float GetScore { get { return scoreCounter.Score; } set { scoreCounter.Score = value; } }


        public Playfield(GameType gameType, float xPosition, int sizeX)
        {
            if (sizeX % 2 == 0)
                sizeX--;
            blockSize = Width / sizeX;
            int sizeY = (int)(Height / blockSize);

            maxBlocks = (sizeX - 1) * sizeY + 2;
            blocks = new List<Block>(maxBlocks);
            explodingBlocks = new List<Block>();
            backgroundImage = new Image();
            cutoffLine = new Image();
            scoreCounter = new ScoreCounter(new Vector2(xPosition - 150, 600));
            this.position = new Vector2(xPosition, YPOSITION);
            scale = new Vector2(blockSize / Block.STANDARDSIZE);
            staticBlocks = new Block[sizeX, sizeY];
            startingColumn = (sizeX + 1) / 2;

            blockImages = new Image[Enum.GetNames(typeof(BlockType)).Length];
            for (int i = 0; i < blockImages.Length; i++)
                blockImages[i] = new Image();
            explosionAnimation = new Animation();
        }

        public void Initialize(SpriteBatch spriteBatch, string difficulty)
        {
            this.spriteBatch = spriteBatch;
            ControlsLocked = true;
            IsPaused = true;
            backgroundImage.Initialize();
            backgroundImage.TextureName = TextureNames.playfieldbackground_yellow;
            backgroundImage.Position = position;
            backgroundImage.Size = new Vector2(Width + 20, Height + 20);
            cutoffLine.Initialize();
            cutoffLine.TextureName = TextureNames.red_line;
            cutoffLine.Position = new Vector2(position.X, YPOSITION + Height * 0.5f - blockSize * (staticBlocks.GetLength(1) - 3));
            cutoffLine.Size = new Vector2(Width, 2f);
            cutoffLine.Layer = 1.0f;
            scoreCounter.Initialize();

            for (int i = 0; i < blockImages.Length; i++)
            {
                blockImages[i].Initialize();
                blockImages[i].Size = new Vector2(Block.STANDARDSIZE);
                blockImages[i].Scale = scale;
                blockImages[i].Layer = 0.6f;
            }

            blockImages[0].TextureName = TextureNames.block_red;
            blockImages[1].TextureName = TextureNames.block_blue;
            blockImages[2].TextureName = TextureNames.block_yellow;
            blockImages[3].TextureName = TextureNames.block_green;
            blockImages[4].TextureName = TextureNames.block_orange;
            blockImages[5].TextureName = TextureNames.block_gray;

            explosionAnimation.Initialize();
            explosionAnimation.Size = new Vector2(64f);
            explosionAnimation.Scale = scale;
            explosionAnimation.IsShowing = false;
            explosionAnimation.IsStarted = false;
            explosionAnimation.Layer = 0.7f;
            explosionAnimation.NumFrames = 16;
            explosionAnimation.FramesPerSecond = 16f;
            explosionAnimation.IsLooped = false;
            explosionAnimation.TextureName = TextureNames.block_explosion;

            CreateNextCluster();
            SpeedMultiplier = 1;
            
            this.difficulty = difficulty;
        }

        public void LoadContent()
        {
            backgroundImage.LoadContent(spriteBatch);
            cutoffLine.LoadContent(spriteBatch);
            scoreCounter.LoadContent(spriteBatch);

            for (int i = 0; i < blockImages.Length; i++)
                blockImages[i].LoadContent(spriteBatch);
            explosionAnimation.LoadContent(spriteBatch);
        }

        public void Update(GameTime gameTime)
        {
            if (!IsPaused)
            {
                if (!explosionAnimation.IsStarted)
                {
                    if (ClearUpBlocks())
                        ReleaseBlocks();

                    UpdateClusters(gameTime);

                    CheckForClusterCollision();

                    UpdateBlocks(gameTime);

                    CheckForExplosions();

                    if (!BlocksAreMoving() && !currentCluster.IsMoving && explodingBlocks.Count == 0)
                    {
                        if (IsGameOver())
                            EndGame();
                        else
                            DropNextCluster();
                    }
                }
                else
                    UpdateExplosion(gameTime);
            }
        }

        /// <summary>
        /// Updates blocks and checks for collisions.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        private void UpdateBlocks(GameTime gameTime)
        {
            foreach (Block block in blocks)
                if (block != null)
                    if (block.IsMoving)
                    {
                        block.Update(gameTime);
                        CheckForBlockCollision(block);
                    }
        }

        /// <summary>
        /// Updates explosion animation and removes exploded block at the end
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdateExplosion(GameTime gameTime)
        {
            explosionAnimation.Update(gameTime);
            if (explosionAnimation.CurrentFrame >= explosionAnimation.NumFrames - 1)
            {
                foreach (Block block in explodingBlocks)
                    block.Dispose();
                explosionAnimation.Stop();
            }
        }

        private void UpdateClusters(GameTime gameTime)
        {
            currentCluster.Update(gameTime);
            nextCluster.Update(gameTime);
            currentCluster.SetDropSpeed(BaseDropSpeed * SpeedMultiplier);
        }

        /// <summary>
        /// Returns true if a block is moving
        /// </summary>
        /// <returns></returns>
        private bool BlocksAreMoving()
        {
            foreach (Block block in blocks)
                if (block.IsMoving)
                    return true;
            return false;
        }

        /// <summary>
        /// Checks newly stationary blocks for explosion conditions
        /// </summary>
        private void CheckForExplosions()
        {
            bool checkBlocks = true;

            if (!currentCluster.IsMoving)
                checkBlocks = !BlocksAreMoving();

            if (checkBlocks)
            {
                foreach (Block block in blocks)
                    if (block.WillBeChecked)
                        CheckBlock(block);
            }
        }

        /// <summary>
        /// Retruns true if game over conditions are met
        /// </summary>
        /// <returns></returns>
        private bool IsGameOver()
        {
            for (int i = 0; i < staticBlocks.GetLength(0); i++)
                for (int j = staticBlocks.GetLength(1) - 3; j < staticBlocks.GetLength(1); j++)
                    if (staticBlocks[i, j] != null)
                        return true;
            return false;
        }

        /// <summary>
        /// Makes blocks fall if there is space below
        /// </summary>
        private void ReleaseBlocks()
        {
            for (int i = 0; i < staticBlocks.GetLength(0); i++)
            {
                bool shouldFall = false;
                for (int j = 0; j < staticBlocks.GetLength(1); j++)
                {
                    if (staticBlocks[i, j] == null)
                        shouldFall = true;
                    else if (shouldFall && staticBlocks[i, j] != null)
                    {
                        staticBlocks[i, j].IsMoving = true;
                        staticBlocks[i, j] = null;
                    }
                }
            }
        }

        /// <summary>
        /// Clears up exploded blocks.
        /// </summary>
        /// <returns>True if blocks ar</returns>
        private bool ClearUpBlocks()
        {
            if (explodingBlocks.Count != 0)
            {
                foreach (Block block in explodingBlocks)
                {
                    block.IsDisposed = true;
                    blocks.Remove(block);
                }
                explodingBlocks.Clear();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if cluster collides with anything
        /// </summary>
        private void CheckForClusterCollision()
        {
            if (currentCluster.IsMoving)
                if (BlockHasCollided(currentCluster.FirstBlock) || BlockHasCollided(currentCluster.SecondBlock))
                {
                    ControlsLocked = true;
                    currentCluster.SetDropSpeed(BaseDropSpeed * SpeedMultiplier);
                    CheckForBlockCollision(currentCluster.FirstBlock);
                    CheckForBlockCollision(currentCluster.SecondBlock);
                    blocks.AddRange(currentCluster.Separate());
                }
        }

        /// <summary>
        /// Checks if a block collides with anything
        /// </summary>
        /// <param name="block"></param>
        private void CheckForBlockCollision(Block block)
        {
            int posX = GridPositionX(block.Position);
            int posY = GridPositionY(block.Position);

            if (BlockHasCollided(block))
            {
                block.Attach(new Vector2(block.Position.X, position.Y + 0.5f * Height - (posY + 0.5f) * blockSize + 0.005f));
                staticBlocks[posX, posY] = block;
            }
        }

        /// <summary>
        /// Determines if a moving block collides with the bottom or a static block.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        private bool BlockHasCollided(Block block)
        {
            int posX = GridPositionX(block.Position);
            int posY = GridPositionY(block.Position);
            //bottom collision (left) & block collision (right)
            return block.Position.Y >= position.Y + 0.5 * (Height - blockSize) || staticBlocks[posX, posY - 1] != null;
        }

        /// <summary>
        /// Calculates grid position x coordinate. Left edge is 0.
        /// </summary>
        /// <param name="blockPosition"></param>
        /// <returns></returns>
        private int GridPositionX(Vector2 blockPosition)
        {
            float ret = (staticBlocks.GetLength(0) + (blockPosition.X - this.position.X - 0.5f * Width) / blockSize);
            if (ret < 0)
                return -1;
            return (int)ret;
        }

        /// <summary>
        /// Calculates grid position y coordinate. Bottom is 0.
        /// </summary>
        /// <param name="blockPosition"></param>
        /// <returns></returns>
        private int GridPositionY(Vector2 blockPosition)
        {
            return (int)(0.5f + (this.position.Y + 0.5f * Height - blockPosition.Y) / blockSize);
        }

        /// <summary>
        /// Determines number of blocks of same color connected to and explodes them if needed
        /// </summary>
        private void CheckBlock(Block block)
        {
            connectedBlocks = new List<Block>(blocks.Count);
            connectedBlocks.Add(block);

            List<Block> sameColorBlocks = new List<Block>();

            foreach (Block otherBlock in blocks)
            {
                if (otherBlock.BlockType == block.BlockType && !otherBlock.IsMoving && !otherBlock.IsExploding)
                    sameColorBlocks.Add(otherBlock);
            }

            if (sameColorBlocks.Count > 3)
            {
                CheckConnectedBlocks(block, sameColorBlocks);

                if (connectedBlocks.Count > 3)
                    ExplodeChain();
            }
        }

        private void ExplodeChain()
        {
            ControlsLocked = true;
            explosionAnimation.Play();

            foreach (Block clearedBlock in connectedBlocks)
                ExplodeAndScore(clearedBlock);

            scoreMultiplier++;

            if (difficulty == "Easy")
                SpeedMultiplier *= 1.01f;
            else if (difficulty == "Normal")
                SpeedMultiplier *= 1.02f;
            else if (difficulty == "Hard")
                SpeedMultiplier *= 1.03f;
        }

        /// <summary>
        /// Explodes a block and increments score counter
        /// </summary>
        /// <param name="block"></param>
        private void ExplodeAndScore(Block block)
        {
            int blockX = GridPositionX(block.Position);
            int blockY = GridPositionY(block.Position);
            staticBlocks[blockX, blockY] = null;
            block.Explode();
            explodingBlocks.Add(block);
            scoreCounter.Score += 50 * scoreMultiplier * SpeedMultiplier;
        }

        /// <summary>
        /// Recursive method for determining number of connected blocks of same type
        /// </summary>
        /// <param name="block"></param>
        /// <param name="sameColorBlocks"></param>
        private void CheckConnectedBlocks(Block block, List<Block> sameColorBlocks)
        {
            int blockX = GridPositionX(block.Position);
            int blockY = GridPositionY(block.Position);
            foreach (Block sameColorBlock in sameColorBlocks)
            {
                if ( block != sameColorBlock && !sameColorBlock.IsExploding && !connectedBlocks.Contains(sameColorBlock))
                {
                    int samecolBlockX = GridPositionX(sameColorBlock.Position);
                    int samecolBlockY = GridPositionY(sameColorBlock.Position);
                    if (blockX == samecolBlockX + 1 && blockY == samecolBlockY)
                    {
                        connectedBlocks.Add(sameColorBlock);
                        CheckConnectedBlocks(sameColorBlock, sameColorBlocks);
                    }
                    if (blockX == samecolBlockX - 1 && blockY == samecolBlockY)
                    {
                        connectedBlocks.Add(sameColorBlock);
                        CheckConnectedBlocks(sameColorBlock, sameColorBlocks);
                    }
                    if (blockX == samecolBlockX && blockY == samecolBlockY + 1)
                    {
                        connectedBlocks.Add(sameColorBlock);
                        CheckConnectedBlocks(sameColorBlock, sameColorBlocks);
                    }
                    if (blockX == samecolBlockX && blockY == samecolBlockY - 1)
                    {
                        connectedBlocks.Add(sameColorBlock);
                        CheckConnectedBlocks(sameColorBlock, sameColorBlocks);
                    }
                }
            }
        }

        public void StartGame()
        {
            DropNextCluster();
            UnPause();
        }

        public void Pause()
        {
            IsPaused = true;
            ControlsLocked = true;
        }

        public void UnPause()
        {
            IsPaused = false;
            ControlsLocked = false;
        }

        public void EndGame()
        {
            Pause();
            GameOver(this, EventArgs.Empty);
        }

        #region Commands
        /// <summary>
        /// Move left command
        /// </summary>
        public void MoveClusterLeft()
        {
            int firstBlockX = GridPositionX(currentCluster.FirstBlock.Position);
            int firstBlockY = GridPositionY(currentCluster.FirstBlock.Position);
            int secondBlockX = GridPositionX(currentCluster.SecondBlock.Position);
            int secondBlockY = GridPositionY(currentCluster.SecondBlock.Position);

            bool collision = false;
            if (firstBlockX == 0 ||  secondBlockX == 0)
                collision = true;
            else if (staticBlocks[firstBlockX - 1, firstBlockY - 1] != null || staticBlocks[secondBlockX - 1, secondBlockY - 1] != null)
                collision = true;

            if (!collision)
                currentCluster.Move(currentCluster.FirstBlock.Position - new Vector2(blockSize, 0));
        }

        /// <summary>
        /// Move right command
        /// </summary>
        public void MoveClusterRight()
        {
            int firstBlockX = GridPositionX(currentCluster.FirstBlock.Position);
            int firstBlockY = GridPositionY(currentCluster.FirstBlock.Position);
            int secondBlockX = GridPositionX(currentCluster.SecondBlock.Position);
            int secondBlockY = GridPositionY(currentCluster.SecondBlock.Position);

            bool collision = false;
            if (firstBlockX == staticBlocks.GetLength(0) - 1 || secondBlockX == staticBlocks.GetLength(0) - 1)
                collision = true;
            else if (staticBlocks[firstBlockX + 1, firstBlockY - 1] != null || staticBlocks[secondBlockX + 1, secondBlockY - 1] != null)
                collision = true;

            if (!collision)
                currentCluster.Move(currentCluster.FirstBlock.Position + new Vector2(blockSize, 0));
        }

        /// <summary>
        /// Rotate command
        /// </summary>
        public void RotateCluster()
        {
            int secondBlockX = GridPositionX(currentCluster.SecondBlock.Position);
            int secondBlockY = GridPositionY(currentCluster.SecondBlock.Position);

            bool rotate = false;

            if (currentCluster.Orientation == Orientation.Down)
            {
                int newSecondBlockX = GridPositionX(currentCluster.SecondBlock.Position + new Vector2(-blockSize));
                int newSecondBlockY = GridPositionY(currentCluster.SecondBlock.Position + new Vector2(-blockSize));

                if (newSecondBlockX >= 0)
                    if (staticBlocks[newSecondBlockX, newSecondBlockY - 1] == null)
                        rotate = true;
                if (!rotate)
                    currentCluster.Invert();
            }
            else if (currentCluster.Orientation == Orientation.Left)
            {
                int newSecondBlockX = GridPositionX(currentCluster.SecondBlock.Position + new Vector2(blockSize, -blockSize));
                int newSecondBlockY = GridPositionY(currentCluster.SecondBlock.Position + new Vector2(blockSize, -blockSize));

                if (newSecondBlockY <= staticBlocks.GetLength(1) - 1)
                    if (staticBlocks[newSecondBlockX, newSecondBlockY - 1] == null)
                        rotate = true;
            }
            else if (currentCluster.Orientation == Orientation.Up)
            {
                int newSecondBlockX = GridPositionX(currentCluster.SecondBlock.Position + new Vector2(blockSize));
                int newSecondBlockY = GridPositionY(currentCluster.SecondBlock.Position + new Vector2(blockSize));

                if (newSecondBlockX < staticBlocks.GetLength(0))
                    if (staticBlocks[newSecondBlockX, newSecondBlockY - 1] == null)
                        rotate = true;
                if (!rotate)
                    currentCluster.Invert();
            }
            else if (currentCluster.Orientation == Orientation.Right)
            {
                int newSecondBlockX = GridPositionX(currentCluster.SecondBlock.Position + new Vector2(-blockSize, blockSize));
                int newSecondBlockY = GridPositionY(currentCluster.SecondBlock.Position + new Vector2(-blockSize, blockSize));

                if (newSecondBlockY > 1)
                    if (staticBlocks[newSecondBlockX, newSecondBlockY - 1] == null)
                        rotate = true;
            }

            if (rotate)
                currentCluster.Rotate(true);
        }

        /// <summary>
        /// Move down command
        /// </summary>
        public void MoveClusterDown()
        {
            currentCluster.SetDropSpeed(2 * BaseDropSpeed * SpeedMultiplier);
        }
        #endregion

        private void DropNextCluster()
        {
            currentCluster = nextCluster;
            currentCluster.Move(position - new Vector2(0f, (Height - blockSize) * 0.5f));
            currentCluster.IsMoving = true;
            currentCluster.SetDropSpeed(BaseDropSpeed * SpeedMultiplier);
            CreateNextCluster();
            ControlsLocked = false;
            scoreMultiplier = 1;
        }

        private void CreateNextCluster()
        {
            nextCluster = new Cluster(position - new Vector2(250f, 175f), blockSize);
            nextCluster.Initialize();
            nextCluster.IsMoving = false;
        }

        public void Draw(GameTime gameTime)
        {
            if (IsShowing)
            {
                backgroundImage.Draw(gameTime);
                cutoffLine.Draw(gameTime);
                scoreCounter.Draw(gameTime);

                if (currentCluster != null)
                {
                    DrawBlock(currentCluster.FirstBlock, gameTime);
                    DrawBlock(currentCluster.SecondBlock, gameTime);
                }
                DrawBlock(nextCluster.FirstBlock, gameTime);
                DrawBlock(nextCluster.SecondBlock, gameTime);

                foreach (Block block in blocks)
                    DrawBlock(block, gameTime);
            }
        }

        private void DrawBlock(Block block, GameTime gameTime)
        {
            if (block != null && !block.IsDisposed)
                block.Draw(gameTime, GetImage(block));
        }

        private Image GetImage(Block block)
        {
            if (block.IsExploding)
                return explosionAnimation;
            return blockImages[(int)block.BlockType];
        }
    }
}
