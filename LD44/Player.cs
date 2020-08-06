using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LD44
{
    class Player
    {
        public Texture2D Sprite { get; set; }
        public Vector2 Position { get; set; }
        public float Speed { get; set; }
        public int Lives { get; set; }
        public Direction direction { get; set; }
        public int Money { get; set; }
        public double DeathCooldown { get; set; }

        public Player(Texture2D sprite, int x = 0, int y = 0)
        {
            Sprite = sprite;
            Position = new Vector2(x, y);
            Speed = 5;
            Lives = 3;
            direction = Direction.Down;
            Money = 0;
            DeathCooldown = 500;
        }
    }

    class Enemy : Player
    {
        public bool IsDead { get; set; }

        public Enemy(Texture2D sprite, Random r) : base(sprite)
        {
            Sprite = sprite;
            Position = GetRandomSpawnPosition(r);
            Speed = (float)(r.NextDouble() * 3) + 2f;
            Lives = 1;
            IsDead = false;
            direction = Direction.Down;
            //Console.WriteLine(Speed);
        }

        public void MoveToward(Player p)
        {
            if(!IsDead)
            {
                Random r = new Random();
                Vector2 direction = Vector2.Normalize(p.Position - Position);
                Position += direction * Speed * new Vector2((float)r.NextDouble(), (float)r.NextDouble());
            }
        }

        /// <summary>
        /// Gets random spawn position off camera for enemies
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        private Vector2 GetRandomSpawnPosition(Random r)
        {
            int x = r.Next(2);
            int y = r.Next(2);

            if (x == 0)
            {
                x = -32 - r.Next(300);
            }
            else if(x == 1)
            {
                x = 1280 + r.Next(300);

            }
            if (y == 0)
            {
                y = -32 - r.Next(300);
            }
            else if (y == 1)
            {
                y = 720 + r.Next(300);
            }

            return new Vector2(x, y);
        }
    }
}
