using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshService
{
    // Position of each vertex
    public List<Vector3> verticesPositions;
    // Indices of the faces each vertex belongs to.
    public List<List<int>> verticesFaces;
    // indices of each vertices belonging to each face
    public List<List<int>> facesVertices;
    // indices of each vertices belonging to each face
    public List<List<int>> edgesVertices;

}
