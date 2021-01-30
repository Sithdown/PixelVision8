using Microsoft.Xna.Framework;
using PixelVision8.Runner;

namespace PixelVision8.Runner
{
    public partial class GameRunner
    {
        protected bool _resolutionInvalid = true;
        public DisplayTarget DisplayTarget;

        /// <summary>
        ///     Scale the resolution.
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="fullScreen"></param>
        public virtual int Scale(int? scale = null)
        {
            if (scale.HasValue)
            {
                DisplayTarget.MonitorScale = scale.Value;

                InvalidateResolution();
            }

            return DisplayTarget.MonitorScale;
        }

        public virtual bool Fullscreen(bool? value = null)
        {
            if (value.HasValue)
            {
                DisplayTarget.Fullscreen = value.Value;

                InvalidateResolution();
            }

            return
                DisplayTarget.Fullscreen;
        }

        public virtual void ConfigureDisplayTarget()
        {
            // Create the default display target
            DisplayTarget = new DisplayTarget(_graphics, 512, 480);
        }

        public void InvalidateResolution()
        {
            _resolutionInvalid = true;
        }

        public void ResetResolutionValidation()
        {
            _resolutionInvalid = false;
        }

        public virtual void ResetResolution()
        {
            DisplayTarget.ResetResolution(ActiveEngine);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (ActiveEngine == null) return;

            _frameCounter++;

            // Clear with black and draw the runner.
            _graphics.GraphicsDevice.Clear(Color.Black);

            // Now it's time to call the PixelVisionEngine's Draw() method. This Draw() call propagates throughout all of the Chips that have 
            // registered themselves as being able to draw such as the GameChip and the DisplayChip.

            // Only call draw if the window has focus
            if (RunnerActive) ActiveEngine.Draw();

            DisplayTarget
                .Render(ActiveEngine); //ActiveEngine.DisplayChip.Pixels, ActiveEngine.ColorChip.backgroundColor);

            // displayTarget.spriteBatch.End();
            if (_resolutionInvalid)
            {
                ResetResolution();
                ResetResolutionValidation();
            }
        }
    }
}