using Microsoft.Xna.Framework;


namespace GameStateManager
{
    public struct Circle
    {
        public Vector2 Center { get; set; }
        public float Radius { get; set; }
        public float RadiusSquared { get; private set; }

        public Circle(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
            RadiusSquared = Radius * Radius;
        }


        public bool Contains(Vector2 point)
        {
            return Vector2.DistanceSquared(point, Center) < RadiusSquared;
        }


        public bool Intersects(Circle other)
        {
            //return (other.Center - Center).Length() < (other.Radius - Radius);
            return Vector2.DistanceSquared(other.Center, Center) < (other.RadiusSquared - RadiusSquared);
        }
    }
}