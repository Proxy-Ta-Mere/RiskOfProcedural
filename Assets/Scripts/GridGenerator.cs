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
    bool subdivideGrid;

    Vector3 impossiblePoint;

    Vector3[] vertices;
    int[][] verticesTriangles;
    int[] triangles;
    int[] quads;

    public GridGenerator(Mesh mesh, int circleResolution, int gridResolution, int mergeTriangles, bool subdivideGrid)
    {
        this.mesh = mesh;
        this.circleResolution = circleResolution;
        this.gridResolution = gridResolution;
        this.mergeTriangles = mergeTriangles;
        this.subdivideGrid = subdivideGrid;

        vertices = new Vector3[circleResolution + 1 + (gridResolution * circleResolution)];
        triangles = new int[(gridResolution + 1) * (gridResolution + 1) * circleResolution * 3];
        quads = new int[0];

        impossiblePoint = GeneratePointOnCircle(1, 2 * Mathf.PI / (circleResolution + 1));
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

        SubdivideGrid();

        //Debug.Log("Vertices : " + vertices.Length + "; Triangles : " + triangles.Length / 3 + "; Quads : " + quads.Length / 4);

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

        float angle = 2 * Mathf.PI / circleResolution;

        GeneratePointsOnCircle(radius, ref vertices, stepCircleIndex, angle);
        GenerateInterpolatedPoints(nbInterpolatedPoints, ref vertices, stepCircleIndex);

        return vertices;
    }

    /// <summary>
    /// Init "main" points of circle
    /// </summary>
    private void GeneratePointsOnCircle(float radius, ref Vector3[] vertices, int stepCircleIndex, float angle)
    {
        for (int i = 0; i < circleResolution; i++)
        {
            vertices[i * stepCircleIndex] = GeneratePointOnCircle(radius, angle * i);
        }
    }

    public Vector3 GeneratePointOnCircle(float radius, float angle)
    {
        float x = radius * Mathf.Cos(angle );
        float z = radius * Mathf.Sin(angle );

        return new Vector3(x, 0, z);
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

    private void SubdivideGrid()
    {
        Debug.Log("1) Vertices : " + vertices.Length + "; Triangles : " + triangles.Length / 3 + "; Quads : " + quads.Length / 4);


        if (!subdivideGrid) return;

        int[] newQuads = new int[triangles.Length * 4 + quads.Length * 5];
        Vector3[] newVertices = new Vector3[2 * vertices.Length + 2 * (triangles.Length / 3 + quads.Length / 4) - 1];
        //Vector3[] newVertices = new Vector3[1000];

        for (int i = 0; i < newVertices.Length; i++)
        {
            newVertices[i] = impossiblePoint;
        }

        int newQuadIndex = 0;
        int newVertexIndex = 0;

        // For each triangle, find triangle center, lines centers and generate 3 quads from this triangle
        for (int triangleIndex = 0; triangleIndex < triangles.Length; triangleIndex+=3)
        {
            Vector3 p0 = vertices[triangles[triangleIndex]];
            Vector3 p1 = vertices[triangles[triangleIndex + 1]];
            Vector3 p2 = vertices[triangles[triangleIndex + 2]];
            Vector3 triangleCenter = GetTriangleCenter(p0, p1, p2);

            Vector3 c0 = GetLineCenter(p0, p1);
            Vector3 c1 = GetLineCenter(p1, p2);
            Vector3 c2 = GetLineCenter(p2, p0);

            // First quad
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, triangleCenter);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, c2);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, p0);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, c0);

            // Second quad
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, triangleCenter);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, c0);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, p1);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, c1);

            // Third quad
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, triangleCenter);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, c1);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, p2);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, c2);
        }

        // For each quad, find quad center, lines centers and generate 4 quads from this quad
        for (int quadIndex = 0; quadIndex < quads.Length; quadIndex += 4)
        {
            Vector3 p0 = vertices[quads[quadIndex]];
            Vector3 p1 = vertices[quads[quadIndex + 1]];
            Vector3 p2 = vertices[quads[quadIndex + 2]];
            Vector3 p3 = vertices[quads[quadIndex + 3]];
            Vector3 quadCenter = GetQuadCenter(p0, p1, p2, p3);
            Vector3 c0 = GetLineCenter(p0, p1);
            Vector3 c1 = GetLineCenter(p1, p2);
            Vector3 c2 = GetLineCenter(p2, p3);
            Vector3 c3 = GetLineCenter(p3, p0);

            // First quad
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, quadCenter);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, c3);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, p0);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, c0);

            // Second quad
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, quadCenter);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, c0);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, p1);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, c1);

            // Third quad
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, quadCenter);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, c1);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, p2);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, c2);

            // Fourth quad
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, quadCenter);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, c2);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, p3);
            Subdivide_AddVertex(ref newVertices, ref newQuads, ref newVertexIndex, ref newQuadIndex, c3);
        }

        vertices = newVertices.Where(point => point != impossiblePoint).ToArray();
        //vertices = newVertices;
        triangles = new int[0];
        quads = newQuads;

        Debug.Log("2) Vertices : " + vertices.Length + "; Triangles : " + triangles.Length / 3 + "; Quads : " + quads.Length / 4);
    }

    private void Subdivide_AddVertex(ref Vector3[] newVertices, ref int[] newQuads, ref int newVerticesIndex, ref int newQuadsIndex, Vector3 newVertex)
    {
        var index = Array.IndexOf(newVertices, newVertex);
        if (index == -1)
        {
            index = newVerticesIndex;
            newVerticesIndex++;
        }

        newVertices[index] = newVertex;
        newQuads[newQuadsIndex] = index;

        newQuadsIndex++;
    }

    private Vector3 GetLineCenter(Vector3 p0, Vector3 p1)
    {
        return Vector3.Lerp(p0, p1, 0.5f);
    }

    private Vector3 GetTriangleCenter(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return new Vector3((p0.x + p1.x + p2.x) / 3f, 0, (p0.z + p1.z + p2.z) / 3f);
    }

    private Vector3 GetQuadCenter(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return new Vector3((p0.x + p1.x + p2.x + p3.x) / 4f, 0, (p0.z + p1.z + p2.z + p3.z) / 4f);
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
            int vertexIndex = triangles[triangle2 + i];
            if (!quadVertices.Contains(vertexIndex))
            {
                quadVertices[3] = vertexIndex;
            }
        }

        return quadVertices;
    }

    private int[][] GetEdges(int triangleIndex)
    {
        int v0 = triangles[triangleIndex];
        int v1 = triangles[triangleIndex + 1];
        int v2 = triangles[triangleIndex + 2];

        int[] edge0 = new int[2] { v0, v1 };
        int[] edge1 = new int[2] { v1, v2 };
        int[] edge2 = new int[2] { v2, v0 };

        var res = new int[3][] { edge0, edge1, edge2 };

        System.Random rnd = new System.Random();
        return res.OrderBy(x => rnd.Next()).ToArray();
    }

    private int GetTriangleFromEdge(int[] edge, int triangleIndex)
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
