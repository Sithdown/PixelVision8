//   
// Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by Pixel Vision 8 are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman - @JesseFreeman
// Christina-Antoinette Neofotistou @CastPixel
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany
//

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using PixelVision8.Player;

namespace PixelVision8.Runner
{
    public class DisplayTarget
    {
        protected Vector2 Offset;
        protected Texture2D RenderTexture;
        protected readonly GraphicsDeviceManager GraphicManager;
        protected readonly SpriteBatch SpriteBatch;
        protected Color[] CachedColors;
        protected Rectangle VisibleRect;
        private readonly int _monitorHeight;
        private readonly int _monitorWidth;
        private int _monitorScale = 1;
        private int _totalPixels;
        private Color[] _pixelData = new Color[0];
        private int _colorId;
        private int _i;

        public Vector2 Scale = new Vector2(1, 1);
        protected int DisplayWidth;
        protected int DisplayHeight;

        public bool StretchScreen { get; set; }
        public bool Fullscreen { get; set; }

        // public Vector2 Scale => _scale;

        public DisplayTarget(GraphicsDeviceManager graphicManager, int width, int height)
        {
            GraphicManager = graphicManager;

            GraphicManager.HardwareModeSwitch = false;

            SpriteBatch = new SpriteBatch(graphicManager.GraphicsDevice);

            _monitorWidth = MathHelper.Clamp(width, 64, 640);
            _monitorHeight = MathHelper.Clamp(height, 64, 480);
        }

        public int MonitorScale
        {
            get => _monitorScale;
            set
            {
                var fits = false;

                while (fits == false)
                {
                    var newWidth = _monitorWidth * value;
                    var newHeight = _monitorHeight * value;

                    if (newWidth < GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width &&
                        newHeight < GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                    {
                        fits = true;
                        _monitorScale = value;
                    }
                    else
                    {
                        value--;
                    }
                }
            }
        }

        public virtual void ResetResolution(IPlayerChips engine)
        {
            var displayChip = engine.DisplayChip;

            var gameWidth = displayChip.Width;
            var gameHeight = displayChip.Height;

            if (RenderTexture == null || RenderTexture.Width != gameWidth || RenderTexture.Height != gameHeight)
            {
                RenderTexture = new Texture2D(GraphicManager.GraphicsDevice, gameWidth, gameHeight);
            }

            // Calculate the game's resolution
            VisibleRect.Width = RenderTexture.Width;
            VisibleRect.Height = RenderTexture.Height;

            var tmpMonitorScale = Fullscreen ? 1 : MonitorScale;

            // Calculate the monitor's resolution
            DisplayWidth = Fullscreen
                ? GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width
                : _monitorWidth *
                  tmpMonitorScale;
            DisplayHeight = Fullscreen
                ? GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
                : _monitorHeight * tmpMonitorScale;

            CalculateDisplayScale();

            CalculateDisplayOffset();

            // Apply changes
            GraphicManager.IsFullScreen = Fullscreen;

            if (GraphicManager.PreferredBackBufferWidth != DisplayWidth ||
                GraphicManager.PreferredBackBufferHeight != DisplayHeight)
            {
                GraphicManager.PreferredBackBufferWidth = DisplayWidth;
                GraphicManager.PreferredBackBufferHeight = DisplayHeight;
                GraphicManager.ApplyChanges();
            }

            _totalPixels = gameWidth * gameHeight;
            if (_pixelData.Length != _totalPixels)
            {
                Array.Resize(ref _pixelData, _totalPixels);
            }
        }

        protected virtual void CalculateDisplayOffset()
        {
            Offset.X = (DisplayWidth - VisibleRect.Width * Scale.X) * .5f;
            Offset.Y = (DisplayHeight - VisibleRect.Height * Scale.Y) * .5f;
        }

        protected virtual void CalculateDisplayScale()
        {
            // Calculate the game scale
            Scale.X = (float) DisplayWidth / VisibleRect.Width;
            Scale.Y = (float) DisplayHeight / VisibleRect.Height;

            if (!StretchScreen)
            {
                // To preserve the aspect ratio,
                // use the smaller scale factor.
                Scale.X = Math.Min(Scale.X, Scale.Y);
                Scale.Y = Scale.X;
            }
        }

        public virtual void RebuildColorPalette(ColorChip colorChip)
        {
            if (colorChip.Invalid)
            {
                CachedColors = Utilities.ConvertColors(colorChip.HexColors, colorChip.MaskColor, colorChip.DebugMode,
                    colorChip.BackgroundColor);

                colorChip.ResetValidation();
            }
        }

        public virtual void Render(IPlayerChips engine) //int[] pixels, int defaultColor)
        {
            // Make sure the color palette doesn't need to rebuild itself
            RebuildColorPalette(engine.ColorChip);

            // We can only update the display if the pixel lengths match up
            if (engine.DisplayChip.Pixels.Length != _totalPixels)
                return;

            SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp);

            for (_i = 0; _i < _totalPixels; _i++)
            {
                _colorId = engine.DisplayChip.Pixels[_i];
                _pixelData[_i] = CachedColors[_colorId < 0 ? engine.ColorChip.BackgroundColor : _colorId];
            }

            RenderTexture.SetData(_pixelData);
            SpriteBatch.Draw(RenderTexture, Offset, VisibleRect, Color.White, 0f, Vector2.Zero, Scale,
                SpriteEffects.None, 1f);
            SpriteBatch.End();
        }
    }
}