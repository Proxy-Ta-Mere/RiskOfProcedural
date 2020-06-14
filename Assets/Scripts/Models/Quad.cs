using System.Collections.Generic;
using System.Linq;

public class Quad : Polygon
{

    public Quad(Vertex vertex0, Vertex vertex1, Vertex vertex2, Vertex vertex3) : base(new List<Vertex>() { vertex0, vertex1, vertex2, vertex3 })
    {
    }

    public Quad(List<Vertex> vertices) : base(vertices) { }
}
