using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using runner;
using runner.Platform;

namespace runner
{
    /// <summary>
    /// Class representing a main character
    /// </summary>
    class Player
    {
        public Texture2D texture;
        public Rectangle boundingBox;
        public bool isRunning;
        Animation animation;

        public int Y
        {
            get { return (int)boundingBox.Y; }
            set { boundingBox.Y = value; }
        }

        public void Initialize(Texture2D texture, int x, int y)
        {
            isRunning = true;
            canJumpAgain = true;
            this.texture = texture;
            boundingBox = new Rectangle(x, y, 30, 50);
            this.animation = new Animation(Textures.runner, new Vector2(0, 0), 50, 50, 8, 85, Color.White, 1.0f, true);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(texture, boundingBox, Color.White);
            animation.Draw(spriteBatch);
        }

        //int startY;
        bool jumping;
        public bool standing;
        PlatformTemplate currentPlatform;

        /// <summary>
        /// Checks if player is colliding with a platform
        /// </summary>
        /// <param name="platforms">list of platforms</param>
        void checkForCollisions(List<PlatformTemplate> platforms)
        {
            if (!isRunning)
                return;
            standing = false;
            foreach (PlatformTemplate p in platforms)
            {
                if (p is TunnelPlatform)
                {
                    if (boundingBox.Right > ((TunnelPlatform)p).topBoundingBox.Left && 
                        boundingBox.Right < ((TunnelPlatform)p).topBoundingBox.Left + GameState.scrollingSpeed &&
                        boundingBox.Top < ((TunnelPlatform)p).topBoundingBox.Bottom - flySpeed &&
                        boundingBox.Right < ((TunnelPlatform)p).topBoundingBox.Right)
                    {
                        flySpeed = 0;
                        standing = false;
                        isRunning = false;
                        break;
                    }
                    else if (jumping && boundingBox.Intersects(((TunnelPlatform)p).topBoundingBox))
                    {
                        flySpeed = 0;
                        jumping = false;
                    }
                }

                if (boundingBox.Right >= p.boundingBox.Left && 
                    boundingBox.Right <= p.boundingBox.Left + GameState.scrollingSpeed && 
                    boundingBox.Bottom > p.boundingBox.Top + flySpeed && 
                    boundingBox.Right < p.boundingBox.Right)
                {
                    standing = false;
                    isRunning = false;
                    break;
                }
                else if (boundingBox.Bottom >= p.boundingBox.Top && 
                    boundingBox.Right >= p.boundingBox.Left && 
                    boundingBox.Left <= p.boundingBox.Right)
                {
                    currentPlatform = p;
                    if (!standing)
                        currentPlatform.HandleCollision();
                    boundingBox.Y -= boundingBox.Bottom - currentPlatform.boundingBox.Top - 2;
                    standing = true;
                    break;
                    //return true;
                }
            }
            //return false;
        }

        //int maxHeight;
        int flySpeed;
        bool canJumpAgain;
        int maxHeight;

        public void Update(GameTime gameTime, List<PlatformTemplate> platforms)
        {
            animation.animating = standing;
            animation.frameTime = (int)(75 - GameState.scrollingSpeed);
            animation.Position.X = boundingBox.X-20;
            animation.Position.Y = boundingBox.Y;
            animation.Update(gameTime);
            checkForCollisions(platforms);
            if (!standing)
            {
                animation.rowI = 1;
                if (!jumping)
                {
                    //descending
                    if (flySpeed < maxHeight / 4)
                        animation.currentFrame = 2;
                    else if (flySpeed < maxHeight / 2)
                        animation.currentFrame = 3;
                    else
                        animation.currentFrame = 4;
                    flySpeed++;
                }
                else
                {
                    //ascending
                    if (flySpeed > 3*maxHeight/4)
                        animation.currentFrame = 0;
                    else if (flySpeed > maxHeight / 2)
                        animation.currentFrame = 1;
                    else
                        animation.currentFrame = 2;
                    flySpeed--;
                }
            }
            else
            {
                animation.rowI = 0;
                flySpeed = 0;
            }

            TouchCollection touchCollection = TouchPanel.GetState();
            foreach (TouchLocation tl in touchCollection)
            {
                if (tl.State == TouchLocationState.Released)
                {
                    canJumpAgain = true;
                    jumping = false;
                }
                if (tl.State == TouchLocationState.Pressed || tl.State == TouchLocationState.Moved)
                {
                    if (standing && canJumpAgain)
                    {
                        //started jumping
                        //maxHeight = (int)GameState.scrollingSpeed * 8 + 30;
                        if (GameState.scrollingSpeed < 12)
                            flySpeed = 12;
                        else if (GameState.scrollingSpeed < 15)
                            flySpeed = (int)GameState.scrollingSpeed;//12;
                        else
                            flySpeed = 15;
                        maxHeight = flySpeed;
                        jumping = true;
                        //startY = boundingBox.Y;
                        canJumpAgain = false;
                        currentPlatform.HandleRelease();
                    }
                    if (jumping && flySpeed>0)//Math.Abs(boundingBox.Y - startY) <= maxHeight)
                    {
                        //is ascending
                        boundingBox.Y -= flySpeed;
                    }
                    if (flySpeed<=0)//Math.Abs(boundingBox.Y - startY) >= maxHeight)
                    {
                        //stopped ascending
                        jumping = false;
                    }
                    break;
                }
            }

            if (!jumping && !standing)
            {
                //fall
                boundingBox.Y += flySpeed;
            }
        }

    }
}
