using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security;
using UnityEngine;
using UnityEngine.UIElements;

public class GridGenerator
{

    Mesh mesh;
    int circleResolution;
    int gridResolution;

    Vector3[] vertices;
    int[] triangles;


    public GridGenerator(Mesh mesh, int circleResolution, int gridResolution)
    {
        this.mesh = mesh;
        this.circleResolution = circleResolution;
        this.gridResolution = gridResolution;

        vertices = new Vector3[circleResolution + 1 + (gridResolution * circleResolution)];

        triangles = new int[(gridResolution + 1) * (gridResolution + 1) * circleResolution * 3];
    }

    public void ConstructMesh()
    {
        Vector3[] tempVertices = new Vector3[0];
        float radiusStep = 1f / (gridResolution + 1);
        for (int i = 0; i <= gridResolution; i++)
        {
            float radius = 1 - radiusStep * (i);
            Vector3[] currentVertices = GeneratePointsOnPolygon(radius, gridResolution - i);
            tempVertices = tempVertices.Concat(currentVertices).ToArray();
        }

        // Array of one point containing the center vertex
        Vector3[] centerGrid = new Vector3[1] { Vector3.zero };

        vertices = tempVertices.Concat(centerGrid).ToArray();

        GenerateTriangles();
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
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

                secondVertex = lastVertex ? nextCircleFirstVertex: vertexOnNextCircleIndex;
                thirdVertex = lastVertex ? vertexOffset : vertexIndex + 1;
                CreateTriangle(ref triangleIndex, firstVertex, secondVertex, thirdVertex);
            }
            vertexOffset += circleSize;
        }
    }
    private void CreateTriangle(ref int triangleIndex, int firstVertex, int secondVertex, int thirdVertex)
    {
        triangles[triangleIndex] = firstVertex;
        triangles[triangleIndex + 1] = secondVertex;
        triangles[triangleIndex + 2] = thirdVertex;
        triangleIndex += 3;

    }
}