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
        public bool WaitingForBlackBlocks { get; set; }

        public NetworkPlayfield(GameType gameType, Vector2 position, int sizeX) 
            : base(gameType, position, sizeX) { }

        public override void Initialize(SpriteBatch spriteBatch, string difficulty)
        {
            WaitingForCluster = false;
            WaitingForBlackBlocks = false;

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

        public void AddBlackBlock(int gridPosX, int gridPosY)
        {

            Block blackBlock = new Block(BlockType.Black, GetPositionFromIndex(gridPosX, gridPosY), blockSize);
            blackBlock.Initialize();
            blackBlock.IsMoving = false;
            blackBlock.WillBeChecked = false;
            blocks.Add(blackBlock);
            staticBlocks[gridPosX, gridPosY] = blackBlock;

            if (BlackBlocksQueued > 0)
                BlackBlocksQueued--;
            else
                throw new Exception("Wtf exception");
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
