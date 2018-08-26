using System.Collections.Generic;

/// <summary>
/// The world
/// </summary>
public class World {

  /// <summary>
  /// The diameter of the square of active(rendered) chunks (must be odd)
  /// </summary>
  public const int ACTIVE_CHUNKS_RADIUS = 5;

  /// <summary>
  /// The world size of blocks
  /// </summary>
  public const float BLOCK_SIZE = 1;

  /// <summary>
  /// The distance in which islands may spawn.
  /// </summary>
  public const int WORLD_NEXUS_LENGTH = 5000;

  /// <summary>
  /// The max number of players in a world
  /// </summary>
  public const int PLAYER_LIMIT = 4;

  /// <summary>
  /// The curently rendered chunks
  /// </summary>
  public Chunk[,] activeChunks;

  /// <summary>
  /// The current unique chunk id for chunk generation.
  /// </summary>
  public int currentChunkID = 0;

  /// <summary>
  /// The current unique chunk id for chunk generation.
  /// </summary>
  public int currentIslandID = 0;

  /// <summary>
  /// All the levels indexed by world nexus
  /// </summary>
  private Dictionary<Coordinate, Level> levels;

  /// <summary>
  /// The players in this world
  /// </summary>
  public Player[] players = new Player[PLAYER_LIMIT];

  /// <summary>
  /// Instanciate a new world
  /// </summary>
  public World() {
    levels = new Dictionary<Coordinate, Level>();
  }

  /// <summary>
  /// get the island at world nexus location
  /// </summary>
  /// <param name="location">The nexus point to grab at</param>
  public Level getLevel(Coordinate location) {
    if (levels.ContainsKey(location)) {
      return levels[location];
    }
    return null;
  }

  /// <summary>
  /// Create a new island at the world location
  /// </summary>
  /// <param name="location"></param>
  /// <returns></returns>
  public Island createNewIsland(Coordinate location) {
    if (getLevel(location) == null) {
      Island newIsland = new Island(this, location);
      levels.Add(location, newIsland);
      return newIsland;
    }
    return null;
  }
}