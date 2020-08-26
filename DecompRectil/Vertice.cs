
namespace DecompRectil
{
    class Point2D
    {
        public float x { get; set; }
        public float y { get; set; }

        public Point2D(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    class Vertice
    {
        public Point2D point;
        public int index;
        public bool concave;
        public Vertice next;
        public Vertice previous;
        public bool visited;

        public Vertice(Point2D point, int index, bool concave)
        {
            this.point = point;
            this.index = index;
            this.concave = concave;
            visited = false;
        }
    }
}
