using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NotTetris.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NotTetris.Controls
{
    class TextBox
    {
        public Vector2 Position 
        {
            get { return background.Position; }
            set { text.Position = value; background.Position = value; }
        }

        Text text;
        Image background;

        public TextBox()
        {
            text = new Text();
            background = new Image();
        }

        public void Initialize()
        {
            text.Initialize();

            background.Initialize();
        }

        public void LoadContent(SpriteBatch spriteBatch)
        {
            text.LoadContent(spriteBatch);
            background.LoadContent(spriteBatch);
        }

        public void Update(GameTime gameTime)
        { 

        }

        public void Draw(GameTime gameTime)
        {
 
        }
    }
}
