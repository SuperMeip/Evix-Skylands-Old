using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Generate a world.
/// </summary>
public class WorldGenerator : MonoBehaviour {
  /// <summary>
  /// The max amount of generation jobs allowed
  /// </summary>
  public const int MAX_GEN_JOB_COUNT = 8;

  /// <summary>
  /// The current amount of generation jobs
  /// </summary>
  private int genJobCount = 0;

  /// <summary>
  /// Generation job queue, chunk genertion jobs that are waiting to be threaded
  /// </summary>
  private List<ThreadedJob> genJobQueue = new List<ThreadedJob>();

  /// <summary>
  /// Currently running generation jobs
  /// </summary>
  private List<ThreadedJob> genRunningJobs = new List<ThreadedJob>();

  /// <summary>
  /// If the starting island has finished generating
  /// </summary>
  private bool startingIslandGenerationComplete = false;

  /// <summary>
  /// Where the player one starts
  /// </summary>
  private Level startingLevel;

  /// <summary>
  /// The prefab GameObject to render islands.
  /// </summary>
  public GameObject islandObject;

  /// <summary>
  /// The starting player object
  /// </summary>
  public GameObject playerObject;

  /// <summary>
  /// Za Warudo of the Aether
  /// </summary>
  World world;

  // Use this for initialization
  void Awake() {
    world = new World();
    wakePlayerOne();
    generateStartingIsland();
  }

  // Each frame
  void Update() {
    checkGenerateJobQueue();
  }

  /// <summary>
  /// Set up player one
  /// </summary>
  void wakePlayerOne() {
    playerObject.transform.position = new Vector3(200 * World.BLOCK_SIZE, 70 * World.BLOCK_SIZE, 200 * World.BLOCK_SIZE);
    Player playerOne = new Player(world, playerObject.transform.position.getCoordinate());
    playerObject.GetComponent<PlayerController>().player = playerOne;
    world.players[0] = playerOne;
  }

  /// <summary>
  /// Create the starting island of the world around the first player.
  /// </summary>
  [Obsolete("Old method for creating and rendering the starting island")]
  void createStartingIsland() {
    Island island = world.createNewIsland(new Coordinate(0, 0, 0));
    GameObject startingIsland = Instantiate(islandObject, island.location.vec3 * World.WORLD_NEXUS_LENGTH * World.BLOCK_SIZE, new Quaternion(), transform);
    IslandRenderer islandRenderer = startingIsland.GetComponent<IslandRenderer>();
    islandRenderer.initialize(island);
    playerObject.GetComponent<PlayerController>().currentRenderer = islandRenderer;
    Player player1 = world.players[0];
    player1.updateLevel(island);
    islandRenderer.renderAroundPlayer(player1);
  }

  /// <summary>
  /// Queue up am entire starting island for generation
  /// </summary>
  void generateStartingIsland() {
    Island island = world.createNewIsland(new Coordinate(0, 0, 0));
    startingLevel = island;
    Coordinate chunkColumnLocation = new Coordinate(0, 0);
    // queue up generation for all the chunk data for the island
    for (chunkColumnLocation.x = 0; chunkColumnLocation.x < island.widthInChunks; chunkColumnLocation.x++) {
      for (chunkColumnLocation.z = 0; chunkColumnLocation.z < island.depthInChunks; chunkColumnLocation.z++) {
        ThreadedJob generationJob = island.queueChunkForGeneration(chunkColumnLocation);
        if (generationJob != null) {
          genJobQueue.Add(generationJob);
        }
      }
    }

    // create the island world object and set player one's current renderer to it's
    GameObject startingIslandObject = Instantiate(islandObject, island.location.vec3 * World.WORLD_NEXUS_LENGTH * World.BLOCK_SIZE, new Quaternion(), transform);
    IslandRenderer islandRenderer = startingIslandObject.GetComponent<IslandRenderer>();
    islandRenderer.initialize(island);
    playerObject.GetComponent<PlayerController>().currentRenderer = islandRenderer;

    // queue the chunks around the player for rendering
    world.playerOne.updateLevel(startingLevel);
    islandRenderer.renderAroundPlayer(world.playerOne);
  }

  /// <summary>
  /// Check and manage any chunk generation jobs in the queue
  /// </summary>
  void checkGenerateJobQueue() {
    if (genJobQueue.Count > 0 && genJobCount < MAX_GEN_JOB_COUNT) {
      genJobCount++;
      ThreadedJob jobToStart = genJobQueue[0];
      genJobQueue.RemoveAt(0);
      genRunningJobs.Add(jobToStart);
      jobToStart.Start();
    }
    if (genRunningJobs.Count != 0) {
      genRunningJobs.RemoveAll((ThreadedJob runningJob) => {
        if (runningJob != null && runningJob.Update()) {
          genJobCount--;
          return true;
        }
        else {
          return false;
        }
      });
    }
  }
}
