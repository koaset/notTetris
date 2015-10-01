using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Input;

namespace NotTetris
{
    public class Settings
    {
        public string WindowTitle = "Not Tetris";
        public string Difficulty = "Easy";

        public float BlockDropSpeed = 3f;
        public int PlayTime = 3;
        public int PlayfieldSize = 9;

        public Keys Player1Start = Keys.Enter;
        public Keys Player1Left = Keys.Left;
        public Keys Player1Right = Keys.Right;
        public Keys Player1Down = Keys.Down;
        public Keys Player1Rotate = Keys.RightControl;

        public Keys Player2Start = Keys.T;
        public Keys Player2Left = Keys.A;
        public Keys Player2Right = Keys.D;
        public Keys Player2Down = Keys.S;
        public Keys Player2Rotate = Keys.G;

        public int[] score = { 0, 0, 0, 0, 0 };
        
        /// <summary>
        /// Saves the current settings
        /// </summary>
        /// <param name="filename">The filename to save to</param>
        public void Save(string filename)
        {
            Stream stream = File.Create(filename);

            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            serializer.Serialize(stream, this);
            stream.Close();
        }

        /// <summary>
        /// Loads settings from a file
        /// </summary>
        /// <param name="filename">The filename to load</param>
        public static Settings Load(string filename)
        {
            Stream stream = File.OpenRead(filename);
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            return (Settings)serializer.Deserialize(stream);
        }
    }
}
