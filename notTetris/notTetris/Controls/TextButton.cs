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
    #region ButtonType Enum
    public enum TextButtonType
    {
        OnePlayer,
        TwoPlayer,
        Ok,
        Cancel,
        Settings,
        Exit,
        ChangeDifficulty,
        HighScore,
        Back,
    }
    #endregion

    class TextButton
    {
        private enum ButtonState
        {
            Normal,
            Hover,
            Down,
        }

        private MouseState oldState;
        private TextButtonType type;
        private bool enabled;
        private Vector2 position;
        private OutlineText buttonText;
        private Vector2 size;
        public event ButtonEventHandler Click;

        public TextButton(TextButtonType type, Vector2 position)
        {
            this.type = type;
            this.position = position;
            buttonText = new OutlineText();
        }

        public void Initialize()
        {
            enabled = true;
            
            buttonText.Initialize();
            buttonText.IsCentered = false;
            buttonText.Font = FontNames.Segoe_UI_Mono_Large;
            buttonText.Position = position;
            buttonText.TextColor = Color.Yellow;
            buttonText.OutlineColor = Color.Black;
            buttonText.Position = position;
            buttonText.Layer = 0.8f;
            buttonText.OutlineSize = 1.0f;

            #region Set Text
            if (type == TextButtonType.OnePlayer)
                buttonText.TextValue = "One Player Game";
            else if (type == TextButtonType.Cancel)
                buttonText.TextValue = "Cancel";
            else if (type == TextButtonType.Ok)
                buttonText.TextValue = "Ok";
            else if (type == TextButtonType.TwoPlayer)
                buttonText.TextValue = "Two Player Game";
            else if (type == TextButtonType.Settings)
                buttonText.TextValue = "Settings";
            else if (type == TextButtonType.Exit)
                buttonText.TextValue = "Exit";
            else if (type == TextButtonType.ChangeDifficulty)
            {
                buttonText.TextValue = "Change Difficulty";
                buttonText.Scale = new Vector2(0.75f);
            }
            else if (type == TextButtonType.HighScore)
                buttonText.TextValue = "High Score";
            else if (type == TextButtonType.Back)
                buttonText.TextValue = "Back";
            #endregion

            //Todo: set size for all buttontypes
            size = new Vector2(250f, 50f);

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
                Rectangle buttonPosition = new Rectangle(Convert.ToInt16(position.X), Convert.ToInt16(position.Y),
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
