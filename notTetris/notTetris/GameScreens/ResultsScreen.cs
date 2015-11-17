using System;
using System.Collections.Generic;
using System.Text;
using NotTetris.Graphics;
using NotTetris.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace NotTetris.GameScreens
{
    class ResultsScreen : GameScreen
    {
        Cursor cursor;
        Image backGroundImage;
        Text gameoverText;
        Text infoText;
        Text time;
        Text p1Score;
        Text p2Score;
        bool newHighscore;
        private GameResult results;
        bool isNetwork;

        public ResultsScreen(GameResult results, bool isNetwork)
        {
            this.results = results;
            this.isNetwork = isNetwork;
            backGroundImage = new Image();
            gameoverText = new Text();
            infoText = new Text();
            results = new GameResult();
            time = new Text();
            p1Score = new Text();
            p2Score = new Text();
            cursor = new Cursor();
        }

        public override void Initialize(SpriteBatch spriteBatch, Settings settings)
        {
            base.Initialize(spriteBatch, settings);


            backGroundImage.Initialize();
            backGroundImage.TextureName = TextureNames.game_background;
            backGroundImage.Size = new Vector2(1000f, 720f);
            backGroundImage.Position = backGroundImage.Size / 2;

            gameoverText.Initialize();
            gameoverText.IsCentered = true;
            gameoverText.Font = FontNames.Segoe_UI_Mono;
            gameoverText.Layer = 0.7f;
            gameoverText.Position = new Vector2(500, 150);
            gameoverText.TextColor = Color.MintCream;
            gameoverText.TextValue = "GAME OVER";

            infoText.Initialize();
            infoText.IsCentered = true;
            infoText.Font = FontNames.Segoe_UI_Mono;
            infoText.Layer = 0.7f;
            infoText.Position = new Vector2(500, 300);
            infoText.TextColor = Color.MintCream;
            infoText.TextValue = "Press F10 to return to the menu";

            time.Initialize();
            time.Font = FontNames.Segoe_UI_Mono;
            time.Layer = 0.7f;
            time.IsCentered = true;
            time.Position = new Vector2(500, 425);
            time.TextColor = Color.Red;
            time.TextValue = "Time played: " + results.Time.Minutes.ToString() + ":" + results.Time.Seconds.ToString();

            p1Score.Initialize();
            p1Score.Font = FontNames.Segoe_UI_Mono;
            p1Score.Layer = 0.7f;
            p1Score.IsCentered = true;
            p1Score.Position = new Vector2(500, 500);
            p1Score.TextColor = Color.Red;
            p1Score.TextValue = "Score: " + results.Player1Score.ToString("F0");

            cursor.Initialize();

            if (!results.IsSinglerplayer)
            {
                if (results.Player1Won)
                {
                    if (!isNetwork)
                        gameoverText.TextValue = "Player 1 Won!";
                    else
                        gameoverText.TextValue = "You Won!";
                }
                else
                {
                    if (!isNetwork)
                        gameoverText.TextValue = "Player 2 Won!";
                    else
                        gameoverText.TextValue = "You Lost!";
                }

                if (!isNetwork)
                    p1Score.TextValue = "Player 1 Score: " + results.Player1Score.ToString("F0");
                else
                    p1Score.TextValue = "Your score: " + results.Player1Score.ToString("F0");
                p2Score.Initialize();
                p2Score.Font = FontNames.Segoe_UI_Mono;
                p2Score.Layer = 0.7f;
                p2Score.IsCentered = true;
                p2Score.Position = new Vector2(500, 575);
                p2Score.TextColor = Color.Red;
                if (!isNetwork)
                    p2Score.TextValue = "Player 2 Score: " + results.Player2Score.ToString("F0");
                else
                    p2Score.TextValue = "Opponent Score: " + results.Player2Score.ToString("F0");
                p2Score.IsShowing = true;
            }

            if (results.IsSinglerplayer)
                foreach (int i in settings.score)
                    if (i < results.Player1Score || i < results.Player2Score)
                        newHighscore = true;

            if (newHighscore)
            {
                List<int> newList = new List<int>();
                newList.AddRange(settings.score);
                newList.Add((int)results.Player1Score);
                newList.Sort();
                newList.Reverse();
                newList.RemoveAt(5);
                int[] newHighscoreList = newList.ToArray();
                settings.score = newHighscoreList;
            }
        }

        public override void LoadContent()
        {
            backGroundImage.LoadContent(spriteBatch);
            gameoverText.LoadContent(spriteBatch);
            infoText.LoadContent(spriteBatch);
            time.LoadContent(spriteBatch);
            p1Score.LoadContent(spriteBatch);
            cursor.LoadContent(spriteBatch);

            if (!results.IsSinglerplayer)
            {
                p2Score.LoadContent(spriteBatch);
            }
        }

        public override void Update(GameTime gameTime)
        {

            if (oldState.IsKeyUp(Keys.F10) && Keyboard.GetState().IsKeyDown(Keys.F10))
            {
                if (!isNetwork)
                    NewScreen(new MainMenu());
                else
                    NewScreen(new NetworkGameSetup());
            }

            cursor.Update();
        }

        public override void Draw(GameTime gameTime)
        {
            backGroundImage.Draw(gameTime);
            gameoverText.Draw(gameTime);
            infoText.Draw(gameTime);
            if (!isNetwork)
                time.Draw(gameTime);
            p1Score.Draw(gameTime);
            cursor.Draw(gameTime);

            if (!results.IsSinglerplayer)
            {
                p2Score.Draw(gameTime);
            }
        }
    }
}
