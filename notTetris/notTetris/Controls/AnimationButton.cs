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
    

    public enum AnimationButtonType
    {
        Increase,
        Decrease,
    }

    public delegate void ButtonEventHandler(object o, EventArgs e);

    class AnimationButton
    {
        private enum ButtonState
        {
            Normal,
            Hover,
            Down,
        }

        private MouseState oldState;
        private AnimationButtonType type;
        private bool enabled;
        private Vector2 position;
        private Animation buttonImage;
        public event ButtonEventHandler Click;

        
        public AnimationButton(AnimationButtonType type, Vector2 position)
        {
            this.type = type;
            buttonImage = new Animation();
            this.position = position;

            buttonImage.Position = position;

            #region Set Texture

            if (type == AnimationButtonType.Increase || type == AnimationButtonType.Decrease)
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
            if (type == AnimationButtonType.Increase)
                buttonImage.Size = new Vector2(46, 52);
            else if (type == AnimationButtonType.Decrease)
            {
                buttonImage.Size = new Vector2(46, 52);
                buttonImage.Effects = SpriteEffects.FlipHorizontally;
            }
            #endregion

            SetImage(ButtonState.Normal);
        }

        public void LoadContent(SpriteBatch spriteBatch)
        {
            buttonImage.LoadContent(spriteBatch);
        }

        private void SetImage(ButtonState image)
        {

            if (image == ButtonState.Normal)
                buttonImage.CurrentFrame = 0.0f;

            else if (image == ButtonState.Hover)
                buttonImage.CurrentFrame = 1.0f;


            else if (image == ButtonState.Down)
                buttonImage.CurrentFrame = 2.0f;
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
                    SetImage(ButtonState.Hover);

                    if (newState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                        SetImage(ButtonState.Down);

                    if (newState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && oldState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                        Click(this, EventArgs.Empty);
                }
                else
                    SetImage(ButtonState.Normal);

                oldState = newState;
            }
        }

        public void Draw(GameTime gameTime)
        {
            buttonImage.Draw(gameTime);
        }
    }
}
