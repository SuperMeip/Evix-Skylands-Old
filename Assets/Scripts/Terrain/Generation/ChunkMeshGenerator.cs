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

    public ChunkMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, Coordinate chunkLocation) {
      this.vertices = vertices;
      this.triangles = triangles;
      this.uvs = uvs;
      this.chunkLocation = chunkLocation;
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
        if (block.type != Block.Type.air) {
          if (block.north != null && block.north.type == Block.Type.air) {
            CubeNorth(block.location.x, block.location.y, block.location.z, block);
          }
          if (block.east != null && block.east.type == Block.Type.air) {
            CubeEast(block.location.x, block.location.y, block.location.z, block);
          }
          if (block.south != null && block.south.type == Block.Type.air) {
            CubeSouth(block.location.x, block.location.y, block.location.z, block);
          }
          if (block.west != null && block.west.type == Block.Type.air) {
            CubeWest(block.location.x, block.location.y, block.location.z, block);
          }
          if (block.up != null && block.up.type == Block.Type.air) {
            CubeTop(block.location.x, block.location.y, block.location.z, block);
          }
          if (block.down != null && block.down.type == Block.Type.air) {
            CubeBottom(block.location.x, block.location.y, block.location.z, block);
          }
        }
      }
    );
  }

  void Cube(Block block) {
    Vector2 texturePos = block.isSelected ? tSelected : block.uvBase.vec2;
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
    chunkMesh.vertices.Add(new Vector3(x, y + 1, z + 1));
    chunkMesh.vertices.Add(new Vector3(x + 1, y + 1, z + 1));
    chunkMesh.vertices.Add(new Vector3(x + 1, y + 1, z));
    chunkMesh.vertices.Add(new Vector3(x, y + 1, z));
    Cube(block);
  }

  void CubeNorth(int x, int y, int z, Block block) {
    chunkMesh.vertices.Add(new Vector3(x + 1, y, z + 1));
    chunkMesh.vertices.Add(new Vector3(x + 1, y + 1, z + 1));
    chunkMesh.vertices.Add(new Vector3(x, y + 1, z + 1));
    chunkMesh.vertices.Add(new Vector3(x, y, z + 1));
    Cube(block);
  }

  void CubeEast(int x, int y, int z, Block block) {
    chunkMesh.vertices.Add(new Vector3(x + 1, y, z));
    chunkMesh.vertices.Add(new Vector3(x + 1, y + 1, z));
    chunkMesh.vertices.Add(new Vector3(x + 1, y + 1, z + 1));
    chunkMesh.vertices.Add(new Vector3(x + 1, y, z + 1));
    Cube(block);
  }

  void CubeSouth(int x, int y, int z, Block block) {
    chunkMesh.vertices.Add(new Vector3(x, y, z));
    chunkMesh.vertices.Add(new Vector3(x, y + 1, z));
    chunkMesh.vertices.Add(new Vector3(x + 1, y + 1, z));
    chunkMesh.vertices.Add(new Vector3(x + 1, y, z));
    Cube(block);
  }

  void CubeWest(int x, int y, int z, Block block) {
    chunkMesh.vertices.Add(new Vector3(x, y, z + 1));
    chunkMesh.vertices.Add(new Vector3(x, y + 1, z + 1));
    chunkMesh.vertices.Add(new Vector3(x, y + 1, z));
    chunkMesh.vertices.Add(new Vector3(x, y, z));
    Cube(block);
  }

  void CubeBottom(int x, int y, int z, Block block) {
    chunkMesh.vertices.Add(new Vector3(x, y, z));
    chunkMesh.vertices.Add(new Vector3(x + 1, y, z));
    chunkMesh.vertices.Add(new Vector3(x + 1, y, z + 1));
    chunkMesh.vertices.Add(new Vector3(x, y, z + 1));
    Cube(block);
  }
}