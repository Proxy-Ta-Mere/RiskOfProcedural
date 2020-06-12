using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public static class GridService
{
    public static bool EdgeEquals(Vector2 edge1, Vector2 edge2)
    {
        if (Vector2.Equals(edge1, edge2)) return true;
        if (edge1[0] == edge2[1] && edge1[1] == edge2[0]) return true;

        return false;
    }

    public static List<int> GetQuadVertices(Triangle triangle, Triangle triangleToCompare)
    {
        List<int> quadVertices = new List<int>();

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (triangle.Vertices()[i] != triangleToCompare.Vertices()[j])
                {
                    quadVertices.Add(triangle.Vertices()[i]);
                    quadVertices.Add(triangleToCompare.Vertices()[j]);
                }
            }
        }

        quadVertices = quadVertices.Distinct().ToList();
        return quadVertices;
    }

    public static int[] OrderVertices(List<int> quadVertices, Vector3[] vertices)
    {
        float centerX = 0.25f * (vertices[quadVertices[0]].x
            + vertices[quadVertices[1]].x
            + vertices[quadVertices[2]].x
            + vertices[quadVertices[3]].x);

        float centerY = 0.25f * (vertices[quadVertices[0]].y
            + vertices[quadVertices[1]].y
            + vertices[quadVertices[2]].y
            + vertices[quadVertices[3]].y);

        float centerZ = 0.25f * (vertices[quadVertices[0]].z
            + vertices[quadVertices[1]].z
            + vertices[quadVertices[2]].z
            + vertices[quadVertices[3]].z);

        Vector3 quadCenter = new Vector3(centerX, centerY, centerZ);
        int[] newQuadVertices = new int[4];

        // Init p0
        newQuadVertices[0] = quadVertices[0];
        Vector3 p0 = vertices[newQuadVertices[0]];
        Vector3 centerToPointZero = (p0 - quadCenter).normalized;

        for (int i = 1; i < 4; i++)
        {
            Vector3 point = vertices[quadVertices[i]];
            Vector3 centerToPoint = (point - quadCenter).normalized;

            if(Vector3.Dot(centerToPointZero, centerToPoint) <= -0.99f) // p2
            {
                newQuadVertices[2] = Array.IndexOf(vertices, point);
            } 
            else
            {
                var res = (point.x - quadCenter.x) * (p0.z - quadCenter.z) - (point.z - quadCenter.z) * (p0.x - quadCenter.x);
                if (res > 0) newQuadVertices[1] = Array.IndexOf(vertices, point);
                else if (res < 0) newQuadVertices[3] = Array.IndexOf(vertices, point);
            }
        }

        return newQuadVertices;
    }

    public static int[] ListTriangleToArray(List<Triangle> triangles)
    {
        List<int> triangleList = new List<int>();

        foreach (Triangle triangle in triangles)
        {
            triangleList.Add(triangle.vertex0);
            triangleList.Add(triangle.vertex1);
            triangleList.Add(triangle.vertex2);
        }

        return triangleList.ToArray();
    }

    public static int[] ListTriangleLines(List<Triangle> triangles)
    {
        List<int> triangleList = new List<int>();

        foreach (Triangle triangle in triangles)
        {
            int[] points = triangle.Vertices();

            triangleList.Add(points[0]);
            triangleList.Add(points[1]);
            triangleList.Add(points[1]);
            triangleList.Add(points[2]);
            triangleList.Add(points[2]);
            triangleList.Add(points[0]);
        }

        return triangleList.ToArray();
    }

    public static int[] ListQuadLines(List<Quad> quads)
    {
        List<int> quadList = new List<int>();

        foreach (Quad quad in quads)
        {
            quadList.Add(quad.vertex0);
            quadList.Add(quad.vertex1);
            quadList.Add(quad.vertex1);
            quadList.Add(quad.vertex2);
            quadList.Add(quad.vertex2);
            quadList.Add(quad.vertex3);
            quadList.Add(quad.vertex3);
            quadList.Add(quad.vertex0);
        }

        return quadList.ToArray();
    }


    public static int[] ListQuadToArray(List<Quad> quads)
    {
        List<int> quadList = new List<int>();

        foreach (Quad quad in quads)
        {
            quadList.Add(quad.vertex0);
            quadList.Add(quad.vertex1);
            quadList.Add(quad.vertex2);
            quadList.Add(quad.vertex3);
        }

        return quadList.ToArray();
    }

    public static bool IsConvex(List<int> vertexIndices, Vector3[] vertices)
    {

        return true;
    }
    public static List<Vector3> GetVerticesCoordinates(List<int> vertexIndices, Vector3[] vertices)
    {

    }
}