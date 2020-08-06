using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace LD44
{
    class Projectile
    {
        Vector2 Direction { get; set; }
        public Vector2 Position { get; set; }
        public Texture2D Sprite { get; set; }
        int speed = 10;
        public float Rotation { get; set; }

        public Projectile(int x, int y, Vector2 moveDir, Texture2D sprite)
        {
            Position = new Vector2(x, y);
            Direction = moveDir;
            Sprite = sprite;
            Rotation = 0;
        }

        public void Move()
        {
            Rotation += 0.25f;

            //if(Direction == Direction.Up)
            //{
            //    Position -= new Vector2(0, speed);
            //}
            //else if(Direction == Direction.Down)
            //{
            //    Position += new Vector2(0, speed);

            //}
            //else if(Direction == Direction.Left)
            //{
            //    Position -= new Vector2(speed, 0);

            //}
            //else if(Direction == Direction.Right)
            //{
            //    Position += new Vector2(speed, 0);
            //}

            Position += Direction * speed;
        }

    }
}
