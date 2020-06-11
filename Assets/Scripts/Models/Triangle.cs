using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle : ICloneable
{
    public int vertex0;
    public int vertex1;
    public int vertex2;

    public Triangle() { }

    public Triangle(int vertex0, int vertex1, int vertex2)
    {
        this.vertex0 = vertex0;
        this.vertex1 = vertex1;
        this.vertex2 = vertex2;
    }

    public List<Vector2> GetEdgesTriangle()
    {
        return new List<Vector2>() {
            new Vector2(vertex0, vertex1),
            new Vector2(vertex1, vertex2),
            new Vector2(vertex2, vertex0)
        };
    }

    public int[] Vertices()
    {
        return new[] {vertex0, vertex1, vertex2};
    }

    public override bool Equals(object obj)
    {
        return obj is Triangle triangle &&
               vertex0 == triangle.vertex0 &&
               vertex1 == triangle.vertex1 &&
               vertex2 == triangle.vertex2;
    }

    public object Clone()
    {
        Triangle newTriangle = new Triangle
        {
            vertex0 = this.vertex0,
            vertex1 = this.vertex1,
            vertex2 = this.vertex2
        };

        return (object) newTriangle;
    }

    public override int GetHashCode()
    {
        int hashCode = -23138170;
        hashCode = hashCode * -1521134295 + vertex0.GetHashCode();
        hashCode = hashCode * -1521134295 + vertex1.GetHashCode();
        hashCode = hashCode * -1521134295 + vertex2.GetHashCode();
        return hashCode;
    }
}
