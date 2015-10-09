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
        OutlineText ipText;
        OutlineText infoText;
        TextButton ipButton;
        TextButton hostButton;
        TextButton connectButton;
        TextButton backButton;
        TextButton startButton;
        NetClient client;
        NetServer server;
        bool hosting;
        bool connecting;
        NetConnection connection;
        NetConnectionStatus oldStatus;
        double connectTimer;
        double connectTimeLimit;
        int port;
        string ip;

        public NetworkGameSetup()
        {
            backgroundImage = new Image();
            cursor = new Cursor();
            ipPopup = new TextboxPopup("Enter target IP", 15);
            ipText = new OutlineText();
            infoText = new OutlineText();
            connectButton = new TextButton();
            hostButton = new TextButton();
            ipButton = new TextButton();
            backButton = new TextButton();
            startButton = new TextButton();
        }

        public override void Initialize(SpriteBatch spriteBatch, Settings settings)
        {
            base.Initialize(spriteBatch, settings);

            connectTimer = 0;
            connectTimeLimit = 5;
            hosting = false;
            connecting = false;
            ip = settings.IP;
            port = settings.PORT;
            if (port <= 0 || port >= 60000)
                port = 12345;

            #region Initialize controls
            backgroundImage.Initialize();
            backgroundImage.TextureName = TextureNames.game_background;
            backgroundImage.Size = new Vector2(1000, 720);
            backgroundImage.Position = new Vector2(500, 360);
            backgroundImage.Layer = 0.3f;
            cursor.Initialize();
            ipPopup.Initialize();
            ipPopup.ClosePopup += new ClosePopupEventHandler(OnClosePopup);
            ipText.Initialize();
            ipText.Position = new Vector2(500f, 150);
            ipText.TextValue = "IP: " + ip;
            infoText.Initialize();
            infoText.Position = new Vector2(500f, 250);
            infoText.IsShowing = true;
            infoText.TextValue = "";
            connectButton.Initialize();
            connectButton.Text = "Connect to IP";
            connectButton.Scale = new Vector2(0.75f);
            connectButton.Position = new Vector2(100f, 150f);
            connectButton.Click += OnConnectButtonClick;
            ipButton.Initialize();
            ipButton.Text = "Change target IP";
            ipButton.Scale = new Vector2(0.75f);
            ipButton.Position = new Vector2(100f, 250f);
            ipButton.Click += OnIPButtonClick;
            hostButton.Initialize();
            hostButton.Text = "Host Game";
            hostButton.Scale = new Vector2(0.75f);
            hostButton.Position = new Vector2(100f, 350f);
            hostButton.Click += OnHostButtonClick;
            backButton.Initialize();
            backButton.Text = "Back";
            backButton.Position = new Vector2(100f, 500f);
            backButton.Click += OnBackButtonClick;
            startButton.Initialize();
            startButton.Text = "Start";
            startButton.Position = new Vector2(300f, 500f);
            startButton.Click += OnStartButtonClick;
            startButton.Enabled = false;
            startButton.IsShowing = false;
            #endregion
        }

        public override void LoadContent()
        {
            cursor.LoadContent(spriteBatch);
            backgroundImage.LoadContent(spriteBatch);
            ipText.LoadContent(spriteBatch);
            infoText.LoadContent(spriteBatch);
            ipPopup.LoadContent(spriteBatch);
            ipButton.LoadContent(spriteBatch);
            hostButton.LoadContent(spriteBatch);
            connectButton.LoadContent(spriteBatch);
            backButton.LoadContent(spriteBatch);
            startButton.LoadContent(spriteBatch);
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
                    startButton.Update(gameTime);
                }
            }

            oldState = newState;

            if (connecting)
                UpdateClient(gameTime);
            else if (hosting)
                UpdatesServer();
        }

        private void UpdateClient(GameTime gameTime)
        {
            NetIncomingMessage msg;
            NetConnectionStatus newStatus = client.ConnectionStatus;

            if (newStatus == NetConnectionStatus.Connected && oldStatus != NetConnectionStatus.Connected)
                infoText.TextValue = "Connected.\nWaiting for host...";
            else if (newStatus != NetConnectionStatus.Connected && oldStatus == NetConnectionStatus.Connected)
            {
                StopConnecting();
                infoText.TextValue = "Lost connection";
            }
            else if (newStatus != NetConnectionStatus.Connected && oldStatus != NetConnectionStatus.Connected)
            {
                if (connectTimer > connectTimeLimit)
                {
                    connectTimer = 0;
                    StopConnecting();
                    infoText.TextValue = "No host found";
                }
                connectTimer += gameTime.ElapsedGameTime.TotalSeconds;
            }
                
            while ((msg = client.ReadMessage()) != null)
            {
                if (msg.MessageType == NetIncomingMessageType.Data)
                    if (msg.ReadString() == "Start")
                    {
                        Settings readSettings = ReadSettingsFromMessage(msg);
                        settings.IP = ip;
                        settings.PORT = port;
                        NewScreen(new NetworkGame(readSettings, client));
                    }
                if (msg.MessageType == NetIncomingMessageType.ConnectionApproval)
                    msg.SenderConnection.Approve();
                client.Recycle(msg);
            }

            oldStatus = newStatus;
        }

        private void UpdatesServer()
        {
            if (server.ConnectionsCount > 0)
                connection = server.Connections.ToArray()[0];
            if (connection != null)
            {
                NetConnectionStatus newStatus = connection.Status;
                if (oldStatus != NetConnectionStatus.Connected && newStatus == NetConnectionStatus.Connected)
                {
                    startButton.IsShowing = true;
                    startButton.Enabled = true;
                    infoText.TextValue = "Client found";
                }
                else if (newStatus != NetConnectionStatus.Connected && oldStatus == NetConnectionStatus.Connected)
                {
                    startButton.IsShowing = false;
                    startButton.Enabled = false;
                    infoText.TextValue = "Client disconnected";
                }
                oldStatus = newStatus;
            }

            NetIncomingMessage msg;
            while ((msg = server.ReadMessage()) != null)
            {
                if (msg.MessageType == NetIncomingMessageType.ConnectionApproval)
                    msg.SenderConnection.Approve();
                server.Recycle(msg);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            cursor.Draw(gameTime);
            backgroundImage.Draw(gameTime);
            ipText.Draw(gameTime);
            infoText.Draw(gameTime);
            ipPopup.Draw(gameTime);
            ipButton.Draw(gameTime);
            hostButton.Draw(gameTime);
            connectButton.Draw(gameTime);
            backButton.Draw(gameTime);
            startButton.Draw(gameTime);
        }

        private bool IsValidIP(string ip)
        {
            int numDots = 0;
            int lastDotPos = 0;
            for (int i = 0; i < ip.Length; i++)
            {
                if (ip.ToCharArray()[i] == '.')
                {
                    if (lastDotPos == i)
                        return false;
                    string sub = ip.Substring(lastDotPos, i - lastDotPos);
                    if (Convert.ToInt32(sub) > 255)
                        return false;
                    numDots++;
                    lastDotPos = i + 1;
                }
            }
            if (Convert.ToInt32(ip.Substring(lastDotPos, ip.Length - lastDotPos)) > 255)
                return false;

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
                    ipText.TextValue = "IP: " + ip;
                    infoText.TextValue = "";
                }
                else
                {
                    infoText.TextValue = "Entered IP not valid";
                }
        }

        private void OnIPButtonClick(object o, EventArgs e)
        {
            if (connecting)
                StopConnecting();
            if (hosting)
                StopHosting();
            ipPopup.Show();
        }

        private void OnConnectButtonClick(object o, EventArgs e)
        {
            if (hosting)
                StopHosting();
            if (!connecting)
            {
                if (IsValidIP(ip))
                    StartConnecting();
                else
                    ipText.TextValue = "IP invalid";
            }
            else
                StopConnecting();
        }

        private void StartConnecting()
        {
            connecting = true;
            connectButton.Text = "Cancel connection";
            infoText.TextValue = "Connecting...";
            NetPeerConfiguration config = new NetPeerConfiguration("NotTetris");
            client = new NetClient(config);
            client.Start();
            connection = client.Connect(ip, Convert.ToInt32(port));
        }

        private void StopConnecting()
        {
            client.Shutdown("Client shutdown");
            connecting = false;
            connectButton.Text = "Connect to IP";
            infoText.TextValue = "";
        }

        private void OnHostButtonClick(object o, EventArgs e)
        {
            if (connecting)
                StopConnecting();
            if (!hosting)
                StartHosting();
            else
                StopHosting();
        }

        private void StartHosting()
        {
            hosting = true;
            hostButton.Text = "Stop Hosting";
            infoText.TextValue = "Hosting...";
            NetPeerConfiguration config = new NetPeerConfiguration("NotTetris");
            config.MaximumConnections = 1;
            config.Port = Convert.ToInt32(port);
            server = new NetServer(config);
            server.Start();
        }

        private void StopHosting()
        {
            server.Shutdown("Server shutdown");
            hosting = false;
            hostButton.Text = "Host Game";
            infoText.TextValue = "";
        }

        private void OnBackButtonClick(object o, EventArgs e)
        {
            if (hosting)
                StopHosting();
            if (connecting)
                StopConnecting();
            settings.IP = ip;
            settings.PORT = port;
            NewScreen(new MainMenu());
        }
        private void OnStartButtonClick(object o, EventArgs e)
        {
            NetOutgoingMessage message = server.CreateMessage();
            message.Write("Start");
            WriteSettingsToMessage(message, this.settings);
            server.SendMessage(message, connection, NetDeliveryMethod.ReliableOrdered);
            NewScreen(new NetworkGame(settings, server));
        }

        private void WriteSettingsToMessage(NetOutgoingMessage msg, Settings settings)
        {
            msg.Write(settings.Difficulty);
            msg.Write(settings.BlockDropSpeed);
            msg.Write(settings.PlayTime);
            msg.Write(settings.PlayfieldSize);
        }

        private Settings ReadSettingsFromMessage(NetIncomingMessage msg)
        {
            Settings readSettings = settings.Clone();
            readSettings.Difficulty = msg.ReadString();
            readSettings.BlockDropSpeed = (float)Convert.ToDouble(msg.ReadFloat());
            readSettings.PlayTime = Convert.ToInt32(msg.ReadInt32());
            readSettings.PlayfieldSize = Convert.ToInt32(msg.ReadInt32());
            return readSettings;
        }


    }
}
