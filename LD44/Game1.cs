using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace LD44
{

    enum Direction { Up, Down, Left, Right }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D enemy1Sprite;
        Texture2D groundSprite;
        Texture2D lifeSprite;
        Texture2D lifeTakerSprite;
        Texture2D lifeTakerEatingSprite;
        Texture2D pixel;
        Texture2D slimeBallSprite;
        Texture2D enemy1CorpseSprite;
        Texture2D planet;
        SoundEffect slimeBall;
        SoundEffect enemyDeath;
        SoundEffect life;
        SoundEffect pickup;

        Player player;
        LifeTaker lifeTaker;
        KeyboardState keyboard;
        MouseState mouse;

        List<Projectile> projectiles;
        List<Enemy> enemies;
        List<Vector2> starPositions;
        int round = 0;
        bool roundEnd = true;
        bool title = true;

        Random random;

        const int TILE_SIZE = 64;
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.Window.Title = "Mortality-Money-Maker: Alien Edition";
            player = new Player(new Texture2D(graphics.GraphicsDevice, 16, 16), 300, 300);
            projectiles = new List<Projectile>();
            enemies = new List<Enemy>();
            random = new Random();
            starPositions = new List<Vector2>();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            //textures
            player.Sprite = Content.Load<Texture2D>("Player");
            enemy1Sprite = Content.Load<Texture2D>("Enemy1");
            enemy1CorpseSprite = Content.Load<Texture2D>("Corpse");
            groundSprite = Content.Load<Texture2D>("Ground");
            lifeSprite = Content.Load<Texture2D>("Life");
            lifeTakerSprite = Content.Load<Texture2D>("LifeTaker");
            lifeTakerEatingSprite = Content.Load<Texture2D>("LifeTakerEating");
            slimeBallSprite = Content.Load<Texture2D>("SlimeBall");
            pixel = Content.Load<Texture2D>("Pixel");
            planet = Content.Load<Texture2D>("Planet");

            slimeBall = Content.Load<SoundEffect>("sfx_throw");
            enemyDeath = Content.Load<SoundEffect>("sfx_enemy");
            life = Content.Load<SoundEffect>("sfx_lifetaker");
            pickup = Content.Load<SoundEffect>("sfx_pickup");

            lifeTaker = new LifeTaker(600, 400, lifeTakerSprite);

            font = Content.Load<SpriteFont>("font");

            PopulateStars();

            // TODO: use this.Content to load your game content here
        }

        private void PopulateStars()
        {
            int numStars = random.Next(100, 200);
            for (int i = 0; i < numStars; i++)
            {
                starPositions.Add(new Vector2(random.Next(0, graphics.PreferredBackBufferWidth), random.Next(0, graphics.PreferredBackBufferHeight)));
            }
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState prevState = keyboard;
            keyboard = Keyboard.GetState();

            if (title)
            {
                if(keyboard.IsKeyDown(Keys.Space))
                {
                    title = false;
                }
            }
            else if (player.Lives > 0)
            {
                lifeTaker.TimeSinceLastUse += gameTime.ElapsedGameTime.TotalMilliseconds;
                player.DeathCooldown -= gameTime.ElapsedGameTime.TotalMilliseconds;

                if(lifeTaker.TimeSinceLastUse > 1000)
                {
                    lifeTaker.Sprite = lifeTakerSprite;
                }
            
                NextRound();
                GetKeyboardInput(prevState);
                CheckFireInput();
                MoveProjectiles();
                CheckProjectileCollisions();
                CheckPlayerPickUpLife();
                MoveEnemies();
                CheckPlayerEnemyCollision();
            }
            else
            {
                CheckReplay();
            }

            base.Update(gameTime);
        }

        private void CheckReplay()
        {
            if(keyboard.IsKeyDown(Keys.R))
            {
                ResetGame();
            }
        }

        private void ResetGame()
        {
            player.Lives = 3;
            player.Money = 0;
            round = 0;
        }

        private void CheckPlayerEnemyCollision()
        {
            if(enemies.Count > 0 && player.DeathCooldown <= 0)
            {
                foreach(Enemy e in enemies)
                {
                    float distance = Vector2.Distance(e.Position, player.Position);
                    if(Math.Abs(distance) < TILE_SIZE / 2)
                    {
                        player.Lives--;
                        player.DeathCooldown = 500;
                        enemyDeath.Play();
                    }
                }
                CheckPlayerDeath();
            }
        }

        private void CheckPlayerDeath()
        {
            if(player.Lives <= 0)
            {
                enemies.Clear();
                projectiles.Clear();
            }
        }

        private void NextRound()
        {
            if(roundEnd || round == 0)
            {
                if(keyboard.IsKeyDown(Keys.R) || round == 0)
                {
                    roundEnd = false;
                    round++;
                    SpawnEnemies();
                }
            }
        }

        private void SpawnEnemies()
        {
            int numEnemies = 3 + (round * 2);
            for(int i = 0; i < numEnemies; i++)
            {
                enemies.Add(new Enemy(enemy1Sprite, random));
            }
        }

        private void CheckPlayerPickUpLife()
        {
            for(int i = 0; i < enemies.Count; i++)
            {
                Enemy e = enemies[i];
                if (Vector2.Distance(new Vector2(e.Position.X + TILE_SIZE / 2, e.Position.Y + TILE_SIZE / 2), new Vector2(player.Position.X + TILE_SIZE / 2, player.Position.Y + TILE_SIZE / 2)) < TILE_SIZE / 2 && e.IsDead)
                {
                    enemies.Remove(e);
                    i--;
                    player.Lives++;
                    pickup.Play();
                }
            }
        }

        private void CheckProjectileCollisions()
        {
            for(int i = 0; i < projectiles.Count; i++)
            {
                Projectile p = projectiles[i];
                Vector2 pPos = new Vector2(p.Position.X, p.Position.Y);
                for (int j = 0; j < enemies.Count; j++)
                {
                    Enemy e = enemies[j];

                    int combinedWidths = TILE_SIZE / 2 + p.Sprite.Width;

                    if(Vector2.Distance(new Vector2(e.Position.X + TILE_SIZE / 2, e.Position.Y + TILE_SIZE / 2), pPos) < combinedWidths && !e.IsDead && e.Position.X > 30 && e.Position.X < 1250 && e.Position.Y > 30 && e.Position.Y < 690)
                    {
                        e.IsDead = true;
                        e.Sprite = enemy1CorpseSprite;
                        projectiles.Remove(p);
                        i--;
                        break;
                    }
                }
            }
        }

        private void MoveEnemies()
        {
            if(enemies.Count == 0)
            {
                roundEnd = true;
            }
            foreach(Enemy e in enemies)
            {
                e.MoveToward(player);
            }
        }

        private void MoveProjectiles()
        {
            //move projectiles, remove if off edge
            for (int i = 0; i < projectiles.Count; i++)
            {
                Projectile p = projectiles[i];
                if (p.Position.X > graphics.PreferredBackBufferWidth || p.Position.X < 0 ||
                   p.Position.Y > graphics.PreferredBackBufferHeight || p.Position.Y < 0)
                {
                    projectiles.Remove(p);
                    i--;
                }

                p.Move();
            }
        }

        private void CheckFireInput()
        {
            MouseState prevMouse = mouse;
            mouse = Mouse.GetState();

            if(mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
            {
                projectiles.Add(new Projectile((int)player.Position.X + TILE_SIZE / 2, (int)player.Position.Y + TILE_SIZE / 2, Vector2.Normalize(new Vector2(mouse.Position.X, mouse.Position.Y) - player.Position), slimeBallSprite));
                slimeBall.Play();
            }
        }

        private void GetKeyboardInput(KeyboardState prevState)
        {
            lifeTaker.IsActive = false;

            //adjust speed for diagonals
            if(keyboard.GetPressedKeys().Length > 1)
            {
                player.Speed = 4;
            }
            else
            {
                player.Speed = 5;
            }

            if ((keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up)) && (player.Position.Y + 5 > 0))
            {
                player.Position -= new Vector2(0, player.Speed);
                player.direction = Direction.Up;
            }
            if ((keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down)) && (player.Position.Y + TILE_SIZE < graphics.PreferredBackBufferHeight))
            {
                player.Position += new Vector2(0, player.Speed);
                player.direction = Direction.Down;
            }
            if ((keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left)) && (player.Position - new Vector2(player.Speed, 0)).X > 0)
            {
                player.Position -= new Vector2(player.Speed, 0);
                player.direction = Direction.Left;
            }
            if ((keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right)) && (player.Position - new Vector2(player.Speed, 0)).X + TILE_SIZE < graphics.PreferredBackBufferWidth)
            {
                player.Position += new Vector2(player.Speed, 0);
                player.direction = Direction.Right;
            }
            
            if((Vector2.Distance(player.Position, lifeTaker.Position) < TILE_SIZE))
            {
                lifeTaker.IsActive = true;
            }

            if(keyboard.IsKeyDown(Keys.E) && player.Lives > 1 && !prevState.IsKeyDown(Keys.E) && lifeTaker.IsActive)
            {
                player.Lives--;
                player.Money += 25;
                lifeTaker.Sprite = lifeTakerEatingSprite;
                lifeTaker.TimeSinceLastUse = 0d;
                life.Play();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            if(!title)
            {
                //ground
                for(int x = 0; x < graphics.PreferredBackBufferWidth; x += TILE_SIZE)
                {
                    for (int y = 0; y < graphics.PreferredBackBufferWidth; y += TILE_SIZE)
                    {
                        spriteBatch.Draw(groundSprite, new Rectangle(x, y, TILE_SIZE, TILE_SIZE), new Rectangle(0, 0, 16, 16), Color.White);
                    }
                }
                if(player.Lives > 0)
                {

                    //projectiles
                    foreach (Projectile p in projectiles)
                    {
                        spriteBatch.Draw(p.Sprite, new Rectangle((int)p.Position.X, (int)p.Position.Y, p.Sprite.Width * 2, p.Sprite.Height * 2), new Rectangle(0, 0, 8, 8), Color.White, p.Rotation, new Vector2(p.Sprite.Width / 2, p.Sprite.Height / 2), SpriteEffects.None, 1);
                    }

                    if (roundEnd)
                    {
                        //lifeTaker
                        spriteBatch.Draw(lifeTaker.Sprite, new Rectangle((int)lifeTaker.Position.X, (int)lifeTaker.Position.Y, TILE_SIZE, TILE_SIZE), new Rectangle(0, 0, 16, 16), Color.White);

                        spriteBatch.DrawString(font, "End of Round, R to start next round", new Vector2(350, 250), Color.White);

                        if (lifeTaker.IsActive)
                        {
                            spriteBatch.DrawString(font, "E to Sell Lives", new Vector2(500, 300), Color.Yellow);
                        }
                    }

                    //player
                    spriteBatch.Draw(player.Sprite, new Rectangle((int)player.Position.X, (int)player.Position.Y, TILE_SIZE, TILE_SIZE), new Rectangle(0, 0, 16, 16), Color.White);

                    //enemies
                    foreach (Enemy e in enemies)
                    {
                        spriteBatch.Draw(e.Sprite, new Rectangle((int)e.Position.X, (int)e.Position.Y, TILE_SIZE, TILE_SIZE), new Rectangle(0, 0, 16, 16), Color.White);

                    }

                    if (roundEnd)
                    {
                        spriteBatch.DrawString(font, "End of Round, R to start next round", new Vector2(350, 250), Color.White);
                    }
                }
                else
                {
                    spriteBatch.DrawString(font, "Your life earned $" + player.Money +"!", new Vector2(500, 300), Color.White);
                    spriteBatch.DrawString(font, "Press R to replay, Esc to exit", new Vector2(450, 400), Color.Yellow);
                }

                //UI
                for (int x = 0; x < graphics.PreferredBackBufferWidth; x += 32)
                {
                    spriteBatch.Draw(pixel, new Rectangle(x, 0, 32, 32), new Color(0, 0, 0, 0.5f));
                }

                //lives
                for(int x = 0; x < player.Lives; x++)
                {
                    spriteBatch.Draw(lifeSprite, new Rectangle((x * 32) + (2 * x), 2, 28, 28), Color.White);
                }

                //money
                spriteBatch.DrawString(font, "$" + player.Money, new Vector2(1160, -2), Color.GreenYellow);

                //Enemy Count
                spriteBatch.Draw(enemy1Sprite, new Rectangle(1080, 2, 28, 28), new Rectangle(0, 0, 16, 16), Color.White);
                spriteBatch.DrawString(font, enemies.Count.ToString(), new Vector2(1120, -2), Color.Red);
            }
            else //title screen
            {
                //draw stars
                foreach(Vector2 starPos in starPositions)
                {
                    spriteBatch.Draw(pixel, starPos, Color.White);
                }

                spriteBatch.Draw(planet, new Rectangle(75, 75, 256, 256), new Rectangle(0, 0, 64, 64), Color.White);
                spriteBatch.Draw(enemy1Sprite, new Rectangle(380, 485, 64, 64), new Rectangle(0, 0, 16, 16), Color.White);
                spriteBatch.Draw(enemy1Sprite, new Rectangle(770, 485, 64, 64), new Rectangle(0, 0, 16, 16), Color.White);
                spriteBatch.Draw(player.Sprite, new Rectangle(575, 350, 64, 64), new Rectangle(0, 0, 16, 16), Color.White);
                spriteBatch.Draw(lifeTakerEatingSprite, new Rectangle(575, 100, 64, 64), new Rectangle(0, 0, 16, 16), Color.White);

                spriteBatch.DrawString(font, "Mortality-Money-Maker: Alien Edition", new Vector2(350, 200), Color.GreenYellow);
                spriteBatch.DrawString(font, "Press Space to start" , new Vector2(465, 500), Color.Red);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
