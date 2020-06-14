using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex
{
    public int id { get; set; }
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
    public List<Polygon> polygons { get; set; }

    public Vertex(int id, float x, float y, float z)
    {
        this.id = id;
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vertex(int id, Vector3 vertex)
    {
        this.id = id;
        this.x = vertex.x;
        this.y = vertex.y;
        this.z = vertex.z;
    }

    public Vector3 AsVector()
    {
        return new Vector3(x, y, z);
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Vertex v = (Vertex)obj;

        return v.id == this.id;
    }

    public static Vertex DefaultVertex()
    {
        return new Vertex(-1, Vector3.zero);
    }
}
