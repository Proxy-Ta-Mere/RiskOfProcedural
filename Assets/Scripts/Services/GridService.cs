using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GridService
{

    public static Vector3 GetQuadCenter(int[] unorderedQuadVerts, int[] diag1, Vector3[] vertices)
    {
        // Diag 2 is last points of our quad vertices (minus diag 1)
        int[] diag2 = unorderedQuadVerts.Except(diag1).ToArray();

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

        Vector3 quadCenter = new Vector3(x, 0, y);

        return quadCenter;
    }

    public static int[] OrderVertices(int[] quadVertices, Vector3 quadCenter, Vector3[] vertices)
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

            if (Vector3.Dot(centerToPointZero, centerToPoint) <= -0.99f) // p2
            {
                newQuadVertices[2] = quadVertices[i];
            }
            else
            {
                var res = (point.x - quadCenter.x) * (p0.z - quadCenter.z) - (point.z - quadCenter.z) * (p0.x - quadCenter.x);
                if (res > 0) newQuadVertices[1] = quadVertices[i]; // p3
                else if (res < 0) newQuadVertices[3] = quadVertices[i]; // p1
            }
        }

        return newQuadVertices;
    }

    public static int[] ListTriangleLines(int[] triangles)
    {
        int[] res = new int[triangles.Length * 2];

        for (int i = 0; i < triangles.Length; i += 3)
        {
            res[i * 2] = triangles[i];
            res[i * 2 + 1] = triangles[i + 1];

            res[i * 2 + 2] = triangles[i + 1];
            res[i * 2 + 3] = triangles[i + 2];

            res[i * 2 + 4] = triangles[i + 2];
            res[i * 2 + 5] = triangles[i];
        }

        return res;
    }

    public static int[] ListQuadLines(int[] quads)
    {
        int[] res = new int[quads.Length * 2];

        for (int i = 0; i < quads.Length; i += 4)
        {
            res[i * 2] = quads[i];
            res[i * 2 + 1] = quads[i + 1];

            res[i * 2 + 2] = quads[i + 1];
            res[i * 2 + 3] = quads[i + 2];

            res[i * 2 + 4] = quads[i + 2];
            res[i * 2 + 5] = quads[i + 3];

            res[i * 2 + 6] = quads[i + 3];
            res[i * 2 + 7] = quads[i];
        }

        return res;
    }

    public static bool IsConvex(Vector3[] quadPoints)
    {
        foreach (Vector3 point in quadPoints)
        {
            Vector3[] otherPoints = quadPoints.Where(quadPoint => quadPoint != point).ToArray();

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

    public static bool IsTriangle(Vector3[] quadCoordinates)
    {
        foreach (Vector3 quadCoord in quadCoordinates)
        {
            Vector3[] otherQuadCoords = quadCoordinates.Where(el => el != quadCoord).ToArray();
            if (AreAligned(otherQuadCoords[0], otherQuadCoords[1], otherQuadCoords[2])) return true;
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