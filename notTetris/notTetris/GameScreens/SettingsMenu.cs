using System;
using System.Collections.Generic;
using System.Text;
using NotTetris.Graphics;
using NotTetris.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NotTetris.GameScreens
{
    class SettingsMenu : GameScreen
    {
        Image backgroundImage;
        Cursor cursor;
        Text titleText;
        Button okButton;
        Button cancelButton;
        Button difficultyButton;
        Text[] difficulties;
        Button increaseTimeButton;
        Button decreaseTimeButton;
        Text timeText;
        Text timeLimitText;
        Button increaseSizeButton;
        Button decreaseSizeButton;
        Text sizeText;
        Text playfieldSizeText;
        int timeLimit;
        int size;

        public SettingsMenu()
        {
            backgroundImage = new Image();
            cursor = new Cursor();
            difficulties = new Text[3];
            okButton = new Button(ButtonType.Ok, new Vector2(100, 600));
            cancelButton = new Button(ButtonType.Cancel, new Vector2(250, 600));
            difficultyButton = new Button(ButtonType.ChangeDifficulty, new Vector2(200, 200));
            increaseTimeButton = new Button(ButtonType.Increase, new Vector2(500, 325));
            decreaseTimeButton = new Button(ButtonType.Decrease, new Vector2(380, 325));
            increaseSizeButton = new Button(ButtonType.Increase, new Vector2(500, 425));
            decreaseSizeButton = new Button(ButtonType.Decrease, new Vector2(380, 425));
            titleText = new Text();
            timeText = new Text();
            timeLimitText = new Text();
            sizeText = new Text();
            playfieldSizeText = new Text();
        }

        public override void Initialize(SpriteBatch spriteBatch, Settings settings)
        {
            base.Initialize(spriteBatch, settings);

            backgroundImage.Initialize();
            backgroundImage.TextureName = TextureNames.game_background;
            backgroundImage.Size = new Vector2(1000, 720);
            backgroundImage.Position = new Vector2(500, 360);
            backgroundImage.Layer = 0.4f;

            cursor.Initialize();

            for (int i = 0; i < 3; i++)
            {
                difficulties[i] = new Text();
                difficulties[i].Initialize();
                difficulties[i].IsCentered = true;
                difficulties[i].Font = FontNames.Segoe_UI_Mono;
                difficulties[i].Layer = 0.6f;
                difficulties[i].Position = new Vector2(450 + 150 * i, 205);
                difficulties[i].TextColor = Color.Red;
            }

            difficulties[0].TextValue = "Easy";
            difficulties[1].TextValue = "Normal";
            difficulties[2].TextValue = "Hard";

            if (settings.Difficulty == "Easy")
                difficulties[0].TextColor = Color.Blue;
            else if (settings.Difficulty == "Normal")
                difficulties[1].TextColor = Color.Blue;
            else if (settings.Difficulty == "Hard")
                difficulties[2].TextColor = Color.Blue;

            titleText.Initialize();
            titleText.Font = FontNames.Segoe_UI_Mono;
            titleText.TextColor = Color.Blue;
            titleText.TextValue = "Settings";
            titleText.IsCentered = true;
            titleText.Position = new Vector2(500, 50);

            timeText.Initialize();
            timeText.Font = FontNames.Segoe_UI_Mono;
            timeText.TextColor = Color.Blue;
            timeText.TextValue = "2p Time limit:";
            timeText.Position = new Vector2(70, 300);

            timeLimit = settings.PlayTime;
            timeLimitText.Initialize();
            timeLimitText.Font = FontNames.Segoe_UI_Mono;
            timeLimitText.TextColor = Color.Blue;
            timeLimitText.IsCentered = true;
            timeLimitText.TextValue = timeLimit.ToString();
            timeLimitText.Position = new Vector2(440, 325);

            size = settings.PlayfieldSize;
            sizeText.Initialize();
            sizeText.Font = FontNames.Segoe_UI_Mono;
            sizeText.TextColor = Color.Blue;
            sizeText.TextValue = size.ToString();
            sizeText.Position = new Vector2(440, 425);
            sizeText.IsCentered = true;

            playfieldSizeText.Initialize();
            playfieldSizeText.Font = FontNames.Segoe_UI_Mono;
            playfieldSizeText.TextColor = Color.Blue;
            playfieldSizeText.TextValue = "Playfield size:";
            playfieldSizeText.Position = new Vector2(70, 400);

            okButton.Initialize();
            okButton.Click += new ButtonEventHandler(OnOk);
            cancelButton.Initialize();
            cancelButton.Click += new ButtonEventHandler(OnCancel);
            difficultyButton.Initialize();
            difficultyButton.Click += new ButtonEventHandler(OnChangeDifficulty);
            increaseTimeButton.Initialize();
            increaseTimeButton.Click += new ButtonEventHandler(OnIncreaseTime);
            decreaseTimeButton.Initialize();
            decreaseTimeButton.Click += new ButtonEventHandler(OnDecreaseTime);
            increaseSizeButton.Initialize();
            increaseSizeButton.Click += new ButtonEventHandler(OnIncreaseSize);
            decreaseSizeButton.Initialize();
            decreaseSizeButton.Click += new ButtonEventHandler(OnDecreaseSize);
        }

        #region Events

        private void OnOk(object o, EventArgs e)
        {
            if (difficulties[0].TextColor == Color.Blue)
                settings.Difficulty = "Easy";
            else if (difficulties[1].TextColor == Color.Blue)
                settings.Difficulty = "Normal";
            else if (difficulties[2].TextColor == Color.Blue)
                settings.Difficulty = "Hard";
            settings.PlayTime = timeLimit;
            settings.PlayfieldSize = size;

            NewScreen(ScreenType.MainMenu);
        }

        private void OnCancel(object o, EventArgs e)
        {
            NewScreen(ScreenType.MainMenu);
        }

        private void OnChangeDifficulty(object o, EventArgs e)
        {
            if (difficulties[0].TextColor == Color.Blue)
            {
                difficulties[0].TextColor = Color.Red;
                difficulties[1].TextColor = Color.Blue;
            }
            else if (difficulties[1].TextColor == Color.Blue)
            {
                difficulties[1].TextColor = Color.Red;
                difficulties[2].TextColor = Color.Blue;
            }
            else if (difficulties[2].TextColor == Color.Blue)
            {
                difficulties[2].TextColor = Color.Red;
                difficulties[0].TextColor = Color.Blue;
            }
        }

        private void OnIncreaseTime(object o, EventArgs e)
        {
            if (timeLimit < 10)
                timeLimit++;
        }

        private void OnDecreaseTime(object o, EventArgs e)
        {
            if (timeLimit > 1)
                timeLimit--;
        }

        private void OnIncreaseSize(object o, EventArgs e)
        {
            if (size < 13)
                size += 2;
        }

        private void OnDecreaseSize(object o, EventArgs e)
        {
            if (size > 7)
                size -= 2;
        }
        #endregion

        public override void LoadContent()
        {
            backgroundImage.LoadContent(spriteBatch);
            cursor.LoadContent(spriteBatch);

            foreach (Text text in difficulties)
                text.LoadContent(spriteBatch);

            okButton.LoadContent(spriteBatch);
            cancelButton.LoadContent(spriteBatch);

            difficultyButton.LoadContent(spriteBatch);
            increaseTimeButton.LoadContent(spriteBatch);
            decreaseTimeButton.LoadContent(spriteBatch);
            increaseSizeButton.LoadContent(spriteBatch);
            decreaseSizeButton.LoadContent(spriteBatch);
            titleText.LoadContent(spriteBatch);
            timeText.LoadContent(spriteBatch);
            timeLimitText.LoadContent(spriteBatch);
            sizeText.LoadContent(spriteBatch);
            playfieldSizeText.LoadContent(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            backgroundImage.Update(gameTime);
            cursor.Update();

            okButton.Update(gameTime);
            cancelButton.Update(gameTime);

            difficultyButton.Update(gameTime);
            increaseTimeButton.Update(gameTime);
            decreaseTimeButton.Update(gameTime);
            timeLimitText.TextValue = timeLimit.ToString();
            increaseSizeButton.Update(gameTime);
            decreaseSizeButton.Update(gameTime);
            sizeText.TextValue = size.ToString();
        }

        public override void Draw(GameTime gameTime)
        {
            backgroundImage.Draw(gameTime);
            cursor.Draw(gameTime);

            foreach (Text text in difficulties)
                text.Draw(gameTime);

            okButton.Draw(gameTime);
            cancelButton.Draw(gameTime);

            difficultyButton.Draw(gameTime);
            increaseTimeButton.Draw(gameTime);
            decreaseTimeButton.Draw(gameTime);
            increaseSizeButton.Draw(gameTime);
            decreaseSizeButton.Draw(gameTime);
            timeLimitText.Draw(gameTime);
            timeText.Draw(gameTime);
            sizeText.Draw(gameTime);
            playfieldSizeText.Draw(gameTime);

            titleText.Draw(gameTime);
        }
    }
}
