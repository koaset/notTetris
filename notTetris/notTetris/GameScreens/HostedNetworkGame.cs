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
    class HostedNetworkGame : GameScreen
    {
        Playfield localPlayerField;
        NetworkPlayfield remotePlayerField;
        Image backgroundImage;
        Image pauseImage;
        OutlineText countdownText;
        float countdownValue;
        bool p1Won;
        bool isStarted;
        NetServer server;
        NetConnection connection;
        bool remotePlayerDown;
        float updateTime;
        BlockType nextFirstBlock;
        BlockType nextSecondBlock;

        public HostedNetworkGame(Settings settings, NetServer server)
        {
            this.server = server;
            localPlayerField = new Playfield(GameType.Normal, new Vector2(780f, 325f), settings.PlayfieldSize);
            remotePlayerField = new NetworkPlayfield(GameType.Normal, new Vector2(300f, 325f), settings.PlayfieldSize);
            backgroundImage = new Image();
            pauseImage = new Image();
            //timer = new Text();
            countdownText = new OutlineText();
        }

        public override void Initialize(SpriteBatch spriteBatch, Settings settings)
        {
            base.Initialize(spriteBatch, settings);

            updateTime = 0;
            isStarted = false;
            remotePlayerDown = false;
            localPlayerField.Initialize(spriteBatch, settings.Difficulty);
            localPlayerField.IsShowing = true;
            System.Threading.Thread.Sleep(10);
            remotePlayerField.Initialize(spriteBatch, settings.Difficulty);
            remotePlayerField.IsShowing = true;

            if (server.ConnectionsCount == 0)
                NewScreen(new NetworkGameSetup());
            else
                connection = server.Connections.ToArray()[0];

            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write("init");
            msg.Write((int)localPlayerField.NextCluster.FirstBlock.BlockType);
            msg.Write((int)localPlayerField.NextCluster.SecondBlock.BlockType);
            server.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
            ListenForInit();

            localPlayerField.BaseDropSpeed = settings.BlockDropSpeed;
            remotePlayerField.BaseDropSpeed = settings.BlockDropSpeed;

            pauseImage.Initialize();
            pauseImage.Layer = 0.9f;
            pauseImage.Size = new Vector2(487, 120);
            pauseImage.Position = SCREENSIZE * 0.5f;
            pauseImage.IsShowing = false;
            pauseImage.TextureName = TextureNames.game_paused;

            backgroundImage.Initialize();
            backgroundImage.Layer = 0.3f;
            backgroundImage.Size = SCREENSIZE;
            backgroundImage.Position = SCREENSIZE * 0.5f;
            backgroundImage.TextureName = TextureNames.game_background;

            localPlayerField.GameOver += new GameOverEventHandler(OnGameOver);
            localPlayerField.NewNextCluster += new NewNextClusterEventHandler(localPlayerField_NewNextCluster);
            localPlayerField.ClusterSeparate += new ClusterSeparateEventHandler(localPlayerField_ClusterSeparate);

            remotePlayerField.GameOver += new GameOverEventHandler(OnGameOver);

            /*timeLimit = new TimeSpan(0, settings.PlayTime, 0);
            timer.Initialize();
            timer.Font = FontNames.Segoe_UI_Mono;
            timer.Layer = 0.8f;
            timer.Position = new Vector2(10);
            timer.TextColor = Color.Navy;
            timer.TextValue = "Time left: " + timeLimit.Minutes.ToString() + ":" + timeLimit.Seconds.ToString();*/

            countdownValue = 5.0f;
            countdownText.Initialize();
            countdownText.Font = FontNames.Segoe_UI_Mono_Huge;
            countdownText.Layer = 0.9f;
            countdownText.Position = SCREENSIZE * 0.5f - new Vector2(0f, 50f);
            countdownText.IsCentered = true;
            countdownText.TextColor = Color.Navy;
            countdownText.OutlineColor = Color.White;
            countdownText.OutlineSize = 3f;
            countdownText.TextValue = Convert.ToString(countdownValue);
        }

        private void ListenForInit()
        {
            bool notDone = true;
            while (notDone)
            {
                NetIncomingMessage msg;
                while ((msg = server.ReadMessage()) != null)
                {
                    if (msg.MessageType == NetIncomingMessageType.Data)
                    {
                        if (msg.ReadString() == "init")
                        {
                            remotePlayerField.CreateNextCluster((BlockType)msg.ReadInt32(), (BlockType)msg.ReadInt32());
                            notDone = false;
                        }
                    }
                    server.Recycle(msg);
                }
            }
        }

        public override void LoadContent()
        {
            localPlayerField.LoadContent();
            remotePlayerField.LoadContent();
            pauseImage.LoadContent(spriteBatch);
            backgroundImage.LoadContent(spriteBatch);
            //timer.LoadContent(spriteBatch);
            countdownText.LoadContent(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            if (server.ConnectionsCount != 0)
            {
                connection = server.Connections.ToArray()[0];
                KeyboardState newState = Keyboard.GetState();

                if (newState.IsKeyDown(Keys.F10) && oldState.IsKeyUp(Keys.F10))
                    NewScreen(new MainMenu(), "F10");

                if (!isStarted)
                    UpdateCountDown(gameTime);
                else
                {

                    localPlayerField.Update(gameTime);



                    #region Local Player Controls
                    if (!localPlayerField.ControlsLocked)
                    {
                        if (newState.IsKeyDown(settings.Player1Rotate) && oldState.IsKeyUp(settings.Player1Rotate))
                            localPlayerField.RotateCluster();

                        else if (newState.IsKeyDown(settings.Player1Left) && oldState.IsKeyUp(settings.Player1Left))
                            localPlayerField.MoveClusterLeft();

                        else if (newState.IsKeyDown(settings.Player1Right) && oldState.IsKeyUp(settings.Player1Right))
                            localPlayerField.MoveClusterRight();
                        if (newState.IsKeyDown(settings.Player1Down))
                        {
                            localPlayerField.MoveClusterDown();
                        }
                    }
                    #endregion

                    if (updateTime > 200 && localPlayerField.CurrentCluster.IsMoving)
                    {
                        NetOutgoingMessage outMsg = server.CreateMessage();
                        outMsg.Write("pos");
                        outMsg.Write(localPlayerField.CurrentCluster.FirstBlock.Position.X);
                        outMsg.Write(localPlayerField.CurrentCluster.FirstBlock.Position.Y);
                        outMsg.Write(localPlayerField.CurrentCluster.SecondBlock.Position.X);
                        outMsg.Write(localPlayerField.CurrentCluster.SecondBlock.Position.Y);
                        server.SendMessage(outMsg, connection, NetDeliveryMethod.ReliableOrdered);
                        updateTime = 0;
                    }
                    else
                        updateTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                    #region read remote messages
                    if (isStarted && !remotePlayerField.ControlsLocked)
                    {
                        NetIncomingMessage msg;
                        while ((msg = server.ReadMessage()) != null)
                        {
                            if (msg.MessageType == NetIncomingMessageType.Data)
                            {
                                string temp = msg.ReadString();
                                if (temp == "pos" && !remotePlayerField.ControlsLocked && !remotePlayerField.WaitingForCluster)
                                {
                                    float firstX = msg.ReadFloat() - 480;
                                    float firstY = msg.ReadFloat();
                                    float secondX = msg.ReadFloat() - 480;
                                    float secondY = msg.ReadFloat();
                                    //remotePlayerField.CurrentCluster.FirstBlock.Position = new Vector2(firstX, firstY);
                                    //remotePlayerField.CurrentCluster.SecondBlock.Position = new Vector2(secondX, secondY);
                                }
                                else if (temp == "nc")
                                {
                                    nextFirstBlock = (BlockType)msg.ReadInt32();
                                    nextSecondBlock = (BlockType)msg.ReadInt32();
                                }
                                else if (temp == "cs")
                                {
                                    float firstX = msg.ReadFloat() - 480;
                                    float firstY = msg.ReadFloat();
                                    float secondX = msg.ReadFloat() - 480;
                                    float secondY = msg.ReadFloat();
                                    remotePlayerField.MoveAndSeparate(new Vector2(firstX, firstY), new Vector2(secondX, secondY));
                                }
                                else if (temp == "Game Over")
                                    WinGame();
                            }
                            server.Recycle(msg);
                        }
                    }
                    #endregion

                    if (remotePlayerField.WaitingForCluster)
                        remotePlayerField.CreateNextCluster(nextFirstBlock, nextSecondBlock);

                    if (remotePlayerDown)
                        remotePlayerField.MoveClusterDown();

                    remotePlayerField.Update(gameTime);

                    /*TimeSpan timeLeft = timeLimit - time;

                    timer.TextValue = "Time left: " + timeLeft.Minutes.ToString() + ":" + timeLeft.Seconds.ToString();

                    if (timeLeft.Minutes == 0 && timeLeft.Seconds == 0)
                        if (localPlayerField.GetScore > remotePlayerField.GetScore)
                            remotePlayerField.EndGame();
                        else
                            localPlayerField.EndGame();*/
                }
                oldState = newState;
            }
            else
            {
                NewScreen(new NetworkGameSetup());
            }
        }

        private void SendMessage(string code, string message)
        {
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write(code);
            msg.Write(message);
            server.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
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
            //timer.Draw(gameTime);
            countdownText.Draw(gameTime);
        }

        void localPlayerField_ClusterSeparate(object o, ClusterSeparateEventArgs e)
        {
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write("cs");
            msg.Write(e.FirstBlockPosition.X);
            msg.Write(e.FirstBlockPosition.Y);
            msg.Write(e.SecondBlockPosition.X);
            msg.Write(e.SecondBlockPosition.Y);
            server.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
        }

        void localPlayerField_NewNextCluster(object o, NewNextClusterEventArgs e)
        {
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write("nc");
            msg.Write(e.FirstBlock);
            msg.Write(e.SecondBlock);
            server.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
        }

        private void NewScreen(GameScreen newScreen, string reason)
        {
            server.Shutdown(reason);
            NewScreen(newScreen);
        }

        private void WinGame()
        {
            p1Won = true;
            NewScreen(new ResultsScreen(GetResults(), true));
        }

        private void OnGameOver(object o, EventArgs e)
        {
            p1Won = false;

            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write("Game Over");
            server.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
            System.Threading.Thread.Sleep(1000);
            NewScreen(new ResultsScreen(GetResults(), true), "Game Over");         
        }

        public override Results GetResults()
        {
            Results r = new Results();

            r.IsSinglerplayer = false;
            r.Player1Won = p1Won;
            //r.Time = time;
            r.Player1Score = localPlayerField.GetScore;
            r.Player2Score = remotePlayerField.GetScore;            
            r.Difficulty = settings.Difficulty;            

            return r;
        }
    }
}
