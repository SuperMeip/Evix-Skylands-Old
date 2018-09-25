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
  /// Wether to set chunks active on creation
  /// </summary>
  public bool setChunkActive = true;

  /// <summary>
  /// The chunk meshes ready to render queue for multhreading
  /// </summary>
  List<ChunkMeshGenerator.ChunkMesh> chunkRenderQueue;

  /// <summary>
  /// The chunks to generate meshes for with multhreading
  /// </summary>
  List<Coordinate> chunkMeshGenerationQueue;

  /// <summary>
  /// The chunks to activate and deactivate with multithread safety
  /// </summary>
  List<chunkActivationQueueObject> chunkActivationQueue;

  /// <summary>
  /// An object for queueing chunk activation and deactivation
  /// </summary>
  struct chunkActivationQueueObject {
    /// <summary>
    /// The location of the chunk to toggle activation for
    /// </summary>
    public Coordinate chunkLocation;

    /// <summary>
    /// What to set the active of the chunk to
    /// </summary>
    public bool setActiveTo;
  }

  /// <summary>
  /// set up the queues
  /// </summary>
  public void initialize(Island island) {
    this.island = island;
    chunkRenderQueue = new List<ChunkMeshGenerator.ChunkMesh>();
    chunkMeshGenerationQueue = new List<Coordinate>();
    chunkActivationQueue = new List<chunkActivationQueueObject>();
  }

  /// <summary>
  /// Run a jobs to empty async queues
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
    // render one chunk a frame
    if (chunkRenderQueue != null && chunkRenderQueue.Count > 0) {
      ChunkMeshGenerator.ChunkMesh chunkMesh = chunkRenderQueue[0];
      GameObject currentChunk = Instantiate(ChunkObject, chunkMesh.chunkLocation.vec3 * Chunk.CHUNK_DIAMETER * World.BLOCK_SIZE, new Quaternion(), transform);
      ChunkController chunkController = currentChunk.GetComponent<ChunkController>();
      chunkController.chunk = island.getChunk(chunkMesh.chunkLocation, true);
      chunkController.chunk.controller = chunkController;
      chunkController.renderChunk(chunkMesh);
      currentChunk.SetActive(setChunkActive);
      chunkRenderQueue.RemoveAt(0);
    }
    // Empty the activation/deactivation queue
    if (chunkActivationQueue != null && chunkActivationQueue.Count > 0) {
      chunkActivationQueue.RemoveAll((chunkActivationQueueObject queueObject) => {
        return toggleChunkActive(queueObject.chunkLocation, queueObject.setActiveTo);
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
  /// Activate inactive chunks around the given player
  /// </summary>
  /// <param name="player"></param>
  public void activateAroundPlayer(Player player) {
    if (island == null) {
      return;
    }
    Coordinate location = new Coordinate(0, 0, 0);
    for (location.x = player.chunk.location.x - World.ACTIVE_CHUNKS_RADIUS; location.x <= player.chunk.location.x + World.ACTIVE_CHUNKS_RADIUS; location.x++) {
      for (location.z = player.chunk.location.z - World.ACTIVE_CHUNKS_RADIUS; location.z <= player.chunk.location.z + World.ACTIVE_CHUNKS_RADIUS; location.z++) {
        queueColumnForActivation(location);
      }
    }
  }

  /// <summary>
  /// Queue up one column of chunks at the given location x,z for deactivation
  /// </summary>
  /// <param name="columnLocation"></param>
  public void queueColumnForActivation(Coordinate columnLocation) {
    for (columnLocation.y = 0; columnLocation.y < Chunk.CHUNK_HEIGHT; columnLocation.y++) {
      chunkActivationQueue.Add(new chunkActivationQueueObject() {
        chunkLocation = columnLocation,
        setActiveTo = true
      });
    }
  }

  /// <summary>
  /// Queue up one column of chunks forat the given location x,z for activation
  /// </summary>
  /// <param name="columnLocation"></param>
  public void queueColumnForDeActivation(Coordinate columnLocation) {
    for (columnLocation.y = 0; columnLocation.y < Chunk.CHUNK_HEIGHT; columnLocation.y++) {
      chunkActivationQueue.Add(new chunkActivationQueueObject() {
        chunkLocation = columnLocation,
        setActiveTo = false
      });
    }
  }

  /// <summary>
  /// Toggle the chunk active or inactive in unityengine
  /// </summary>
  /// <param name="location">the chunk to toggle's logaction</param>
  /// <param name="setActiveTo">whether to set it active or inactive, toggles to oposite by default</param>
  /// <returns>True if it worked, false if the chunk isn't valid for toggling atm</returns>
  bool toggleChunkActive(Coordinate location, bool? setActiveTo = null) {
    Chunk chunkToActivate = island.getChunk(location);
    if (chunkToActivate != null && chunkToActivate.hasBeenRendered && chunkToActivate.controller != null) {
      if (setActiveTo == null) {
        setActiveTo = !chunkToActivate.controller.gameObject.activeSelf;
      }
      chunkToActivate.controller.gameObject.SetActive((bool)setActiveTo);
      return true;
    }
    return false;
  }

  /// <summary>
  /// Queue up the correct chunk columns for activation and deactivation around the player when they change chunks
  /// </summary>
  /// <param name=""></param>
  /// <param name=""></param>
  public void queueActiveChangesForNewPosition(Chunk newChunk, Chunk oldChunk) {
    if (newChunk.level == island && oldChunk.level == island && !oldChunk.location.Equals(newChunk.location)) {
      List<Coordinate> columnsToRender = new List<Coordinate>();
      List<Coordinate> columnsToDeRender = new List<Coordinate>();
      Directions[] directionsMoved = oldChunk.location.getDirectionsTo(newChunk.location);
      foreach (Directions direction in directionsMoved) {
        switch (direction) {
          case Directions.north:
            for (int x = newChunk.location.x - World.ACTIVE_CHUNKS_RADIUS; x <= newChunk.location.x + World.ACTIVE_CHUNKS_RADIUS; x++) {
              queueColumnForActivation(new Coordinate(x, 0, newChunk.location.z + World.ACTIVE_CHUNKS_RADIUS));
              queueColumnForDeActivation(new Coordinate(x, 0, oldChunk.location.z - World.ACTIVE_CHUNKS_RADIUS));
            }
            break;
          case Directions.south:
            for (int x = newChunk.location.x - World.ACTIVE_CHUNKS_RADIUS; x <= newChunk.location.x + World.ACTIVE_CHUNKS_RADIUS; x++) {
              queueColumnForActivation(new Coordinate(x, 0, newChunk.location.z - World.ACTIVE_CHUNKS_RADIUS));
              queueColumnForDeActivation(new Coordinate(x, 0, oldChunk.location.z + World.ACTIVE_CHUNKS_RADIUS));
            }
            break;
          case Directions.east:
            for (int z = newChunk.location.z - World.ACTIVE_CHUNKS_RADIUS; z <= newChunk.location.z + World.ACTIVE_CHUNKS_RADIUS; z++) {
              queueColumnForActivation(new Coordinate(newChunk.location.x + World.ACTIVE_CHUNKS_RADIUS, 0, z));
              queueColumnForDeActivation(new Coordinate(oldChunk.location.x - World.ACTIVE_CHUNKS_RADIUS, 0, z));
            }
            break;
          case Directions.west:
            for (int z = newChunk.location.z - World.ACTIVE_CHUNKS_RADIUS; z <= newChunk.location.z + World.ACTIVE_CHUNKS_RADIUS; z++) {
              queueColumnForActivation(new Coordinate(newChunk.location.x - World.ACTIVE_CHUNKS_RADIUS, 0, z));
              queueColumnForDeActivation(new Coordinate(oldChunk.location.x + World.ACTIVE_CHUNKS_RADIUS, 0, z));
            }
            break;
        }
      }
    }
  }

  /// <summary>
  /// Queue the whole island for render
  /// </summary>
  public void renderAll() {
    if (island == null) {
      return;
    }
    Coordinate location = new Coordinate(0, 0, 0);
    for (location.x = 0; location.x < island.widthInChunks; location.x++) {
      for (location.z = 0; location.z < island.depthInChunks; location.z++) {
        chunkMeshGenerationQueue.Add(location);
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
}