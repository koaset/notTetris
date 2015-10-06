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
    class ConnectionScreen : GameScreen
    {
        Cursor cursor;
        Image backgroundImage;
        OutlineText infoText;
        TextButton cancelButton;
        NetClient client;
        NetConnection connection;
        string ip;
        private const int PORT = 12345;

        public ConnectionScreen(string ip)
        {
            this.ip = ip;
            cursor = new Cursor();
            backgroundImage = new Image();
            infoText = new OutlineText();
            cancelButton = new TextButton(TextButtonType.Cancel, new Vector2(100f, 500f));

            NetPeerConfiguration config = new NetPeerConfiguration("NotTetris");
            client = new NetClient(config);
            client.Start();
            connection = client.Connect(ip, PORT);
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
            infoText.OutlineSize = 1.2f;
            infoText.IsCentered = true;
            infoText.Position = new Vector2(500, 200);
            infoText.TextValue = "Connecting to " + ip;
            infoText.OutlineColor = Color.Black;
            infoText.TextColor = Color.White;
            cancelButton.Initialize();
            cancelButton.Click += OnCancelButtonClick;
        }

        public override void LoadContent()
        {
            cursor.LoadContent(spriteBatch);
            backgroundImage.LoadContent(spriteBatch);
            infoText.LoadContent(spriteBatch);
            cancelButton.LoadContent(spriteBatch);
        }

        String message;

        public override void Update(GameTime gameTime)
        {
            cursor.Update();
            cancelButton.Update(gameTime);

            if (connection.Status == NetConnectionStatus.Connected)
                infoText.TextValue = connection.Status.ToString();

            NetIncomingMessage msg;
            if ((msg = client.ReadMessage()) != null)
            {
                if (msg.MessageType == NetIncomingMessageType.Data)
                    message = "\nMessage recieved: " + msg.ReadString();
                client.Recycle(msg);
            }
            infoText.TextValue += message;
        }

        public override void Draw(GameTime gameTime)
        {
            cursor.Draw(gameTime);
            backgroundImage.Draw(gameTime);
            infoText.Draw(gameTime);
            cancelButton.Draw(gameTime);
        }

        void OnCancelButtonClick(object o, EventArgs e)
        {
            client.Shutdown("Connection closed");
            NewScreen(new NetworkGameSetup());
        }
    }
}
