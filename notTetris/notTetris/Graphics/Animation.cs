using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NotTetris.Graphics
{
    /// <summary>
    /// An animated image
    /// </summary>
    class Animation : Image
    {
        #region Props
        public bool IsStarted
        {
            get { return isStarted; }
            set { isStarted = value; }
        }

        public int NumFrames
        {
            get { return this.numFrames; }
            set 
            {
                if (this.numFrames != value && value > 0)
                    this.numFrames = value;
            }
        }

        public float FramesPerSecond
        {
            get { return this.framesPerSecond; }
            set
            {
                if (this.framesPerSecond != value && value > 0)
                    this.framesPerSecond = value;
            }
        }

        public float CurrentFrame
        {
            get { return this.currentFrame; }
            set
            {
                if (this.currentFrame != value && value >= 0 && value <= this.numFrames)
                    this.currentFrame = value;
            }
        }

        public bool IsLooped
        {
            get { return IsLooped; }
            set { isLooped = value; }
        }
        #endregion

        List<Rectangle> frames;
        protected bool isLooped;
        protected bool isStarted;
        protected float currentFrame;
        protected float framesPerSecond;
        protected int numFrames;

        public override void Initialize()
        {
            base.Initialize();

            this.numFrames = 1;
            this.framesPerSecond = 1f;
        }

        public override void LoadContent(SpriteBatch spriteBatch)
        {
            base.LoadContent(spriteBatch);

            this.frames = new List<Rectangle>();

            this.origin = new Vector2(this.size.X / 2, this.size.Y / 2);

            int firstFrame = 1;

            for (int index = firstFrame; index <= numFrames; index++)
                this.frames.Add(CalculateSource(index));
        }

        public void Play()
        {
            isShowing = true;
            isStarted = true;
            currentFrame = 0;
        }

        public void Stop()
        {
            isStarted = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (this.isStarted)
            {
                this.currentFrame += this.framesPerSecond * (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (this.currentFrame > this.frames.Count)
                {
                    if (this.isLooped)
                        currentFrame = 0f;
                    else
                    {
                        this.currentFrame = this.frames.Count - 1;
                        this.isStarted = false;
                    }
                }
            }

            this.source = frames[(int)this.currentFrame];
        }
    }
}
