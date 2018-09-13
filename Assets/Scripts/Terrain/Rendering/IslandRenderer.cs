using UnityEngine;
using UnityEngine.Profiling;
using System.Collections.Generic;

/// <summary>
/// Used to manage the rendering of an island and it's chunks
/// </summary>
public class IslandRenderer : MonoBehaviour {

  /// <summary>
  /// The island model
  /// </summary>
  public Island island { get; private set; }

  /// <summary>
  /// The chunks used for rendering
  /// </summary>
  public GameObject ChunkObject;

  /// <summary>
  /// The chunk meshes ready to render queue for multhreading
  /// </summary>
  List<ChunkMeshGenerator.ChunkMesh> chunkRenderQueue;

  /// <summary>
  /// The chunks to generate meshes for with multhreading
  /// </summary>
  List<Coordinate> chunkMeshGenerationQueue;

  /// <summary>
  /// set up the queues
  /// </summary>
  public void initialize(Island island) {
    this.island = island;
    chunkRenderQueue = new List<ChunkMeshGenerator.ChunkMesh>();
    chunkMeshGenerationQueue = new List<Coordinate>();
  }

  /// <summary>
  /// Run a job to render each column of chunks in the queue.
  /// </summary>
  void Update() {
    // Empty the mesh gen queue
    if (chunkMeshGenerationQueue != null && chunkMeshGenerationQueue.Count > 0) {
      chunkMeshGenerationQueue.RemoveAll((Coordinate chunkColumn) => {
        if (island.columnHasBeenGenerated(chunkColumn)) {
          GenerateChunkColumnMeshsJob meshGenJob = new GenerateChunkColumnMeshsJob();
          meshGenJob.columnLocation = chunkColumn;
          meshGenJob.island = island;
          meshGenJob.outputQueue = chunkRenderQueue;
          meshGenJob.Start();
          return true;
        }
        return false;
      });
    }
    // Empty the render queue
    if (chunkRenderQueue != null && chunkRenderQueue.Count > 0) {
      chunkRenderQueue.RemoveAll((ChunkMeshGenerator.ChunkMesh chunkMesh) => {
        GameObject currentChunk = Instantiate(ChunkObject, chunkMesh.chunkLocation.vec3 * Chunk.CHUNK_DIAMETER * World.BLOCK_SIZE, new Quaternion(), transform);
        ChunkController chunkController = currentChunk.GetComponent<ChunkController>();
        chunkController.chunk = island.getChunk(chunkMesh.chunkLocation, true);
        chunkController.chunk.controller = chunkController;
        chunkController.renderChunk(chunkMesh);
        return true;
      });
    }
  }

  /// <summary>
  /// Render active chunks of an island around a player that haven't been rendered yet.
  ///   This loops over all chunks but skips all logic on empty, alreary rendered, and ungenerated chunks.
  /// </summary>
  /// <param name="playerLocation"></param>
  public void renderAroundPlayer(Player player) {
    if (island == null) {
      return;
    }
    Coordinate location = new Coordinate(0, 0, 0);
    for (location.x = player.chunk.location.x - World.ACTIVE_CHUNKS_RADIUS; location.x < player.chunk.location.x + World.ACTIVE_CHUNKS_RADIUS; location.x++) {
      for (location.z = player.chunk.location.z - World.ACTIVE_CHUNKS_RADIUS; location.z < player.chunk.location.z + World.ACTIVE_CHUNKS_RADIUS; location.z++) {
        chunkMeshGenerationQueue.Add(location);
      }
    }
  }

  /// <summary>
  /// Enqueue a column of chunks at location for rendering
  /// </summary>
  /// <param name="columnLocation">The x and z of the column of chunks to render</param>
  public void renderChunkColumn(Coordinate columnLocation) {
    if (island == null || !columnLocation.isInitialized) {
      return;
    }
    Chunk chunk = island.getChunk(new Coordinate(columnLocation.x, 0, columnLocation.z));
    if (chunk != null && !chunk.hasBeenRendered) {
      // if it hasn't been rendered before, toss it in the mesh gen queue.
      // @todo: load mesh from saved memory to optimize maybe
      chunkMeshGenerationQueue.Add(chunk.location);
    } else if (chunk != null && chunk.hasBeenGenerated && chunk.renderMesh.isValid) {
      // if it's just been hidden/destroyed re-render it from it's mesh
      chunkRenderQueue.Add(chunk.renderMesh);
    }
  }

  /// <summary>
  /// Destroy a column of chunks
  /// </summary>
  /// <param name="columnLocation">The x and z of the column of chunks to de-render</param>
  public void deRenderChunkColumn(Coordinate columnLocation) {
    if (island == null || !columnLocation.isInitialized) {
      return;
    }
    Coordinate location = new Coordinate(columnLocation.x, 0, columnLocation.z);
    for (location.y = island.heightInChunks - 1; location.y >= 0; location.y--) {
      Chunk chunk = island.getChunk(location, false);
      if (chunk != null) {
        if (chunk.controller != null && chunk.isRendered) {
          chunk.isRendered = false;
          Destroy(chunk.controller.gameObject);
        }
      }
    }
  }

  /// <summary>
  /// A threaded job to generate a single column of chunks.
  /// </summary>
  class GenerateChunkColumnMeshsJob : ThreadedJob {

    /// <summary>
    /// The x and z of the chunk column to generate
    /// </summary>
    public Coordinate columnLocation;

    /// <summary>
    /// The island to generate it for.
    /// </summary>
    public Island island;

    /// <summary>
    /// The queue to output this data to
    /// </summary>
    public List<ChunkMeshGenerator.ChunkMesh> outputQueue;

    /// <summary>
    /// Generate just the mesh and uv values for the column of chunks
    /// </summary>
    protected override void ThreadFunction() {
      // @todo: breaks here?
      if (columnLocation.isInitialized && island != null) {
        for (int y = island.heightInChunks - 1; y >= 0; y--) {
          Chunk chunk = island.getChunk(new Coordinate(columnLocation.x, y, columnLocation.z));
          if (chunk != null && !chunk.isEmpty) {
            if (!chunk.hasBeenRendered) {
              if (chunk.hasBeenGenerated) {
                ChunkMeshGenerator chunkRenderer = new ChunkMeshGenerator();
                ChunkMeshGenerator.ChunkMesh mesh = chunkRenderer.generateMeshFor(chunk);
                outputQueue.Add(mesh);
              }
            }
          }
        }
      }
    }
  }

  /// <summary>
  /// Queue up the correct chunk columns for rendering around the player when they change chunks
  /// </summary>
  /// <param name=""></param>
  /// <param name=""></param>
  public void renderPositionChange(Chunk newChunk, Chunk oldChunk) {
    if (newChunk.level == island && oldChunk.level == island && !oldChunk.location.Equals(newChunk.location)) {
      Directions direction;
      // @todo: the parts of the switch below can probably just be brought up in here:
      if (newChunk.location.x < oldChunk.location.x) {
        direction = Directions.west;
      } else if (newChunk.location.x > oldChunk.location.x) {
        direction = Directions.east;
      } else if (newChunk.location.z < oldChunk.location.z) {
        direction = Directions.south;
      } else if (newChunk.location.z > oldChunk.location.z) {
        direction = Directions.north;
      } else {
        return;
      }
      List<Coordinate> columnsToRender = new List<Coordinate>();
      List<Coordinate> columnsToDeRender = new List<Coordinate>();
      switch (direction) {
        case Directions.north:
          for (int x = newChunk.location.x - World.ACTIVE_CHUNKS_RADIUS; x < newChunk.location.x + World.ACTIVE_CHUNKS_RADIUS; x++) {
            columnsToRender.Add(new Coordinate(x, newChunk.location.z + World.ACTIVE_CHUNKS_RADIUS));
          }
          for (int x = oldChunk.location.x - World.ACTIVE_CHUNKS_RADIUS; x < oldChunk.location.x + World.ACTIVE_CHUNKS_RADIUS; x++) {
            columnsToDeRender.Add(new Coordinate(x, oldChunk.location.z - World.ACTIVE_CHUNKS_RADIUS));
          }
          break;
        case Directions.east:
          for (int z = newChunk.location.z - World.ACTIVE_CHUNKS_RADIUS; z < newChunk.location.z + World.ACTIVE_CHUNKS_RADIUS; z++) {
            columnsToRender.Add(new Coordinate(newChunk.location.x + World.ACTIVE_CHUNKS_RADIUS, z));
          }
          for (int z = oldChunk.location.z - World.ACTIVE_CHUNKS_RADIUS; z < oldChunk.location.z + World.ACTIVE_CHUNKS_RADIUS; z++) {
            columnsToDeRender.Add(new Coordinate(oldChunk.location.x - World.ACTIVE_CHUNKS_RADIUS, z));
          }
          break;
        case Directions.south:
          for (int x = newChunk.location.x - World.ACTIVE_CHUNKS_RADIUS; x < newChunk.location.x + World.ACTIVE_CHUNKS_RADIUS; x++) {
            columnsToRender.Add(new Coordinate(x, newChunk.location.z - World.ACTIVE_CHUNKS_RADIUS));
          }
          for (int x = oldChunk.location.x - World.ACTIVE_CHUNKS_RADIUS; x < oldChunk.location.x + World.ACTIVE_CHUNKS_RADIUS; x++) {
            columnsToDeRender.Add(new Coordinate(x, oldChunk.location.z + World.ACTIVE_CHUNKS_RADIUS));
          }
          break;
        case Directions.west:
          for (int z = newChunk.location.z - World.ACTIVE_CHUNKS_RADIUS; z < newChunk.location.z + World.ACTIVE_CHUNKS_RADIUS; z++) {
            columnsToRender.Add(new Coordinate(newChunk.location.x - World.ACTIVE_CHUNKS_RADIUS, z));
          }
          for (int z = oldChunk.location.z - World.ACTIVE_CHUNKS_RADIUS; z < oldChunk.location.z + World.ACTIVE_CHUNKS_RADIUS; z++) {
            columnsToDeRender.Add(new Coordinate(oldChunk.location.x + World.ACTIVE_CHUNKS_RADIUS, z));
          }
          break;
        default:
          return;
      }
      foreach (Coordinate columnToRender in columnsToRender) {
        renderChunkColumn(columnToRender);
      }
      foreach (Coordinate columnToDeRender in columnsToDeRender) {
        deRenderChunkColumn(columnToDeRender);
      }
    }
  }
}
