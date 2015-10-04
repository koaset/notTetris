﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NotTetris.Controls;
using NotTetris.Graphics;

namespace NotTetris.GameScreens
{
    class MainMenu : GameScreen
    {
        Cursor cursor;
        TextButton onePlayerGameButton;
        TextButton twoPlayerGameButton;
        TextButton settingsButton;
        TextButton highscoreButton;
        TextButton exitButton;
        Image backgroundImage;
        Image titleImage;

        public MainMenu()
        {
            cursor = new Cursor();
            backgroundImage = new Image();
            onePlayerGameButton = new TextButton(TextButtonType.OnePlayer, new Vector2(300, 150f));
            twoPlayerGameButton = new TextButton(TextButtonType.TwoPlayer, new Vector2(300f, 250f));
            highscoreButton = new TextButton(TextButtonType.HighScore, new Vector2(300, 350));
            settingsButton = new TextButton(TextButtonType.Settings, new Vector2(300, 450));
            exitButton = new TextButton(TextButtonType.Exit, new Vector2(300, 550));
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

            onePlayerGameButton.Initialize();
            onePlayerGameButton.Click += new ButtonEventHandler(StartOnePlayerGame);
            twoPlayerGameButton.Initialize();
            twoPlayerGameButton.Click += new ButtonEventHandler(StartTwoPlayerGame);
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
            NewScreen(ScreenType.OnePlayerGame);
        }

        private void StartTwoPlayerGame(object o, EventArgs e)
        {
            NewScreen(ScreenType.TwoPlayerGame);
        }

        private void StartSettings(object o, EventArgs e)
        {
            NewScreen(ScreenType.SettingsMenu);
        }

        private void StartHighScore(object o, EventArgs e)
        {
            NewScreen(ScreenType.HighscoreScreen);
        }

        private void OnExit(object o, EventArgs e)
        {
            NewScreen(ScreenType.Exit);
        }
        #endregion

        public override void LoadContent()
        {
            cursor.LoadContent(spriteBatch);
            backgroundImage.LoadContent(spriteBatch);
            onePlayerGameButton.LoadContent(spriteBatch);
            twoPlayerGameButton.LoadContent(spriteBatch);
            settingsButton.LoadContent(spriteBatch);
            highscoreButton.LoadContent(spriteBatch);
            exitButton.LoadContent(spriteBatch);
            titleImage.LoadContent(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            cursor.Update();
            backgroundImage.Update(gameTime);
            onePlayerGameButton.Update(gameTime);
            twoPlayerGameButton.Update(gameTime);
            settingsButton.Update(gameTime);
            highscoreButton.Update(gameTime);
            exitButton.Update(gameTime);
            titleImage.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            cursor.Draw(gameTime);
            backgroundImage.Draw(gameTime);
            onePlayerGameButton.Draw(gameTime);
            twoPlayerGameButton.Draw(gameTime);
            settingsButton.Draw(gameTime);
            highscoreButton.Draw(gameTime);
            exitButton.Draw(gameTime);
            titleImage.Draw(gameTime);
        }
    }
}
