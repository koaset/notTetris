using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NotTetris.Graphics;

namespace NotTetris.Controls
{
    /// <summary>
    /// Mouse cursor
    /// </summary>
    class Cursor
    {
        Image image;

        public Vector2 Position
        {
            get { return image.Position; }
            set { image.Position = value; }
        }

        public void Initialize()
        {
            image = new Image();
            image.Initialize();
            image.Layer = 0.9f;
            image.Size = new Vector2(14f, 28f);
            image.TextureName = TextureNames.mouse_cursor;
            image.Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }

        public void LoadContent(SpriteBatch spriteBatch)
        {
            image.LoadContent(spriteBatch);
        }

        public void Update()
        {
            MouseState state = Mouse.GetState();

            Position = new Vector2(state.X + image.Size.X / 2, state.Y + image.Size.Y / 2);
        }

        public void Draw(GameTime gameTime)
        {
            image.Draw(gameTime);
        }
    }
}
