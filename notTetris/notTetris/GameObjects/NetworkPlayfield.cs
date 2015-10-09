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
