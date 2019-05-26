using Raylib;

namespace ILCrawford.MCGame.MissileCommand.Models
{

    class Explosion : IEntity
    {
        private const int EXPLOSION_INCREASE_TIME = 90;          // In frames
        private const int EXPLOSION_TOTAL_TIME = 210;         // In frames


        public Vector2 position;
        public float radiusMultiplier;
        public int frame;
        public bool active;

        void IEntity.Update()
        {
            if (this.active)
            {
                this.frame++;

                if (this.frame <= EXPLOSION_INCREASE_TIME) this.radiusMultiplier = this.frame / (float)EXPLOSION_INCREASE_TIME;
                else if (this.frame <= EXPLOSION_TOTAL_TIME) this.radiusMultiplier = 1 - (this.frame - (float)EXPLOSION_INCREASE_TIME) / (float)EXPLOSION_TOTAL_TIME;
                else
                {
                    this.frame = 0;
                    this.active = false;
                }
            }
        }
    };
}
