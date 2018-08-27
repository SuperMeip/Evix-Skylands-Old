using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generate a world.
/// </summary>
public class WorldGenerator : MonoBehaviour {

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
    playerObject.transform.position = new Vector3(200 * World.BLOCK_SIZE, 70 * World.BLOCK_SIZE, 200 * World.BLOCK_SIZE);
    Player player1 = new Player(world, playerObject.transform.position.getCoordinate());
    playerObject.GetComponent<PlayerController>().player = player1;
    world.players[0] = player1;
    createStartingIsland();
  }

  /// <summary>
  /// Create the starting island of the world around the first player.
  /// </summary>
  void createStartingIsland() {
    Island island = world.createNewIsland(new Coordinate(0, 0, 0));
    GameObject startingIsland = Instantiate(islandObject, island.location.vec3 * World.WORLD_NEXUS_LENGTH * World.BLOCK_SIZE, new Quaternion(), transform);
    IslandRenderer islandRenderer = startingIsland.GetComponent<IslandRenderer>();
    islandRenderer.island = island;
    playerObject.GetComponent<PlayerController>().currentRenderer = islandRenderer;
    Player player1 = world.players[0];
    player1.updateLevel(island);
    islandRenderer.renderAroundPlayer(player1);
  }
}
