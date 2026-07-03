namespace Makarov.Framework.Graphics
{
    public struct Vertex
    {
        public Vertex(Point3D coords, ColorF color, Point3D normal)
        {
            Coords = coords;
            Color = color;
            Normal = normal;
        }

        public Point3D Coords;
        public ColorF Color;
        public Point3D Normal;
    }
}
