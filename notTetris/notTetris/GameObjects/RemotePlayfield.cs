using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NotTetris.Graphics;

namespace NotTetris.GameObjects
{
    /// <summary>
    /// The playfield used for displaying the opponents playfield during network play.
    /// </summary>
    class RemotePlayfield : Playfield
    {
        public bool WaitingForCluster { get; set; }
        public bool WaitingForBlackBlocks { get; set; }

        public RemotePlayfield(GameType gameType, Vector2 position, int sizeX) 
            : base(gameType, position, sizeX) { }

        private Block[] movingBlackBlocks;

        public override void Initialize(SpriteBatch spriteBatch, string difficulty)
        {
            WaitingForCluster = false;
            WaitingForBlackBlocks = false;
            movingBlackBlocks = new Block[staticBlocks.GetLength(0)];

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

                    UpdateClusters(gameTime);

                    CheckForExplosions();

                    if (!BlocksAreMoving() && !currentCluster.IsMoving && explodingBlocks.Count == 0)
                    {
                        if (IsGameOver())
                            EndGame();
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

        /// <summary>
        /// Drops black blocks from top at columns in indexes
        /// </summary>
        /// <param name="indexes"></param>
        public void AddBlackBlocks(List<int> indexes)
        {
            foreach (int i in indexes)
            {
                Block blackBlock = CreateBlackBlock(i);
                blocks.Add(blackBlock);
                movingBlackBlocks[i] = blackBlock;
            }
        }

        /// <summary>
        /// Creates a stationary black block at a position 
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        public void SetBlackBlock(float posX, float posY)
        {
            var pos = new Vector2(posX, posY);
            int gridPosX = GridPositionX(pos);
            int gridPosY = GridPositionY(pos);

            Block blackBlock;
            if (movingBlackBlocks[gridPosX] != null)
            {
                blackBlock = movingBlackBlocks[gridPosX];
                movingBlackBlocks[gridPosX] = null;
            }
            else
                blackBlock = CreateBlackBlock(gridPosX);

            
            blackBlock.Attach(new Vector2(posX, posY));
            staticBlocks[gridPosX, gridPosY] = blackBlock;
            BlackBlocksQueued--;
        }

        /// <summary>
        /// Moves current to position and separates according to position from remote
        /// </summary>
        /// <param name="firstBlock"></param>
        /// <param name="secondBlock"></param>
        public void MoveAndSeparate(Vector2 firstBlock, Vector2 secondBlock)
        {
            State = GameState.BlocksFalling;
            currentCluster.FirstBlock.Position = firstBlock;
            currentCluster.SecondBlock.Position = secondBlock;
            currentCluster.SetDropSpeed(BaseDropSpeed * dropSpeedBonus);
            CheckForBlockCollision(currentCluster.FirstBlock);
            CheckForBlockCollision(currentCluster.SecondBlock);
            blocks.AddRange(currentCluster.Separate());
            MovementLocked = true;   
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
            State = GameState.ClusterFalling;
            currentCluster = nextCluster;
            currentCluster.Move(position - new Vector2(0f, (Height - 3f * blockSize) * 0.5f));
            currentCluster.IsMoving = false;
            currentCluster.SetDropSpeed(BaseDropSpeed * SpeedMultiplier);
            nextCluster = null;
            scoreMultiplier = 1;
            WaitingForCluster = true;
            dropTimer = 0;
            waitForDropTimer = true;
            WaitingForBlackBlocks = false;
            MovementLocked = false;
            movingBlackBlocks = new Block[movingBlackBlocks.Length];
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
