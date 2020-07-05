using UnityEngine;

public class Grid : MonoBehaviour
{
    [Range(3, 100)]
    public int cicleResolution = 6;

    [Range(0, 100)]
    public int gridResolution = 0;

    [Range(0, 100)]
    public int mergeTriangles = 0;

    [SerializeField, HideInInspector]
    MeshFilter meshFilter;
    GridGenerator generator;

    private void Update()
    {
        GenerateGrid();
    }

    void Initialize()
    {
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = new Mesh();
        }

        generator = new GridGenerator(meshFilter.sharedMesh, cicleResolution, gridResolution, mergeTriangles);
    }

    public void GenerateGrid()
    {
        Initialize();
        GenerateMesh();
    }

    void GenerateMesh()
    {
        if (meshFilter.gameObject.activeSelf)
        {
            generator.ConstructMesh();
        }
    }
}
