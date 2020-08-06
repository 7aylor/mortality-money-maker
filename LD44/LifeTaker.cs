using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LD44
{
    class LifeTaker
    {
        public Vector2 Position { get; set; }
        public Texture2D Sprite { get; set; }
        public bool IsActive { get; set; }
        public double TimeSinceLastUse { get; set; }

        public LifeTaker(int x, int y, Texture2D sprite)
        {
            Position = new Vector2(x, y);
            Sprite = sprite;
            IsActive = false;
            TimeSinceLastUse = 0;
        }
    }
}
