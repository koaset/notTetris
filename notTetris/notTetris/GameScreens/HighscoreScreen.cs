﻿using System;
using System.Collections.Generic;
using System.Text;
using NotTetris.Graphics;
using NotTetris.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NotTetris.GameScreens
{
    /// <summary>
    /// Shows the single player high scores saved
    /// </summary>
    class HighscoreScreen : GameScreen
    {
        Image backgroundImage;
        TextButton backButton;
        Text header;
        Text[] highscores;
        Cursor cursor;

        public HighscoreScreen()
        {
            backgroundImage = new Image();
            header = new Text();
            highscores = new Text[5];
            backButton = new TextButton();
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
            backButton.Text = "Back";
            backButton.Position = new Vector2(100, 620);
            backButton.Click += new ButtonEventHandler(OnBack);

            header.Initialize();
            header.Font = FontNames.Segoe_UI_Mono;
            header.Layer = 0.7f;
            header.IsCentered = true;
            header.Position = new Vector2(500, 100);
            header.TextColor = Color.Navy;
            header.TextValue = "Single Player\n High Score";

            cursor.Initialize();

            for (int i = 0; i < highscores.Length; i++)
            {
                highscores[i] = new Text();
                highscores[i].Initialize();
                highscores[i].Font = FontNames.Segoe_UI_Mono;
                highscores[i].Layer = 0.7f;
                highscores[i].Position = new Vector2(375, 175 + i * 75);
                highscores[i].TextColor = Color.MintCream;
                int num = i + 1;
                highscores[i].TextValue = num.ToString() + ":    " + settings.score[i].ToString("F0");
            }
            
            base.Initialize(spriteBatch, settings);
        }

        public override void LoadContent()
        {
            LoadAndAddToDrawables(backgroundImage);
            LoadAndAddToDrawables(backButton);
            LoadAndAddToDrawables(cursor);
            LoadAndAddToDrawables(header);
            foreach (Text score in highscores)
                LoadAndAddToDrawables(score);
        }

        public override void Update(GameTime gameTime)
        {
            if (isFocused)
                backButton.Update(gameTime);
            cursor.Update();
        }

        private void OnBack(object o, EventArgs e)
        {
            NewScreen(new MainMenu());
        }
    }
}
