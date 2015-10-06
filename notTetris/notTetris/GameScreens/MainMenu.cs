using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NotTetris.Controls;
using NotTetris.Graphics;

namespace NotTetris.GameScreens
{
    class MainMenu : GameScreen
    {
        Cursor cursor;
        TextButton singleplayerButton;
        TextButton splitscreenButton;
        TextButton networkButton;
        TextButton settingsButton;
        TextButton highscoreButton;
        TextButton exitButton;
        Image backgroundImage;
        Image titleImage;

        public MainMenu()
        {
            cursor = new Cursor();
            backgroundImage = new Image();
            singleplayerButton = new TextButton(TextButtonType.Singleplayer, new Vector2(300f, 150f));
            splitscreenButton = new TextButton(TextButtonType.Splitscreen, new Vector2(300f, 235));
            networkButton = new TextButton(TextButtonType.Network, new Vector2(300f, 320));
            highscoreButton = new TextButton(TextButtonType.HighScore, new Vector2(300, 405));
            settingsButton = new TextButton(TextButtonType.Settings, new Vector2(300f, 490));
            exitButton = new TextButton(TextButtonType.Exit, new Vector2(300f, 575));
            titleImage = new Image();
        }

        public override void Initialize(SpriteBatch spriteBatch, Settings settings)
        {
            base.Initialize(spriteBatch, settings);

            cursor.Initialize();

            backgroundImage.Initialize();
            backgroundImage.TextureName = TextureNames.mainmenu_background;
            backgroundImage.Size = new Vector2(1000, 720);
            backgroundImage.Position = new Vector2(500, 360);
            backgroundImage.Layer = 0.4f;

            titleImage.Initialize();
            titleImage.TextureName = TextureNames.title;
            titleImage.Size = new Vector2(281, 58);
            titleImage.Position = new Vector2(230, 95);
            titleImage.Layer = 0.6f;

            singleplayerButton.Initialize();
            singleplayerButton.Click += new ButtonEventHandler(StartOnePlayerGame);
            splitscreenButton.Initialize();
            splitscreenButton.Click += new ButtonEventHandler(StartTwoPlayerGame);
            networkButton.Initialize();
            networkButton.Click += new ButtonEventHandler(StartNetworkGame);
            settingsButton.Initialize();
            settingsButton.Click += new ButtonEventHandler(StartSettings);
            highscoreButton.Initialize();
            highscoreButton.Click += new ButtonEventHandler(StartHighScore);
            exitButton.Initialize();
            exitButton.Click += new ButtonEventHandler(OnExit);
        }

        #region Events

        private void StartOnePlayerGame(object o, EventArgs e)
        {
            NewScreen(new SinglePlayerGame(settings));
            
        }

        private void StartTwoPlayerGame(object o, EventArgs e)
        {
            NewScreen(new SplitScreenGame(settings));
        }

        private void StartNetworkGame(object o, EventArgs e)
        {
            NewScreen(new NetworkGameSetup());
        }

        private void StartSettings(object o, EventArgs e)
        {
            NewScreen(new SettingsMenu());
        }

        private void StartHighScore(object o, EventArgs e)
        {
            NewScreen(new HighscoreScreen());
        }

        private void OnExit(object o, EventArgs e)
        {
            NewScreen(null);
        }
        #endregion

        public override void LoadContent()
        {
            cursor.LoadContent(spriteBatch);
            backgroundImage.LoadContent(spriteBatch);
            singleplayerButton.LoadContent(spriteBatch);
            splitscreenButton.LoadContent(spriteBatch);
            networkButton.LoadContent(spriteBatch);
            settingsButton.LoadContent(spriteBatch);
            highscoreButton.LoadContent(spriteBatch);
            exitButton.LoadContent(spriteBatch);
            titleImage.LoadContent(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            cursor.Update();
            if (isFocused)
            {
                singleplayerButton.Update(gameTime);
                splitscreenButton.Update(gameTime);
                networkButton.Update(gameTime);
                settingsButton.Update(gameTime);
                highscoreButton.Update(gameTime);
                exitButton.Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            cursor.Draw(gameTime);
            backgroundImage.Draw(gameTime);
            singleplayerButton.Draw(gameTime);
            splitscreenButton.Draw(gameTime);
            networkButton.Draw(gameTime);
            settingsButton.Draw(gameTime);
            highscoreButton.Draw(gameTime);
            exitButton.Draw(gameTime);
            titleImage.Draw(gameTime);
        }
    }
}
