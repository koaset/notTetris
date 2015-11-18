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

        public const string SETTINGSPATH = "Settings.xml";
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
            currentScreen.SetFocus(this.IsActive);

            KeyboardState newState = Keyboard.GetState();

            currentScreen.Update(gameTime);

            oldState = newState;

            base.Update(gameTime);
        }

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

        private void ChangeScreen(GameScreen newScreen)
        {
            spriteBatch.Dispose();

            if (newScreen == null)
            {
                settings.Save(SETTINGSPATH);
                UnloadContent();
                Exit();
            }
            else
            {
                spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
                newScreen.Initialize(spriteBatch, settings);
                newScreen.ChangeScreen += new ChangeScreenEventHandler(OnChangeScreen);
                newScreen.LoadContent();
                currentScreen = newScreen;
            }

            GC.Collect();
        }

        private void OnChangeScreen(object o, ChangeScreenEventArgs e)
        {
            ChangeScreen(e.NewScreen);
        }
    }
}
