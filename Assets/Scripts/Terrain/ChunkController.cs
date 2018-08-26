using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ChunkController : MonoBehaviour {

  public Chunk chunk;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

  /// <summary>
  /// Render this chunk using the given mesh
  /// </summary>
  public void renderChunk(ChunkMeshGenerator.ChunkMesh? chunkMesh = null) {
    Mesh mesh = GetComponent<MeshFilter>().mesh;
    MeshCollider col = GetComponent<MeshCollider>();
    ChunkMeshGenerator.ChunkMesh meshToUse = chunkMesh ?? (new ChunkMeshGenerator()).generateMeshFor(chunk);

    mesh.Clear();
    mesh.vertices = meshToUse.vertices.ToArray();
    mesh.uv = meshToUse.uvs.ToArray();
    mesh.triangles = meshToUse.triangles.ToArray();
    mesh.RecalculateNormals();
    col.sharedMesh = mesh;
  }
}
