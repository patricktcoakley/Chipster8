using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chip8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Chipster8
{
    public class Chipster8 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly Color[] _pixels = new Color[2048];
        private readonly List<string> _roms = new();
        private bool _isMuted;
        private SpriteBatch _spriteBatch;
        private Chip8.Chip8 _chip8 = new();
        private Texture2D _canvas;
        private KeyboardState _keyboardState;
        private KeyboardState _previousKeyboardState;
        private Rectangle _scaleSize;
        private SoundEffectInstance _beepInstance;
        private SpriteFont _font;
        private Color _backgroundColor = Color.CornflowerBlue;
        private Color _foregroundColor = Color.White;
        private byte _speed = 1;

        private byte Speed
        {
            get => _speed;
            set
            {
                _speed = value switch
                {
                    0 => 0,
                    > 10 => 10,
                    _ => value
                };
            }
        }

        private int _currentRom;

        private int CurrentRom
        {
            get => _currentRom;
            set
            {
                if (value < 0)
                {
                    _currentRom = 0;
                }
                else if (value >= _roms.Count)
                {
                    _currentRom = _roms.Count - 1;
                }
                else
                {
                    _currentRom = value;
                }
            }
        }

        private static readonly Tuple<Color, Color>[] ColorSchemes =
        {
            new(Color.CornflowerBlue, Color.White),
            new(Color.White, Color.Black),
            new(Color.Black, Color.LimeGreen),
            new(Color.LightGray, Color.CadetBlue)
        };

        private byte _currentColorScheme;

        private byte CurrentColorScheme
        {
            get => (byte) (_currentColorScheme % ColorSchemes.Length);
            set => _currentColorScheme = value;
        }

        public Chipster8()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        private void PlaySound()
        {
            if (!_isMuted && _chip8.ShouldPlaySound)
            {
                _beepInstance.Play();
            }
        }

        private bool CanBePressed(Keys key) => _keyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);

        private bool CanBeHeld(Keys key) => _keyboardState.IsKeyDown(key);

        protected override void Initialize()
        {
            IsMouseVisible = false;
            Window.Title = "Chipster8";
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 640;
            _graphics.ApplyChanges();
            _scaleSize = GraphicsDevice.PresentationParameters.Bounds;
            _canvas = new Texture2D(GraphicsDevice, Chip8.Chip8.VideoWidth, Chip8.Chip8.VideoHeight, true, SurfaceFormat.Color);
            _roms.AddRange(Directory.GetFiles("Roms").Select(Path.GetFileName));
            _roms.Sort();
            _roms.Add("Exit");
            _beepInstance = Content.Load<SoundEffect>("Beep").CreateInstance();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("Font");
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            _keyboardState = Keyboard.GetState();

            if (CanBePressed(Keys.Escape))
            {
                if (_chip8.IsRunning())
                {
                    _chip8.State = Chip8State.Off;
                }
                else
                {
                    Exit();
                }
            }
            
            if (CanBePressed(Keys.F11))
            {
                _graphics.ToggleFullScreen();
            }

            if (CanBePressed(Keys.Back))
            {
                _isMuted = !_isMuted;
            }

            if (_chip8.IsOff())
            {
                if (CanBePressed(Keys.Down))
                {
                    ++CurrentRom;
                }

                if (CanBePressed(Keys.Up))
                {
                    --CurrentRom;
                }

                if (CanBePressed(Keys.Enter))
                {
                    var selection = _roms[CurrentRom];
                    if (selection.Equals("Exit"))
                    {
                        Exit();
                    }
                    else
                    {
                        _chip8 = new Chip8.Chip8();
                        _chip8.PowerOn();
                        _chip8.LoadRom(_roms[CurrentRom]);
                    }
                }
            }

            if (_chip8.IsOn())
            {
                if (CanBePressed(Keys.Space))
                {
                    _chip8.Pause();
                }

                if (CanBeHeld(Keys.D1))
                {
                    _chip8.Keypad[0] = true;
                }

                if (CanBeHeld(Keys.D2))
                {
                    _chip8.Keypad[1] = true;
                }

                if (CanBeHeld(Keys.D3))
                {
                    _chip8.Keypad[2] = true;
                }

                if (CanBeHeld(Keys.D4))
                {
                    _chip8.Keypad[3] = true;
                }

                if (CanBeHeld(Keys.Q))
                {
                    _chip8.Keypad[4] = true;
                }

                if (CanBeHeld(Keys.W))
                {
                    _chip8.Keypad[5] = true;
                }

                if (CanBeHeld(Keys.E))
                {
                    _chip8.Keypad[6] = true;
                }

                if (CanBeHeld(Keys.R))
                {
                    _chip8.Keypad[7] = true;
                }

                if (CanBeHeld(Keys.A))
                {
                    _chip8.Keypad[8] = true;
                }

                if (CanBeHeld(Keys.S))
                {
                    _chip8.Keypad[9] = true;
                }

                if (CanBeHeld(Keys.D))
                {
                    _chip8.Keypad[10] = true;
                }

                if (CanBeHeld(Keys.F))
                {
                    _chip8.Keypad[11] = true;
                }

                if (CanBeHeld(Keys.Z))
                {
                    _chip8.Keypad[12] = true;
                }

                if (CanBeHeld(Keys.X))
                {
                    _chip8.Keypad[13] = true;
                }

                if (CanBeHeld(Keys.C))
                {
                    _chip8.Keypad[14] = true;
                }

                if (CanBeHeld(Keys.V))
                {
                    _chip8.Keypad[15] = true;
                }

                if (CanBePressed(Keys.F1))
                {
                    Console.WriteLine(_chip8.DumpMemory());
                }

                if (CanBePressed(Keys.F2))
                {
                    Console.WriteLine(_chip8.DumpVideo());
                }

                if (CanBePressed(Keys.F3))
                {
                    Console.WriteLine(_chip8.DumpRegisters());
                }

                if (CanBePressed(Keys.F4))
                {
                    --Speed;
                }

                if (CanBePressed(Keys.F5))
                {
                    ++Speed;
                }

                if (CanBePressed(Keys.F6))
                {
                    (_backgroundColor, _foregroundColor) = ColorSchemes[CurrentColorScheme++];
                }

                if (_chip8.IsRunning())
                {
                    for (var i = 0; i < 10 * Speed; ++i)
                    {
                        _chip8.Step();
                        PlaySound();
                    }

                    for (var j = 0; j < _chip8.Video.Length; ++j)
                    {
                        _pixels[j] = _chip8.HasColor(j) ? _backgroundColor : _foregroundColor;
                    }
                }

                Array.Fill(_chip8.Keypad, false);
            }

            _previousKeyboardState = _keyboardState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (_chip8.IsOff())
            {
                GraphicsDevice.Clear(Color.Black);
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None);

                const uint step = 75;
                var x = Math.Abs(_graphics.PreferredBackBufferWidth * 0.43f);
                var y = step;

                _spriteBatch.DrawString(_font, _roms[CurrentRom], new Vector2(x, 0), Color.Blue);

                for (var i = CurrentRom + 1; i < _roms.Count; ++i)
                {
                    _spriteBatch.DrawString(_font, _roms[i], new Vector2(x, y), Color.White);
                    y += step;
                }

                _spriteBatch.End();
            }
            else if (_chip8.IsPaused())
            {
                GraphicsDevice.Clear(_backgroundColor);
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None);

                _spriteBatch.DrawString(_font, "PAUSED",
                    new Vector2(_graphics.PreferredBackBufferWidth * 0.43f, _graphics.PreferredBackBufferHeight * 0.5f),
                    Color.White);
                _spriteBatch.End();
            }
            else
            {
                if (_chip8.ShouldDraw)
                {
                    GraphicsDevice.Clear(_backgroundColor);
                    _canvas.SetData(_pixels, 0, _pixels.Length);
                    _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap);
                    _spriteBatch.Draw(_canvas, new Rectangle(0, 0, _scaleSize.Width, _scaleSize.Height), Color.White);
                    _spriteBatch.End();
                    _chip8.ShouldDraw = false;
                }
            }

            base.Draw(gameTime);
        }
    }
}