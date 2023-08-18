using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chip8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Chipster8;

public class Chipster8 : Game
{
    private static readonly Tuple<Color, Color>[] ColorSchemes =
    {
        new(Color.CornflowerBlue, Color.White),
        new(Color.White, Color.Black),
        new(Color.Black, Color.LimeGreen),
        new(Color.LightGray, Color.CadetBlue)
    };

    private readonly GraphicsDeviceManager _graphics;
    private readonly Color[] _pixels = new Color[2048];
    private readonly List<string> _romTitles = new();
    private Color _backgroundColor = Color.CornflowerBlue;
    private SoundEffectInstance _beepInstance;
    private Texture2D _canvas;
    private Chip8.Chip8 _chip8 = new();

    private byte _currentColorScheme;

    private int _currentRom;
    private SpriteFont _font;
    private Color _foregroundColor = Color.White;
    private bool _isMuted;
    private KeyboardState _keyboardState;
    private KeyboardState _previousKeyboardState;
    private Rectangle _scaleSize;
    private byte _speed = 1;
    private SpriteBatch _spriteBatch;

    public Chipster8()
    {
        _graphics = new GraphicsDeviceManager(this);
    }

    private byte Speed
    {
        get => _speed;
        set => _speed = value switch
        {
            0 => 0,
            > 10 => 10,
            _ => value
        };
    }

    private int CurrentRom
    {
        get => _currentRom;
        set
        {
            if (value < 0)
            {
                _currentRom = 0;
            }
            else if (value >= _romTitles.Count)
            {
                _currentRom = _romTitles.Count - 1;
            }
            else
            {
                _currentRom = value;
            }
        }
    }

    private byte CurrentColorScheme
    {
        get => (byte)(_currentColorScheme % ColorSchemes.Length);
        set => _currentColorScheme = value;
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
        base.Initialize();
    }

    protected override void LoadContent()
    {
        Content.RootDirectory = "Content";
        _canvas = new Texture2D(GraphicsDevice, Chip8.Chip8.VideoWidth, Chip8.Chip8.VideoHeight, true,
            SurfaceFormat.Color);

        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>("Font");
        _beepInstance = Content.Load<SoundEffect>("Beep").CreateInstance();

        var rootPath = Path.Combine(Content.RootDirectory, "Roms");
        _romTitles.AddRange(Directory.GetFiles(rootPath).Select(Path.GetFileName));
        _romTitles.Sort();
        _romTitles.Add("Exit");

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        _keyboardState = Keyboard.GetState();

        if (CanBePressed(Keys.Escape))
        {
            if (_chip8.IsRunning)
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

        switch (_chip8.State)
        {
            case Chip8State.Running:
                if (CanBePressed(Keys.Space))
                {
                    _chip8.Pause();
                }

                if (CanBeHeld(Keys.D1))
                {
                    _chip8.Memory.Keypad[0] = true;
                }

                if (CanBeHeld(Keys.D2))
                {
                    _chip8.Memory.Keypad[1] = true;
                }

                if (CanBeHeld(Keys.D3))
                {
                    _chip8.Memory.Keypad[2] = true;
                }

                if (CanBeHeld(Keys.D4))
                {
                    _chip8.Memory.Keypad[3] = true;
                }

                if (CanBeHeld(Keys.Q))
                {
                    _chip8.Memory.Keypad[4] = true;
                }

                if (CanBeHeld(Keys.W))
                {
                    _chip8.Memory.Keypad[5] = true;
                }

                if (CanBeHeld(Keys.E))
                {
                    _chip8.Memory.Keypad[6] = true;
                }

                if (CanBeHeld(Keys.R))
                {
                    _chip8.Memory.Keypad[7] = true;
                }

                if (CanBeHeld(Keys.A))
                {
                    _chip8.Memory.Keypad[8] = true;
                }

                if (CanBeHeld(Keys.S))
                {
                    _chip8.Memory.Keypad[9] = true;
                }

                if (CanBeHeld(Keys.D))
                {
                    _chip8.Memory.Keypad[10] = true;
                }

                if (CanBeHeld(Keys.F))
                {
                    _chip8.Memory.Keypad[11] = true;
                }

                if (CanBeHeld(Keys.Z))
                {
                    _chip8.Memory.Keypad[12] = true;
                }

                if (CanBeHeld(Keys.X))
                {
                    _chip8.Memory.Keypad[13] = true;
                }

                if (CanBeHeld(Keys.C))
                {
                    _chip8.Memory.Keypad[14] = true;
                }

                if (CanBeHeld(Keys.V))
                {
                    _chip8.Memory.Keypad[15] = true;
                }

                if (CanBePressed(Keys.F1))
                {
                    --Speed;
                }

                if (CanBePressed(Keys.F2))
                {
                    ++Speed;
                }

                if (CanBePressed(Keys.F3))
                {
                    (_backgroundColor, _foregroundColor) = ColorSchemes[CurrentColorScheme++];
                }


                for (var i = 0; i < 10 * Speed; ++i)
                {
                    _chip8.Step();
                    PlaySound();
                }

                for (var j = 0; j < _chip8.Memory.Video.Length; ++j)
                {
                    _pixels[j] = _chip8.Memory.HasColor(j) ? _backgroundColor : _foregroundColor;
                }

                break;
            case Chip8State.Paused:
                if (CanBePressed(Keys.Space))
                {
                    _chip8.Pause();
                }

                break;
            case Chip8State.Off:
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
                    var selection = _romTitles[CurrentRom];
                    if (selection.Equals("Exit"))
                    {
                        Exit();
                    }
                    else
                    {
                        LoadSelection(selection);
                    }
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Array.Clear(_chip8.Memory.Keypad);
        _previousKeyboardState = _keyboardState;
        base.Update(gameTime);
    }

    private void LoadSelection(string selection)
    {
        var rootPath = Path.Combine(Content.RootDirectory, "Roms");
        var path = Path.Combine(rootPath, selection);
        using var reader = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
        var rom = reader.ReadBytes((int)reader.BaseStream.Length);
        _chip8 = new Chip8.Chip8();
        _chip8.Run(rom);
    }

    protected override void Draw(GameTime gameTime)
    {
        switch (_chip8.State)
        {
            case Chip8State.Running:
                GraphicsDevice.Clear(_backgroundColor);
                _canvas.SetData(_pixels, 0, _pixels.Length);
                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap);
                _spriteBatch.Draw(_canvas, new Rectangle(0, 0, _scaleSize.Width, _scaleSize.Height), Color.White);
                _spriteBatch.End();
                break;
            case Chip8State.Paused:
                GraphicsDevice.Clear(_backgroundColor);
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None);

                _spriteBatch.DrawString(_font, "PAUSED",
                    new Vector2(_graphics.PreferredBackBufferWidth * 0.43f, _graphics.PreferredBackBufferHeight * 0.5f),
                    Color.White);

                _spriteBatch.End();
                break;
            case Chip8State.Off:
                GraphicsDevice.Clear(Color.Black);
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None);

                const uint step = 75;
                var x = Math.Abs(_graphics.PreferredBackBufferWidth * 0.43f);
                var y = step;

                _spriteBatch.DrawString(_font, _romTitles[CurrentRom], new Vector2(x, 0), Color.Blue);

                for (var i = CurrentRom + 1; i < _romTitles.Count; ++i)
                {
                    _spriteBatch.DrawString(_font, _romTitles[i], new Vector2(x, y), Color.White);
                    y += step;
                }

                _spriteBatch.End();
                break;
        }

        base.Draw(gameTime);
    }
}
