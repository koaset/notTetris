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

        public NetworkPlayfield(GameType gameType, Vector2 position, int sizeX) : base(gameType, position, sizeX) { }

        public override void Initialize(SpriteBatch spriteBatch, string difficulty)
        {
            WaitingForCluster = false;

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
                if (!explosionAnimation.IsStarted)
                {
                    if (ClearUpBlocks())
                        ReleaseBlocks();

                    UpdateClusters(gameTime);

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
                {
                    UpdateExplosion(gameTime);
                    scoreFloater.Update(gameTime);
                }
            }
        }

        /// <summary>
        /// Moves current to position and separates according to position from remote
        /// </summary>
        /// <param name="firstBlock"></param>
        /// <param name="secondBlock"></param>
        public void MoveAndSeparate(Vector2 firstBlock, Vector2 secondBlock)
        {
            ControlsLocked = true;
            currentCluster.FirstBlock.Position = firstBlock;
            currentCluster.SecondBlock.Position = secondBlock;
            currentCluster.SetDropSpeed(BaseDropSpeed * SpeedMultiplier);
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
            currentCluster.Move(position - new Vector2(0f, (Height - blockSize) * 0.5f));
            currentCluster.IsMoving = true;
            currentCluster.SetDropSpeed(BaseDropSpeed * SpeedMultiplier);
            ControlsLocked = false;
            nextCluster = null;
            scoreMultiplier = 1;
            WaitingForCluster = true;
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
