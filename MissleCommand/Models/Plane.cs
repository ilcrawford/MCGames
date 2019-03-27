using Raylib;

namespace ILCrawford.MCGame.MissileCommand.Models
{
    class Plane
    {
        public Vector2 origin;
        public Vector2 position;
        public Vector2 objective;
        public Vector2 speed;

        public bool active;

        public Raylib.Texture2D texture2D;
    }
}
