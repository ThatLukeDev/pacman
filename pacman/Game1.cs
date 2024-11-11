﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;

namespace pacman
{
    public class Entity
    {
        public Vector2 position;
        public Vector2 size;
        public Vector2 velocity;
        public Texture2D sprite;

        public bool pointToVelocity = false;

        public Entity()
        {
            position = new Vector2();
            size = new Vector2();
            velocity = new Vector2();
        }

        public Entity(Vector2 _position, Vector2 _size, Texture2D _sprite)
        {
            position = _position;
            size = _size;
            velocity = new Vector2();
            sprite = _sprite;
        }

        public virtual Rectangle bounds()
        {
            return new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
        }

        public virtual void update(float dtime)
        {
            position += dtime * velocity;
        }

        public virtual void draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(sprite, bounds(), Color.White);
        }
    }

    public class Scene
    {
        public Dictionary<string, Entity> entities = new Dictionary<string, Entity>();

        public Scene() { }

        public Entity this[string name]
        {
            get
            {
                return entities[name];
            }
            set
            {
                if (entities.ContainsKey(name))
                {
                    entities[name] = value;
                }
                else
                {
                    entities.Add(name, value);
                }
            }
        }

        public void update(float dtime)
        {
            foreach (var entity in entities.Values)
            {
                entity.update(dtime);
            }
        }

        public void update()
        {
            update(1 / 60f);
        }

        public virtual void draw(SpriteBatch _spriteBatch)
        {
            foreach (var entity in entities.Values)
            {
                if (entity.sprite != null)
                {
                    entity.draw(_spriteBatch);
                }
            }
        }
    }

    public enum directionType
    {
        none = 0,
        up = 1,
        down = 2,
        left = 3,
        right = 4
    }

    public class Ghost : Entity
    {
        float speed = 100;

        Texture2D m_spriteLeft1;
        Texture2D m_spriteLeft2;
        Texture2D m_spriteRight1;
        Texture2D m_spriteRight2;
        Texture2D m_spriteUp1;
        Texture2D m_spriteUp2;
        Texture2D m_spriteDown1;
        Texture2D m_spriteDown2;

        long count = 0;

        public long renders = 10;

        public directionType direction = directionType.none;

        int points = 0;

        public Ghost(Vector2 _position, Vector2 _size,
            Texture2D _spriteLeft1, Texture2D _spriteLeft2,
            Texture2D _spriteRight1, Texture2D _spriteRight2,
            Texture2D _spriteUp1, Texture2D _spriteUp2,
            Texture2D _spriteDown1, Texture2D _spriteDown2
            )
        {
            position = _position;
            size = _size;
            velocity = new Vector2();
            sprite = _spriteUp1;
            m_spriteLeft1 = _spriteLeft1;
            m_spriteLeft2 = _spriteLeft2;
            m_spriteRight1 = _spriteRight1;
            m_spriteRight2 = _spriteRight2;
            m_spriteUp1 = _spriteUp1;
            m_spriteUp2 = _spriteUp2;
            m_spriteDown1 = _spriteDown1;
            m_spriteDown2 = _spriteDown2;
        }

        public override Rectangle bounds()
        {
            return new Rectangle((int)(position.X + size.X / 2), (int)(position.Y + size.Y / 2), (int)size.X, (int)size.Y);
        }

        public override void draw(SpriteBatch _spriteBatch)
        {
            if (count / renders % 2 == 0)
                switch (direction)
                {
                    case directionType.down:
                        sprite = m_spriteUp1;
                        break;
                    case directionType.left:
                        sprite = m_spriteLeft1;
                        break;
                    case directionType.right:
                        sprite = m_spriteRight1;
                        break;
                    default:
                        sprite = m_spriteUp1;
                        break;
                }
            else
                switch (direction)
                {
                    case directionType.down:
                        sprite = m_spriteUp2;
                        break;
                    case directionType.left:
                        sprite = m_spriteLeft2;
                        break;
                    case directionType.right:
                        sprite = m_spriteRight2;
                        break;
                    default:
                        sprite = m_spriteUp2;
                        break;
                }
            count++;

            _spriteBatch.Draw(sprite, bounds(), null, Color.White, (float)Math.Atan2(velocity.Y, velocity.X), new Vector2(sprite.Width / 2, sprite.Height / 2), SpriteEffects.None, 1);
        }

        public void stepVelocity(byte[][] map, Pacman pacman)
        {
            if (position.X % Map.OBJ_WIDTH <= speed / 60 && position.Y % Map.OBJ_HEIGHT <= speed / 60)
            {
                int x = (int)(position.X / Map.OBJ_WIDTH);
                int y = (int)(position.Y / Map.OBJ_HEIGHT);

                int movx = 0;
                int movy = 0;

                switch (direction)
                {
                    case directionType.up:
                        movy = -1;
                        break;
                    case directionType.down:
                        movy = 1;
                        break;
                    case directionType.left:
                        movx = -1;
                        break;
                    case directionType.right:
                        movx = 1;
                        break;
                }

                if (map[x + movx][y + movy] != 1)
                {
                    velocity.X = movx * speed;
                    velocity.Y = movy * speed;
                }
                else
                {
                    movx = 0;
                    if (velocity.X < 0)
                        movx = -1;
                    else if (velocity.X > 0)
                        movx = 1;

                    movy = 0;
                    if (velocity.Y < 0)
                        movy = -1;
                    else if (velocity.Y > 0)
                        movy = 1;

                    if (map[x + movx][y + movy] == 1)
                    {
                        velocity = Vector2.Zero;
                    }
                }
            }
        }
    }

    public class Pacman : Entity
    {
        float speed = 100;

        Texture2D m_sprite1;
        Texture2D m_sprite2;

        long count = 0;

        public long renders = 10;

        public directionType direction = directionType.none;

        int points = 0;

        public Pacman(Vector2 _position, Vector2 _size, Texture2D _sprite1, Texture2D _sprite2)
        {
            position = _position;
            size = _size;
            velocity = new Vector2();
            sprite = _sprite1;
            m_sprite1 = _sprite1;
            m_sprite2 = _sprite2;
        }

        public override Rectangle bounds()
        {
            return new Rectangle((int)(position.X + size.X / 2), (int)(position.Y + size.Y / 2), (int)size.X, (int)size.Y);
        }

        public override void draw(SpriteBatch _spriteBatch)
        {
            if (count / renders % 2 == 0)
                sprite = m_sprite1;
            else
                sprite = m_sprite2;
            count++;

            _spriteBatch.Draw(sprite, bounds(), null, Color.White, (float)Math.Atan2(velocity.Y, velocity.X), new Vector2(sprite.Width / 2, sprite.Height / 2), SpriteEffects.None, 1);
        }

        public void stepVelocity(byte[][] map)
        {
            if (position.X % Map.OBJ_WIDTH <= speed / 60 && position.Y % Map.OBJ_HEIGHT <= speed / 60)
            {
                int x = (int)(position.X / Map.OBJ_WIDTH);
                int y = (int)(position.Y / Map.OBJ_HEIGHT);

                int movx = 0;
                int movy = 0;

                switch (direction)
                {
                    case directionType.up:
                        movy = -1;
                        break;
                    case directionType.down:
                        movy = 1;
                        break;
                    case directionType.left:
                        movx = -1;
                        break;
                    case directionType.right:
                        movx = 1;
                        break;
                }

                if (map[x + movx][y + movy] != 1)
                {
                    velocity.X = movx * speed;
                    velocity.Y = movy * speed;
                }
                else
                {
                    movx = 0;
                    if (velocity.X < 0)
                        movx = -1;
                    else if (velocity.X > 0)
                        movx = 1;

                    movy = 0;
                    if (velocity.Y < 0)
                        movy = -1;
                    else if (velocity.Y > 0)
                        movy = 1;

                    if (map[x + movx][y + movy] == 1)
                    {
                        velocity = Vector2.Zero;
                    }
                }
            }
        }

        public void collectBits(ref byte[][] map)
        {
            if (position.X % Map.OBJ_WIDTH <= speed / 60 && position.Y % Map.OBJ_HEIGHT <= speed / 60)
            {
                int x = (int)(position.X / Map.OBJ_WIDTH);
                int y = (int)(position.Y / Map.OBJ_HEIGHT);

                if (map[x][y] != 0)
                {
                    map[x][y] = 0;
                    points++;
                }
            }
        }
    }

    public class Map : Scene
    {
        Random rnd = new Random();

        public const int WINDOW_WIDTH = 800;
        public const int WINDOW_HEIGHT = 480;
        public const int OBJ_WIDTH = 32;
        public const int OBJ_HEIGHT = 32;

        public byte[][] mapdata = new byte[WINDOW_WIDTH / OBJ_WIDTH][];

        Texture2D _wallTex;
        Texture2D _bitText;

        public Map(Texture2D wallTex, Texture2D bitText)
        {
            _wallTex = wallTex;
            _bitText = bitText;

            for (int i = 0; i < WINDOW_WIDTH / OBJ_WIDTH; i++)
            {
                mapdata[i] = new byte[WINDOW_HEIGHT / OBJ_HEIGHT];
                for (int j = 0; j < WINDOW_HEIGHT / OBJ_HEIGHT; j++)
                {
                    mapdata[i][j] = 1;
                    if (i % 2 == 1 && j % 2 == 1)
                    {
                        mapdata[i][j] = 0;
                    }
                    else if (i % 2 != j % 2)
                    {
                        mapdata[i][j] = (byte)rnd.Next(0, 2);
                    }
                    else if (i % 2 == 1 || j % 2 == 1)
                    {
                        mapdata[i][j] = 1;
                    }
                }
            }
            for (int i = 0; i < WINDOW_WIDTH / OBJ_WIDTH; i++)
            {
                mapdata[i][0] = 1;
                mapdata[i][WINDOW_HEIGHT / OBJ_HEIGHT - 1] = 1;
            }
            for (int j = 0; j < WINDOW_HEIGHT / OBJ_HEIGHT; j++)
            {
                mapdata[0][j] = 1;
                mapdata[WINDOW_WIDTH / OBJ_WIDTH - 1][j] = 1;
            }
            for (int i = 1; i < WINDOW_WIDTH / OBJ_WIDTH; i += 2)
            {
                for (int j = 1; j < WINDOW_HEIGHT / OBJ_HEIGHT; j += 2)
                {
                    int connections = 0;
                    if (mapdata[i - 1][j] != 1)
                        connections++;
                    if (mapdata[i + 1][j] != 1)
                        connections++;
                    if (mapdata[i][j - 1] != 1)
                        connections++;
                    if (mapdata[i][j + 1] != 1)
                        connections++;

                    while (connections < 2)
                    {
                        int xoffset = 0;
                        int yoffset = 0;
                        while (!((xoffset == 0 || yoffset == 0) && (xoffset != 0 || yoffset != 0) && mapdata[i + xoffset][j + yoffset] == 1))
                        {
                            xoffset = rnd.Next(-1, 2);
                            yoffset = rnd.Next(-1, 2);
                        }
                        mapdata[i + xoffset][j + yoffset] = 0;
                        connections++;
                    }
                }
            }
            for (int i = 0; i < WINDOW_WIDTH / OBJ_WIDTH; i++)
            {
                mapdata[i][0] = 1;
                mapdata[i][WINDOW_HEIGHT / OBJ_HEIGHT - 1] = 1;
            }
            for (int j = 0; j < WINDOW_HEIGHT / OBJ_HEIGHT; j++)
            {
                mapdata[0][j] = 1;
                mapdata[WINDOW_WIDTH / OBJ_WIDTH - 1][j] = 1;
            }
            for (int i = 0; i < WINDOW_WIDTH / OBJ_WIDTH; i++)
            {
                for (int j = 0; j < WINDOW_HEIGHT / OBJ_HEIGHT; j++)
                {
                    if (mapdata[i][j] == 0)
                    {
                        mapdata[i][j] = 2;
                    }
                }
            }
        }

        public override void draw(SpriteBatch _spriteBatch)
        {
            for (int x = 0; x < WINDOW_WIDTH / OBJ_WIDTH; x++)
            {
                for (int y = 0; y < WINDOW_HEIGHT / OBJ_HEIGHT; y++)
                {
                    if (mapdata[x][y] == 1)
                    {
                        _spriteBatch.Draw(_wallTex, new Rectangle(x * OBJ_WIDTH, y * OBJ_HEIGHT, OBJ_WIDTH, OBJ_HEIGHT), Color.White);
                    }
                    if (mapdata[x][y] == 2)
                    {
                        _spriteBatch.Draw(_bitText, new Rectangle(x * OBJ_WIDTH, y * OBJ_HEIGHT, OBJ_WIDTH, OBJ_HEIGHT), Color.White);
                    }
                }
            }

            foreach (var entity in entities.Values)
            {
                if (entity.sprite != null)
                {
                    entity.draw(_spriteBatch);
                }
            }
        }

        public List<directionType> pathFind(Vector2 pos1, Vector2 pos2)
        {
            List<directionType> path = new List<directionType>();

            return null;
        }
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Map gameScene;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            gameScene = new Map(Content.Load<Texture2D>("walls/full"), Content.Load<Texture2D>("walls/bit"));

            gameScene["plr"] = new Pacman(
                new Vector2(32, 32),
                new Vector2(32, 32),
                Content.Load<Texture2D>("pacman/1"),
                Content.Load<Texture2D>("pacman/2")
            );
            gameScene["plr"].pointToVelocity = true;
            gameScene["ghost1"] = new Ghost(
                new Vector2(288, 224),
                new Vector2(32, 32),
                Content.Load<Texture2D>("ghosts/red/left/1"),
                Content.Load<Texture2D>("ghosts/red/left/2"),
                Content.Load<Texture2D>("ghosts/red/right/1"),
                Content.Load<Texture2D>("ghosts/red/right/2"),
                Content.Load<Texture2D>("ghosts/red/up/1"),
                Content.Load<Texture2D>("ghosts/red/up/2"),
                Content.Load<Texture2D>("ghosts/red/down/1"),
                Content.Load<Texture2D>("ghosts/red/down/2")
            );
            gameScene["ghost2"] = new Ghost(
                new Vector2(352, 224),
                new Vector2(32, 32),
                Content.Load<Texture2D>("ghosts/yellow/left/1"),
                Content.Load<Texture2D>("ghosts/yellow/left/2"),
                Content.Load<Texture2D>("ghosts/yellow/right/1"),
                Content.Load<Texture2D>("ghosts/yellow/right/2"),
                Content.Load<Texture2D>("ghosts/yellow/up/1"),
                Content.Load<Texture2D>("ghosts/yellow/up/2"),
                Content.Load<Texture2D>("ghosts/yellow/down/1"),
                Content.Load<Texture2D>("ghosts/yellow/down/2")
            );
            gameScene["ghost3"] = new Ghost(
                new Vector2(416, 224),
                new Vector2(32, 32),
                Content.Load<Texture2D>("ghosts/pink/left/1"),
                Content.Load<Texture2D>("ghosts/pink/left/2"),
                Content.Load<Texture2D>("ghosts/pink/right/1"),
                Content.Load<Texture2D>("ghosts/pink/right/2"),
                Content.Load<Texture2D>("ghosts/pink/up/1"),
                Content.Load<Texture2D>("ghosts/pink/up/2"),
                Content.Load<Texture2D>("ghosts/pink/down/1"),
                Content.Load<Texture2D>("ghosts/pink/down/2")
            );
            gameScene["ghost4"] = new Ghost(
                new Vector2(480, 224),
                new Vector2(32, 32),
                Content.Load<Texture2D>("ghosts/blue/left/1"),
                Content.Load<Texture2D>("ghosts/blue/left/2"),
                Content.Load<Texture2D>("ghosts/blue/right/1"),
                Content.Load<Texture2D>("ghosts/blue/right/2"),
                Content.Load<Texture2D>("ghosts/blue/up/1"),
                Content.Load<Texture2D>("ghosts/blue/up/2"),
                Content.Load<Texture2D>("ghosts/blue/down/1"),
                Content.Load<Texture2D>("ghosts/blue/down/2")
            );
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            gameScene.update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (Keyboard.GetState().IsKeyDown(Keys.W))
                ((Pacman)gameScene["plr"]).direction = directionType.up;
            else if (Keyboard.GetState().IsKeyDown(Keys.S))
                ((Pacman)gameScene["plr"]).direction = directionType.down;
            else if (Keyboard.GetState().IsKeyDown(Keys.A))
                ((Pacman)gameScene["plr"]).direction = directionType.left;
            else if (Keyboard.GetState().IsKeyDown(Keys.D))
                ((Pacman)gameScene["plr"]).direction = directionType.right;
            ((Pacman)gameScene["plr"]).stepVelocity(((Map)gameScene).mapdata);
            ((Pacman)gameScene["plr"]).collectBits(ref ((Map)gameScene).mapdata);

            _spriteBatch.Begin(samplerState:SamplerState.PointClamp);
            gameScene.draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
