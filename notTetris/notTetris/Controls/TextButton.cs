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
        Singleplayer,
        Splitscreen,
        Network,
        Ok,
        Cancel,
        Settings,
        Exit,
        ChangeDifficulty,
        HighScore,
        Back,
        IP,
        Host,
        Connect,
        Start,
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

        public bool IsShowing { get { return buttonText.IsShowing; } set { buttonText.IsShowing = value; } }
        public bool Enabled { get { return enabled; } set { enabled = value; } }

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
            buttonText.Layer = 0.7f;
            buttonText.OutlineSize = 1.0f;

            #region Set Text
            if (type == TextButtonType.Singleplayer)
                buttonText.TextValue = "Single Player Game";
            else if (type == TextButtonType.Cancel)
                buttonText.TextValue = "Cancel";
            else if (type == TextButtonType.Ok)
                buttonText.TextValue = "Ok";
            else if (type == TextButtonType.Network)
                buttonText.TextValue = "Network Game";
            else if (type == TextButtonType.Splitscreen)
                buttonText.TextValue = "Split Screen Game";
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
            else if (type == TextButtonType.IP)
            {
                buttonText.TextValue = "Change target IP";
                buttonText.Scale = new Vector2(0.75f);
            }
            else if (type == TextButtonType.Host)
            {
                buttonText.TextValue = "Host Game";
                buttonText.Scale = new Vector2(0.75f);
            }
            else if (type == TextButtonType.Connect)
            {
                buttonText.TextValue = "Connect to IP";
                buttonText.Scale = new Vector2(0.75f);
            }
            else if (type == TextButtonType.Start)
                buttonText.TextValue = "Start";
            #endregion

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
