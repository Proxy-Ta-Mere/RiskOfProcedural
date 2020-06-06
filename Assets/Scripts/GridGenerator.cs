using System.Collections;
using System.Collections.Generic;
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
        vertices[vertices.Length - 1] = Vector3.zero;

        GeneratePointsOnPolygon();
        GenerateTriangles();

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private void GeneratePointsOnPolygon()
    {
        GeneratePointsOnCircle();
        GenerateInterpolatedPoints();
    }

    /// <summary>
    /// Init "main" points of circle
    /// </summary>
    private void GeneratePointsOnCircle()
    {
        float angle = 2 * Mathf.PI / circleResolution;

        for (int i = 0; i < circleResolution; i++)
        {
            float x = Mathf.Cos(angle * i);
            float z = Mathf.Sin(angle * i);

            vertices[i * stepCircleIndex] = new Vector3(x, 0, z);
        }
    }

    /// <summary>
    /// Interpolate points between main points of circle
    /// </summary>
    private void GenerateInterpolatedPoints()
    {
        for (int i = 0; i < circleResolution; i++)
        {
            Vector3 a = vertices[i * stepCircleIndex];
            Vector3 b = vertices[(i + 1) * stepCircleIndex];

            if (i == circleResolution - 1)
            {
                b = vertices[0];
            }

            float step = 1f / stepCircleIndex;

            for (int j = 0; j < gridResolution; j++)
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
            if (i != vertices.Length - 1)
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

        