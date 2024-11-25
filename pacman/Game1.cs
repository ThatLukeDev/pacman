using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
        public float speed = 100;

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
                        sprite = m_spriteDown1;
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
                        sprite = m_spriteDown2;
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

            _spriteBatch.Draw(sprite, bounds(), null, Color.White, 0, new Vector2(sprite.Width / 2, sprite.Height / 2), SpriteEffects.None, 1);
        }

        public void stepVelocity(Map ghostMap, Vector2 pacmanPos)
        {
            byte[][] map = ghostMap.mapdata;

            Vector2 adjustedGhostPos = (position + size / 2) / new Vector2(Map.OBJ_WIDTH, Map.OBJ_HEIGHT);
            Vector2 ghostPos = new Vector2((int)adjustedGhostPos.X, (int)adjustedGhostPos.Y);

            if (pacmanPos.X % 2 == 0)
                pacmanPos.X--;
            if (pacmanPos.Y % 2 == 0)
                pacmanPos.Y--;

            if (ghostPos.X % 2 == 1 && ghostPos.Y % 2 == 1)
            {
                List<Vector2> path = ghostMap.pathFind(ghostPos, pacmanPos);
                Vector2 offset = path[path.Count - 1] - ghostPos;
                if (path.Count > 1)
                    offset = path[path.Count - 2] - ghostPos;

                switch (offset)
                {
                    case Vector2(-2, 0):
                        direction = directionType.left;
                        break;
                    case Vector2(2, 0):
                        direction = directionType.right;
                        break;
                    case Vector2(0, -2):
                        direction = directionType.up;
                        break;
                    case Vector2(0, 2):
                        direction = directionType.down;
                        break;
                }
            }

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

        public int points = 0;

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

        public Map(Texture2D wallTex, Texture2D bitTex)
        {
            bool mapCorrect = false;
            while (!mapCorrect)
            {
                trialMap(wallTex, bitTex);

                int[][] vals = new int[mapdata.Length][];
                bool[][] closed = new bool[mapdata.Length][];
                for (int i = 0; i < mapdata.Length; i++)
                {
                    vals[i] = new int[mapdata[0].Length];
                    closed[i] = new bool[mapdata[0].Length];
                    for (int j = 0; j < mapdata[0].Length; j++)
                    {
                        vals[i][j] = 2147483647;
                        closed[i][j] = false;
                    }
                }
                vals[1][1] = 0;

                pathFindDistancers(new Vector2(1, 1), ref vals, ref closed);

                mapCorrect = true;
                for (int j = 1; j < mapdata[0].Length; j += 2)
                {
                    for (int i = 1; i < mapdata.Length; i+=2)
                    {
                        if (vals[i][j] == 2147483647)
                            mapCorrect = false;
                    }
                }
            }
        }

        public void trialMap(Texture2D wallTex, Texture2D bitText)
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
            for (int i = 1; i < WINDOW_WIDTH / OBJ_WIDTH; i+=2)
            {
                for (int j = 1; j < WINDOW_HEIGHT / OBJ_HEIGHT; j+=2)
                {
                    mapdata[i][j] = 2;
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

        public List<Vector2> pathFind(Vector2 pos1, Vector2 pos2)
        {
            int[][] vals = new int[mapdata.Length][];
            bool[][] closed = new bool[mapdata.Length][];
            for (int i = 0; i < mapdata.Length; i++)
            {
                vals[i] = new int[mapdata[0].Length];
                closed[i] = new bool[mapdata[0].Length];
                for (int j = 0; j < mapdata[0].Length; j++)
                {
                    vals[i][j] = 2147483647;
                    closed[i][j] = false;
                }
            }
            vals[(int)pos1.X][(int)pos1.Y] = 0;

            pathFindDistancers(pos2, ref vals, ref closed);

            List<Vector2> path = new List<Vector2> { pos2 };

            int workingX = (int)path.Last().X;
            int workingY = (int)path.Last().Y;

            while (workingX != pos1.X || workingY != pos1.Y)
            {
                if (pathFindBackwards(1, 0, ref workingX, ref workingY, ref vals))
                {
                    path.Add(new Vector2(workingX, workingY));
                }
                else if (pathFindBackwards(-1, 0, ref workingX, ref workingY, ref vals))
                {
                    path.Add(new Vector2(workingX, workingY));
                }
                else if (pathFindBackwards(0, 1, ref workingX, ref workingY, ref vals))
                {
                    path.Add(new Vector2(workingX, workingY));
                }
                else if (pathFindBackwards(0, -1, ref workingX, ref workingY, ref vals))
                {
                    path.Add(new Vector2(workingX, workingY));
                }
            }

            return path;
        }
        void pathFindDistancers(Vector2 goal, ref int[][] vals, ref bool[][] closed)
        {
            while (true)
            {
                int workingX = 0;
                int workingY = 0;
                for (int x = 1; x < mapdata.Length; x += 2)
                {
                    for (int y = 1; y < mapdata[0].Length; y += 2)
                    {
                        if (closed[x][y])
                            continue;
                        if (vals[x][y] < vals[workingX][workingY])
                        {
                            workingX = x;
                            workingY = y;
                        }
                    }
                }
                closed[workingX][workingY] = true;
                if (workingX == 0 && workingY == 0)
                    return;
                pathFindDistancersSetAt(-1, 0, workingX, workingY, ref vals);
                pathFindDistancersSetAt(1, 0, workingX, workingY, ref vals);
                pathFindDistancersSetAt(0, -1, workingX, workingY, ref vals);
                pathFindDistancersSetAt(0, 1, workingX, workingY, ref vals);
            }
        }

        void pathFindDistancersSetAt(int x, int y, int workingX, int workingY, ref int[][] vals)
        {
            if (mapdata[workingX + x][workingY + y] != 1)
            {
                int testval = vals[workingX][workingY] + 2;
                if (testval < vals[workingX + x * 2][workingY + y * 2])
                {
                    vals[workingX + x * 2][workingY + y * 2] = testval;
                }
            }
        }

        bool pathFindBackwards(int x, int y, ref int workingX, ref int workingY, ref int[][] vals)
        {
            if (workingX + x * 2 < 0)
                return false;
            if (workingX + x * 2 > mapdata.Length)
                return false;
            if (workingY + y * 2 < 0)
                return false;
            if (workingY + y * 2 > mapdata[0].Length)
                return false;

            if (mapdata[workingX + x][workingY + y] == 1)
                return false;

            if (vals[workingX + x * 2][workingY + y * 2] == vals[workingX][workingY] - 2)
            {
                workingX = workingX + x * 2;
                workingY = workingY + y * 2;

                return true;
            }

            return false;
        }
    }

    enum gameState
    {
        splash,
        game,
        end,
    }
    enum gameDifficulty
    {
        easy,
        medium,
        hard,
        expert,
        expertplus
    }

    public class Game1 : Game
    {
        public void step()
        {
            if (Keyboard.GetState().IsKeyDown(KeyUp))
            {
                firstKeyPressed = true;
                ((Pacman)gameScene["plr"]).direction = directionType.up;
            }
            else if (Keyboard.GetState().IsKeyDown(KeyDown))
            {
                firstKeyPressed = true;
                ((Pacman)gameScene["plr"]).direction = directionType.down;
            }
            else if (Keyboard.GetState().IsKeyDown(KeyLeft))
            {
                firstKeyPressed = true;
                ((Pacman)gameScene["plr"]).direction = directionType.left;
            }
            else if (Keyboard.GetState().IsKeyDown(KeyRight))
            {
                firstKeyPressed = true;
                ((Pacman)gameScene["plr"]).direction = directionType.right;
            }

            ((Pacman)gameScene["plr"]).stepVelocity(((Map)gameScene).mapdata);
            ((Pacman)gameScene["plr"]).collectBits(ref ((Map)gameScene).mapdata);

            Vector2 adjustedPacmanPos = (((Pacman)gameScene["plr"]).position + ((Pacman)gameScene["plr"]).size / 2) / new Vector2(Map.OBJ_WIDTH, Map.OBJ_HEIGHT);
            Vector2 pacmanPos = new Vector2((int)(Math.Round(adjustedPacmanPos.X / 2) * 2), (int)(Math.Round(adjustedPacmanPos.Y / 2) * 2));

            Vector2 ghostTargetPos1 = pacmanPos;
            Vector2 ghostTargetPos2 = pacmanPos;
            Vector2 ghostTargetPos3 = pacmanPos;
            Vector2 ghostTargetPos4 = pacmanPos;

            Random rnd = new Random();
            switch (difficulty)
            {
                case gameDifficulty.easy:
                    ghostTargetPos1 = new Vector2(
                        (int)(rnd.Next(0, gameScene.mapdata.Length - 1) / 2) * 2 + 1,
                        (int)(rnd.Next(0, gameScene.mapdata[0].Length - 1) / 2) * 2 + 1
                    );
                    ghostTargetPos2 = new Vector2(
                        (int)(rnd.Next(0, gameScene.mapdata.Length - 1) / 2) * 2 + 1,
                        (int)(rnd.Next(0, gameScene.mapdata[0].Length - 1) / 2) * 2 + 1
                    );
                    ghostTargetPos3 = new Vector2(
                        (int)(rnd.Next(0, gameScene.mapdata.Length - 1) / 2) * 2 + 1,
                        (int)(rnd.Next(0, gameScene.mapdata[0].Length - 1) / 2) * 2 + 1
                    );
                    ghostTargetPos4 = new Vector2(
                        (int)(rnd.Next(0, gameScene.mapdata.Length - 1) / 2) * 2 + 1,
                        (int)(rnd.Next(0, gameScene.mapdata[0].Length - 1) / 2) * 2 + 1
                    );
                    ((Ghost)gameScene["ghost1"]).speed = 200;
                    ((Ghost)gameScene["ghost2"]).speed = 200;
                    ((Ghost)gameScene["ghost3"]).speed = 200;
                    ((Ghost)gameScene["ghost4"]).speed = 200;
                    break;
                case gameDifficulty.medium:
                    ((Ghost)gameScene["ghost1"]).speed = 50;
                    ((Ghost)gameScene["ghost2"]).speed = 50;
                    ((Ghost)gameScene["ghost3"]).speed = 50;
                    ((Ghost)gameScene["ghost4"]).speed = 50;
                    break;
                case gameDifficulty.hard:
                    ((Ghost)gameScene["ghost1"]).speed = 75;
                    ((Ghost)gameScene["ghost2"]).speed = 75;
                    ((Ghost)gameScene["ghost3"]).speed = 75;
                    ((Ghost)gameScene["ghost4"]).speed = 75;
                    break;
                case gameDifficulty.expert:
                    ((Ghost)gameScene["ghost1"]).speed = 100;
                    ((Ghost)gameScene["ghost2"]).speed = 100;
                    ((Ghost)gameScene["ghost3"]).speed = 100;
                    ((Ghost)gameScene["ghost4"]).speed = 100;
                    break;
                case gameDifficulty.expertplus:
                    ((Ghost)gameScene["ghost1"]).speed = 100;
                    ((Ghost)gameScene["ghost2"]).speed = 100;
                    ((Ghost)gameScene["ghost3"]).speed = 100;
                    ((Ghost)gameScene["ghost4"]).speed = 100;
                    ghostTargetPos1 += new Vector2(1, 0) * (gameScene["plr"].position - gameScene["ghost1"].position).Length() / 128;
                    ghostTargetPos1.X = Math.Clamp((int)ghostTargetPos1.X, 1, gameScene.mapdata.Length - 2);
                    ghostTargetPos1.Y = Math.Clamp((int)ghostTargetPos1.Y, 1, gameScene.mapdata[0].Length - 2);
                    ghostTargetPos2 += new Vector2(-1, 0) * (gameScene["plr"].position - gameScene["ghost2"].position).Length() / 128;
                    ghostTargetPos2.X = Math.Clamp((int)ghostTargetPos2.X, 1, gameScene.mapdata.Length - 2);
                    ghostTargetPos2.Y = Math.Clamp((int)ghostTargetPos2.Y, 1, gameScene.mapdata[0].Length - 2);
                    ghostTargetPos3 += new Vector2(0, 1) * (gameScene["plr"].position - gameScene["ghost3"].position).Length() / 128;
                    ghostTargetPos3.X = Math.Clamp((int)ghostTargetPos3.X, 1, gameScene.mapdata.Length - 2);
                    ghostTargetPos3.Y = Math.Clamp((int)ghostTargetPos3.Y, 1, gameScene.mapdata[0].Length - 2);
                    ghostTargetPos4 += new Vector2(0, -1) * (gameScene["plr"].position - gameScene["ghost4"].position).Length() / 128;
                    ghostTargetPos4.X = Math.Clamp((int)ghostTargetPos4.X, 1, gameScene.mapdata.Length - 2);
                    ghostTargetPos4.Y = Math.Clamp((int)ghostTargetPos4.Y, 1, gameScene.mapdata[0].Length - 2);
                    break;
            }

            if (firstKeyPressed)
            {
                ((Ghost)gameScene["ghost1"]).stepVelocity((Map)gameScene, ghostTargetPos1);
                ((Ghost)gameScene["ghost2"]).stepVelocity((Map)gameScene, ghostTargetPos2);
                ((Ghost)gameScene["ghost3"]).stepVelocity((Map)gameScene, ghostTargetPos3);
                ((Ghost)gameScene["ghost4"]).stepVelocity((Map)gameScene, ghostTargetPos4);
            }

            if ((gameScene["ghost1"].position - gameScene["plr"].position).Length() < 16)
                state = gameState.end;
            if ((gameScene["ghost2"].position - gameScene["plr"].position).Length() < 16)
                state = gameState.end;
            if ((gameScene["ghost3"].position - gameScene["plr"].position).Length() < 16)
                state = gameState.end;
            if ((gameScene["ghost4"].position - gameScene["plr"].position).Length() < 16)
                state = gameState.end;
        }

        const string SAVEPATH = "%appdata%\\ThatLukeDev";
        const string SAVEFILE = "save.dat";

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        int highscore = 0;

        FileStream _file;

        Map gameScene;
        gameState state;
        gameDifficulty difficulty = gameDifficulty.expertplus;
        bool changeStateDebounce = false;
        bool changeDifficultyDebounce = false;
        bool firstKeyPressed = false;

        const Keys KeyLeft = Keys.A;
        const Keys KeyRight = Keys.D;
        const Keys KeyUp = Keys.W;
        const Keys KeyDown = Keys.S;
        const Keys KeyAction = Keys.Space;

        SpriteFont _font;
        SpriteFont _smallFont;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            _font = Content.Load<SpriteFont>("font");
            _smallFont = Content.Load<SpriteFont>("fontSmall");

            state = gameState.splash;
            firstKeyPressed = false;

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

            if (File.Exists(SAVEPATH + "\\" + SAVEFILE))
            {
                _file = File.Open(SAVEPATH + "\\" + SAVEFILE, FileMode.Open);
                for (int i = 0; i < 4; i++)
                    highscore |= _file.ReadByte() << (i * 8);
                _file.Close();
            }
            else
            {
                System.IO.Directory.CreateDirectory(SAVEPATH);
                _file = File.Create(SAVEPATH + "\\" + SAVEFILE);
                _file.Close();
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (state)
            {
                case gameState.splash:
                    if (Keyboard.GetState().IsKeyDown(KeyAction))
                    {
                        if (!changeStateDebounce)
                            state = gameState.game;
                        changeStateDebounce = true;
                    }
                    else
                    {
                        changeStateDebounce = false;
                    }
                    if (Keyboard.GetState().IsKeyDown(KeyRight))
                    {
                        if (!changeDifficultyDebounce)
                            difficulty = (gameDifficulty)((int)(difficulty + 1) % (int)(gameDifficulty.expertplus + 1));
                        changeDifficultyDebounce = true;
                    }
                    else if (Keyboard.GetState().IsKeyDown(KeyLeft))
                    {
                        if (!changeDifficultyDebounce)
                            difficulty = (gameDifficulty)((int)(difficulty + (int)gameDifficulty.expertplus) % (int)(gameDifficulty.expertplus + 1));
                        changeDifficultyDebounce = true;
                    }
                    else
                    {
                        changeDifficultyDebounce = false;
                    }
                    break;
                case gameState.game:
                    step();
                    gameScene.update();
                    break;
                case gameState.end:
                    if (((Pacman)gameScene["plr"]).points * Math.Pow(2, (int)difficulty) > highscore)
                    {
                        highscore = (int)(((Pacman)gameScene["plr"]).points * Math.Pow(2, (int)difficulty));
                        _file = File.Open(SAVEPATH + "\\" + SAVEFILE, FileMode.Open);
                        for (int i = 0; i < 4; i++)
                            _file.WriteByte((byte)(highscore << (i * 8)));
                        _file.Close();
                    }
                    if (Keyboard.GetState().IsKeyDown(KeyAction))
                    {
                        if (!changeStateDebounce)
                            Initialize();
                        changeStateDebounce = true;
                    }
                    else
                    {
                        changeStateDebounce = false;
                    }
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(samplerState:SamplerState.PointClamp);

            switch (state)
            {
                case gameState.splash:
                    _spriteBatch.DrawString(_font, "PACMAN", new Vector2(_graphics.GraphicsDevice.Viewport.Width / 2 - 90, _graphics.GraphicsDevice.Viewport.Height / 2 - 30), Color.White);
                    _spriteBatch.DrawString(_smallFont, $"Difficulty: {difficulty}", new Vector2(), Color.White);
                    break;
                case gameState.game:
                    gameScene.draw(_spriteBatch);
                    break;
                case gameState.end:
                    _spriteBatch.DrawString(_font, "YOU LOSE", new Vector2(_graphics.GraphicsDevice.Viewport.Width / 2 - 120, _graphics.GraphicsDevice.Viewport.Height / 2 - 30), Color.White);
                    _spriteBatch.DrawString(_smallFont, $"Points:    {(((Pacman)gameScene["plr"]).points * Math.Pow(2, (int)difficulty)).ToString("000")}", new Vector2(), Color.White);
                    _spriteBatch.DrawString(_smallFont, $"Highscore: {highscore.ToString("000")}", new Vector2(0, 40), Color.White);
                    break;
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
