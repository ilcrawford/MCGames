using Raylib;
using System;

namespace ILCrawford.MCGame.MissileCommand.Models
{
    class Interceptor
    {
        public Vector2 origin;
        public Vector2 position;
        public Vector2 objective;
        public Vector2 speed;

        public bool active;
        public bool arrived ;
        public const int INTERCEPTOR_SPEED = 10;

        public void Update()
        {
            if (this.active)
            {
                // Update position
                this.position.x += this.speed.x;
                this.position.y += this.speed.y;

                // Distance to objective
                var distance = Math.Sqrt(Math.Pow(this.position.x - this.objective.x, 2) +
                                 Math.Pow(this.position.y - this.objective.y, 2));
                arrived = distance < INTERCEPTOR_SPEED;

                if (arrived)
                {
                    // Interceptor dissapears
                    this.active = false;
                }
            }
        }

        
    };
}