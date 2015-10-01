using System;
using System.Collections.Generic;
using System.Text;
using NotTetris.Graphics;
using NotTetris.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NotTetris.GameScreens
{
    class HighscoreScreen : GameScreen
    {
        Image backgroundImage;
        Button backButton;
        Text header;
        Text[] highscores;
        Cursor cursor;

        public HighscoreScreen()
        {
            backgroundImage = new Image();
            header = new Text();
            highscores = new Text[5];
            backButton = new Button(ButtonType.Back, new Vector2(100, 620));
            cursor = new Cursor();
        }

        public override void Initialize(SpriteBatch spriteBatch, Settings settings)
        {
            backgroundImage.Initialize();
            backgroundImage.Layer = 0.1f;
            backgroundImage.Position = new Vector2(500, 360);
            backgroundImage.Size = new Vector2(1000, 720);
            backgroundImage.TextureName = TextureNames.game_background;            

            backButton.Initialize();
            backButton.Click += new ButtonEventHandler(OnBack);

            header.Initialize();
            header.Font = FontNames.Segoe_UI_Mono;
            header.Layer = 0.7f;
            header.IsCentered = true;
            header.Position = new Vector2(500, 50);
            header.TextColor = Color.Navy;
            header.TextValue = "High Score";

            cursor.Initialize();

            for (int i = 0; i < highscores.Length; i++)
            {
                highscores[i] = new Text();
                highscores[i].Initialize();
                highscores[i].Font = FontNames.Segoe_UI_Mono;
                highscores[i].Layer = 0.7f;
                highscores[i].Position = new Vector2(300, 175 + i * 75);
                highscores[i].TextColor = Color.MintCream;
                int num = i + 1;
                highscores[i].TextValue = num.ToString() + ":    " + settings.score[i].ToString("F0");
            }
            
            base.Initialize(spriteBatch, settings);
        }

        public override void LoadContent()
        {
            backgroundImage.LoadContent(spriteBatch);
            backButton.LoadContent(spriteBatch);
            cursor.LoadContent(spriteBatch);
            header.LoadContent(spriteBatch);

            foreach (Text score in highscores)
                score.LoadContent(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            backgroundImage.Update(gameTime);
            backButton.Update(gameTime);
            cursor.Update();
        } 

        public override void Draw(GameTime gameTime)
        {
            backgroundImage.Draw(gameTime);
            backButton.Draw(gameTime);
            cursor.Draw(gameTime);
            header.Draw(gameTime);
            foreach (Text score in highscores)
                score.Draw(gameTime);
        }

        private void OnBack(object o, EventArgs e)
        {
            NewScreen(ScreenType.MainMenu);
        }
    }
}
