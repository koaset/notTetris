using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NotTetris.Graphics;

namespace NotTetris.GameObjects
{
    public delegate void GameOverEventHandler(object o, EventArgs e);
    public delegate void NewNextClusterEventHandler(object o, NewNextClusterEventArgs e);
    public delegate void ClusterDropEventHandler(object o, EventArgs e);
    public delegate void ClusterSeparateEventHandler(object o, ClusterSeparateEventArgs e);
    public delegate void ShouldDropBlackBlocksEventHandler(object o, ShouldDropBlackBlocksEventArgs e);
    public delegate void BlackBlockColiisionEventHandler(object o, BlackBlockCollision e);

    public enum GameType
    {
        Normal,
        Time,
    }

    public enum GameState
    {
        ClusterFalling,
        BlocksFalling,
        BlocksExploding,
    }

    class Playfield
    {
        public event GameOverEventHandler GameOver;
        public event NewNextClusterEventHandler NewNextCluster;
        public event ClusterDropEventHandler ClusterDrop;
        public event ClusterSeparateEventHandler ClusterSeparate;
        public event ShouldDropBlackBlocksEventHandler ShouldDropBlackBlocks;
        public event BlackBlockColiisionEventHandler BlackBlockCollision;

        private GameType gameType;
        protected Vector2 position;
        SpriteBatch spriteBatch;
        Image backgroundImage;
        Image cutoffLine;
        Image[] blockImages;
        protected Animation explosionAnimation;
        ScoreCounter scoreCounter;
        protected float blockSize;
        Vector2 scale;
        protected List<Block> blocks;
        protected List<Block> explodingBlocks;
        protected Cluster currentCluster;
        protected Cluster nextCluster;
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
        protected bool waitForDropTimer;

        Text stateText;

        public GameState State { get; set; }
        public Vector2 Position { get { return position; } }
        public Cluster NextCluster { get { return nextCluster; } set { nextCluster = value; } }
        public Cluster CurrentCluster { get { return currentCluster; } set { currentCluster = value; } }
        public float SpeedMultiplier { get; set; }
        public float BaseDropSpeed { get; set; }
        public static float Width { get { return 350f; } }
        public static float Height { get { return 500f; } }
        public bool movementLocked { get; set; }
        public bool IsPaused { get; set; }
        public bool IsShowing { get; set; }
        public float GetScore { get { return scoreCounter.Score; } set { scoreCounter.Score = value; } }
        public int BlackBlocksQueued { get; set; }

        public Playfield(GameType gameType, Vector2 position, int sizeX)
        {
            this.gameType = gameType;
            this.position = position;
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
            movementLocked = true;
        }

        protected void InitializeContent(SpriteBatch spriteBatch, string difficulty)
        {
            stateText.Initialize();
            stateText.Position = new Vector2(this.position.X - 175, 0f);
            stateText.Font = FontNames.Segoe_UI_Mono;
            stateText.TextValue = "Init";
            stateText.IsShowing = false;

            this.spriteBatch = spriteBatch;
            movementLocked = true;
            IsPaused = true;

            dropTimer = 0;
            dropDelay = 500;
            moveRightTimer = 0;
            moveLeftTimer = 0;
            moveCooldown = 80;
            dropSpeedBonus = 2.5f;

            #region Initialize images, blocks and text
            backgroundImage.Initialize();
            backgroundImage.TextureName = TextureNames.playfieldbackground_yellow;
            backgroundImage.Position = position;
            backgroundImage.Size = new Vector2(Width + 20, Height + 20);

            cutoffLine.Initialize();
            cutoffLine.TextureName = TextureNames.red_line;
            cutoffLine.Position = new Vector2(position.X, position.Y + Height * 0.5f - blockSize * (staticBlocks.GetLength(1) - 3));
            cutoffLine.Size = new Vector2(Width, 2f);
            cutoffLine.Layer = 1.0f;
            scoreCounter.Initialize();
            scoreCounter.Position = new Vector2(position.X - 150, (position.Y - 25f) * 2.0f);


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

        public void LoadContent()
        {
            backgroundImage.LoadContent(spriteBatch);
            cutoffLine.LoadContent(spriteBatch);
            scoreCounter.LoadContent(spriteBatch);
            scoreFloater.LoadContent(spriteBatch);
            largestComboText.LoadContent(spriteBatch);

            for (int i = 0; i < blockImages.Length; i++)
                blockImages[i].LoadContent(spriteBatch);
            explosionAnimation.LoadContent(spriteBatch);

            stateText.LoadContent(spriteBatch);
        }

        public virtual void Update(GameTime gameTime)
        {
            if (!IsPaused)
            {
                if (waitForDropTimer)
                {
                    dropTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (dropTimer > dropDelay)
                    {
                        currentCluster.IsMoving = true;
                        currentCluster.SetDropSpeed(BaseDropSpeed * SpeedMultiplier);
                        waitForDropTimer = false;
                    }
                }
                else if (!explosionAnimation.IsStarted && !waitForDropTimer)
                {
                    if (ClearUpBlocks())
                        ReleaseBlocks();

                    UpdateBlocks(gameTime);
                    UpdateClusters(gameTime);

                    CheckForClusterCollision(gameTime);

                    CheckForExplosions();

                    if (!BlocksAreMoving() && !currentCluster.IsMoving && explodingBlocks.Count == 0)
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
                else
                {
                    UpdateExplosion(gameTime);
                    scoreFloater.Update(gameTime);
                }
            }

            UpdateStateText();
        }

        protected void UpdateStateText()
        {
            switch (State)
            {
                case GameState.BlocksExploding:
                    stateText.TextValue = "Exploding";
                    break;
                case GameState.BlocksFalling:
                    stateText.TextValue = "Blocks falling";
                    break;
                case GameState.ClusterFalling:
                    stateText.TextValue = "Cluster falling";
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
            }
        }

        protected void UpdateClusters(GameTime gameTime)
        {
            currentCluster.Update(gameTime);
            nextCluster.Update(gameTime);
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
            if (!currentCluster.IsMoving && !BlocksAreMoving())
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
            if (currentCluster.IsMoving)
                if (BlockHasCollided(currentCluster.FirstBlock) || BlockHasCollided(currentCluster.SecondBlock))
                {
                    movementLocked = true;
                    currentCluster.SetDropSpeed(BaseDropSpeed * dropSpeedBonus);
                    Block[] separatedCluster = currentCluster.Separate();

                    State = GameState.BlocksFalling;
                    if (ClusterSeparate != null)
                        ClusterSeparate(this, new ClusterSeparateEventArgs(separatedCluster[0].Position, separatedCluster[1].Position));

                    CheckForBlockCollision(separatedCluster[0]);
                    CheckForBlockCollision(separatedCluster[1]);
                    blocks.AddRange(separatedCluster);
                }
        }

        /// <summary>
        /// Checks if a block collides with anything
        /// </summary>
        /// <param name="block"></param>
        protected void CheckForBlockCollision(Block block)
        {
            int posX = GridPositionX(block.Position);
            int posY = GridPositionY(block.Position);

            Vector2 pos = new Vector2(block.Position.X, position.Y + 0.5f * Height - (posY + 0.5f) * blockSize + 0.005f);

            if (BlockHasCollided(block))
            {
                block.Attach(pos);
                staticBlocks[posX, posY] = block;

                if (BlackBlockCollision != null)
                    if (block.BlockType == BlockType.Black)
                        BlackBlockCollision(this, new BlackBlockCollision(posX, posY));
            }
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

            movementLocked = true;
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
        protected virtual void CreateBlackBlocks()
        {
            int numToAdd = Math.Min(staticBlocks.GetLength(0), BlackBlocksQueued);

            var positionsTaken = new List<int>();
            int stepsToOrigin = (staticBlocks.GetLength(0) - 1) / 2;
            Vector2 gridOrigin = this.position - new Vector2(stepsToOrigin * blockSize, (Height - blockSize) * 0.5f);

            for (int i = 0; i < numToAdd; i++)
            {
                int gridIndex;
                do
                    gridIndex = PuzzleGame.r.Next(0, staticBlocks.GetLength(0));
                while (positionsTaken.Contains(gridIndex));

                if (!positionsTaken.Contains(gridIndex))
                {
                    positionsTaken.Add(gridIndex);
                    Block blackBlock = new Block(BlockType.Black,
                        gridOrigin + new Vector2(gridIndex * blockSize, 0), blockSize);
                    blackBlock.IsMoving = true;
                    blackBlock.DropSpeed = BaseDropSpeed * dropSpeedBonus;
                    blocks.Add(blackBlock);
                }
            }

            BlackBlocksQueued -= numToAdd;
        }

        public void StartGame()
        {
            UnPause();
        }

        public void Pause()
        {
            IsPaused = true;
            movementLocked = true;
        }

        public void UnPause()
        {
            IsPaused = false;
            movementLocked = false;
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
        public virtual void MoveClusterLeft(GameTime gameTime, bool first)
        {
            if (!movementLocked)
            {
                int firstBlockX = GridPositionX(currentCluster.FirstBlock.Position);
                int firstBlockY = GridPositionY(currentCluster.FirstBlock.Position);
                int secondBlockX = GridPositionX(currentCluster.SecondBlock.Position);
                int secondBlockY = GridPositionY(currentCluster.SecondBlock.Position);

                bool collision = false;
                if (firstBlockX == 0 || secondBlockX == 0)
                    collision = true;
                else if (staticBlocks[firstBlockX - 1, firstBlockY - 1] != null || staticBlocks[secondBlockX - 1, secondBlockY - 1] != null)
                    collision = true;

                if (!collision)
                {
                    if (first || moveLeftTimer > moveCooldown)
                    {
                        currentCluster.Move(currentCluster.FirstBlock.Position - new Vector2(blockSize, 0));
                        moveLeftTimer = 0;
                    }
                    else
                        moveLeftTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
            }
        }

        /// <summary>
        /// Move right command
        /// </summary>
        public virtual void MoveClusterRight(GameTime gameTime, bool first)
        {
            if (!movementLocked)
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
                {
                    if (first || moveRightTimer > moveCooldown)
                    {
                        currentCluster.Move(currentCluster.FirstBlock.Position + new Vector2(blockSize, 0));
                        moveRightTimer = 0;
                    }
                    else
                        moveRightTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
            }
        }

        /// <summary>
        /// Rotate command
        /// </summary>
        public void RotateCluster()
        {
            if (!movementLocked)
            {
                int firstBlockX = GridPositionX(currentCluster.FirstBlock.Position);
                int firstBlockY = GridPositionY(currentCluster.FirstBlock.Position);

                if (currentCluster.Orientation == Orientation.Down)
                {
                    if (firstBlockX - 1 >= 0 && staticBlocks[firstBlockX - 1, firstBlockY - 1] == null)
                        currentCluster.Rotate(true);
                    else if (currentCluster.FirstBlock.BlockType == currentCluster.SecondBlock.BlockType)
                    {
                        currentCluster.Invert();
                        RotateCluster();
                    }
                    else
                        currentCluster.Invert();
                }
                else if (currentCluster.Orientation == Orientation.Left)
                {
                    if (firstBlockY + 1 <= staticBlocks.GetLength(1))
                        currentCluster.Rotate(true);
                }
                else if (currentCluster.Orientation == Orientation.Up)
                {
                    if (firstBlockX + 1 < staticBlocks.GetLength(0) && staticBlocks[firstBlockX + 1, firstBlockY - 1] == null)
                        currentCluster.Rotate(true);
                    else if (currentCluster.FirstBlock.BlockType == currentCluster.SecondBlock.BlockType)
                    {
                        currentCluster.Invert();
                        RotateCluster();
                    }
                    else
                        currentCluster.Invert();
                }
                else if (currentCluster.Orientation == Orientation.Right)
                {
                    if (firstBlockY - 1 > 0 && staticBlocks[firstBlockX, firstBlockY - 2] == null)
                        currentCluster.Rotate(true);
                    else
                        currentCluster.Rotate(false);
                }
            }
        }

        /// <summary>
        /// Move down command
        /// </summary>
        public void MoveClusterDown(GameTime gameTime)
        {
            if (!movementLocked)
                DropClusterFast(gameTime);
        }
        #endregion

        private void DropClusterFast(GameTime gameTime)
        {
            waitForDropTimer = false;
            currentCluster.IsMoving = true;
            if (currentCluster.Orientation == Orientation.Up)
            {
                int firstX = GridPositionX(currentCluster.FirstBlock.Position);
                Vector2 firstPos = GetAvaliablePositionInColumn(firstX);
                Vector2 secondPos = firstPos - new Vector2(0, blockSize);
                currentCluster.FirstBlock.Position = firstPos;
                currentCluster.SecondBlock.Position = secondPos;
            }
            else if (currentCluster.Orientation == Orientation.Down)
            {
                int secondX = GridPositionX(currentCluster.SecondBlock.Position);
                Vector2 secondPos = GetAvaliablePositionInColumn(secondX);
                Vector2 firstPos = secondPos - new Vector2(0, blockSize);
                currentCluster.FirstBlock.Position = firstPos;
                currentCluster.SecondBlock.Position = secondPos;
            }
            else
            {
                int firstX = GridPositionX(currentCluster.FirstBlock.Position);
                int secondX = GridPositionX(currentCluster.SecondBlock.Position);
                Vector2 firstPos = GetAvaliablePositionInColumn(firstX);
                Vector2 secondPos = GetAvaliablePositionInColumn(secondX);
                currentCluster.FirstBlock.Position = firstPos;
                currentCluster.SecondBlock.Position = secondPos;
            }
            currentCluster.Update(gameTime);
            CheckForClusterCollision(gameTime);
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
            return position + 0.5f * new Vector2(-Width, Height) + blockSize * new Vector2(x + 0.5f, -y - 0.5f);
        }

        public virtual void DropNextCluster()
        {
            currentCluster = nextCluster;
            currentCluster.Move(position - new Vector2(0f, (Height - 3f * blockSize) * 0.5f));
            currentCluster.IsMoving = false;
            nextCluster = null;
            CreateNextCluster();
            movementLocked = false;
            scoreMultiplier = 1;
            dropTimer = 0;
            waitForDropTimer = true;
            State = GameState.ClusterFalling;
            
            if (ClusterDrop != null)
                ClusterDrop(this, new EventArgs());
        }

        private void CreateNextCluster()
        {
            nextCluster = new Cluster(position - new Vector2(250f, 175f), blockSize);
            nextCluster.Initialize();
            nextCluster.IsMoving = false;
            if (NewNextCluster != null && !IsPaused)
                NewNextCluster(this, new NewNextClusterEventArgs((int)nextCluster.FirstBlock.BlockType, (int)nextCluster.SecondBlock.BlockType));
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
                if (currentCluster != null)
                {
                    if (!currentCluster.FirstBlock.IsExploding)
                        DrawBlock(currentCluster.FirstBlock, gameTime);
                    if (!currentCluster.SecondBlock.IsExploding)
                        DrawBlock(currentCluster.SecondBlock, gameTime);
                }
                if (nextCluster != null)
                {
                    DrawBlock(nextCluster.FirstBlock, gameTime);
                    DrawBlock(nextCluster.SecondBlock, gameTime);
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

    public class ShouldDropBlackBlocksEventArgs : EventArgs
    {
        public int NumBlocks { get; set; }

        public ShouldDropBlackBlocksEventArgs(int number)
        {
            NumBlocks = number;
        }
    }

    public class BlackBlockCollision : EventArgs
    {
        public int GridPosX { get; set; }
        public int GridPosY { get; set; }

        public BlackBlockCollision(int gridPosX, int gridPosY)
        {
            GridPosX = gridPosX;
            GridPosY = gridPosY;
        }
    }
}
