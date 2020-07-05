using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class GridGenerator
{

    Mesh mesh;
    int circleResolution;
    int gridResolution;
    int mergeTriangles;

    Vector3[] vertices;
    int[][] verticesTriangles;
    int[] triangles;
    int[] quads;

    public GridGenerator(Mesh mesh, int circleResolution, int gridResolution, int mergeTriangles)
    {
        this.mesh = mesh;
        this.circleResolution = circleResolution;
        this.gridResolution = gridResolution;
        this.mergeTriangles = mergeTriangles;

        vertices = new Vector3[circleResolution + 1 + (gridResolution * circleResolution)];
        triangles = new int[(gridResolution + 1) * (gridResolution + 1) * circleResolution * 3];
        quads = new int[0];
    }

    public void ConstructMesh()
    {
        Vector3[] tempVertices = new Vector3[0];
        Vector3[] concatVertices = new Vector3[0];
        float radiusStep = 1f / (gridResolution + 1);
        for (int i = 0; i <= gridResolution; i++)
        {
            float radius = 1 - radiusStep * (i);
            Vector3[] currentVertices = GeneratePointsOnPolygon(radius, gridResolution - i);
            concatVertices = new Vector3[tempVertices.Length + currentVertices.Length];
            tempVertices.CopyTo(concatVertices, 0);
            currentVertices.CopyTo(concatVertices, tempVertices.Length);

            tempVertices = concatVertices;
        }

        Vector3[] centerGrid = new Vector3[1] { Vector3.zero };

        vertices = tempVertices.Concat(centerGrid).ToArray();
        verticesTriangles = new int[vertices.Length][];

        GenerateTriangles();

        MergeTriangles();

        mesh.Clear();
        mesh.vertices = vertices;

        var triangleLines = GridService.ListTriangleLines(triangles);
        var quadLines = GridService.ListQuadLines(quads);
        var lines = triangleLines.Concat(quadLines);
        mesh.SetIndices(lines.ToArray(), MeshTopology.Lines, 0);
    }

    private Vector3[] GeneratePointsOnPolygon(float radius, int nbInterpolatedPoints)
    {
        Vector3[] vertices = new Vector3[circleResolution + (nbInterpolatedPoints * circleResolution)];
        int stepCircleIndex = nbInterpolatedPoints + 1;

        GeneratePointsOnCircle(radius, ref vertices, stepCircleIndex);
        GenerateInterpolatedPoints(nbInterpolatedPoints, ref vertices, stepCircleIndex);

        return vertices;
    }

    /// <summary>
    /// Init "main" points of circle
    /// </summary>
    private void GeneratePointsOnCircle(float radius, ref Vector3[] vertices, int stepCircleIndex)
    {
        float angle = 2 * Mathf.PI / circleResolution;

        int indexOffset = this.vertices.Length;

        for (int i = 0; i < circleResolution; i++)
        {
            float x = radius * Mathf.Cos(angle * i);
            float z = radius * Mathf.Sin(angle * i);

            vertices[i * stepCircleIndex] = new Vector3(x, 0, z);
        }
    }

    /// <summary>
    /// Interpolate points between main points of circle
    /// </summary>
    private void GenerateInterpolatedPoints(int nbInterlopatedPoints, ref Vector3[] vertices, int stepCircleIndex)
    {
        int indexOffset = this.vertices.Length;
        for (int i = 0; i < circleResolution; i++)
        {
            Vector3 a = vertices[i * stepCircleIndex];
            Vector3 b = Vector3.zero;

            if (i == circleResolution - 1)
            {
                b = vertices[0];
            }
            else
            {
                b = vertices[(i + 1) * stepCircleIndex];
            }

            float step = 1f / stepCircleIndex;

            for (int j = 0; j < nbInterlopatedPoints; j++)
            {
                float t = step * (j + 1);
                vertices[i * stepCircleIndex + (j + 1)] = Vector3.Lerp(a, b, t);
            }
        }
    }

    private void GenerateTriangles()
    {
        int triangleIndex = 0;
        int vertexOffset = 0;
        for (int circleIndex = 0; circleIndex <= gridResolution; circleIndex++)
        {
            int circleEdgeSubdivisions = gridResolution - circleIndex;
            int circleSize = circleResolution + circleResolution * circleEdgeSubdivisions;
            int vertexOnNextCircleIndex = circleSize + vertexOffset;

            int nextCircleFirstVertex = vertexOffset + circleSize;

            for (int vertexCircleIndex = 0; vertexCircleIndex < circleSize; vertexCircleIndex++)
            {
                int vertexIndex = vertexCircleIndex + vertexOffset;
                bool lastVertex = vertexCircleIndex == circleSize - 1;

                int firstVertex = vertexIndex;
                int secondVertex;
                int thirdVertex;

                if (vertexCircleIndex % (circleEdgeSubdivisions + 1) != 0)
                {
                    vertexOnNextCircleIndex++;
                    secondVertex = vertexOnNextCircleIndex - 1;
                    thirdVertex = lastVertex ? nextCircleFirstVertex : vertexOnNextCircleIndex;
                    CreateTriangle(ref triangleIndex, firstVertex, secondVertex, thirdVertex);
                }

                secondVertex = lastVertex ? nextCircleFirstVertex : vertexOnNextCircleIndex;
                thirdVertex = lastVertex ? vertexOffset : vertexIndex + 1;
                CreateTriangle(ref triangleIndex, firstVertex, secondVertex, thirdVertex);
            }
            vertexOffset += circleSize;
        }
    }

    private void MergeTriangles()
    {
        int remainingTriangles = triangles.Length / 3;

        bool[] trianglesRemoved = new bool[triangles.Length];

        //int targetTriangleCount = (int)(triangles.Length / 3f - (mergeTriangles / 100f * triangles.Length / 3f));
        int targetTriangleCount = (int)((1 - (mergeTriangles / 100f)) * triangles.Length / 3f);

        while (remainingTriangles > targetTriangleCount)
        {
            var triangleIndex = GetRandomTriangleIndex();

            if (trianglesRemoved[triangleIndex])
                continue;

            foreach (int[] edge in GetEdges(triangleIndex)) 
            {
                int otherTriangle = GetTriangleFromEdge(edge, triangleIndex);
                
                if (otherTriangle == -1 || trianglesRemoved[otherTriangle])
                    continue;

                int[] unorderedQuadVerts = GetQuadVertices(triangleIndex, otherTriangle);

                Vector3[] points = GetQuadPoints(unorderedQuadVerts);

                Vector3 quadCenter = GridService.GetQuadCenter(unorderedQuadVerts, edge, vertices);

                if (GridService.IsConvex(points) && !GridService.IsTriangle(points))
                {
                    int[] orderedVertices = GridService.OrderVertices(unorderedQuadVerts, quadCenter, vertices);

                    quads = quads.Concat(orderedVertices).ToArray();

                    trianglesRemoved[triangleIndex] = true;
                    trianglesRemoved[triangleIndex + 1] = true;
                    trianglesRemoved[triangleIndex + 2] = true;

                    trianglesRemoved[otherTriangle] = true;
                    trianglesRemoved[otherTriangle + 1] = true;
                    trianglesRemoved[otherTriangle + 2] = true;

                    remainingTriangles--;
                    break;
                }
            }

            remainingTriangles--;
        }

        int nbTrianglesLeft = trianglesRemoved.Count(el => el == false);

        int[] newTriangles = new int[nbTrianglesLeft];

        int index = 0;
        for (int i = 0; i < triangles.Length; i++)
        {
            if (!trianglesRemoved[i])
            {
                newTriangles[index] = triangles[i];
                index++;
            }
        }

        triangles = newTriangles;
    }

    private int GetRandomTriangleIndex()
    {
        int triangleIndex = UnityEngine.Random.Range(0, triangles.Length - 1);
        int numSteps = (int)Mathf.Floor(triangleIndex / 3);
        int adjustedtriangleIndex = numSteps * 3;

        return adjustedtriangleIndex;
    }

    private Vector3[] GetQuadPoints(int[] quadVertices)
    {
        Vector3[] quadPoints = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            quadPoints[i] = vertices[quadVertices[i]];
        }

        return quadPoints;
    }

    private int[] GetQuadVertices(int triangle1, int triangle2)
    {
        int[] quadVertices = new int[4] { -1, -1, -1, -1 };

        for (int i = 0; i < 3; i++)
        {
            quadVertices[i] = triangles[triangle1 + i];
        }
        for (int i = 0; i < 3; i++)
        {
            int vertexIndex = triangles[triangle2 + i]; // TODO Ca pete ici
            if (!quadVertices.Contains(vertexIndex))
            {
                quadVertices[3] = vertexIndex;
            }
        }

        return quadVertices;
    }

    public int[][] GetEdges(int triangleIndex)
    {
        int v0 = triangles[triangleIndex];
        int v1 = triangles[triangleIndex + 1];
        int v2 = triangles[triangleIndex + 2];

        int[] edge0 = new int[2] { v0, v1 };
        int[] edge1 = new int[2] { v1, v2 };
        int[] edge2 = new int[2] { v2, v0 };

        return new int[3][] { edge0, edge1, edge2 };
    }

    public int GetTriangleFromEdge(int[] edge, int triangleIndex)
    {
        int v0 = edge[0];
        int v1 = edge[1];

        int[] v0Triangles = verticesTriangles[v0];
        int[] v1Triangles = verticesTriangles[v1];

        for (int i = 0; i < v0Triangles.Length; i++)
        {
            int tri = v0Triangles[i];
            if (tri == triangleIndex)
            {
                continue;
            }

            for (int j = 0; j < v1Triangles.Length; j++)
            {
                int tri2 = v1Triangles[j];
                if (tri == tri2)
                {
                    return tri;
                }
            }
        }

        return -1;
    }

    private void CreateTriangle(ref int triangleIndex, int firstVertex, int secondVertex, int thirdVertex)
    {
        triangles[triangleIndex] = firstVertex;
        triangles[triangleIndex + 1] = secondVertex;
        triangles[triangleIndex + 2] = thirdVertex;

        AddTriangleToVertex(firstVertex, triangleIndex);
        AddTriangleToVertex(secondVertex, triangleIndex);
        AddTriangleToVertex(thirdVertex, triangleIndex);

        triangleIndex += 3;
    }

    private void AddTriangleToVertex(int vertexIndex, int triangleIndex)
    {
        if (verticesTriangles[vertexIndex] == null)
        {
            //int length = vertexIndex == vertices.Length ? circleResolution : 6;
            int length = 100; // TODO Pourquoi ?
            verticesTriangles[vertexIndex] = new int[length];
            for (int i = 0; i < length; i++)
            {
                verticesTriangles[vertexIndex][i] = -1;
            }
            verticesTriangles[vertexIndex][0] = triangleIndex;
        }
        else
        {
            // Get first null element and add our triangle index
            int index = Array.FindIndex(verticesTriangles[vertexIndex], i => i == -1);
            verticesTriangles[vertexIndex][index] = triangleIndex;
        }
    }
}
