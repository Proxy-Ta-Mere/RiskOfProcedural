using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using UnityEngine;

public class GridGenerator
{

    Mesh mesh;
    int circleResolution;
    int gridResolution;
    int stepCircleIndex;

    Vector3[] vertices;
    int[] triangles;

    public GridGenerator(Mesh mesh, int circleResolution, int gridResolution)
    {
        this.mesh = mesh;
        this.circleResolution = circleResolution;
        this.gridResolution = gridResolution;
        this.stepCircleIndex = gridResolution + 1;

        vertices = new Vector3[circleResolution + 1 + (gridResolution * circleResolution)];
        triangles = new int[circleResolution * stepCircleIndex * 3];
    }

    public void ConstructMesh()
    {
        Vector3[] centerGrid = new Vector3[1] { Vector3.zero };
        Vector3[] points = GeneratePointsOnPolygon(1f, gridResolution);
        GenerateTriangles();

        mesh.Clear();
        mesh.vertices = points.Concat(centerGrid).ToArray();
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private Vector3[] GeneratePointsOnPolygon(float radius, int nbInterpolatedPoints)
    {
        Vector3[] vertices = new Vector3[circleResolution + (nbInterpolatedPoints * circleResolution)];

        GeneratePointsOnCircle(radius, ref vertices);
        GenerateInterpolatedPoints(nbInterpolatedPoints, ref vertices);

        return vertices;
    }

    /// <summary>
    /// Init "main" points of circle
    /// </summary>
    private void GeneratePointsOnCircle(float radius, ref Vector3[] vertices)
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
    private void GenerateInterpolatedPoints(int nbInterlopatedPoints, ref Vector3[] vertices)
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

        for (int i = 0; i < vertices.Length - 1; i++)
        {
            // Every triangle except last
            if (i != vertices.Length - 2)
            {
                triangles[triIndex] = i;
                triangles[triIndex + 1] = vertices.Length - 1;
                triangles[triIndex + 2] = i + 1;
            }
            else // Last triangle
            {
                triangles[triIndex] = i;
                triangles[triIndex + 1] = vertices.Length - 1;
                triangles[triIndex + 2] = 0;
            }

            triIndex += 3;
        }
    }
}

