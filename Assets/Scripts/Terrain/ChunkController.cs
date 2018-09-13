using UnityEngine;

/// <summary>
/// Controls a chunks in game behaviors
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ChunkController : MonoBehaviour {

  /// <summary>
  /// The chunk this is controlling
  /// </summary>
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
    chunk.renderMesh = meshToUse;

    mesh.Clear();
    mesh.vertices = meshToUse.vertices.ToArray();
    mesh.uv = meshToUse.uvs.ToArray();
    mesh.triangles = meshToUse.triangles.ToArray();
    mesh.RecalculateNormals();
    col.sharedMesh = mesh;
  }

  /// <summary>
  /// Destroy the block at the given block location
  /// </summary>
  /// <param name="hitLocation"></param>
  public void destroyBlock(Coordinate blockLocation) {
    Blocks.Block blockToDestroy = chunk.getBlock(blockLocation.trimmed);
    // replace with air in the model
    chunk.destroyBlock(blockToDestroy);

    // update the chunk mesh and re-render.
    renderChunk();
  }
}
