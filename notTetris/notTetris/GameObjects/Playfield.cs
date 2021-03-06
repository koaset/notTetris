﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NotTetris.Graphics;

namespace NotTetris.GameObjects
{
    /// <summary>
    /// Handles game logic for a single local player
    /// </summary>
    class Playfield : NotTetris.Graphics.IDrawable
    {
        public event GameOverEventHandler GameOver;
        public event NewNextClusterEventHandler NewNextCluster;
        public event ClusterDropEventHandler ClusterDrop;
        public event ClusterSeparateEventHandler ClusterSeparate;
        public event BlackBlocksCreatedEventHandler BlackBlocksCreated;
        public event ShouldDropBlackBlocksEventHandler ShouldDropBlackBlocks;
        public event BlackBlockCollisionEventHandler BlackBlockCollision;

        GameType gameType;
        Image backgroundImage;
        Image cutoffLine;
        Image[] blockImages;
        protected Animation explosionAnimation;
        ScoreCounter scoreCounter;
        protected float blockSize;
        Vector2 scale;
        protected List<Block> blocks;
        protected List<Block> explodingBlocks;
        protected Block[,] staticBlocks;
        int startingColumn;
        protected int scoreMultiplier;
        int maxBlocks;
        string difficulty;
        protected float dropSpeedBonus;
        Text largestComboText;
        protected ScoreFloater scoreFloater;
        int largestCombo;
        float moveRightTimer;
        float moveLeftTimer;
        float moveCooldown;
        protected float dropTimer;
        protected float dropDelay;
        protected float graceTimeTimer;
        protected float graceTime;
        protected bool firstClusterCollision;
        Text stateText;

        public GameState State { get; set; }
        public Vector2 Position { get; set; }
        public Cluster NextCluster { get; set; }
        public Cluster CurrentCluster { get; set; }
        public float SpeedMultiplier { get; set; }
        public float BaseDropSpeed { get; set; }
        public static float Width { get { return 350f; } }
        public static float Height { get { return 500f; } }
        public bool MovementLocked { get; set; }
        public bool IsPaused { get; set; }
        public bool IsShowing { get; set; }
        public float GetScore { get; set; }
        public int BlackBlocksQueued { get; set; }

        public Playfield(GameType gameType, Vector2 position, int sizeX)
        {
            this.gameType = gameType;
            this.Position = position;
            if (sizeX % 2 == 0)
                sizeX--;
            blockSize = Width / sizeX;
            int sizeY = (int)(Height / blockSize);

            maxBlocks = (sizeX - 1) * sizeY + 2;
            blocks = new List<Block>(maxBlocks);
            explodingBlocks = new List<Block>();
            BlackBlocksQueued = 0;
            backgroundImage = new Image();
            cutoffLine = new Image();
            
            scoreCounter = new ScoreCounter();
            scoreFloater = new ScoreFloater();
            scale = new Vector2(blockSize / Block.STANDARDSIZE);
            staticBlocks = new Block[sizeX, sizeY];
            startingColumn = (sizeX + 1) / 2;

            blockImages = new Image[Enum.GetNames(typeof(BlockType)).Length];
            for (int i = 0; i < blockImages.Length; i++)
                blockImages[i] = new Image();
            explosionAnimation = new Animation();
            largestComboText = new Text();

            stateText = new Text();
        }

        public virtual void Initialize(SpriteBatch spriteBatch, string difficulty)
        {
            InitializeContent(spriteBatch, difficulty);

            CreateNextCluster();
            DropNextCluster();
            MovementLocked = true;
        }

        protected void InitializeContent(SpriteBatch spriteBatch, string difficulty)
        {
            stateText.Initialize();
            stateText.Position = new Vector2(this.Position.X - 175, 0f);
            stateText.Font = FontNames.Segoe_UI_Mono;
            stateText.TextValue = "Init";
            stateText.IsShowing = false;

            MovementLocked = true;
            IsPaused = true;

            dropTimer = 0;
            dropDelay = 500;

            graceTimeTimer = 0;
            graceTime = 500;

            moveRightTimer = 0;
            moveLeftTimer = 0;
            moveCooldown = 80;
            dropSpeedBonus = 2.5f;

            #region Initialize images, blocks and text
            backgroundImage.Initialize();
            backgroundImage.TextureName = TextureNames.playfieldbackground_yellow;
            backgroundImage.Position = Position;
            backgroundImage.Size = new Vector2(Width + 20, Height + 20);

            cutoffLine.Initialize();
            cutoffLine.TextureName = TextureNames.red_line;
            cutoffLine.Position = new Vector2(Position.X, Position.Y + Height * 0.5f - blockSize * (staticBlocks.GetLength(1) - 3));
            cutoffLine.Size = new Vector2(Width, 2f);
            cutoffLine.Layer = 1.0f;
            scoreCounter.Initialize();
            scoreCounter.Position = new Vector2(Position.X - 150, (Position.Y - 25f) * 2.0f);


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
            blockImages[5].TextureName = TextureNames.block_purple;
            blockImages[6].TextureName = TextureNames.block_gray;

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

            scoreFloater.Initialize();
            scoreFloater.Interval = explosionAnimation.NumFrames / explosionAnimation.FramesPerSecond;

            largestComboText.Initialize();
            largestComboText.Font = FontNames.Segoe_UI_Mono;
            largestComboText.Position = scoreCounter.Position + new Vector2(0.0f, 50f);
            largestComboText.TextColor = Color.MediumSlateBlue;
            largestComboText.Layer = 0.9f;
            largestComboText.IsCentered = false;
            largestComboText.Spacing = 6;
            largestComboText.TextValue = "Max Combo: " + largestCombo;
            #endregion

            SpeedMultiplier = 1;
            largestCombo = 0;
            this.difficulty = difficulty;
        }

        public void LoadContent(SpriteBatch spriteBatch)
        {
            backgroundImage.LoadContent(spriteBatch);
            cutoffLine.LoadContent(spriteBatch);
            scoreCounter.LoadContent(spriteBatch);
            scoreFloater.LoadContent(spriteBatch);
            largestComboText.LoadContent(spriteBatch);
            stateText.LoadContent(spriteBatch);

            for (int i = 0; i < blockImages.Length; i++)
                blockImages[i].LoadContent(spriteBatch);
            explosionAnimation.LoadContent(spriteBatch);
        }

        public virtual void Update(GameTime gameTime)
        {
            if (!IsPaused)
            {
                if (State == GameState.WaitingForDropTimer)
                    UpdateDropTimer(gameTime);
                else if (State == GameState.BlocksExploding)
                {
                    UpdateExplosion(gameTime);
                    scoreFloater.Update(gameTime);
                }
                else if (State == GameState.ClusterGraceTime)
                    UpdateGraceTimer(gameTime);
                else
                {
                    if (ClearUpBlocks())
                        ReleaseBlocks();

                    UpdateBlocks(gameTime);

                    UpdateClusters(gameTime);
                    CheckForClusterCollision(gameTime);

                    CheckForExplosions();

                    if (State != GameState.ClusterGraceTime)
                        if (!BlocksAreMoving() && !CurrentCluster.IsMoving && explodingBlocks.Count == 0)
                        {
                            if (IsGameOver())
                                EndGame();
                            else
                            {
                                if (BlackBlocksQueued > 0)
                                    CreateBlackBlocks();
                                else
                                    DropNextCluster();
                            }
                        }
                }

                if (stateText.IsShowing)
                    UpdateStateText();
            }
        }

        protected void UpdateDropTimer(GameTime gameTime)
        {
            dropTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (dropTimer > dropDelay)
            {
                CurrentCluster.IsMoving = true;
                CurrentCluster.SetDropSpeed(BaseDropSpeed * SpeedMultiplier);
                State = GameState.ClusterFalling;
            }
        }

        protected void UpdateGraceTimer(GameTime gameTime)
        {
            graceTimeTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (graceTimeTimer > graceTime)
            {
                if (!CurrentCluster.IsMoving)
                    SeparateCluster();
                else
                    State = GameState.ClusterFalling;
            }
        }

        protected void UpdateStateText()
        {
            switch (State)
            {
                case GameState.WaitingForDropTimer:
                    stateText.TextValue = "Waiting for drop timer";
                    break;
                case GameState.BlocksExploding:
                    stateText.TextValue = "Exploding";
                    break;
                case GameState.BlocksFalling:
                    stateText.TextValue = "Blocks falling";
                    break;
                case GameState.ClusterFalling:
                    stateText.TextValue = "Cluster falling";
                    break;
                case GameState.ClusterGraceTime:
                    stateText.TextValue = "Cluster collision delay";
                    break;
            }
        }

        /// <summary>
        /// Updates blocks and checks for collisions.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        protected void UpdateBlocks(GameTime gameTime)
        {
            // Sort in ascending order to ensure that the blocks below collide first
            blocks.Sort((b1,b2) => b2.Position.Y.CompareTo(b1.Position.Y));

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
        protected void UpdateExplosion(GameTime gameTime)
        {
            explosionAnimation.Update(gameTime);

            if (explosionAnimation.CurrentFrame >= explosionAnimation.NumFrames - 1)
            {
                foreach (Block block in explodingBlocks)
                    block.Dispose();
                explosionAnimation.Stop();
                scoreFloater.Stop();
                State = GameState.BlocksFalling;
            }
        }

        protected void UpdateClusters(GameTime gameTime)
        {
            CurrentCluster.Update(gameTime);
            NextCluster.Update(gameTime);
        }

        /// <summary>
        /// Returns true if a block is moving
        /// </summary>
        /// <returns></returns>
        protected bool BlocksAreMoving()
        {
            foreach (Block block in blocks)
                if (block.IsMoving)
                    return true;
            return false;
        }

        /// <summary>
        /// Checks newly stationary blocks for explosion conditions
        /// </summary>
        protected void CheckForExplosions()
        {
            if (!CurrentCluster.IsMoving && !BlocksAreMoving())
            {
                foreach (Block block in blocks)
                    if (block.ShouldBeChecked)
                        CheckBlock(block);
            }
        }

        /// <summary>
        /// Retruns true if game over conditions are met
        /// </summary>
        /// <returns></returns>
        protected bool IsGameOver()
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
        protected void ReleaseBlocks()
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
                        staticBlocks[i, j].DropSpeed = BaseDropSpeed * dropSpeedBonus;
                        staticBlocks[i, j].IsMoving = true;
                        staticBlocks[i, j] = null;
                        State = GameState.BlocksFalling;
                    }
                }
            }
        }

        /// <summary>
        /// Clears up exploded blocks.
        /// </summary>
        /// <returns>True if blocks ar</returns>
        protected bool ClearUpBlocks()
        {
            if (explodingBlocks.Count != 0)
            {
                foreach (Block block in explodingBlocks)
                    blocks.Remove(block);
                explodingBlocks.Clear();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if cluster collides with anything
        /// </summary>
        private void CheckForClusterCollision(GameTime gameTime)
        {
            if (CurrentCluster.IsMoving)
                if (ClusterHasCollided())
                {
                    // Start collision delay
                    if (firstClusterCollision)
                    {
                        State = GameState.ClusterGraceTime;
                        graceTimeTimer = 0;
                        CurrentCluster.IsMoving = false;
                        firstClusterCollision = false;

                        // Adjust positions to avoid wobble

                        int firstY = GridPositionY(CurrentCluster.FirstBlock.Position);
                        Vector2 first = GetStaticPosition(CurrentCluster.FirstBlock, firstY);

                        int secondY = GridPositionY(CurrentCluster.SecondBlock.Position);
                        Vector2 second = GetStaticPosition(CurrentCluster.SecondBlock, secondY);

                        CurrentCluster.SetPosition(first, second);
                        
                    }
                    else
                        SeparateCluster();
                }
        }

        private bool ClusterHasCollided()
        {
            return BlockHasCollided(CurrentCluster.FirstBlock) || BlockHasCollided(CurrentCluster.SecondBlock);
        }

        private void SeparateCluster()
        {
            State = GameState.BlocksFalling;
            MovementLocked = true;
            Block[] separatedCluster = CurrentCluster.Separate();
            
            if (ClusterSeparate != null)
                ClusterSeparate(this, new ClusterSeparateEventArgs(
                    separatedCluster[0].Position, separatedCluster[1].Position));

            // Avoids jittering if orientation is down
            if (CurrentCluster.Orientation != Orientation.Down)
            {
                CheckForBlockCollision(separatedCluster[0]);
                CheckForBlockCollision(separatedCluster[1]);
            }
            else
            {
                CheckForBlockCollision(separatedCluster[1]);
                CheckForBlockCollision(separatedCluster[0]);
            }

            blocks.AddRange(separatedCluster);
        }

        /// <summary>
        /// Checks if a block collides with anything
        /// </summary>
        /// <param name="block"></param>
        protected void CheckForBlockCollision(Block block)
        {
            int posX = GridPositionX(block.Position);
            int posY = GridPositionY(block.Position);

            Vector2 pos = GetStaticPosition(block, posY);

            if (BlockHasCollided(block))
            {
                block.Attach(pos);
                staticBlocks[posX, posY] = block;

                if (BlackBlockCollision != null)
                    if (block.BlockType == BlockType.Black)
                        BlackBlockCollision(this, new BlackBlockCollisionEventArgs(pos));
            }
        }

        private Vector2 GetStaticPosition(Block block, int gridPosY)
        {
            return new Vector2(block.Position.X, Position.Y + 0.5f * Height - (gridPosY + 0.5f) * blockSize + 0.005f);
        }

        /// <summary>
        /// Determines if a moving block collides with the bottom or a static block.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        protected bool BlockHasCollided(Block block)
        {
            int posX = GridPositionX(block.Position);
            int posY = GridPositionY(block.Position);
            //bottom collision (left) & block collision (right)
            return block.Position.Y >= Position.Y + 0.5 * (Height - blockSize) || staticBlocks[posX, posY - 1] != null;
        }

        /// <summary>
        /// Calculates grid position x coordinate. Left edge is 0.
        /// </summary>
        /// <param name="blockPosition"></param>
        /// <returns></returns>
        protected int GridPositionX(Vector2 blockPosition)
        {
            float ret = (staticBlocks.GetLength(0) + (blockPosition.X - this.Position.X - 0.5f * Width) / blockSize);
            if (ret < 0)
                return -1;
            return (int)ret;
        }

        /// <summary>
        /// Calculates grid position y coordinate. Bottom is 0.
        /// </summary>
        /// <param name="blockPosition"></param>
        /// <returns></returns>
        protected int GridPositionY(Vector2 blockPosition)
        {
            return (int)(0.5f + (this.Position.Y + 0.5f * Height - blockPosition.Y) / blockSize);
        }

        /// <summary>
        /// Determines number of blocks of same color connected to and explodes them if needed
        /// </summary>
        private void CheckBlock(Block block)
        {
            if (block.BlockType != BlockType.Black)
            {
                List<Block> sameColorChain = new List<Block>();
                sameColorChain = CheckForChain(block, block.BlockType, sameColorChain);

                int notBlackCount = sameColorChain.FindAll(b => b.BlockType != BlockType.Black).Count;

                if (notBlackCount > 3)
                    ExplodeChain(sameColorChain);
            }
        }

        /// <summary>
        /// Adds block if of right type and recursively checks neighbours
        /// </summary>
        /// <param name="block"></param>
        /// <param name="type"></param>
        /// <param name="chain"></param>
        /// <returns></returns>
        private List<Block> CheckForChain(Block block, BlockType type, List<Block> chain)
        {
            if (block != null && !chain.Contains(block))
            {
                if (block.BlockType == type || block.BlockType == BlockType.Black)
                {
                    chain.Add(block);

                    if (block.BlockType != BlockType.Black)
                    {
                        int blockX = GridPositionX(block.Position);
                        int blockY = GridPositionY(block.Position);
                        if (blockX - 1 >= 0)
                            chain = CheckForChain(staticBlocks[blockX - 1, blockY], type, chain);
                        if (blockX + 1 < staticBlocks.GetLength(0))
                            chain = CheckForChain(staticBlocks[blockX + 1, blockY], type, chain);
                        if (blockY - 1 >= 0)
                            chain = CheckForChain(staticBlocks[blockX, blockY - 1], type, chain);
                        if (blockY + 1 < staticBlocks.GetLength(1))
                            chain = CheckForChain(staticBlocks[blockX, blockY + 1], type, chain);
                    }
                }
            }
            return chain;
        }

        private void ExplodeChain(List<Block> chain)
        {
            State = GameState.BlocksExploding;

            MovementLocked = true;
            explosionAnimation.Play();

            float oldScore = scoreCounter.Score;
            Vector2 avgPos = Vector2.Zero;
            foreach (Block clearedBlock in chain)
            {
                avgPos += clearedBlock.Position;
                ExplodeBlockAndScore(clearedBlock);
            }
            avgPos /= chain.Count;
            scoreFloater.Start(scoreCounter.Score - oldScore, avgPos);

            scoreMultiplier++;
            if (scoreMultiplier > largestCombo)
            {
                largestCombo = scoreMultiplier - 1;
                largestComboText.TextValue = "Max Combo: " + largestCombo;
            }

            if (ShouldDropBlackBlocks != null)
                ShouldDropBlackBlocks(this, new ShouldDropBlackBlocksEventArgs(scoreMultiplier - 1));

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
        private void ExplodeBlockAndScore(Block block)
        {
            int blockX = GridPositionX(block.Position);
            int blockY = GridPositionY(block.Position);
            staticBlocks[blockX, blockY] = null;
            block.Explode();
            explodingBlocks.Add(block);

            if (block.BlockType != BlockType.Black)
                scoreCounter.Score += 50 * scoreMultiplier * SpeedMultiplier;
        }

        /// <summary>
        /// Adds black blocks to be dropped
        /// </summary>
        /// <param name="count"></param>
        public void QueueBlackBlocks(int count)
        {
            BlackBlocksQueued += count;
        }

        /// <summary>
        /// Creates black blocks at top and drops them
        /// </summary>
        private void CreateBlackBlocks()
        {
            int numToAdd = Math.Min(staticBlocks.GetLength(0), BlackBlocksQueued);

            var positionsTaken = new List<int>();
           

            for (int i = 0; i < numToAdd; i++)
            {
                int gridIndex;
                do
                    gridIndex = PuzzleGame.r.Next(0, staticBlocks.GetLength(0));
                while (positionsTaken.Contains(gridIndex));

                if (!positionsTaken.Contains(gridIndex))
                {
                    positionsTaken.Add(gridIndex);
                    blocks.Add(CreateBlackBlock(gridIndex));
                }
            }

            if (BlackBlocksCreated != null && positionsTaken.Count > 0)
                BlackBlocksCreated(this, new BlackBlocksCreatedEventArgs(positionsTaken));

            BlackBlocksQueued -= numToAdd;
        }

        protected Block CreateBlackBlock(int gridIndex)
        {
            Block blackBlock = new Block(BlockType.Black,
                        GridOrigin() + new Vector2(gridIndex * blockSize, 0), blockSize);
            blackBlock.IsMoving = true;
            blackBlock.DropSpeed = BaseDropSpeed * dropSpeedBonus;
            return blackBlock;
        }

        private Vector2 GridOrigin()
        {
            int stepsToOrigin = (staticBlocks.GetLength(0) - 1) / 2;
            return this.Position - new Vector2(stepsToOrigin * blockSize, (Height - blockSize) * 0.5f);
        }

        public void StartGame()
        {
            UnPause();
        }

        public void Pause()
        {
            IsPaused = true;
            MovementLocked = true;
        }

        public void UnPause()
        {
            IsPaused = false;
            MovementLocked = false;
        }

        public virtual void EndGame()
        {
            Pause();
            if (GameOver != null)
                GameOver(this, EventArgs.Empty);
        }

        public void SetDebugInfoVisibility(bool isShowing)
        {
            stateText.IsShowing = isShowing;
        }

        #region Commands
        /// <summary>
        /// Move left command
        /// </summary>
        public void MoveClusterLeft(GameTime gameTime, bool first)
        {
            if (!MovementLocked)
            {
                int firstBlockX = GridPositionX(CurrentCluster.FirstBlock.Position);
                int firstBlockY = GridPositionY(CurrentCluster.FirstBlock.Position);
                int secondBlockX = GridPositionX(CurrentCluster.SecondBlock.Position);
                int secondBlockY = GridPositionY(CurrentCluster.SecondBlock.Position);

                if (!MoveCollision(firstBlockX - 1, secondBlockX - 1, firstBlockY, secondBlockY))
                {
                    if (first || moveLeftTimer > moveCooldown)
                    {
                        CurrentCluster.Move(CurrentCluster.FirstBlock.Position - new Vector2(blockSize, 0));
                        moveLeftTimer = 0;
                    }
                    else
                        moveLeftTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }

                CurrentCluster.IsMoving = true;
            }
        }

        /// <summary>
        /// Move right command
        /// </summary>
        public void MoveClusterRight(GameTime gameTime, bool first)
        {
            if (!MovementLocked)
            {
                int firstBlockX = GridPositionX(CurrentCluster.FirstBlock.Position);
                int firstBlockY = GridPositionY(CurrentCluster.FirstBlock.Position);
                int secondBlockX = GridPositionX(CurrentCluster.SecondBlock.Position);
                int secondBlockY = GridPositionY(CurrentCluster.SecondBlock.Position);

                if (!MoveCollision(firstBlockX + 1, secondBlockX + 1, firstBlockY, secondBlockY))
                {
                    if (first || moveRightTimer > moveCooldown)
                    {
                        CurrentCluster.Move(CurrentCluster.FirstBlock.Position + new Vector2(blockSize, 0));
                        moveRightTimer = 0;
                    }
                    else
                        moveRightTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }

                CurrentCluster.IsMoving = true;
            }
        }

        private bool MoveCollision(int firstX, int secondX, int firstY, int secondY)
        {
            if (firstX < 0 || secondX < 0)
                return true;
            if (firstX >= staticBlocks.GetLength(0) || secondX >= staticBlocks.GetLength(0))
                return true;

            if (firstY == 0)
            {
                if (staticBlocks[firstX, firstY] != null)
                    return true;
            }
            else
            {
                if (State == GameState.ClusterGraceTime)
                {
                    if (staticBlocks[firstX, firstY] != null)
                        return true;
                }
                else
                if (staticBlocks[firstX, firstY - 1] != null)
                    return true;
            }

            if (secondY == 0)
            {
                if (staticBlocks[secondX, secondY] != null)
                    return true;
            }
            else
            {
                if (State == GameState.ClusterGraceTime)
                {
                    if (staticBlocks[secondX, secondY] != null)
                        return true;
                }
                else
                    if (staticBlocks[secondX, secondY - 1] != null)
                        return true;
            }

            return false;
        }

        /// <summary>
        /// Rotate command
        /// </summary>
        public void RotateCluster()
        {
            if (!MovementLocked)
            {
                int firstBlockX = GridPositionX(CurrentCluster.FirstBlock.Position);
                int firstBlockY = GridPositionY(CurrentCluster.FirstBlock.Position);

                if (CurrentCluster.Orientation == Orientation.Down)
                {
                    if (firstBlockX - 1 >= 0 && staticBlocks[firstBlockX - 1, firstBlockY - 1] == null)
                        CurrentCluster.Rotate(true);
                    else if (CurrentCluster.FirstBlock.BlockType == CurrentCluster.SecondBlock.BlockType)
                    {
                        CurrentCluster.Invert();
                        RotateCluster();
                    }
                    else
                        CurrentCluster.Invert();
                }
                else if (CurrentCluster.Orientation == Orientation.Left)
                {
                    if (firstBlockY + 1 <= staticBlocks.GetLength(1))
                        CurrentCluster.Rotate(true);
                }
                else if (CurrentCluster.Orientation == Orientation.Up)
                {
                    if (firstBlockX + 1 < staticBlocks.GetLength(0) && staticBlocks[firstBlockX + 1, firstBlockY] == null)
                        CurrentCluster.Rotate(true);
                    else if (CurrentCluster.FirstBlock.BlockType == CurrentCluster.SecondBlock.BlockType)
                    {
                        CurrentCluster.Invert();
                        RotateCluster();
                    }
                    else
                        CurrentCluster.Invert();
                }
                else if (CurrentCluster.Orientation == Orientation.Right)
                {
                    if (firstBlockY - 1 > 0 && staticBlocks[firstBlockX, firstBlockY - 2] == null)
                        CurrentCluster.Rotate(true);
                    else
                        CurrentCluster.Rotate(false);
                }

                CurrentCluster.IsMoving = true;
            }
        }

        /// <summary>
        /// Move down command
        /// </summary>
        public void MoveClusterDown(GameTime gameTime)
        {
            if (!MovementLocked)
                DropClusterFast(gameTime);
        }
        #endregion

        /// <summary>
        /// Drops a cluster instantly.
        /// </summary>
        /// <param name="gameTime"></param>
        private void DropClusterFast(GameTime gameTime)
        {
            Vector2 firstPos;
            Vector2 secondPos;

            if (CurrentCluster.Orientation == Orientation.Up)
            {
                int firstX = GridPositionX(CurrentCluster.FirstBlock.Position);
                firstPos = GetAvaliablePositionInColumn(firstX);
                secondPos = firstPos - new Vector2(0, blockSize);
                CurrentCluster.SetPosition(firstPos, secondPos);
            }
            else if (CurrentCluster.Orientation == Orientation.Down)
            {
                int secondX = GridPositionX(CurrentCluster.SecondBlock.Position);
                secondPos = GetAvaliablePositionInColumn(secondX);
                firstPos = secondPos - new Vector2(0, blockSize);
                CurrentCluster.SetPosition(firstPos, secondPos);
            }
            else
            {
                int firstX = GridPositionX(CurrentCluster.FirstBlock.Position);
                int secondX = GridPositionX(CurrentCluster.SecondBlock.Position);
                firstPos = GetAvaliablePositionInColumn(firstX);
                secondPos = GetAvaliablePositionInColumn(secondX);
            }

            CurrentCluster.SetPosition(firstPos, secondPos);
            SeparateCluster();
        }

        private Vector2 GetAvaliablePositionInColumn(int column)
        {
            for (int i = 0; i < staticBlocks.GetLength(1); i++)
            {
                if (staticBlocks[column, i] == null)
                {
                    if (i == 0)
                        return GetPositionFromIndex(column, 0);
                    else
                        return GetPositionFromIndex(column, i);
                }
            }

            throw new Exception("No empty position exception");
        }

        protected Vector2 GetPositionFromIndex(int x, int y)
        {
            return Position + 0.5f * new Vector2(-Width, Height) + blockSize * new Vector2(x + 0.5f, -y - 0.5f);
        }

        public virtual void DropNextCluster()
        {
            CurrentCluster = NextCluster;
            CurrentCluster.Move(Position - new Vector2(0f, (Height - 3f * blockSize) * 0.5f));
            CurrentCluster.IsMoving = false;
            NextCluster = null;
            CreateNextCluster();
            MovementLocked = false;
            scoreMultiplier = 1;
            dropTimer = 0;
            State = GameState.WaitingForDropTimer;
            firstClusterCollision = true;
            
            if (ClusterDrop != null)
                ClusterDrop(this, new EventArgs());
        }

        private void CreateNextCluster()
        {
            NextCluster = new Cluster(Position - new Vector2(250f, 175f), blockSize);
            NextCluster.Initialize();
            NextCluster.IsMoving = false;
            if (NewNextCluster != null && !IsPaused)
                NewNextCluster(this, new NewNextClusterEventArgs((int)NextCluster.FirstBlock.BlockType, (int)NextCluster.SecondBlock.BlockType));
        }

        public void Draw(GameTime gameTime)
        {
            if (IsShowing)
            {
                stateText.Draw(gameTime);

                backgroundImage.Draw(gameTime);
                cutoffLine.Draw(gameTime);
                scoreCounter.Draw(gameTime);
                scoreFloater.Draw(gameTime);
                largestComboText.Draw(gameTime);

                if (CurrentCluster != null)
                {
                    if (!CurrentCluster.FirstBlock.IsExploding)
                        DrawBlock(CurrentCluster.FirstBlock, gameTime);
                    if (!CurrentCluster.SecondBlock.IsExploding)
                        DrawBlock(CurrentCluster.SecondBlock, gameTime);
                }

                if (NextCluster != null)
                {
                    DrawBlock(NextCluster.FirstBlock, gameTime);
                    DrawBlock(NextCluster.SecondBlock, gameTime);
                }

                foreach (Block block in blocks)
                    if (block.IsMoving || block.IsExploding)
                        DrawBlock(block, gameTime);

                foreach (Block block in staticBlocks)
                    if (block != null)
                        DrawBlock(block, gameTime);
            }
        }

        private void DrawBlock(Block block, GameTime gameTime)
        {
            if (!block.IsDisposing)
                block.Draw(gameTime, GetImage(block));
        }

        private Image GetImage(Block block)
        {
            if (block.IsExploding)
                return explosionAnimation;
            return blockImages[(int)block.BlockType];
        }
    }

    public enum GameType
    {
        Normal,
        Time,
    }

    public enum GameState
    {
        WaitingForDropTimer,
        ClusterFalling,
        BlocksFalling,
        BlocksExploding,
        ClusterGraceTime,
    }

    public delegate void GameOverEventHandler(object o, EventArgs e);
    public delegate void NewNextClusterEventHandler(object o, NewNextClusterEventArgs e);
    public delegate void ClusterDropEventHandler(object o, EventArgs e);
    public delegate void ClusterSeparateEventHandler(object o, ClusterSeparateEventArgs e);
    public delegate void BlackBlocksCreatedEventHandler(object o, BlackBlocksCreatedEventArgs e);
    public delegate void ShouldDropBlackBlocksEventHandler(object o, ShouldDropBlackBlocksEventArgs e);
    public delegate void BlackBlockCollisionEventHandler(object o, BlackBlockCollisionEventArgs e);

    public class NewNextClusterEventArgs : EventArgs
    {
        private int firstBlock;
        private int secondBlock;

        public NewNextClusterEventArgs(int firstBlock, int secondBlock) 
        {
            this.firstBlock = firstBlock;
            this.secondBlock = secondBlock;
        }

        public int FirstBlock { get { return firstBlock; } }
        public int SecondBlock { get { return secondBlock; } }
    }

    public class ClusterSeparateEventArgs : EventArgs
    {
        private Vector2 firstBlockPos;
        private Vector2 secondBlockPos;

        public ClusterSeparateEventArgs(Vector2 firstBlockPos, Vector2 secondBlockPos)
        {
            this.firstBlockPos = firstBlockPos;
            this.secondBlockPos = secondBlockPos;
        }

        public Vector2 FirstBlockPosition { get { return firstBlockPos; } }
        public Vector2 SecondBlockPosition { get { return secondBlockPos; } }
    }

    public class BlackBlocksCreatedEventArgs : EventArgs
    {
        public List<int> Indexes { get; set; }

        public BlackBlocksCreatedEventArgs(List<int> indexes)
        {
            Indexes = indexes;
        }
    }

    public class ShouldDropBlackBlocksEventArgs : EventArgs
    {
        public int NumBlocks { get; set; }

        public ShouldDropBlackBlocksEventArgs(int number)
        {
            NumBlocks = number;
        }
    }

    public class BlackBlockCollisionEventArgs : EventArgs
    {
        public Vector2 Position { get; set; }

        public BlackBlockCollisionEventArgs(Vector2 position)
        {
            Position = position;
        }
    }
}
