using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Net.Sockets;
using System.Net;
using NotTetris.Controls;
using NotTetris.Graphics;

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

        TcpClient client;
        string IP = "127.0.0.1";
        int PORT = 1490;

        public NetworkGameSetup()
        {
            backgroundImage = new Image();
            cursor = new Cursor();
            ipPopup = new TextboxPopup("Enter target IP", 15);
            ipText = new Text();
            connectButton = new TextButton(TextButtonType.Connect, new Vector2(100f, 150f));
            hostButton = new TextButton(TextButtonType.Host, new Vector2(100f, 250f));
            ipButton = new TextButton(TextButtonType.IP, new Vector2(100f, 350f));
            backButton = new TextButton(TextButtonType.Back, new Vector2(100f, 500f));

            client = new TcpClient();
        }

        public override void Initialize(SpriteBatch spriteBatch, Settings settings)
        {
            base.Initialize(spriteBatch, settings);

            cursor.Initialize();
            ipPopup.Initialize();
            ipPopup.ClosePopup += new ClosePopupEventHandler(OnClosePopup);
            ipText.Initialize();
            ipText.Position = new Vector2(550f, 150);
            ipText.TextValue = "Target IP\n192.168.0.1";
            ipButton.Initialize();
            ipButton.Click += OnIPButtonClick;
            hostButton.Initialize();
            hostButton.Click += OnHostButtonClick;
            connectButton.Initialize();
            connectButton.Click += OnConnectButtonClick;
            backButton.Initialize();
            backButton.Click += OnBackButtonClick;

            backgroundImage.Initialize();
            backgroundImage.TextureName = TextureNames.game_background;
            backgroundImage.Size = new Vector2(1000, 720);
            backgroundImage.Position = new Vector2(500, 360);
            backgroundImage.Layer = 0.3f;
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

            ipPopup.Update(gameTime);
            cursor.Update();
            if (!ipPopup.IsShowing)
            {
                ipButton.Update(gameTime);
                hostButton.Update(gameTime);
                connectButton.Update(gameTime);
                backButton.Update(gameTime);
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

        private void OnClosePopup(object o, EventArgs e)
        {
            if (ipPopup.ShouldSave)
                if (IsValidIP(e.ToString()))
                    ipText.TextValue = "Target IP\n" + e.ToString();
        }

        void OnIPButtonClick(object o, EventArgs e)
        {
            ipPopup.Show();
        }

        void OnConnectButtonClick(object o, EventArgs e)
        {
            client.Connect(IP, PORT);
            client.NoDelay = true;
        }

        void OnHostButtonClick(object o, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void OnBackButtonClick(object o, EventArgs e)
        {
            NewScreen(ScreenType.MainMenu);
        }
    }
}
