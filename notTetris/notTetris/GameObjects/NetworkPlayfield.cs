using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NotTetris.Graphics;

namespace NotTetris.GameObjects
{
    class NetworkPlayfield : Playfield
    {
        public bool WaitingForCluster { get; set; }
        public bool WaitForBlackBlocks { get; set; }

        public NetworkPlayfield(GameType gameType, Vector2 position, int sizeX) : base(gameType, position, sizeX) { }

        private List<int> blackBlockQueue;
        private List<Block> movingBlackBlocks;

        public override void Initialize(SpriteBatch spriteBatch, string difficulty)
        {
            WaitingForCluster = false;
            WaitForBlackBlocks = false;
            blackBlockQueue = new List<int>();
            movingBlackBlocks = new List<Block>();

            base.InitializeContent(spriteBatch, difficulty);
        }

        /// <summary>
        /// Does not need to check if cluster has collided as that is done remotely
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
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
                if (!explosionAnimation.IsStarted && !waitForDropTimer)
                {
                    if (ClearUpBlocks())
                        ReleaseBlocks();

                    UpdateBlocks(gameTime);

                    foreach (Block block in movingBlackBlocks)
                        block.Update(gameTime);

                    UpdateClusters(gameTime);

                    CheckForExplosions();

                    if (!BlocksAreMoving() && !currentCluster.IsMoving && explodingBlocks.Count == 0)
                    {
                        if (IsGameOver())
                            EndGame();
                        else if (!WaitForBlackBlocks)
                            DropNextCluster();
                        else if (blackBlockQueue.Count > 0)
                            CreateBlackBlocks();
                        else if (blackBlockQueue.Count == 0 && WaitForBlackBlocks)
                            WaitForBlackBlocks = false;
                    }
                }
                else
                {
                    UpdateExplosion(gameTime);
                    scoreFloater.Update(gameTime);
                }
            }
        }

        public void AddBlackBlocks(List<int> indexes)
        {
            blackBlockQueue.AddRange(indexes);
        }

        /// <summary>
        /// Creates black blocks at the indexes specified in the queue, max one for each x position
        /// </summary>
        protected override void CreateBlackBlocks()
        {
            int stepsToOrigin = (staticBlocks.GetLength(0) - 1) / 2;
            Vector2 gridOrigin = this.position - new Vector2(stepsToOrigin * blockSize, (Height - blockSize) * 0.5f);

            List<int> indexesToAdd = new List<int>();

            foreach (int i in blackBlockQueue)
                if (!indexesToAdd.Contains(i))
                    indexesToAdd.Add(i);

            foreach (int i in indexesToAdd)
            {
                blackBlockQueue.Remove(i);
                Block blackBlock = new Block(BlockType.Black,
                        gridOrigin + new Vector2(i * blockSize, 0), blockSize);
                blackBlock.IsMoving = true;
                blackBlock.DropSpeed = BaseDropSpeed * dropSpeedBonus;
                movingBlackBlocks.Add(blackBlock);
            }

            if (blackBlockQueue.Count == 0)
                WaitForBlackBlocks = false;
        }

        /// <summary>
        /// Moves current to position and separates according to position from remote
        /// </summary>
        /// <param name="firstBlock"></param>
        /// <param name="secondBlock"></param>
        public void MoveAndSeparate(Vector2 firstBlock, Vector2 secondBlock)
        {
            movementLocked = true;
            currentCluster.FirstBlock.Position = firstBlock;
            currentCluster.SecondBlock.Position = secondBlock;
            currentCluster.SetDropSpeed(BaseDropSpeed * dropSpeedBonus);
            CheckForBlockCollision(currentCluster.FirstBlock);
            CheckForBlockCollision(currentCluster.SecondBlock);
            blocks.AddRange(currentCluster.Separate());
        }

        public override void EndGame()
        {
            Pause();
        }

        /// <summary>
        /// Does not create next cluster. Instead waits for one to be sent from remote
        /// </summary>
        public override void DropNextCluster()
        {
            currentCluster = nextCluster;
            currentCluster.Move(position - new Vector2(0f, (Height - 3f * blockSize) * 0.5f));
            currentCluster.IsMoving = true;
            currentCluster.SetDropSpeed(BaseDropSpeed * SpeedMultiplier);
            movementLocked = false;
            nextCluster = null;
            scoreMultiplier = 1;
            WaitingForCluster = true;
            dropTimer = 0;
            waitForDropTimer = true;
        }

        /// <summary>
        /// Creates next cluster according to blocktype from remote
        /// </summary>
        /// <param name="firstBlock"></param>
        /// <param name="secondBlock"></param>
        public void CreateNextCluster(BlockType firstBlock, BlockType secondBlock)
        {
            nextCluster = new Cluster(position - new Vector2(250f, 175f), blockSize);
            nextCluster.Initialize();
            nextCluster.IsMoving = false;
            nextCluster.FirstBlock.BlockType = firstBlock;
            nextCluster.SecondBlock.BlockType = secondBlock;
            WaitingForCluster = false;
        }
    }
}
