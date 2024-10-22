using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace pacman
{
    public class Entity
    {
        Vector2 position;
        Vector2 size;
        Vector2 velocity;
        Texture2D sprite;

        Entity()
        {
            position = new Vector2();
            size = new Vector2();
            velocity = new Vector2();
        }

        Entity(Vector2 _position, Vector2 _size, Texture2D _sprite)
        {
            position = _position;
            size = _size;
            velocity = new Vector2();
            sprite = _sprite;
        }

        Rectangle bounds()
        {
            return new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
        }

        void update(float dtime)
        {
            position += dtime * velocity;
        }
        void update()
        {
            update(1 / 60f);
        }
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

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
