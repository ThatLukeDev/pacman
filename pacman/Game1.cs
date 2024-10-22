using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace pacman
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Texture2D _pacmantex;

        Texture2D _fruit1tex;
        Texture2D _fruit2tex;
        Texture2D _fruit3tex;
        Texture2D _fruit4tex;

        Texture2D _ghost1tex;
        Texture2D _ghost2tex;
        Texture2D _ghost3tex;
        Texture2D _ghost4tex;

        Texture2D _ghost0tex;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _pacmantex = Content.Load<Texture2D>("pacman");

            _fruit1tex = Content.Load<Texture2D>("fruit1");
            _fruit2tex = Content.Load<Texture2D>("fruit2");
            _fruit3tex = Content.Load<Texture2D>("fruit3");
            _fruit4tex = Content.Load<Texture2D>("fruit4");

            _ghost1tex = Content.Load<Texture2D>("ghost1");
            _ghost2tex = Content.Load<Texture2D>("ghost2");
            _ghost3tex = Content.Load<Texture2D>("ghost3");
            _ghost4tex = Content.Load<Texture2D>("ghost4");

            _ghost0tex = Content.Load<Texture2D>("ghost0");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }
    }
}
