using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NotTetris.Graphics;

namespace NotTetris.Controls
{
    /// <summary>
    /// A popup image control
    /// </summary>
    class Popup : Image
    {
        public virtual event ClosePopupEventHandler ClosePopup;
        protected KeyboardState oldState;

        public override void Initialize()
        {
            base.Initialize();
            base.isShowing = false;
            base.TextureName = TextureNames.popup_background;
            base.position = new Vector2(500f, 300f);
            base.Size = new Vector2(400f, 200f);
            base.layer = 0.8f;
        }

        public override void Update(GameTime gameTime)
        {
            if (isShowing)
            {
                KeyboardState newState = Keyboard.GetState();

                if (oldState.IsKeyDown(Keys.Escape))
                    Close();

                oldState = newState;
            }
        }

        public virtual void Show()
        {

            isShowing = true;
        }

        public virtual void Close()
        {
            isShowing = false;
            OnClose(new EventArgs());
        }

        protected virtual void OnClose(EventArgs e)
        {
            ClosePopup(this, e);
        }
    }

    public delegate void ClosePopupEventHandler(object o, EventArgs e);
}
