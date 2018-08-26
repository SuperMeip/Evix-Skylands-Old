using System;

/// <summary>
/// Anything in the world that moves
/// </summary>
[Serializable]
public class Entity {

  /// <summary>
  /// The world this player is in
  /// </summary>
  public World world { get; private set; }

  /// <summary>
  /// The location of the player in the world
  /// </summary>
  public Coordinate location { get; private set; }

  /// <summary>
  /// The current chunk this entity is in
  /// </summary>
  public Chunk chunk { get; private set; }

  /// <summary>
  /// The current level the player is in
  /// </summary>
  public Level level { get; private set; }

  /// <summary>
  /// If the player isn't in a level and is instead flying around in the void
  /// </summary>
  public bool isInTheVoid {
    get {
      return level == null;
    }
  }

  /// <summary>
  /// create a player at location in the world
  /// </summary>
  /// <param name="world"></param>
  /// <param name="location"></param>
  public Entity(World world, Coordinate location) {
    this.world = world;
    this.location = location;
  }

  /// <summary>
  /// Set the location of the entity
  /// </summary>
  /// <param name="location"></param>
  public void updateLocation(Coordinate location) {
    this.location = location;
  }

  /// <summary>
  /// Set the location of the entity
  /// </summary>
  /// <param name="location"></param>
  public void updateChunk(Chunk chunk) {
    this.chunk = chunk;
  }

  /// <summary>
  /// Set the player's new current level
  /// </summary>
  /// <param name="newLevel"></param>
  public void updateLevel(Level newLevel = null) {
    level = newLevel;
    chunk = newLevel.chunkAtWorldLocation(location);
  }
}
