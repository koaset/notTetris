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
    /// <summary>
    /// A popup allowing the user to write input
    /// </summary>
    class TextboxPopup : Popup
    {
        public bool ShouldSave { get; set; }

        Text infoText;
        Text text;
        string info;
        int charLimit;
        
        public TextboxPopup(string info, int charLimit)
        {
            infoText = new Text();
            text = new Text();
            this.info = info;
            this.charLimit = charLimit;
        }

        public override void Initialize()
        {
            base.Initialize();
            ShouldSave = false;

            infoText.Initialize();
            infoText.Font = FontNames.Segoe_UI_Mono;
            infoText.Position = position - new Vector2(0f, 0.2f * size.Y);
            infoText.IsCentered = true;
            infoText.Font = FontNames.Segoe_UI_Mono_Large;
            infoText.TextColor = Color.Green;
            infoText.Layer = 0.9f;
            infoText.IsShowing = false;
            infoText.TextValue = info;

            text.Initialize();
            text.Font = FontNames.Segoe_UI_Mono;
            text.Position = position + new Vector2(0f, 0.15f * size.Y); ;
            text.IsCentered = true;
            text.Font = FontNames.Segoe_UI_Mono_Large;
            text.TextColor = Color.Green;
            text.Layer = 0.9f;
            text.IsShowing = false;
            text.TextValue = "";
        }

        public override void LoadContent(SpriteBatch spriteBatch)
        {
            base.LoadContent(spriteBatch);
            text.LoadContent(spriteBatch);
            infoText.LoadContent(spriteBatch);
        }

        public override void Show()
        {
            base.Show();
            text.TextValue = "";
            infoText.IsShowing = true;
            text.IsShowing = true;
        }

        public override void Close()
        {
            infoText.IsShowing = false;
            text.IsShowing = false;
            isShowing = false;
            OnClose(new TextboxPopupEventArgs(text.TextValue));
        }

        public override void Update(GameTime gameTime)
        {
            if (isShowing)
            {
                KeyboardState newState = Keyboard.GetState();

                if (newState.IsKeyDown(Keys.Enter) && oldState.IsKeyUp(Keys.Enter))
                {
                    ShouldSave = true;
                    Close();
                }
                else if (newState.IsKeyDown(Keys.Escape) && oldState.IsKeyUp(Keys.Escape))
                {
                    ShouldSave = false;
                    Close();
                }

                Keys[] keys = newState.GetPressedKeys();

                foreach (Keys key in keys)
                    if (oldState.IsKeyUp(key))
                        if (key == Keys.Back)
                        {
                            if (text.TextValue.Length > 0)
                                text.TextValue = text.TextValue.Remove(text.TextValue.Length - 1);
                        }
                        else if (text.TextValue.Length < charLimit)
                        {
                            if (IsNumeric(key))
                                text.TextValue += GetNumericChar(key);
                            else if (key == Keys.OemPeriod)
                                text.TextValue += ".";
                        } 
                oldState = newState;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            infoText.Draw(gameTime);
            text.Draw(gameTime);
        }

        #region Key processing
        private bool IsNumeric(Keys key)
        {
            return (key >= Keys.D0 && key <= Keys.D9) || (key >= Keys.NumPad0 && key <= Keys.NumPad9);
        }

        private string GetNumericChar(Keys key)
        {
            if (key == Keys.D0 || key == Keys.NumPad0)
                return "0";
            else if (key == Keys.D1 || key == Keys.NumPad1)
                return "1";
            else if (key == Keys.D2 || key == Keys.NumPad2)
                return "2";
            else if (key == Keys.D3 || key == Keys.NumPad3)
                return "3";
            else if (key == Keys.D4 || key == Keys.NumPad4)
                return "4";
            else if (key == Keys.D5 || key == Keys.NumPad5)
                return "5";
            else if (key == Keys.D6 || key == Keys.NumPad6)
                return "6";
            else if (key == Keys.D7 || key == Keys.NumPad7)
                return "7";
            else if (key == Keys.D8 || key == Keys.NumPad8)
                return "8";
            else if (key == Keys.D9 || key == Keys.NumPad9)
                return "9";
            else
                return "";
        }
        #endregion
    }
        

    public class TextboxPopupEventArgs : EventArgs
    {
        private string message;
        public TextboxPopupEventArgs(string message) { this.message = message; }
        public override string ToString() { return message; }
    }
}
