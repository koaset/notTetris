using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NotTetris.Controls;
using NotTetris.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lidgren.Network;

namespace NotTetris.GameScreens
{
    class HostScreen : GameScreen
    {
        Cursor cursor;
        Image backgroundImage;
        OutlineText infoText;
        TextButton cancelButton;
        TextButton startButton;
        NetServer server;
        NetConnection connection;
        private const int PORT = 12345;

        public HostScreen()
        {
            cursor = new Cursor();
            backgroundImage = new Image();
            infoText = new OutlineText();
            cancelButton = new TextButton();
            startButton = new TextButton();

            NetPeerConfiguration config = new NetPeerConfiguration("NotTetris");
            config.Port = PORT;
            server = new NetServer(config);
            server.Start();
        }

        public override void Initialize(SpriteBatch spriteBatch, Settings settings)
        {
            base.Initialize(spriteBatch, settings);
            cursor.Initialize();
            backgroundImage.Initialize();
            backgroundImage.TextureName = TextureNames.game_background;
            backgroundImage.Size = new Vector2(1000, 720);
            backgroundImage.Position = new Vector2(500, 360);
            backgroundImage.Layer = 0.3f;
            infoText.Initialize();
            infoText.OutlineSize = 1f;
            infoText.IsCentered = true;
            infoText.Position = new Vector2(500, 200);
            infoText.TextValue = "Waiting for connections";
            cancelButton.Initialize();
            cancelButton.Text = "Cancel";
            cancelButton.Position = new Vector2(100f, 500f);
            cancelButton.Click += OnCancelButtonClick;
            startButton.Initialize();
            startButton.Text = "Start";
            startButton.Position = new Vector2(100f, 400f);
            startButton.Click += OnStartButtonClick;
            startButton.IsShowing = false;
            startButton.Enabled = false;
        }

        public override void LoadContent()
        {
            cursor.LoadContent(spriteBatch);
            backgroundImage.LoadContent(spriteBatch);
            infoText.LoadContent(spriteBatch);
            cancelButton.LoadContent(spriteBatch);
            startButton.LoadContent(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            cursor.Update();
            cancelButton.Update(gameTime);
            startButton.Update(gameTime);

            if (server.ConnectionsCount > 0)
                connection = server.Connections.ToArray()[0];

            if (connection != null)
                if (connection.Status == NetConnectionStatus.Connected)
                {
                    startButton.IsShowing = true;
                    startButton.Enabled = true;
                }

            NetIncomingMessage msg;
            while ((msg = server.ReadMessage()) != null)
            {
                if (msg.MessageType == NetIncomingMessageType.ConnectionApproval)
                {
                        NetIncomingMessage hail = msg.SenderConnection.RemoteHailMessage;
                        msg.SenderConnection.Approve();
                        startButton.IsShowing = true;
                        startButton.Enabled = true;
                }
                server.Recycle(msg);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            cursor.Draw(gameTime);
            backgroundImage.Draw(gameTime);
            infoText.Draw(gameTime);
            cancelButton.Draw(gameTime);
            startButton.Draw(gameTime);
        }

        void OnCancelButtonClick(object o, EventArgs e)
        {
            server.Shutdown("Server closed");
            NewScreen(new NetworkGameSetup());
        }

        private void OnStartButtonClick(object o, EventArgs e)
        {
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write("Start you fool!");
            server.SendMessage(msg, server.Connections.ToArray()[0], NetDeliveryMethod.ReliableSequenced);
        }
    }
}
