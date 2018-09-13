using Blocks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class ChunkMeshGenerator {

  /// <summary>
  /// Basic structure containing arrays of information about a chunk's mesh
  /// </summary>
  public struct ChunkMesh {
    public List<Vector3> vertices;
    public List<int> triangles;
    public List<Vector2> uvs;
    public Coordinate chunkLocation;
    public bool isValid;

    public ChunkMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, Coordinate chunkLocation) {
      this.vertices = vertices;
      this.triangles = triangles;
      this.uvs = uvs;
      this.chunkLocation = chunkLocation;
      isValid = true;
    }
  }

  /// <summary>
  /// The chunk to render
  /// </summary>
  Chunk chunk;

  /// <summary>
  /// The values for the mesh being generated
  /// </summary>
  ChunkMesh chunkMesh;

  /// <summary>
  /// The block texture sheet division percentage
  /// </summary>
  float tUnit = 0.25f;

  /// <summary>
  /// Block size unit
  /// </summary>
  float bUnit = World.BLOCK_SIZE;

  /// <summary>
  /// The location of the selected block mask
  /// </summary>
  Vector2 tSelected = new Vector2(2, 2);

  /// <summary>
  /// Total # of faces
  /// </summary>
  int faceCount;

  /// <summary>
  /// Generate the mesh vales for a chunk
  /// </summary>
  /// <param name="chunk"></param>
  /// <returns type="ChunkMesh">The parts of the chunk's mesh</returns>
  public ChunkMesh generateMeshFor(Chunk chunk) {
    chunkMesh = new ChunkMesh(new List<Vector3>(), new List<int>(), new List<Vector2>(), chunk.location);
    faceCount = 0;
    if (chunk == null) {
      Debug.Log("Chunk is empty, cannot generate mesh");
    } else {
      this.chunk = chunk;
      generateMesh();
      chunk.hasBeenRendered = true;
      chunk.isRendered = true;
    }
    return chunkMesh;
  }

  /// <summary>
  /// Generate mesh and uvs etc for the attached chunk
  /// </summary>
  void generateMesh() {
    if (chunk == null) {
      Debug.Log("No Chunk Provided For Render");
      return;
    }
    chunk.forEach(
      (Block block) => {
        if (!block.isEmpty) {
          if (BlockTypes.isEmpty(block.north)) {
            CubeNorth(block.location.x, block.location.y, block.location.z, block);
          }
          if (BlockTypes.isEmpty(block.east)) {
            CubeEast(block.location.x, block.location.y, block.location.z, block);
          }
          if (BlockTypes.isEmpty(block.south)) {
            CubeSouth(block.location.x, block.location.y, block.location.z, block);
          }
          if (BlockTypes.isEmpty(block.west)) {
            CubeWest(block.location.x, block.location.y, block.location.z, block);
          }
          if (BlockTypes.isEmpty(block.up)) {
            CubeTop(block.location.x, block.location.y, block.location.z, block);
          }
          if (BlockTypes.isEmpty(block.down)) {
            CubeBottom(block.location.x, block.location.y, block.location.z, block);
          }
        }
      }
    );
  }

  void Cube(Block block) {
    Vector2 texturePos = block.isSelected ? tSelected : BlockTypes.get(block.type).uvBase.vec2;
    chunkMesh.triangles.Add(faceCount * 4); //1
    chunkMesh.triangles.Add(faceCount * 4 + 1); //2
    chunkMesh.triangles.Add(faceCount * 4 + 2); //3
    chunkMesh.triangles.Add(faceCount * 4); //1
    chunkMesh.triangles.Add(faceCount * 4 + 2); //3
    chunkMesh.triangles.Add(faceCount * 4 + 3); //4

    chunkMesh.uvs.Add(new Vector2(tUnit * texturePos.x + tUnit, tUnit * texturePos.y));
    chunkMesh.uvs.Add(new Vector2(tUnit * texturePos.x + tUnit, tUnit * texturePos.y + tUnit));
    chunkMesh.uvs.Add(new Vector2(tUnit * texturePos.x, tUnit * texturePos.y + tUnit));
    chunkMesh.uvs.Add(new Vector2(tUnit * texturePos.x, tUnit * texturePos.y));

    faceCount++;
  }

  void CubeTop(int x, int y, int z, Block block) {
    chunkMesh.vertices.Add(new Vector3(x, y + 1, z + 1) * bUnit);
    chunkMesh.vertices.Add(new Vector3(x + 1, y + 1, z + 1) * bUnit);
    chunkMesh.vertices.Add(new Vector3(x + 1, y + 1, z) * bUnit);
    chunkMesh.vertices.Add(new Vector3(x, y + 1, z) * bUnit);
    Cube(block);
  }

  void CubeNorth(int x, int y, int z, Block block) {
    chunkMesh.vertices.Add(new Vector3(x + 1, y, z + 1) * bUnit);
    chunkMesh.vertices.Add(new Vector3(x + 1, y + 1, z + 1) * bUnit);
    chunkMesh.vertices.Add(new Vector3(x, y + 1, z + 1) * bUnit);
    chunkMesh.vertices.Add(new Vector3(x, y, z + 1) * bUnit);
    Cube(block);
  }

  void CubeEast(int x, int y, int z, Block block) {
    chunkMesh.vertices.Add(new Vector3(x + 1, y, z) * bUnit);
    chunkMesh.vertices.Add(new Vector3(x + 1, y + 1, z) * bUnit);
    chunkMesh.vertices.Add(new Vector3(x + 1, y + 1, z + 1) * bUnit);
    chunkMesh.vertices.Add(new Vector3(x + 1, y, z + 1) * bUnit);
    Cube(block);
  }

  void CubeSouth(int x, int y, int z, Block block) {
    chunkMesh.vertices.Add(new Vector3(x, y, z) * bUnit);
    chunkMesh.vertices.Add(new Vector3(x, y + 1, z) * bUnit);
    chunkMesh.vertices.Add(new Vector3(x + 1, y + 1, z) * bUnit);
    chunkMesh.vertices.Add(new Vector3(x + 1, y, z) * bUnit);
    Cube(block);
  }

  void CubeWest(int x, int y, int z, Block block) {
    chunkMesh.vertices.Add(new Vector3(x, y, z + 1) * bUnit);
    chunkMesh.vertices.Add(new Vector3(x, y + 1, z + 1) * bUnit);
    chunkMesh.vertices.Add(new Vector3(x, y + 1, z) * bUnit);
    chunkMesh.vertices.Add(new Vector3(x, y, z) * bUnit);
    Cube(block);
  }

  void CubeBottom(int x, int y, int z, Block block) {
    chunkMesh.vertices.Add(new Vector3(x, y, z) * bUnit);
    chunkMesh.vertices.Add(new Vector3(x + 1, y, z) * bUnit);
    chunkMesh.vertices.Add(new Vector3(x + 1, y, z + 1) * bUnit);
    chunkMesh.vertices.Add(new Vector3(x, y, z + 1) * bUnit);
    Cube(block);
  }
}