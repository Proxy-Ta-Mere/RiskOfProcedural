using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridGenerator
{

    Mesh mesh;
    int circleResolution;
    int gridResolution;
    bool mergeTriangles;

    List<Vertex> vertices;
    List<Triangle> triangles;
    List<Quad> quads;

    public GridGenerator(Mesh mesh, int circleResolution, int gridResolution, bool mergeTriangles)
    {
        this.mesh = mesh;
        this.circleResolution = circleResolution;
        this.gridResolution = gridResolution;
        this.mergeTriangles = mergeTriangles;

        //vertices = Helper.CreateList<Vertex>(circleResolution + 1 + (gridResolution * circleResolution), Vertex.DefaultVertex());
        vertices = new List<Vertex>();
        triangles = Helper.CreateList<Triangle>((gridResolution + 1) * (gridResolution + 1) * circleResolution);
        quads = new List<Quad>();
    }

    public void ConstructMesh()
    {
        List<Vertex> tempVertices = new List<Vertex>();
        float radiusStep = 1f / (gridResolution + 1);
        for (int i = 0; i <= gridResolution; i++)
        {
            float radius = 1 - radiusStep * (i);
            List<Vertex> currentVertices = GeneratePointsOnPolygon(radius, gridResolution - i);
            tempVertices = tempVertices.Concat(currentVertices).ToList();
            vertices = tempVertices;
        }

        //vertices = tempVertices.ToList();

        // center vertex
        //vertices[vertices.Count -1] = new Vertex(vertices.Count - 1, Vector3.zero);
        vertices.Add(new Vertex(vertices.Count, Vector3.zero));

        GenerateTriangles();

        if (mergeTriangles) MergeTriangles();

        List<Vector3> verticesAsVector3 = new List<Vector3>();
        foreach (Vertex vertex in vertices)
        {
            verticesAsVector3.Add(vertex.AsVector());
        }

        mesh.Clear();
        mesh.vertices = verticesAsVector3.ToArray();
        //mesh.subMeshCount = 1;
        //mesh.SetIndices(GridService.ListTriangleToArray(triangles), MeshTopology.Triangles, 0);
        //mesh.SetIndices(GridService.ListQuadToArray(quads), MeshTopology.Quads, 1);

        var triangleLines = GridService.ListTriangleLines(triangles);
        var quadLines = GridService.ListQuadLines(quads);
        var lines = triangleLines.Concat(quadLines);
        mesh.SetIndices(lines.ToArray(), MeshTopology.Lines, 0);

        //mesh.RecalculateNormals();
    }

    private List<Vertex> GeneratePointsOnPolygon(float radius, int nbInterpolatedPoints)
    {
        List<Vertex> vertices = Helper.CreateList<Vertex>(circleResolution + (nbInterpolatedPoints * circleResolution), Vertex.DefaultVertex());
        int stepCircleIndex = nbInterpolatedPoints + 1;

        GeneratePointsOnCircle(radius, ref vertices, stepCircleIndex);
        GenerateInterpolatedPoints(nbInterpolatedPoints, ref vertices, stepCircleIndex);

        return vertices;
    }

    /// <summary>
    /// Init "main" points of circle
    /// </summary>
    private void GeneratePointsOnCircle(float radius, ref List<Vertex> vertices, int stepCircleIndex)
    {
        float angle = 2 * Mathf.PI / circleResolution;

        int indexOffset = this.vertices.Count ;

        for (int i = 0; i < circleResolution; i++)
        {
            float x = radius * Mathf.Cos(angle * i);
            float z = radius * Mathf.Sin(angle * i);

            vertices[i * stepCircleIndex] = new Vertex(indexOffset + i * stepCircleIndex, new Vector3(x, 0, z));
        }
    }

    /// <summary>
    /// Interpolate points between main points of circle
    /// </summary>
    private void GenerateInterpolatedPoints(int nbInterlopatedPoints, ref List<Vertex> vertices, int stepCircleIndex)
    {
        int indexOffset = this.vertices.Count ;
        for (int i = 0; i < circleResolution; i++)
        {
            Vector3 a = vertices[i * stepCircleIndex].AsVector();
            Vector3 b = Vector3.zero;

            if (i == circleResolution - 1)
            {
                b = vertices[0].AsVector();
            }
            else
            {
                b = vertices[(i + 1) * stepCircleIndex].AsVector();
            }

            float step = 1f / stepCircleIndex;

            for (int j = 0; j < nbInterlopatedPoints; j++)
            {
                float t = step * (j + 1);
                vertices[i * stepCircleIndex + (j + 1)] = new Vertex(indexOffset + i * stepCircleIndex + (j + 1), Vector3.Lerp(a, b, t));
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

                Vertex firstVertex = vertices[vertexIndex];
                Vertex secondVertex;
                Vertex thirdVertex;

                if (vertexCircleIndex % (circleEdgeSubdivisions + 1) != 0)
                {
                    vertexOnNextCircleIndex++;
                    secondVertex = vertices[vertexOnNextCircleIndex - 1];
                    thirdVertex = lastVertex ? vertices[nextCircleFirstVertex] : vertices[vertexOnNextCircleIndex];

                    triangles[triangleIndex] = new Triangle(firstVertex, secondVertex, thirdVertex);
                    triangleIndex += 1;
                }

                secondVertex = lastVertex ? vertices[nextCircleFirstVertex] : vertices[vertexOnNextCircleIndex];
                thirdVertex = lastVertex ? vertices[vertexOffset] : vertices[vertexIndex + 1];

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
        bool remainingTriangles = true;

        while (remainingTriangles)
        {
            var triangleIndex = Random.Range(0, trianglesClone.Count);

            remainingTriangles = false;

            if (trianglesExist[triangleIndex])
            {
                Triangle triangle = trianglesClone[triangleIndex];

                for (var j = 0; j < trianglesClone.Count; j++)
                {
                    if (trianglesExist[triangleIndex] && trianglesExist[j] && !triangle.Equals(trianglesClone[j]))
                    {
                        Triangle triangleToCompare = trianglesClone[j];
                        foreach (Edge edge in triangle.edges)
                        {
                            List<Edge> edgesToCompare = triangleToCompare.edges;

                            foreach (Edge edgeToCompare in edgesToCompare)
                            {
                                if (edge.Equals(edgeToCompare))
                                {
                                    List<Vertex> unorderedQuadVerts = GridService.GetQuadVertices(triangle, triangleToCompare);
                                    Vector3 quadCenter = GridService.GetQuadCenter(unorderedQuadVerts, edge);
                                    if (quadCenter != new Vector3(1000, 1000, 1000))
                                    {
                                        if (GridService.IsConvex(unorderedQuadVerts) && !GridService.IsTriangle(unorderedQuadVerts))
                                        {
                                            List<Vertex> orderedVertices = GridService.OrderVertices(unorderedQuadVerts, quadCenter);

                                            Quad quad = new Quad(orderedVertices);
                                            quads.Add(quad);
                                            triangles.Remove(triangle);
                                            triangles.Remove(triangleToCompare);

                                            trianglesExist[triangleIndex] = false;
                                            trianglesExist[j] = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                trianglesExist[triangleIndex] = false;
            }

            for (int i = 0; i < trianglesExist.Count; i++)
            {
                if (trianglesExist[i] == true) remainingTriangles = true;
            }
        }
    }
}