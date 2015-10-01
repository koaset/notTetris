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
        OnePlayerGame,
        TwoPlayerGame,
        ResultsScreen,
        SettingsMenu,
        HighscoreScreen,
    }
    #endregion

    public delegate void ChangeScreenEventHandler(object o, ScreenEventArgs e);

    abstract class GameScreen
    {
        public event ChangeScreenEventHandler ChangeScreen;
        protected SpriteBatch spriteBatch;
        protected Settings settings;

        public virtual void Initialize(SpriteBatch spriteBatch, Settings settings)
        {
            this.spriteBatch = spriteBatch;
            this.settings = settings;
        }

        public abstract void LoadContent();

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(GameTime gameTime);

        protected void NewScreen(ScreenType type)
        {
            ScreenEventArgs args = new ScreenEventArgs();
            args.ScreenType = type;
            ChangeScreen(this, args);
        }
    }

    public class ScreenEventArgs : EventArgs
    {
        private ScreenType screenType;

        public ScreenType ScreenType
        {
            get { return screenType; }
            set { screenType = value; }
        }
    }
}
