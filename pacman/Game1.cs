﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.CodeDom;
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

        public Rectangle bounds()
        {
            return new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
        }

        public void update(float dtime)
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

        public void draw(SpriteBatch _spriteBatch)
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

    public class Pacman : Entity
    {
        Texture2D m_sprite1;
        Texture2D m_sprite2;

        long count = 0;

        public long renders = 10;

        public Pacman(Vector2 _position, Vector2 _size, Texture2D _sprite1, Texture2D _sprite2)
        {
            position = _position;
            size = _size;
            velocity = new Vector2();
            sprite = _sprite1;
            m_sprite1 = _sprite1;
            m_sprite2 = _sprite2;
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
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        const float PLR_SPEED = 100;

        Scene gameScene;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            gameScene = new Scene();

            gameScene["plr"] = new Pacman(new Vector2(0, 0), new Vector2(50, 50), Content.Load<Texture2D>("pacman/1"), Content.Load<Texture2D>("pacman/2"));
            gameScene["plr"].pointToVelocity = true;
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
                gameScene["plr"].velocity = new Vector2(0, -PLR_SPEED);
            else if (Keyboard.GetState().IsKeyDown(Keys.S))
                gameScene["plr"].velocity = new Vector2(0, PLR_SPEED);
            else if (Keyboard.GetState().IsKeyDown(Keys.A))
                gameScene["plr"].velocity = new Vector2(-PLR_SPEED, 0);
            else if (Keyboard.GetState().IsKeyDown(Keys.D))
                gameScene["plr"].velocity = new Vector2(PLR_SPEED, 0);

            _spriteBatch.Begin(samplerState:SamplerState.PointClamp);
            gameScene.draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
