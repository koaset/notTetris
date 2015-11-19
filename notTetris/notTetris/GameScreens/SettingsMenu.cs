using System;
using System.Collections.Generic;
using System.Text;
using NotTetris.Graphics;
using NotTetris.Controls;
using NotTetris.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NotTetris.GameScreens
{
    /// <summary>
    /// The settings menu screen.
    /// </summary>
    class SettingsMenu : GameScreen
    {
        Image backgroundImage;
        Cursor cursor;
        Text titleText;
        TextButton okButton;
        TextButton cancelButton;
        TextButton difficultyButton;
        Text[] difficulties;
        TextButton gameModeButton;
        Text[] gameModes;
        AnimationButton increaseTimeButton;
        AnimationButton decreaseTimeButton;
        Text timeText;
        Text timeLimitText;
        AnimationButton increaseSizeButton;
        AnimationButton decreaseSizeButton;
        Text sizeText;
        Text playfieldSizeText;
        int timeLimit;
        int size;

        public SettingsMenu()
        {
            backgroundImage = new Image();
            cursor = new Cursor();
            difficulties = new Text[3];
            gameModes = new Text[2];
            okButton = new TextButton();
            cancelButton = new TextButton();
            difficultyButton = new TextButton();
            gameModeButton = new TextButton();
            increaseTimeButton = new AnimationButton(AnimationButtonType.Increase, new Vector2(500, 475));
            decreaseTimeButton = new AnimationButton(AnimationButtonType.Decrease, new Vector2(380, 475));
            increaseSizeButton = new AnimationButton(AnimationButtonType.Increase, new Vector2(500, 375));
            decreaseSizeButton = new AnimationButton(AnimationButtonType.Decrease, new Vector2(380, 375));
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

            for (int i = 0; i < difficulties.Length; i++)
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

            for (int i = 0; i < gameModes.Length; i++)
            {
                gameModes[i] = new Text();
                gameModes[i].Initialize();
                gameModes[i].IsCentered = true;
                gameModes[i].Font = FontNames.Segoe_UI_Mono;
                gameModes[i].Layer = 0.6f;
                gameModes[i].Position = new Vector2(600 + 150 * i, 275);
                gameModes[i].TextColor = Color.Red;
            }

            gameModes[0].TextValue = "Normal";
            gameModes[1].TextValue = "Time";

            if (settings.GameType == GameType.Normal)
                gameModes[0].TextColor = Color.Blue;
            else if (settings.GameType == GameType.Time)
                gameModes[1].TextColor = Color.Blue;

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
            timeText.Position = new Vector2(70, 450);

            timeLimit = settings.PlayTime;
            timeLimitText.Initialize();
            timeLimitText.Font = FontNames.Segoe_UI_Mono;
            timeLimitText.TextColor = Color.Blue;
            timeLimitText.IsCentered = true;
            timeLimitText.TextValue = timeLimit.ToString();
            timeLimitText.Position = new Vector2(440, 475);

            size = settings.PlayfieldSize;
            sizeText.Initialize();
            sizeText.Font = FontNames.Segoe_UI_Mono;
            sizeText.TextColor = Color.Blue;
            sizeText.TextValue = size.ToString();
            sizeText.Position = new Vector2(440, 375);
            sizeText.IsCentered = true;

            playfieldSizeText.Initialize();
            playfieldSizeText.Font = FontNames.Segoe_UI_Mono;
            playfieldSizeText.TextColor = Color.Blue;
            playfieldSizeText.TextValue = "Playfield size:";
            playfieldSizeText.Position = new Vector2(70, 350);

            okButton.Initialize();
            okButton.Text = "Ok";
            okButton.Position = new Vector2(100f, 600f);
            okButton.Click += new ButtonEventHandler(OnOk);

            cancelButton.Initialize();
            cancelButton.Text = "Cancel";
            cancelButton.Position = new Vector2(250f, 600f);
            cancelButton.Click += new ButtonEventHandler(OnCancel);

            difficultyButton.Initialize();
            difficultyButton.Text = "Change Difficulty";
            difficultyButton.Scale = new Vector2(0.75f);
            difficultyButton.Position = new Vector2(60, 180);
            difficultyButton.Click += new ButtonEventHandler(OnChangeDifficulty);

            gameModeButton.Initialize();
            gameModeButton.Text = "Multiplayer Game Mode";
            gameModeButton.Scale = new Vector2(0.75f);
            gameModeButton.Position = new Vector2(60, 250);
            gameModeButton.Click += new ButtonEventHandler(OnGameMode);

            increaseTimeButton.Initialize();
            increaseTimeButton.Click += new ButtonEventHandler(OnIncreaseTime);

            decreaseTimeButton.Initialize();
            decreaseTimeButton.Click += new ButtonEventHandler(OnDecreaseTime);

            increaseSizeButton.Initialize();
            increaseSizeButton.Click += new ButtonEventHandler(OnIncreaseSize);

            decreaseSizeButton.Initialize();
            decreaseSizeButton.Click += new ButtonEventHandler(OnDecreaseSize);
        }

        void OnGameMode(object o, EventArgs e)
        {
            if (gameModes[0].TextColor == Color.Blue)
            {
                gameModes[0].TextColor = Color.Red;
                gameModes[1].TextColor = Color.Blue;
            }
            else if (gameModes[1].TextColor == Color.Blue)
            {
                gameModes[1].TextColor = Color.Red;
                gameModes[0].TextColor = Color.Blue;
            }
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

            if (gameModes[0].TextColor == Color.Blue)
                settings.GameType = GameType.Normal;
            else if (gameModes[1].TextColor == Color.Blue)
                settings.GameType = GameType.Time;

            settings.PlayTime = timeLimit;
            settings.PlayfieldSize = size;
            settings.Save(PuzzleGame.SETTINGSPATH);
            NewScreen(new MainMenu());
        }

        private void OnCancel(object o, EventArgs e)
        {
            NewScreen(new MainMenu());
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

            foreach (Text text in gameModes)
                text.LoadContent(spriteBatch);

            okButton.LoadContent(spriteBatch);
            cancelButton.LoadContent(spriteBatch);

            difficultyButton.LoadContent(spriteBatch);
            gameModeButton.LoadContent(spriteBatch);
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
            cursor.Update();

            if (isFocused)
            {
                okButton.Update(gameTime);
                cancelButton.Update(gameTime);
                difficultyButton.Update(gameTime);
                gameModeButton.Update(gameTime);
                increaseTimeButton.Update(gameTime);
                decreaseTimeButton.Update(gameTime);
                timeLimitText.TextValue = timeLimit.ToString();
                increaseSizeButton.Update(gameTime);
                decreaseSizeButton.Update(gameTime);
                sizeText.TextValue = size.ToString();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            backgroundImage.Draw(gameTime);
            cursor.Draw(gameTime);

            foreach (Text text in difficulties)
                text.Draw(gameTime);

            foreach (Text text in gameModes)
                text.Draw(gameTime);

            okButton.Draw(gameTime);
            cancelButton.Draw(gameTime);

            difficultyButton.Draw(gameTime);
            gameModeButton.Draw(gameTime);

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
