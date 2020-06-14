using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Polygon 
{
    public List<Vertex> vertices;

    public List<Edge> edges;

    public Polygon(List<Vertex> vertices)
    {
        this.vertices = vertices;

        GenerateEdges();
    }

    public Polygon(Vertex[] vertices)
    {
        this.vertices = vertices.ToList();
        
        GenerateEdges();
    }

    public override bool Equals(object obj)
    {
        return obj is Polygon quad &&
               vertices == quad.vertices;
    }

    private void GenerateEdges()
    {
        this.edges = new List<Edge>();
        for (int i = 0; i < vertices.Count; i++)
        {
            if (i != vertices.Count - 1)
                this.edges.Add(new Edge(new List<Vertex>() { vertices[i], vertices[i + 1] }));
            else
                this.edges.Add(new Edge(new List<Vertex>() { vertices[i], vertices[0] }));
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

    public object Clone()
    {
        Polygon newPolygon = new Polygon(this.vertices);

        return (object)newPolygon;
    }
}
