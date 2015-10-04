using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using NotTetris.Graphics;
using NotTetris.GameScreens;

namespace NotTetris
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PuzzleGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        ContentManager content;
        SpriteBatch spriteBatch;
        GameScreen currentScreen;
        KeyboardState oldState;
        Settings settings;

        public static Random r = new Random();

        private const string SETTINGSPATH = "Settings.xml";
        private const int WINDOWWIDTH = 1000;
        private const int WINDOWHEIGHT = 720;

        public PuzzleGame()
        {
            IsFixedTimeStep = false;
            graphics = new GraphicsDeviceManager(this);
            content = new ContentManager(Services);
            graphics.PreferredBackBufferWidth = WINDOWWIDTH;
            graphics.PreferredBackBufferHeight = WINDOWHEIGHT;
            graphics.SynchronizeWithVerticalRetrace = true;
            currentScreen = new MainMenu();
            currentScreen.ChangeScreen += new ChangeScreenEventHandler(OnChangeScreen);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            try
            {
                settings = Settings.Load(SETTINGSPATH);
            }
            catch
            {
                settings = new Settings();
            }

            Window.Title = settings.WindowTitle;

            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            currentScreen.Initialize(spriteBatch, settings);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            GraphicsManager.LoadAllContent(content);
            currentScreen.LoadContent();

            base.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            content.Unload();

            base.UnloadContent();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                KeyboardState newState = Keyboard.GetState();

                if (newState.IsKeyDown(Keys.Escape))
                    Exit();

                if (newState.IsKeyDown(Keys.LeftAlt) && newState.IsKeyDown(Keys.Enter) && oldState.IsKeyUp(Keys.Enter))
                    graphics.ToggleFullScreen();

                if (newState.IsKeyDown(Keys.F10) && oldState.IsKeyUp(Keys.F10))
                    ChangeScreen(ScreenType.MainMenu);

                currentScreen.Update(gameTime);

                oldState = newState;
            }


            base.Update(gameTime);
        }

        int count = 0;
        double time = 0;

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);
            currentScreen.Draw(gameTime);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        #region ChangeScreen

        private void ChangeScreen(ScreenType type)
        {
            spriteBatch.Dispose();
            if (currentScreen is SettingsMenu)
                settings.Save(SETTINGSPATH);

            else if (currentScreen is ResultsScreen)
                settings.Save(SETTINGSPATH);

            if (type == ScreenType.MainMenu)
            {
                currentScreen = new MainMenu();
                currentScreen.ChangeScreen += new ChangeScreenEventHandler(OnChangeScreen);
                spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
                currentScreen.Initialize(spriteBatch, settings);
                currentScreen.LoadContent();
            }

            if (type == ScreenType.OnePlayerGame)
            {
                currentScreen = new OnePlayerGame(settings);
                currentScreen.ChangeScreen += new ChangeScreenEventHandler(OnChangeScreen);
                spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
                currentScreen.Initialize(spriteBatch, settings);
                currentScreen.LoadContent();
            }

            if (type == ScreenType.TwoPlayerGame)
            {
                currentScreen = new TwoPlayerGame(settings);
                currentScreen.ChangeScreen += new ChangeScreenEventHandler(OnChangeScreen);
                spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
                currentScreen.Initialize(spriteBatch, settings);
                currentScreen.LoadContent();
            }

            if (type == ScreenType.ResultsScreen)
            {

                Results r = new Results();

                if (currentScreen is OnePlayerGame)
                {
                    OnePlayerGame g = (OnePlayerGame)currentScreen;
                    r = g.GetResults();
                }
                else if (currentScreen is TwoPlayerGame)
                {
                    TwoPlayerGame g = (TwoPlayerGame)currentScreen;
                    r = g.GetResults();
                }

                currentScreen = new ResultsScreen();
                currentScreen.ChangeScreen += new ChangeScreenEventHandler(OnChangeScreen);
                spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
                ResultsScreen rs = (ResultsScreen)currentScreen;
                rs.SetResults = r;
                currentScreen.Initialize(spriteBatch, settings);
                currentScreen.LoadContent();
            }

            if (type == ScreenType.SettingsMenu)
            {
                currentScreen = new SettingsMenu();
                currentScreen.ChangeScreen += new ChangeScreenEventHandler(OnChangeScreen);
                spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
                currentScreen.Initialize(spriteBatch, settings);
                currentScreen.LoadContent();
            }

            if (type == ScreenType.HighscoreScreen)
            {
                currentScreen = new HighscoreScreen();
                currentScreen.ChangeScreen += new ChangeScreenEventHandler(OnChangeScreen);
                spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
                currentScreen.Initialize(spriteBatch, settings);
                currentScreen.LoadContent();
            }

            if (type == ScreenType.Exit)
            {
                UnloadContent();
                Exit();
            }

            GC.Collect();
        }
        #endregion

        private void OnChangeScreen(object o, ScreenEventArgs e)
        {
            ChangeScreen(e.ScreenType);
        }
    }
}
