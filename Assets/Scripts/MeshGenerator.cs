using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    public int bandwidth = 5;
    public float step = 0.2f;
    public bool dancing = false;
    public MetaBallField Field = new MetaBallField();
    
    private MeshFilter _filter;
    private Mesh _mesh;
    
    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();
    private List<int> indices = new List<int>();

    private Vector3 dx = new Vector3(0.001f, 0, 0);
    private Vector3 dy = new Vector3(0, 0.001f, 0);
    private Vector3 dz = new Vector3(0, 0, 0.001f);

    /// <summary>
    /// Executed by Unity upon object initialization. <see cref="https://docs.unity3d.com/Manual/ExecutionOrder.html"/>
    /// </summary>
    private void Awake()
    {
        // Getting a component, responsible for storing the mesh
        _filter = GetComponent<MeshFilter>();
        
        // instantiating the mesh
        _mesh = _filter.mesh = new Mesh();
        
        // Just a little optimization, telling unity that the mesh is going to be updated frequently
        _mesh.MarkDynamic();
    }

    private void ClearMeshData() {
        vertices.Clear();
        indices.Clear();
        normals.Clear();
        _mesh.Clear();
    }

    private void SetMesh() {
        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(indices, 0);
        _mesh.SetNormals(normals);

        // Upload mesh data to the GPU
        _mesh.UploadMeshData(false);
    }

    private Vector3 CalcNormal(Vector3 vertPosition) {
        Vector3 normal = new Vector3(
            Field.F(vertPosition + dx) - Field.F(vertPosition - dx),
            Field.F(vertPosition + dy) - Field.F(vertPosition - dy),
            Field.F(vertPosition + dz) - Field.F(vertPosition - dz)
        );
        normal.Normalize();

        return -normal;
    }

    private int GetCubeConfiguration(float[] cube) {
        int caseIndex = 0;
        for (int i = 0; i < 8; i++) {
            if (cube[i] >= 0)
                caseIndex |= 1 << i;
        }

        return caseIndex;
    }

    private void CreateMeshData() {
        for (float x = -bandwidth; x <= bandwidth; x+=step) {
            for (float y = -bandwidth; y <= bandwidth; y+=step) {
                for (float z = -bandwidth; z <= bandwidth; z+=step) {
                    
                    Vector3 position = new Vector3(x, y, z);
                    float[] cube = new float[8];
                    for (int i = 0; i < 8; i++) {
                        Vector3 vert = position + MarchingCubes.Tables._cubeVertices[i] * step;
                        cube[i] = Field.F(vert);
                    }

                    MarchCube(position, cube);
                }
            }
        }
    }

    private void MarchCube (Vector3 position, float[] cube) {

        int caseIndex = GetCubeConfiguration(cube);
        if (caseIndex == 0 || caseIndex == 255)
            return;

        int3[] edges = MarchingCubes.Tables.CaseToVertices[caseIndex];

        for(int i = 0; i < 5; i++) {
            for(int j = 0; j < 3; j++) {

                int indx = edges[i][j];
                if (indx == -1)
                    return;
                
                int[] vert_ids = MarchingCubes.Tables._cubeEdges[indx];
                Vector3 vert1 = position + MarchingCubes.Tables._cubeVertices[vert_ids[0]] * step;
                Vector3 vert2 = position + MarchingCubes.Tables._cubeVertices[vert_ids[1]] * step;

                float value1 = Field.F(vert1);
                float value2 = Field.F(vert2);
                float lerpParam = -value2 / (value1 - value2);
                Vector3 vertPosition = lerpParam * vert1 + (1f - lerpParam) * vert2;

                vertices.Add(vertPosition);
                indices.Add(vertices.Count - 1);
                normals.Add(CalcNormal(vertPosition));
            }
        }
    }

    
    /// <summary>
    /// Executed by Unity on every frame <see cref="https://docs.unity3d.com/Manual/ExecutionOrder.html"/>
    /// You can use it to animate something in runtime.
    /// </summary>
    private void Update()
    {
        ClearMeshData();
        Field.Update();
        CreateMeshData();
        SetMesh();
    }
}