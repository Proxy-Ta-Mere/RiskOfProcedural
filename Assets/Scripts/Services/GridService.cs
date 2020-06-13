using System;
using System.Collections.Generic;
using System.Linq;
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

    public static Vector3 GetQuadCenter(List<int> unorderedQuadVerts, Vector2 commonEdge, List<Vector3> vertices)
    {
        Vector3 quadCenter = new Vector3(1000, 1000, 1000);

        // Diag 1 is common edge of triangles
        List<int> diag1 = new List<int> { Convert.ToInt32(commonEdge.x), Convert.ToInt32(commonEdge.y) };

        // Diag 2 is last points of our quad vertices (minus diag 1)
        List<int> diag2 = unorderedQuadVerts.Except(diag1).ToList();

        // Init 4 points
        Vector3 p0 = vertices[diag1[0]];
        Vector3 p1 = vertices[diag1[1]];
        Vector3 p2 = vertices[diag2[0]];
        Vector3 p3 = vertices[diag2[1]];

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

    public static int[] OrderVertices(List<int> quadVertices, Vector3 quadCenter, List<Vector3> vertices)
    {
        int[] newQuadVertices = new int[4];

        // Init p0
        newQuadVertices[0] = quadVertices[0];
        Vector3 p0 = vertices[newQuadVertices[0]];
        Vector3 centerToPointZero = (p0 - quadCenter).normalized;

        for (int i = 1; i < 4; i++)
        {
            Vector3 point = vertices[quadVertices[i]];
            Vector3 centerToPoint = (point - quadCenter).normalized;

            int vertexIndex = vertices.IndexOf(point);

            if (Vector3.Dot(centerToPointZero, centerToPoint) <= -0.99f) // p2
            {
                newQuadVertices[2] = vertexIndex;
            }
            else
            {
                var res = (point.x - quadCenter.x) * (p0.z - quadCenter.z) - (point.z - quadCenter.z) * (p0.x - quadCenter.x);
                if (res > 0) newQuadVertices[1] = vertexIndex; // p3
                else if (res < 0) newQuadVertices[3] = vertexIndex; // p1
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

    public static bool IsConvex(List<Vector3> quadCoordinates)
    {
        foreach (Vector3 point in quadCoordinates)
        {
            List<Vector3> otherPoints = quadCoordinates.Where(quadCoord => quadCoord != point).ToList();

            if (Helper.PointInTriangle(point, otherPoints)) return false;
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

    public static bool IsTriangle(List<Vector3> quadCoordinates)
    {
        foreach (Vector3 point in quadCoordinates)
        {
            List<Vector3> otherPoints = quadCoordinates.Where(quadCoord => quadCoord != point).ToList();
            if (AreAligned(otherPoints[0], otherPoints[1], otherPoints[2])) return true;
        }

        return false;
    }

    public static bool AreAligned(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        Vector3 a = (p1 - p0).normalized;
        Vector3 b = (p2 - p0).normalized;

        return Vector3.Dot(a, b) >= 0.99f || Vector3.Dot(a, b) <= -0.99f;
    }
}