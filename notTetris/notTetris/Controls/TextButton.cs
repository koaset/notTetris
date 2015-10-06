using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NotTetris.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace NotTetris.Controls
{
    class TextButton
    {
        private enum ButtonState
        {
            Normal,
            Hover,
            Down,
        }

        public bool IsShowing { get { return buttonText.IsShowing; } set { buttonText.IsShowing = value; } }
        public bool Enabled { get { return enabled; } set { enabled = value; } }
        public string Text { get { return buttonText.TextValue; } set { buttonText.TextValue = value; } }
        public Vector2 Position { get { return buttonText.Position; } set { buttonText.Position = value; } }
        public Vector2 Scale { get { return buttonText.Scale; } set { buttonText.Scale = value; } }

        private MouseState oldState;
        private bool enabled;
        private OutlineText buttonText;
        private Vector2 size;
        public event ButtonEventHandler Click;

        public TextButton()
        {
            buttonText = new OutlineText();
        }

        public void Initialize()
        {
            enabled = true;
            buttonText.Initialize();
            buttonText.IsCentered = false;
            buttonText.Font = FontNames.Segoe_UI_Mono_Large;
            buttonText.TextColor = Color.Yellow;
            buttonText.OutlineColor = Color.Black;
            buttonText.Layer = 0.7f;
            buttonText.OutlineSize = 1.0f;

            SetState(ButtonState.Normal);
        }

        public void LoadContent(SpriteBatch spriteBatch)
        {
            buttonText.LoadContent(spriteBatch);
            size = buttonText.GetSize();
        }

        private void SetState(ButtonState image)
        {

            if (image == ButtonState.Normal)
                buttonText.TextColor = Color.Yellow;

            else if (image == ButtonState.Hover)
                buttonText.TextColor = Color.Orange;
                

            else if (image == ButtonState.Down)
                buttonText.TextColor = Color.Red;
        }

        public void Update(GameTime gameTime)
        {
            if (enabled)
            {
                MouseState newState = Mouse.GetState();
                Rectangle mousePosition = new Rectangle(newState.X, newState.Y, 1, 1);
                Rectangle buttonPosition = new Rectangle(Convert.ToInt16(buttonText.Position.X), Convert.ToInt16(buttonText.Position.Y),
                    Convert.ToInt16(size.X), Convert.ToInt16(size.Y));

                if (mousePosition.Intersects(buttonPosition))
                {
                    SetState(ButtonState.Hover);

                    if (newState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                        SetState(ButtonState.Down);

                    if (newState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && oldState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                        Click(this, EventArgs.Empty);
                }
                else
                    SetState(ButtonState.Normal);

                oldState = newState;
            }
        }

        public void Draw(GameTime gameTime)
        {
            buttonText.Draw(gameTime);
        }
    }
}
