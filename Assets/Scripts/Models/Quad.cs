using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quad
{
    public int vertex0;
    public int vertex1;
    public int vertex2;
    public int vertex3;

    public Quad() { }

    public Quad(int vertex0, int vertex1, int vertex2, int vertex3)
    {
        this.vertex0 = vertex0;
        this.vertex1 = vertex1;
        this.vertex2 = vertex2;
        this.vertex3 = vertex3;
    }

    public Quad(int[] vertices)
    {
        this.vertex0 = vertices[0];
        this.vertex1 = vertices[1];
        this.vertex2 = vertices[2];
        this.vertex3 = vertices[3];
    }

}
