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

    private int circleCount;

    public GridGenerator(Mesh mesh, int circleResolution, int gridResolution)
    {
        this.mesh = mesh;
        this.circleResolution = circleResolution;
        this.gridResolution = gridResolution;
        circleCount = gridResolution + 1;

        vertices = new Vector3[circleResolution + 1 + (gridResolution * circleResolution)];
        //triangles = new int[circleResolution * stepCircleIndex * 3];
    }

    public void ConstructMesh()
    {
        Vector3[] temp_vertices = new Vector3[0];
        for (int i = 0; i <= gridResolution; i++)
        {
            float radius = 1f / (i + 1f);
            Debug.Log(radius);
            Vector3[] current_vertices = GeneratePointsOnPolygon(radius, gridResolution - i);
            temp_vertices = temp_vertices.Concat(current_vertices).ToArray();
        }

        Vector3[] centerGrid = new Vector3[1] { Vector3.zero };
        vertices = temp_vertices.Concat(centerGrid).ToArray();

        //GenerateTriangles();
        //mesh.Clear();
        //mesh.vertices = vertices;
        //mesh.triangles = triangles;
        //mesh.RecalculateNormals();
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
        int triIndex = 0;
        int inbetweenCount = gridResolution;
        int vertexOffset = 0;
        for (int i = 0; i < circleCount; i++)
        {
            inbetweenCount -= i;
            int vertexCount = circleResolution + circleResolution * inbetweenCount;

            for (int j = 0; j < vertexCount; j++)
            {
                int vertexIndex = j + vertexOffset;

                // create every triangle that have 2
                int vertex1 = vertexIndex;
                int vertex2 = vertexIndex + vertexCount;
                int vertex3 = vertexIndex + 1;
                if (j == vertexCount - 1)
                    vertex3 = vertexOffset;

                triangles[triIndex] = vertex1;
                triangles[triIndex + 1] = vertex2;
                triangles[triIndex + 2] = vertex3;
                triIndex += 3;

                
            }
            vertexOffset += vertexCount;
            
        }
    }
}