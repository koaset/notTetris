using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Not_Tetris__Rework_.GameScreens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NotTetris.GameScreens
{
    #region ScreenType enum
    public enum ScreenType
    {
        Exit,
        MainMenu,
        SingleplayerGame,
        SplitscreenGame,
        NetworkGame,
        NetworkGameSetup,
        ResultsScreen,
        SettingsMenu,
        HighscoreScreen,
        HostScreen,
        ConnectionScreen,
    }
    #endregion

    internal delegate void ChangeScreenEventHandler(object o, ScreenEventArgs e);

    internal abstract class GameScreen
    {
        public event ChangeScreenEventHandler ChangeScreen;
        protected SpriteBatch spriteBatch;
        protected Settings settings;
        protected KeyboardState oldState;
        protected bool isFocused;
        protected bool mouseVisible;

        public virtual void Initialize(SpriteBatch spriteBatch, Settings settings)
        {
            this.spriteBatch = spriteBatch;
            this.settings = settings;
            mouseVisible = false;
        }

        public abstract void LoadContent();

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(GameTime gameTime);

        protected void NewScreen(GameScreen newScreen)
        {
            ScreenEventArgs args = new ScreenEventArgs(newScreen);
            ChangeScreen(this, args);
        }

        public void SetFocus(bool focus)
        {
            isFocused = focus;
        }

        public virtual Results GetResults()
        {
            return null;
        }
    }

    internal class ScreenEventArgs : EventArgs
    {
        private GameScreen newScreen;

        public ScreenEventArgs(GameScreen newScreen) {this.newScreen = newScreen;}

        public GameScreen NewScreen
        {
            get { return newScreen; }
        }
    }
}
