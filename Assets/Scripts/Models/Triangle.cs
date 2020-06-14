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

    //public override bool Equals(object obj)
    //{
    //    return obj is Triangle triangle &&
    //           vertices == triangle.vertices;
    //}

    public new object Clone()
    {
        Triangle newTriangle = new Triangle(this.vertices);

        return (object)newTriangle;
    }
}
