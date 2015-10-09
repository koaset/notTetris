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
    class SplitScreenGame : GameScreen
    {
        Playfield playerOneField;
        Playfield playerTwoField;
        Image backgroundImage;
        Image pauseImage;
        TimeSpan time;
        bool p1Won;
        TimeSpan timeLimit;
        Text timer;
        bool isStarted;

        public SplitScreenGame(Settings settings)
        {
            playerOneField = new Playfield(GameType.Normal, new Vector2(780f, 325f), settings.PlayfieldSize);
            playerTwoField = new Playfield(GameType.Normal, new Vector2(300f, 325f), settings.PlayfieldSize);
            backgroundImage = new Image();
            pauseImage = new Image();
            timer = new Text();
        }

        public override void Initialize(SpriteBatch spriteBatch, Settings settings)
        {
            base.Initialize(spriteBatch, settings);

            isStarted = false;
            playerOneField.Initialize(spriteBatch, settings.Difficulty);
            playerOneField.IsShowing = true;
            System.Threading.Thread.Sleep(10);
            playerTwoField.Initialize(spriteBatch, settings.Difficulty);
            playerTwoField.IsShowing = true;

            playerOneField.BaseDropSpeed = settings.BlockDropSpeed;
            playerTwoField.BaseDropSpeed = settings.BlockDropSpeed;

            pauseImage.Initialize();
            pauseImage.Layer = 0.9f;
            pauseImage.Size = new Vector2(487, 120);
            pauseImage.Position = new Vector2(500, 360);
            pauseImage.IsShowing = false;
            pauseImage.TextureName = TextureNames.game_paused;

            backgroundImage.Initialize();
            backgroundImage.Layer = 0.3f;
            backgroundImage.Size = new Vector2(1000f, 720f);
            backgroundImage.Position = new Vector2(500f, 360f);
            backgroundImage.TextureName = TextureNames.game_background;

            playerOneField.GameOver += new GameOverEventHandler(OnGameOver);
            playerTwoField.GameOver += new GameOverEventHandler(OnGameOver);

            timeLimit = new TimeSpan(0, settings.PlayTime, 0);
            timer.Initialize();
            timer.Font = FontNames.Segoe_UI_Mono;
            timer.Layer = 0.8f;
            timer.Position = new Vector2(10);
            timer.TextColor = Color.Navy;
            timer.TextValue = "Time left: " + timeLimit.Minutes.ToString() + ":" + timeLimit.Seconds.ToString();
        }

        public override void LoadContent()
        {
            playerOneField.LoadContent();
            playerTwoField.LoadContent();
            pauseImage.LoadContent(spriteBatch);
            backgroundImage.LoadContent(spriteBatch);
            timer.LoadContent(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.F10) && oldState.IsKeyUp(Keys.F10))
                NewScreen(new MainMenu());

            playerOneField.Update(gameTime);

            if (!playerOneField.IsPaused)
                time += gameTime.ElapsedGameTime;

            #region Pause

            if (isStarted) 
            {
                if (newState.IsKeyDown(Keys.Pause) && oldState.IsKeyUp(Keys.Pause))
                {
                    if (playerOneField.IsPaused)
                        UnPause();
                    else
                    {
                        Pause();
                        time.Add(gameTime.ElapsedGameTime);
                    }
                }
                else if (!isFocused)
                    Pause();
            }
            #endregion

            if (!isStarted)
                if (newState.IsKeyDown(settings.Player1Start) && oldState.IsKeyUp(settings.Player1Start))
                {
                    isStarted = true;
                    playerOneField.StartGame();
                    playerTwoField.StartGame();
                }

            #region Player 1 Controls
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
            #endregion

            playerTwoField.Update(gameTime);

            #region Player 2 Controls
            if (!playerTwoField.ControlsLocked)
            {
                if (newState.IsKeyDown(settings.Player2Rotate) && oldState.IsKeyUp(settings.Player2Rotate))
                    playerTwoField.RotateCluster();
                else if (newState.IsKeyDown(settings.Player2Left) && newState.IsKeyUp(settings.Player2Right))
                    playerTwoField.MoveClusterLeft(gameTime, oldState.IsKeyUp(settings.Player2Left));
                else if (newState.IsKeyDown(settings.Player2Right) && newState.IsKeyUp(settings.Player1Left))
                    playerTwoField.MoveClusterRight(gameTime, oldState.IsKeyUp(settings.Player2Right));
                if (newState.IsKeyDown(settings.Player2Down) && oldState.IsKeyUp(settings.Player2Down))
                    playerTwoField.MoveClusterDown();
            }
            #endregion

            TimeSpan timeLeft = timeLimit - time;

            timer.TextValue = "Time left: " + timeLeft.Minutes.ToString() + ":" + timeLeft.Seconds.ToString();

            if (timeLeft.Minutes == 0 && timeLeft.Seconds == 0)
                if (playerOneField.GetScore > playerTwoField.GetScore)
                    playerTwoField.EndGame();
                else
                    playerOneField.EndGame();

            oldState = newState;
        }

        private void Pause()
        {
            playerOneField.Pause();
            playerTwoField.Pause();
            pauseImage.IsShowing = true;
        }

        private void UnPause()
        {
            playerOneField.UnPause();
            playerTwoField.UnPause();
            pauseImage.IsShowing = false;
        }

        public override void Draw(GameTime gameTime)
        {
            backgroundImage.Draw(gameTime);
            pauseImage.Draw(gameTime);
            playerOneField.Draw(gameTime);
            playerTwoField.Draw(gameTime);
            timer.Draw(gameTime);
        }

        private void OnGameOver(object o, EventArgs e)
        {
            if (o == playerOneField)
                p1Won = false;
            else
                p1Won = true;

            NewScreen(new ResultsScreen(GetResults(), false));            
        }

        public override Results GetResults()
        {
            Results r = new Results();

            r.IsSinglerplayer = false;
            r.Player1Won = p1Won;
            r.Time = time;
            r.Player1Score = playerOneField.GetScore;
            r.Player2Score = playerTwoField.GetScore;            
            r.Difficulty = settings.Difficulty;            

            return r;
        }
    }
}
