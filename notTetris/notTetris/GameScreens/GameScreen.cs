using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NotTetris.GameScreens
{
    /// <summary>
    /// Base class different game screens
    /// </summary>
    internal abstract class GameScreen
    {
        public event ChangeScreenEventHandler ChangeScreen;
        protected SpriteBatch spriteBatch;
        protected Settings settings;
        protected KeyboardState oldState;
        protected bool isFocused;
        protected bool mouseVisible;
        protected static Vector2 WINDOWSIZE = new Vector2(1000, 720);

        public virtual void Initialize(SpriteBatch spriteBatch, Settings settings)
        {
            this.spriteBatch = spriteBatch;
            this.settings = settings;
            mouseVisible = false;
        }

        public abstract void LoadContent();

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(GameTime gameTime);

        protected virtual void NewScreen(GameScreen newScreen)
        {
            ChangeScreenEventArgs args = new ChangeScreenEventArgs(newScreen);
            ChangeScreen(this, args);
        }

        public void SetFocus(bool focus)
        {
            isFocused = focus;
        }

        public virtual GameResult GetResults()
        {
            return null;
        }
    }

    internal class ChangeScreenEventArgs : EventArgs
    {
        private GameScreen newScreen;

        public ChangeScreenEventArgs(GameScreen newScreen) {this.newScreen = newScreen;}

        public GameScreen NewScreen
        {
            get { return newScreen; }
        }
    }

    internal delegate void ChangeScreenEventHandler(object o, ChangeScreenEventArgs e);
}
