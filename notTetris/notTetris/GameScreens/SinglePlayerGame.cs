using System;
using System.Collections.Generic;
using System.Text;
using NotTetris.GameScreens;
using NotTetris.Graphics;
using NotTetris.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace NotTetris.GameScreens
{
    class SinglePlayerGame : GameScreen
    {
        Playfield playerOneField;
        Image backgroundImage;
        Image pauseImage;
        Text startText;
        TimeSpan time;
        Text timer;
        bool isStarted;

        public SinglePlayerGame(Settings settings)
        {
            playerOneField = new Playfield(GameType.Normal, new Vector2(500f, 325f), settings.PlayfieldSize);
            backgroundImage = new Image();
            pauseImage = new Image();
            startText = new Text();
            timer = new Text();
        }

        public override void Initialize(SpriteBatch spriteBatch, Settings settings)
        {
            base.Initialize(spriteBatch, settings);

            isStarted = false;
            playerOneField.Initialize(spriteBatch, settings.Difficulty);
            playerOneField.IsShowing = true;
            playerOneField.BaseDropSpeed = settings.BlockDropSpeed;

            pauseImage.Initialize();
            pauseImage.Layer = 0.9f;
            pauseImage.Size = new Vector2(487, 120);
            pauseImage.Position = new Vector2(500, 300);
            pauseImage.IsShowing = false;
            pauseImage.TextureName = TextureNames.game_paused;

            backgroundImage.Initialize();
            backgroundImage.Layer = 0.1f;
            backgroundImage.Size = new Vector2(1000f, 720f);
            backgroundImage.Position = new Vector2(500f, 360f);
            backgroundImage.TextureName = TextureNames.game_background;

            startText.Initialize();
            startText.Font = FontNames.Segoe_UI_Mono;
            startText.Position = new Vector2(500, 300);
            startText.TextColor = Color.Navy;
            startText.Layer = 0.9f;
            startText.IsCentered = true;
            startText.Spacing = 6;
            startText.TextValue = "Press Enter to\nstart the game";

            timer.Initialize();
            timer.Font = FontNames.Segoe_UI_Mono;
            timer.IsShowing = true;
            timer.Layer = 0.8f;
            timer.Position = new Vector2(10);
            timer.TextColor = Color.Navy;
            timer.TextValue = "Time played: " + time.Minutes.ToString() + ":" + time.Seconds.ToString();

            playerOneField.GameOver += new GameOverEventHandler(OnGameOver);
        }

        public override void LoadContent()
        {
            playerOneField.LoadContent();
            backgroundImage.LoadContent(spriteBatch);
            pauseImage.LoadContent(spriteBatch);
            startText.LoadContent(spriteBatch);
            timer.LoadContent(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.F10) && oldState.IsKeyUp(Keys.F10))
                NewScreen(new MainMenu());

            playerOneField.Update(gameTime);

            if (!playerOneField.IsPaused)
            {
                time += gameTime.ElapsedGameTime;
                timer.TextValue = "Time played: " + time.Minutes.ToString() + ":" + time.Seconds.ToString();
            }

            if (!isStarted)
            {
                if (newState.IsKeyDown(settings.Player1Start) && oldState.IsKeyUp(settings.Player1Start))
                {
                    playerOneField.StartGame();
                    isStarted = true;
                    startText.IsShowing = false;
                }
            }

            if (!playerOneField.ControlsLocked)
            {
                if (newState.IsKeyDown(settings.Player1Rotate) && oldState.IsKeyUp(settings.Player1Rotate))
                    playerOneField.RotateCluster();
                else if (newState.IsKeyDown(settings.Player1Left) && newState.IsKeyUp(settings.Player1Right))
                    playerOneField.MoveClusterLeft(gameTime, oldState.IsKeyUp(settings.Player1Left));
                else if (newState.IsKeyDown(settings.Player1Right) && newState.IsKeyUp(settings.Player1Left))
                    playerOneField.MoveClusterRight(gameTime, oldState.IsKeyUp(settings.Player1Right));
                if (newState.IsKeyDown(settings.Player1Down) && oldState.IsKeyUp(settings.Player1Down))
                    playerOneField.MoveClusterDown();
            }

            if (isStarted)
            {
                if (newState.IsKeyDown(Keys.Pause) && oldState.IsKeyUp(Keys.Pause)) 
                {
                    if (playerOneField.IsPaused)
                        UnPause();
                    else
                        Pause();
                }
                else if (!isFocused)
                    Pause();
            }

            oldState = newState;
        }

        private void Pause()
        {
            playerOneField.Pause();
            pauseImage.IsShowing = true;
        }

        private void UnPause()
        {
            pauseImage.IsShowing = false;
            playerOneField.UnPause();
        }

        public override void Draw(GameTime gameTime)
        {
            backgroundImage.Draw(gameTime);
            pauseImage.Draw(gameTime);
            playerOneField.Draw(gameTime);
            startText.Draw(gameTime);
            timer.Draw(gameTime);
        }

        public void OnGameOver(object o, EventArgs e)
        {
            settings.Save(PuzzleGame.SETTINGSPATH);
            NewScreen(new ResultsScreen(GetResults(), false));
        }

        public override Results GetResults()
        {
            Results r = new Results();

            r.IsSinglerplayer = true;
            r.Time = time;
            r.Player1Score = playerOneField.GetScore;
            r.Difficulty = settings.Difficulty;

            return r;
        }
    }
}
