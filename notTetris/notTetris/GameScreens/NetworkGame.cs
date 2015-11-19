using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using NotTetris.Graphics;
using NotTetris.Controls;
using NotTetris.GameObjects;
using Lidgren.Network;

namespace NotTetris.GameScreens
{
    /// <summary>
    /// The screen used for network multiplayer
    /// </summary>
    class NetworkGame : GameScreen
    {
        Playfield localPlayerField;
        RemotePlayfield remotePlayerField;
        Image backgroundImage;
        Image pauseImage;
        OutlineText countdownText;
        Text timeText;
        TimeSpan timePlayed;
        TimeSpan timeLimit;
        float countdownValue;
        bool p1Won;
        bool isStarted;
        NetPeer peer;
        NetConnection connection;
        bool remotePlayerDown;
        float updateTime;
        float updateInterval;
        BlockType nextFirstBlock;
        BlockType nextSecondBlock;
        KeyboardState newState;
        float xDiff;

        public NetworkGame(Settings settings, NetPeer peer)
        {
            this.settings = settings;
            this.peer = peer;
            localPlayerField = new Playfield(settings.GameType, new Vector2(780f, 325f), settings.PlayfieldSize);
            remotePlayerField = new RemotePlayfield(settings.GameType, new Vector2(300f, 325f), settings.PlayfieldSize);
            xDiff = localPlayerField.Position.X - remotePlayerField.Position.X;
            backgroundImage = new Image();
            pauseImage = new Image();
            timeText = new Text();
            countdownText = new OutlineText();
            if (peer.ConnectionsCount == 0)
                NewScreen(new NetworkGameSetup(), "No Connection");
            else
                connection = peer.Connections.ToArray()[0];
        }

        public override void Initialize(SpriteBatch spriteBatch, Settings localSettings)
        {
            this.spriteBatch = spriteBatch;
            mouseVisible = false;

            updateTime = 0;
            updateInterval = 30;
            isStarted = false;
            remotePlayerDown = false;

            #region Initialize playfields
            localPlayerField.Initialize(spriteBatch, settings.Difficulty);
            localPlayerField.IsShowing = true;
            localPlayerField.SetDebugInfoVisibility(localSettings.ShowDebugInfo);
            System.Threading.Thread.Sleep(10);
            remotePlayerField.Initialize(spriteBatch, settings.Difficulty);
            remotePlayerField.IsShowing = true;
            remotePlayerField.SetDebugInfoVisibility(localSettings.ShowDebugInfo);

            localPlayerField.GameOver += OnGameOver;
            localPlayerField.NewNextCluster += localPlayerField_NewNextCluster;
            localPlayerField.ClusterDrop += localPlayerField_ClusterDrop;
            localPlayerField.ClusterSeparate += localPlayerField_ClusterSeparate;
            if (settings.GameType == GameType.Normal)
            {
                localPlayerField.BlackBlocksCreated += localPlayerField_BlackBlocksCreated;
                localPlayerField.ShouldDropBlackBlocks += localPlayerField_RemoteShouldDropBlackBlocks;
                localPlayerField.BlackBlockCollision += localPlayerField_BlackBlockCollision;
            }

            NetOutgoingMessage msg = peer.CreateMessage();
            msg.Write("init");
            msg.Write((int)localPlayerField.CurrentCluster.FirstBlock.BlockType);
            msg.Write((int)localPlayerField.CurrentCluster.SecondBlock.BlockType);
            msg.Write((int)localPlayerField.NextCluster.FirstBlock.BlockType);
            msg.Write((int)localPlayerField.NextCluster.SecondBlock.BlockType);
            peer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
            ListenForInit();

            localPlayerField.BaseDropSpeed = settings.BlockDropSpeed;
            remotePlayerField.BaseDropSpeed = settings.BlockDropSpeed;
            #endregion

            #region Initialize images & text
            pauseImage.Initialize();
            pauseImage.Layer = 0.9f;
            pauseImage.Size = new Vector2(487, 120);
            pauseImage.Position = WINDOWSIZE * 0.5f;
            pauseImage.IsShowing = false;
            pauseImage.TextureName = TextureNames.game_paused;

            backgroundImage.Initialize();
            backgroundImage.Layer = 0.3f;
            backgroundImage.Size = WINDOWSIZE;
            backgroundImage.Position = WINDOWSIZE * 0.5f;
            backgroundImage.TextureName = TextureNames.game_background;

            timeLimit = new TimeSpan(0, settings.PlayTime, 0);
            timeText.Initialize();
            timeText.Font = FontNames.Segoe_UI_Mono;
            timeText.Layer = 0.8f;
            timeText.Position = new Vector2(10);
            timeText.TextColor = Color.Navy;
            timeText.TextValue = "Time left: " + timeLimit.Minutes.ToString() + ":" + timeLimit.Seconds.ToString();

            if (settings.GameType != GameType.Time)
                timeText.IsShowing = false;

            countdownValue = 5.0f;
            countdownText.Initialize();
            countdownText.Font = FontNames.Segoe_UI_Mono_Huge;
            countdownText.Layer = 0.9f;
            countdownText.Position = WINDOWSIZE * 0.5f - new Vector2(0f, 50f);
            countdownText.IsCentered = true;
            countdownText.TextColor = Color.Navy;
            countdownText.OutlineColor = Color.White;
            countdownText.OutlineSize = 3f;
            countdownText.TextValue = Convert.ToString(countdownValue);
            #endregion
        }

        private void ListenForInit()
        {
            bool notDone = true;
            while (notDone)
            {
                NetIncomingMessage msg;
                while ((msg = peer.ReadMessage()) != null)
                {
                    if (msg.MessageType == NetIncomingMessageType.Data)
                    {
                        if (msg.ReadString() == "init")
                        {
                            remotePlayerField.CreateNextCluster((BlockType)msg.ReadInt32(), (BlockType)msg.ReadInt32());
                            remotePlayerField.DropNextCluster();
                            remotePlayerField.CreateNextCluster((BlockType)msg.ReadInt32(), (BlockType)msg.ReadInt32());
                            notDone = false;
                        }
                    }
                    peer.Recycle(msg);
                }
            }
        }

        public override void LoadContent()
        {
            localPlayerField.LoadContent();
            remotePlayerField.LoadContent();
            pauseImage.LoadContent(spriteBatch);
            backgroundImage.LoadContent(spriteBatch);
            timeText.LoadContent(spriteBatch);
            countdownText.LoadContent(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.F10) && oldState.IsKeyUp(Keys.F10))
                NewScreen(new MainMenu(), "F10");

            if (peer.ConnectionsCount == 0)
                NewScreen(new NetworkGameSetup(), "Lost Connection");

            if (!isStarted)
                UpdateCountDown(gameTime);
            else
            {
                localPlayerField.Update(gameTime);

                HandleInput(gameTime);

                if (updateTime > updateInterval && localPlayerField.CurrentCluster.IsMoving)
                {
                    SendPositionMessage();
                    updateTime = 0;
                }
                else
                    updateTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                ReadMessages();

                if (remotePlayerField.WaitingForCluster)
                    remotePlayerField.CreateNextCluster(nextFirstBlock, nextSecondBlock);

                if (remotePlayerDown)
                    remotePlayerField.MoveClusterDown(gameTime);

                remotePlayerField.Update(gameTime);

                HandleTimer(gameTime);
            }

            oldState = newState;
        }

        private void UpdateCountDown(GameTime gameTime)
        {
            countdownValue -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (countdownValue < 0)
            {
                countdownText.TextValue = "0";
                isStarted = true;
                localPlayerField.StartGame();
                remotePlayerField.StartGame();
                countdownText.IsShowing = false;
            }
            else
                countdownText.TextValue = Convert.ToString((int)countdownValue + 1);
        }

        private void HandleInput(GameTime gameTime)
        {
            if (!localPlayerField.MovementLocked)
            {
                if (newState.IsKeyDown(settings.Player1Rotate) && oldState.IsKeyUp(settings.Player1Rotate))
                    localPlayerField.RotateCluster();
                else if (newState.IsKeyDown(settings.Player1Left) && newState.IsKeyUp(settings.Player1Right))
                    localPlayerField.MoveClusterLeft(gameTime, oldState.IsKeyUp(settings.Player1Left));
                else if (newState.IsKeyDown(settings.Player1Right) && newState.IsKeyUp(settings.Player1Left))
                    localPlayerField.MoveClusterRight(gameTime, oldState.IsKeyUp(settings.Player1Right));
                if (newState.IsKeyDown(settings.Player1Down) && oldState.IsKeyUp(settings.Player1Down))
                    localPlayerField.MoveClusterDown(gameTime);
            }
        }

        private void SendPositionMessage()
        {
            NetOutgoingMessage outMsg = peer.CreateMessage();
            outMsg.Write("pos");
            outMsg.Write(localPlayerField.CurrentCluster.FirstBlock.Position.X);
            outMsg.Write(localPlayerField.CurrentCluster.FirstBlock.Position.Y);
            outMsg.Write(localPlayerField.CurrentCluster.SecondBlock.Position.X);
            outMsg.Write(localPlayerField.CurrentCluster.SecondBlock.Position.Y);
            if (newState.IsKeyDown(settings.Player1Down))
                outMsg.Write("Down");
            else
                outMsg.Write("Up");
            peer.SendMessage(outMsg, connection, NetDeliveryMethod.ReliableOrdered);
        }

        private void ReadMessages()
        {
            if (isStarted)
            {
                NetIncomingMessage msg;
                while ((msg = peer.ReadMessage()) != null)
                {
                    if (msg.MessageType == NetIncomingMessageType.Data)
                    {
                        string temp = msg.ReadString();
                        if (remotePlayerField.State == GameState.ClusterFalling)
                        {
                            if (temp == "pos")
                            {
                                float firstX = msg.ReadFloat() - xDiff;
                                float firstY = msg.ReadFloat();
                                float secondX = msg.ReadFloat() - xDiff;
                                float secondY = msg.ReadFloat();
                                remotePlayerField.CurrentCluster.FirstBlock.Position = new Vector2(firstX, firstY);
                                remotePlayerField.CurrentCluster.SecondBlock.Position = new Vector2(secondX, secondY);
                                remotePlayerField.CurrentCluster.IsMoving = true;
                                if (msg.ReadString() == "Down")
                                    remotePlayerDown = true;
                                else
                                    remotePlayerDown = false;
                            }
                            else if (temp == "cs")
                            {
                                float firstX = msg.ReadFloat() - xDiff;
                                float firstY = msg.ReadFloat();
                                float secondX = msg.ReadFloat() - xDiff;
                                float secondY = msg.ReadFloat();
                                remotePlayerField.MoveAndSeparate(new Vector2(firstX, firstY), new Vector2(secondX, secondY));
                            }
                        }

                        if (temp == "nc")
                        {
                            nextFirstBlock = (BlockType)msg.ReadInt32();
                            nextSecondBlock = (BlockType)msg.ReadInt32();
                        }
                        else if (temp == "cd")
                        {
                            remotePlayerField.DropNextCluster();
                        }
                        else if (temp == "bb")
                        {
                            int numBlocks = msg.ReadInt32();
                            localPlayerField.QueueBlackBlocks(numBlocks);
                        }
                        else if (temp == "bbc")
                        {
                            float posX = msg.ReadFloat() - xDiff;
                            float posY = msg.ReadFloat();
                            remotePlayerField.SetBlackBlock(posX, posY);
                        }
                        else if (temp == "bbcr")
                        {
                            int num = msg.ReadInt32();
                            var indexes = new List<int>();
                            for (int i = 0; i < num; i++)
                                indexes.Add(msg.ReadInt32());
                            remotePlayerField.AddBlackBlocks(indexes);
                        }
                        else if (temp == "Game Over")
                        {
                            NetOutgoingMessage outMsg = peer.CreateMessage();
                            outMsg.Write("Ok");
                            peer.SendMessage(outMsg, connection, NetDeliveryMethod.ReliableOrdered);
                            WinGame();
                        }
                    }
                    peer.Recycle(msg);
                }
            }
        }

        private void HandleTimer(GameTime gameTime)
        {
            if (settings.GameType == GameType.Time)
            {
                timePlayed += gameTime.ElapsedGameTime;
                TimeSpan timeLeft = timeLimit - timePlayed;
                timeText.TextValue = "Time left: " + timeLeft.Minutes.ToString() + ":" + timeLeft.Seconds.ToString();
                if (timeLeft.Seconds < 0)
                {
                    if (localPlayerField.GetScore > remotePlayerField.GetScore)
                        WinGame();
                    else
                    {
                        p1Won = false;
                        System.Threading.Thread.Sleep(1000);
                        NewScreen(new ResultsScreen(GetResults(), true), "Game Over");
                    }
                }
            }
        }

        private void Pause()
        {
            localPlayerField.Pause();
            remotePlayerField.Pause();
            pauseImage.IsShowing = true;
        }

        private void UnPause()
        {
            localPlayerField.UnPause();
            remotePlayerField.UnPause();
            pauseImage.IsShowing = false;
        }

        public override void Draw(GameTime gameTime)
        {
            backgroundImage.Draw(gameTime);
            pauseImage.Draw(gameTime);
            localPlayerField.Draw(gameTime);
            remotePlayerField.Draw(gameTime);
            timeText.Draw(gameTime);
            countdownText.Draw(gameTime);
        }

        private void localPlayerField_NewNextCluster(object o, NewNextClusterEventArgs e)
        {
            NetOutgoingMessage msg = peer.CreateMessage();
            msg.Write("nc");
            msg.Write(e.FirstBlock);
            msg.Write(e.SecondBlock);
            peer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
        }

        private void localPlayerField_ClusterDrop(object o, EventArgs e)
        {
            NetOutgoingMessage msg = peer.CreateMessage();
            msg.Write("cd");
            peer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
        }

        private void localPlayerField_ClusterSeparate(object o, ClusterSeparateEventArgs e)
        {
            NetOutgoingMessage msg = peer.CreateMessage();
            msg.Write("cs");
            msg.Write(e.FirstBlockPosition.X);
            msg.Write(e.FirstBlockPosition.Y);
            msg.Write(e.SecondBlockPosition.X);
            msg.Write(e.SecondBlockPosition.Y);
            peer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
        }

        private void localPlayerField_BlackBlocksCreated(object o, BlackBlocksCreatedEventArgs e)
        {
            NetOutgoingMessage msg = peer.CreateMessage();
            msg.Write("bbcr");
            msg.Write(e.Indexes.Count);
            foreach (int i in e.Indexes)
                msg.Write(i);
            peer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
        }

        private void localPlayerField_RemoteShouldDropBlackBlocks(object o, ShouldDropBlackBlocksEventArgs e)
        {
            remotePlayerField.BlackBlocksQueued += e.NumBlocks;
            NetOutgoingMessage msg = peer.CreateMessage();
            msg.Write("bb");
            msg.Write(e.NumBlocks);
            peer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
        }

        private void localPlayerField_BlackBlockCollision(object o, BlackBlockCollisionEventArgs e)
        {
            NetOutgoingMessage msg = peer.CreateMessage();
            msg.Write("bbc");
            msg.Write(e.Position.X);
            msg.Write(e.Position.Y);
            peer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
        }

        private void NewScreen(GameScreen newScreen, string reason)
        {
            peer.Shutdown(reason);
            NewScreen(newScreen);
        }

        private void WinGame()
        {
            p1Won = true;
            System.Threading.Thread.Sleep(1000);
            NewScreen(new ResultsScreen(GetResults(), true), "Win");
        }

        private void LoseGame()
        {
            p1Won = false;
            System.Threading.Thread.Sleep(1000);
            NewScreen(new ResultsScreen(GetResults(), true), "Lose");
        }

        private void OnGameOver(object o, EventArgs e)
        {
            p1Won = false;
            NetOutgoingMessage outMsg = peer.CreateMessage();
            outMsg.Write("Game Over");
            peer.SendMessage(outMsg, connection, NetDeliveryMethod.ReliableOrdered);
            bool waiting = true;
            while (waiting)
            {
                NetIncomingMessage inMsg;
                while ((inMsg = peer.ReadMessage()) != null)
                {
                    if (inMsg.MessageType == NetIncomingMessageType.Data)
                    {
                        if (inMsg.ReadString() == "Ok")
                            waiting = false;
                    }
                    peer.Recycle(inMsg);
                }
            }
            System.Threading.Thread.Sleep(1000);
            NewScreen(new ResultsScreen(GetResults(), true), "Game Over");
        }

        public override GameResult GetResults()
        {
            GameResult r = new GameResult();

            r.IsSingleplayer = false;
            r.Player1Won = p1Won;
            r.Time = timePlayed;
            r.Player1Score = localPlayerField.GetScore;
            r.Player2Score = remotePlayerField.GetScore;
            r.Difficulty = settings.Difficulty;

            return r;
        }
    }
}
