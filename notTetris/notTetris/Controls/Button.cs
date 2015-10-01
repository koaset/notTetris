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
    public enum ButtonType
    {
        OnePlayer,
        TwoPlayer,
        Ok,
        Cancel,
        Settings,
        Exit,
        ChangeDifficulty,
        HighScore,
        Increase,
        Decrease,
        Back,
    }
    #endregion

    public delegate void ButtonEventHandler(object o, EventArgs e);

    class Button
    {
        private MouseState oldState;
        private ButtonType type;
        private bool enabled;
        private Vector2 position;
        private Animation buttonImage;
        public event ButtonEventHandler Click;

        private enum ButtonImage
        {
            Normal,
            Hover,
            Down,
        }

        public Button(ButtonType type, Vector2 position)
        {
            this.type = type;
            buttonImage = new Animation();
            this.position = position;

            buttonImage.Position = position;

            #region Set Texture

            if (type == ButtonType.OnePlayer)
                buttonImage.TextureName = TextureNames.button_oneplayer;
            else if (type == ButtonType.Cancel)
                buttonImage.TextureName = TextureNames.button_cancel;
            else if (type == ButtonType.Ok)
                buttonImage.TextureName = TextureNames.button_ok;
            else if (type == ButtonType.TwoPlayer)
                buttonImage.TextureName = TextureNames.button_twoplayer;
            else if (type == ButtonType.Settings)
                buttonImage.TextureName = TextureNames.button_settings;
            else if (type == ButtonType.Exit)
                buttonImage.TextureName = TextureNames.button_exit;
            else if (type == ButtonType.ChangeDifficulty)
                buttonImage.TextureName = TextureNames.button_difficulty;
            else if (type == ButtonType.HighScore)
                buttonImage.TextureName = TextureNames.button_highscore;
            else if (type == ButtonType.Increase || type == ButtonType.Decrease)
                buttonImage.TextureName = TextureNames.button_increase;
            else if (type == ButtonType.Back)
                buttonImage.TextureName = TextureNames.button_back;
            else
                buttonImage.TextureName = TextureNames.button_increase;
            #endregion
        }

        public void Initialize()
        {
            enabled = true;

            buttonImage.Initialize();

            buttonImage.IsStarted = false;
            buttonImage.NumFrames = 4;
            buttonImage.Position = position;

            #region Set Size

            if (type == ButtonType.Cancel)
                buttonImage.Size = new Vector2(164, 55);
            else if (type == ButtonType.Ok)
                buttonImage.Size = new Vector2(94, 55);
            else if (type == ButtonType.OnePlayer)
                buttonImage.Size = new Vector2(320, 55);
            else if (type == ButtonType.TwoPlayer)
                buttonImage.Size = new Vector2(320, 55);
            else if (type == ButtonType.Settings)
                buttonImage.Size = new Vector2(177, 55);
            else if (type == ButtonType.Exit)
                buttonImage.Size = new Vector2(104, 55);
            else if (type == ButtonType.ChangeDifficulty)
                buttonImage.Size = new Vector2(320, 55);
            else if (type == ButtonType.HighScore)
                buttonImage.Size = new Vector2(227, 55);
            else if (type == ButtonType.Increase)
                buttonImage.Size = new Vector2(46, 52);
            else if (type == ButtonType.Decrease)
            {
                buttonImage.Size = new Vector2(46, 52);
                buttonImage.Effects = SpriteEffects.FlipHorizontally;
            }
            else if (type == ButtonType.Back)
                buttonImage.Size = new Vector2(113, 43);
            else
                buttonImage.Size = new Vector2(46, 52);
            #endregion

            SetImage(ButtonImage.Normal);
        }

        public void LoadContent(SpriteBatch spriteBatch)
        {
            buttonImage.LoadContent(spriteBatch);
        }

        private void SetImage(ButtonImage image)
        {

            if (image == ButtonImage.Normal)
                buttonImage.CurrentFrame = 0f;

            else if (image == ButtonImage.Hover)
                buttonImage.CurrentFrame = 1f;

            else if (image == ButtonImage.Down)
                buttonImage.CurrentFrame = 2f;
        }

        public void Update(GameTime gameTime)
        {
            buttonImage.Update(gameTime);

            if (enabled)
            {
                MouseState newState = Mouse.GetState();
                Rectangle mousePosition = new Rectangle(newState.X, newState.Y, 1, 1);

                Vector2 position = buttonImage.Position;
                Vector2 size = buttonImage.Size;

                Rectangle buttonPosition = new Rectangle(Convert.ToInt16(position.X - size.X / 2), Convert.ToInt16(position.Y - size.Y / 2), Convert.ToInt16(size.X), Convert.ToInt16(size.Y));

                if (mousePosition.Intersects(buttonPosition))
                {
                    SetImage(ButtonImage.Hover);

                    if (newState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                        SetImage(ButtonImage.Down);

                    if (newState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && oldState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                        Click(this, EventArgs.Empty);
                }
                else
                    SetImage(ButtonImage.Normal);

                oldState = newState;
            }
        }

        public void Draw(GameTime gameTime)
        {
            buttonImage.Draw(gameTime);
        }
    }
}
