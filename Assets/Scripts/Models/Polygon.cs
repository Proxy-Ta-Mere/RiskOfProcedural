using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Polygon 
{
    public List<Vertex> vertices;
    public List<Edge> edges;
    public string name = "";

    public Polygon(List<Vertex> vertices)
    {
        this.vertices = vertices;

        foreach (Vertex vertex in vertices)
        {
            vertex.polygons.Add(this);
            this.name += vertex.id + ", ";
        }

        GenerateEdges();
    }

    private void GenerateEdges()
    {
        this.edges = new List<Edge>();
        for (int i = 0; i < vertices.Count; i++)
        {
            Edge edge;
            if (i != vertices.Count - 1)
                edge = new Edge(new List<Vertex>() { vertices[i], vertices[i + 1] });
            else
                edge = new Edge(new List<Vertex>() { vertices[i], vertices[0] });

            edges.Add(edge);
        }
    }

    public List<int> GetLinesIndices()
    {
        List<int> indices = new List<int>();
        foreach (Edge edge in this.edges)
        {
            indices = indices.Concat(edge.GetIndices()).ToList();
        }

        return indices;
    }

    public override bool Equals(object obj)
    {
        return obj is Polygon quad &&
               vertices == quad.vertices;
    }

    public object Clone()
    {
        Polygon newPolygon = new Polygon(this.vertices);

        return (object)newPolygon;
    }
}
