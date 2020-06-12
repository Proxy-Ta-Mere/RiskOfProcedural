using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GridGenerator
{

    Mesh mesh;
    int circleResolution;
    int gridResolution;
    bool mergeTriangles;

    Vector3[] vertices;
    List<Triangle> triangles;
    List<Quad> quads;

    public GridGenerator(Mesh mesh, int circleResolution, int gridResolution, bool mergeTriangles)
    {
        this.mesh = mesh;
        this.circleResolution = circleResolution;
        this.gridResolution = gridResolution;
        this.mergeTriangles = mergeTriangles;

        vertices = new Vector3[circleResolution + 1 + (gridResolution * circleResolution)];

        triangles = Helper.CreateList<Triangle>((gridResolution + 1) * (gridResolution + 1) * circleResolution);
        quads = new List<Quad>();
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

        if (mergeTriangles) MergeTriangles();

        mesh.Clear();
        mesh.vertices = vertices;
        //mesh.subMeshCount = 1;
        //mesh.SetIndices(GridService.ListTriangleToArray(triangles), MeshTopology.Triangles, 0);
        //mesh.SetIndices(GridService.ListQuadToArray(quads), MeshTopology.Quads, 1);

        var lines = GridService.ListTriangleLines(triangles).Concat(GridService.ListQuadLines(quads));
        mesh.SetIndices(lines.ToArray(), MeshTopology.Lines, 0);

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

                    triangles[triangleIndex] = new Triangle(firstVertex, secondVertex, thirdVertex);
                    triangleIndex += 1;
                }

                secondVertex = lastVertex ? nextCircleFirstVertex : vertexOnNextCircleIndex;
                thirdVertex = lastVertex ? vertexOffset : vertexIndex + 1;

                triangles[triangleIndex] = new Triangle(firstVertex, secondVertex, thirdVertex);
                triangleIndex += 1;
            }
            vertexOffset += circleSize;
        }
    }

    private void MergeTriangles()
    {
        List<Triangle> trianglesClone = Helper.Clone<Triangle>(triangles);
        List<bool> trianglesExist = Helper.CreateList<bool>(triangles.Count, true);

        for (var i = 0; i < trianglesClone.Count; i++)
        {
            if (trianglesExist[i])
            {
                Triangle triangle = trianglesClone[i];

                List<Vector2> edges = triangle.GetEdgesTriangle();

                for (var j = 0; j < trianglesClone.Count; j++)
                {
                    if (trianglesExist[i] && trianglesExist[j] && !triangle.Equals(trianglesClone[j]))
                    {
                        Triangle triangleToCompare = trianglesClone[j];
                        foreach (Vector2 edge in edges)
                        {
                            List<Vector2> edgesToCompare = triangleToCompare.GetEdgesTriangle();

                            foreach (Vector2 edgeToCompare in edgesToCompare)
                            {
                                if (GridService.EdgeEquals(edge, edgeToCompare))
                                {
                                    trianglesExist[i] = false;
                                    trianglesExist[j] = false;

                                    List<int> unorderedQuadVerts = GridService.GetQuadVertices(triangle, triangleToCompare);
                                    if(!GridService.IsConvex(unorderedQuadVerts))
                                    {
                                        Quad quad = new Quad(GridService.OrderVertices(unorderedQuadVerts, vertices));
                                        quads.Add(quad);
                                        triangles.Remove(triangle);
                                        triangles.Remove(triangleToCompare);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}