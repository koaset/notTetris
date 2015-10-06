using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Net.Sockets;
using System.Net;
using NotTetris.Controls;
using NotTetris.Graphics;
using Lidgren.Network;

namespace NotTetris.GameScreens
{
    class NetworkGameSetup : GameScreen
    {
        Image backgroundImage;
        Cursor cursor;
        TextboxPopup ipPopup;
        Text ipText;
        TextButton ipButton;
        TextButton hostButton;
        TextButton connectButton;
        TextButton backButton;

        string ip = "127.0.0.1";
            //"127.0.0.1";
            //"192.168.0.104";
            //"83.233.223.246";

        public NetworkGameSetup()
        {
            backgroundImage = new Image();
            cursor = new Cursor();
            ipPopup = new TextboxPopup("Enter target IP", 15);
            ipText = new Text();
            connectButton = new TextButton();
            hostButton = new TextButton();
            ipButton = new TextButton();
            backButton = new TextButton();
        }

        public override void Initialize(SpriteBatch spriteBatch, Settings settings)
        {
            base.Initialize(spriteBatch, settings);

            backgroundImage.Initialize();
            backgroundImage.TextureName = TextureNames.game_background;
            backgroundImage.Size = new Vector2(1000, 720);
            backgroundImage.Position = new Vector2(500, 360);
            backgroundImage.Layer = 0.3f;
            cursor.Initialize();
            ipPopup.Initialize();
            ipPopup.ClosePopup += new ClosePopupEventHandler(OnClosePopup);
            ipText.Initialize();
            ipText.Position = new Vector2(550f, 150);
            ipText.TextValue = "Target IP\n" + ip;
            connectButton.Initialize();
            connectButton.Text = "Connect to IP";
            connectButton.Scale = new Vector2(0.75f);
            connectButton.Position = new Vector2(100f, 150f);
            connectButton.Click += OnConnectButtonClick;
            hostButton.Initialize();
            hostButton.Text = "Host Game";
            hostButton.Scale = new Vector2(0.75f);
            hostButton.Position = new Vector2(100f, 250f);
            hostButton.Click += OnHostButtonClick;
            ipButton.Initialize();
            ipButton.Text = "Change target IP";
            ipButton.Scale = new Vector2(0.75f);
            ipButton.Position = new Vector2(100f, 350f);
            ipButton.Click += OnIPButtonClick;
            backButton.Initialize();
            backButton.Text = "Back";
            backButton.Position = new Vector2(100f, 500f);
            backButton.Click += OnBackButtonClick;
        }

        public override void LoadContent()
        {
            cursor.LoadContent(spriteBatch);
            backgroundImage.LoadContent(spriteBatch);
            ipText.LoadContent(spriteBatch);
            ipPopup.LoadContent(spriteBatch);
            ipButton.LoadContent(spriteBatch);
            hostButton.LoadContent(spriteBatch);
            connectButton.LoadContent(spriteBatch);
            backButton.LoadContent(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            
            KeyboardState newState = Keyboard.GetState();
            cursor.Update();

            if (isFocused)
            {
                ipPopup.Update(gameTime);

                if (!ipPopup.IsShowing)
                {
                    ipButton.Update(gameTime);
                    hostButton.Update(gameTime);
                    connectButton.Update(gameTime);
                    backButton.Update(gameTime);
                }
            }
            oldState = newState;
        }

        public override void Draw(GameTime gameTime)
        {
            cursor.Draw(gameTime);
            backgroundImage.Draw(gameTime);
            ipText.Draw(gameTime);
            ipPopup.Draw(gameTime);
            ipButton.Draw(gameTime);
            hostButton.Draw(gameTime);
            connectButton.Draw(gameTime);
            backButton.Draw(gameTime);
        }

        private bool IsValidIP(string ip)
        {
            int numDots = 0;
            int numbers = 0;
            for (int i = 0; i < ip.Length; i++)
            {
                if (ip.ToCharArray()[i] == '.')
                {
                    numDots++;
                }
                else
                    numbers++;
            }

            if (numDots != 3)
                return false;
            return true;
        }

        public override string ToString()
        {
            return ip;
        }

        private void OnClosePopup(object o, EventArgs e)
        {
            if (ipPopup.ShouldSave)
                if (IsValidIP(e.ToString()))
                {
                    ip = e.ToString();
                    ipText.TextValue = "Target IP\n" + ip;
                }
        }

        private void OnIPButtonClick(object o, EventArgs e)
        {
            ipPopup.Show();
            /*NetOutgoingMessage sendMsg = client.CreateMessage();
            sendMsg.Write("Hello");
            client.SendMessage(sendMsg, connection, NetDeliveryMethod.ReliableOrdered);*/
        }
        private void OnConnectButtonClick(object o, EventArgs e)
        {
            if (IsValidIP(ipText.TextValue))
                NewScreen(new ConnectionScreen(ip));
            else
                ipText.TextValue = "IP invalid";
            /*NetPeerConfiguration clientConfig = new NetPeerConfiguration("NotTetris");
            client = new NetClient(clientConfig);
            client.Start();
            connection = client.Connect(host, port);*/
            
        }
        private void OnHostButtonClick(object o, EventArgs e)
        {
            NewScreen(new HostScreen());
        }

        private void OnBackButtonClick(object o, EventArgs e)
        {
            NewScreen(new MainMenu());
        }


    }
}
