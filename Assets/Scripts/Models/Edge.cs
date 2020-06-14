using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    public List<Vertex> vertices;

    public Edge(List<Vertex> vertices)
    {
        this.vertices = vertices;
    }

    public List<int> GetIndices()
    {
        return new List<int>() { vertices[0].id, vertices[1].id };
    }

    // override object.Equals
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Edge otherEdge = (Edge)obj;

        if (vertices[0] == otherEdge.vertices[1] && vertices[1] == otherEdge.vertices[0]) return true;
        if (vertices[0] == otherEdge.vertices[0] && vertices[1] == otherEdge.vertices[1]) return true;

        return false;
    }
}
