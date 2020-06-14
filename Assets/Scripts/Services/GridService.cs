using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GridService
{
    public static List<Vertex> GetQuadVertices(Triangle triangle, Triangle triangleToCompare)
    {
        List<Vertex> quadVertices = new List<Vertex>();

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (triangle.vertices[i] != triangleToCompare.vertices[j])
                {
                    quadVertices.Add(triangle.vertices[i]);
                    quadVertices.Add(triangleToCompare.vertices[j]);
                }
            }
        }

        quadVertices = quadVertices.Distinct().ToList();
        return quadVertices;
    }

    public static Vector3 GetQuadCenter(List<Vertex> unorderedQuadVerts, Edge diag1)
    {
        Vector3 quadCenter = new Vector3(1000, 1000, 1000);

        // Diag 2 is last points of our quad vertices (minus diag 1)
        Edge diag2 = new Edge(unorderedQuadVerts.Except(diag1.vertices).ToList());

        // Init 4 points
        Vertex p0 = diag1.vertices[0];
        Vertex p1 = diag1.vertices[1];
        Vertex p2 = diag2.vertices[0];
        Vertex p3 = diag2.vertices[1];

        float A1 = (p1.z - p0.z);
        float B1 = (p0.x - p1.x);
        float A2 = (p3.z - p2.z);
        float B2 = (p2.x - p3.x);
        float delta = A1 * B2 - A2 * B1;

        if (delta == 0)
            throw new ArgumentException("Lines are parallel");

        float C1 = A1 * p0.x + B1 * p0.z;
        float C2 = A2 * p2.x + B2 * p2.z;

        float x = (B2 * C1 - B1 * C2) / delta;
        float y = (A1 * C2 - A2 * C1) / delta;

        quadCenter = new Vector3(x, 0, y);

        return quadCenter;
    }

    public static List<Vertex> OrderVertices(List<Vertex> quadVertices, Vector3 quadCenter)
    {
        List<Vertex> newQuadVertices = Helper.CreateList<Vertex>(4, Vertex.DefaultVertex());

        // Init p0
        newQuadVertices[0] = quadVertices[0];
        Vector3 p0 = newQuadVertices[0].AsVector();
        Vector3 centerToPointZero = (p0 - quadCenter).normalized;

        for (int i = 1; i < 4; i++)
        {
            Vertex vertex = quadVertices[i];
            Vector3 point = vertex.AsVector();
            Vector3 centerToPoint = (point - quadCenter).normalized;

            if (Vector3.Dot(centerToPointZero, centerToPoint) <= -0.99f) // p2
            {
                newQuadVertices[2] = vertex;
            }
            else
            {
                var res = (point.x - quadCenter.x) * (p0.z - quadCenter.z) - (point.z - quadCenter.z) * (p0.x - quadCenter.x);
                if (res > 0) newQuadVertices[1] = vertex; // p3
                else if (res < 0) newQuadVertices[3] = vertex; // p1
            }
        }

        return newQuadVertices;
    }

    public static int[] ListTriangleToArray(List<Triangle> triangles)
    {
        List<int> triangleList = new List<int>();

        foreach (Triangle triangle in triangles)
        {
            triangleList.Add(triangle.vertices[0].id);
            triangleList.Add(triangle.vertices[1].id);
            triangleList.Add(triangle.vertices[2].id);
        }

        return triangleList.ToArray();
    }

    public static int[] ListTriangleLines(List<Triangle> triangles)
    {
        List<int> triangleList = new List<int>();

        foreach (Triangle triangle in triangles)
        {
            triangleList = triangleList.Concat(triangle.GetLinesIndices()).ToList();
        }

        return triangleList.ToArray();
    }

    public static int[] ListQuadLines(List<Quad> quads)
    {
        List<int> quadList = new List<int>();

        foreach (Quad quad in quads)
        {
            quadList.Add(quad.vertices[0].id);
            quadList.Add(quad.vertices[1].id);
            quadList.Add(quad.vertices[1].id);
            quadList.Add(quad.vertices[2].id);
            quadList.Add(quad.vertices[2].id);
            quadList.Add(quad.vertices[3].id);
            quadList.Add(quad.vertices[3].id);
            quadList.Add(quad.vertices[0].id);
        }

        return quadList.ToArray();
    }


    public static int[] ListQuadToArray(List<Quad> quads)
    {
        List<int> quadList = new List<int>();

        foreach (Quad quad in quads)
        {
            quadList.Add(quad.vertices[0].id);
            quadList.Add(quad.vertices[1].id);
            quadList.Add(quad.vertices[2].id);
            quadList.Add(quad.vertices[3].id);
        }

        return quadList.ToArray();
    }

    public static bool IsConvex(List<Vertex> quadVertices)
    {
        foreach (Vertex vertex in quadVertices)
        {
            List<Vertex> otherVertices = quadVertices.Where(quadVertex => quadVertex != vertex).ToList();

            if (Helper.PointInTriangle(vertex, otherVertices)) return false;
        }

        return true;
    }
    public static List<Vector3> GetVerticesCoordinates(List<int> vertexIndices, List<Vector3> vertices)
    {
        List<Vector3> VerticesCoordinates = Helper.CreateList<Vector3>(vertexIndices.Count);

        for (int i = 0; i < vertexIndices.Count; i++)
        {
            VerticesCoordinates[i] = vertices[vertexIndices[i]];
        }

        return VerticesCoordinates;
    }

    public static bool IsTriangle(List<Vertex> quadCoordinates)
    {
        foreach (Vertex vertex in quadCoordinates)
        {
            List<Vertex> otherVertices = quadCoordinates.Where(quadCoord => quadCoord != vertex).ToList();
            if (AreAligned(otherVertices[0], otherVertices[1], otherVertices[2])) return true;
        }

        return false;
    }

    public static bool AreAligned(Vertex p0, Vertex p1, Vertex p2)
    {
        Vector3 a = (p1.AsVector() - p0.AsVector()).normalized;
        Vector3 b = (p2.AsVector() - p0.AsVector()).normalized;

        return Vector3.Dot(a, b) >= 0.99f || Vector3.Dot(a, b) <= -0.99f;
    }
}