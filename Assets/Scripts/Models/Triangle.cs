using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Triangle : Polygon, ICloneable
{
    public Triangle(Vertex vertex0, Vertex vertex1, Vertex vertex2) : base(new List<Vertex>() { vertex0, vertex1, vertex2 })
    {
    }

    public Triangle(List<Vertex> vertices) : base(vertices) { }

    public List<Triangle> GetAdjacentTriangles()
    {
        List<Triangle> adjacentTriangles = new List<Triangle>();

        foreach (Vertex vertex in vertices)
        {
            foreach (Polygon polygon in vertex.polygons)
            {
                if (polygon is Triangle)
                {
                    Triangle triangle2 = (Triangle)polygon;
                    if (triangle2 is Triangle && !triangle2.Equals(this))
                    {
                        int commonVertex = 0;
                        foreach (Vertex vertexTriangle2 in triangle2.vertices)
                        {
                            foreach (Vertex vertexAgain in this.vertices)
                            {
                                if (vertexAgain.id == vertexTriangle2.id) commonVertex++;
                            }
                        }

                        if (commonVertex == 2) adjacentTriangles.Add(triangle2);
                    }
                }
            }
        }

        return adjacentTriangles;
    }

    public Edge GetCommonEdgeWith(Triangle triangleToCompare)
    {
        foreach (Edge edge in this.edges)
        {
            foreach (Edge edgeToCompare in triangleToCompare.edges)
            {
                if (edge.Equals(edgeToCompare))
                    return edge;
            }
        }

        throw new Exception("Triangles don't have common edge");
    }

    public new object Clone()
    {
        Triangle newTriangle = new Triangle(this.vertices);

        return (object)newTriangle;
    }

    public override bool Equals(object obj)
    {
        return obj is Triangle triangle &&
               vertices == triangle.vertices;
    }
}
