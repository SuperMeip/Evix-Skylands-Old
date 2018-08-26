using System;

/// <summary>
/// A collection of chunks floating in the aether
/// </summary>
[Serializable]
public abstract class Level {

  /// <summary>
  /// X size
  /// </summary>
  protected int width = Chunk.CHUNK_DIAMETER * 20;

  /// <summary>
  /// y size
  /// </summary>
  protected int height = Chunk.CHUNK_HEIGHT * 10;

  /// <summary>
  /// z size
  /// </summary>
  protected int depth = Chunk.CHUNK_DIAMETER * 20;

  /// <summary>
  /// The world location of the 0,0,0 chunk of this island
  /// </summary>
  public Coordinate location { get; private set; }

  /// <summary>
  /// The world this island is in.
  /// </summary>
  public World world { get; private set; }

  /// <summary>
  /// The width x of the island in chunks
  /// </summary>
  public int widthInChunks { get; private set; }

  /// <summary>
  /// The depth z of the island in chunks
  /// </summary>
  public int depthInChunks { get; private set; }

  /// <summary>
  /// The height y of the island in chunks
  /// </summary>
  public int heightInChunks { get; private set; }

  /// <summary>
  /// The island seed.
  /// </summary>
  public int seed { get; private set; }

  /// <summary>
  /// The chunks that make up this island
  /// </summary>
  [NonSerialized]
  private Chunk[][][] chunks;

  /// <summary>
  /// Create a new level at the nexus location in the world
  /// </summary>
  /// <param name="world"></param>
  /// <param name="location"></param>
  public Level(World world, Coordinate location) {
    this.world = world;
    this.location = location.copy;
    widthInChunks = (int)Math.Ceiling((double)width / Chunk.CHUNK_DIAMETER);
    heightInChunks = (int)Math.Ceiling((double)height / Chunk.CHUNK_HEIGHT);
    depthInChunks = (int)Math.Ceiling((double)width / Chunk.CHUNK_DIAMETER);
    chunks = createJaggedArray();
    // @todo: update this eventually
    seed = 8675309;
  }

  /// <summary>
  /// Get the chunk at the given location
  /// </summary>
  /// <param name="location">local level location of the chunk to grab</param>
  /// <param name="forceLoad">Whether or not to force load a new chunk if one isn't found</param>
  /// <returns type="Chunk"></returns>
  public Chunk getChunk(Coordinate location, bool forceLoad = true) {
    if(!chunkLocationIsInBounds(location)) {
      return null;
    }
    Chunk foundChunk = chunks[location.x][location.y][location.z];
    if (forceLoad && foundChunk == null) {
      foundChunk = createNewChunk(location);
    }
    return foundChunk;
  }

  /// <summary>
  /// Preform a function on each chunk in this island
  /// </summary>
  /// <param name="action">The action (function) to preform on each block</param>
  public void forEach(Action<Chunk> action) {
    Coordinate location = new Coordinate(0, 0, 0);
    for (location.x = 0; location.x < widthInChunks; location.x++) {
      for (location.y = 0; location.y < heightInChunks; location.y++) {
        for (location.z = 0; location.z < depthInChunks; location.z++) {
          action(getChunk(location));
        }
      }
    }
  }

  /// <summary>
  /// Check if a chunk location is within the bounds of the island
  /// </summary>
  /// <param name="location"></param>
  /// <returns></returns>
  public bool chunkLocationIsInBounds(Coordinate location) {
    return location.x >= 0
      && location.x < widthInChunks
      && location.y >= 0
      && location.y < heightInChunks
      && location.z >= 0
      && location.z < depthInChunks;
  }

  /// <summary>
  /// Get the chunk given the world location
  /// </summary>
  /// <param name="location"></param>
  public Chunk chunkAtWorldLocation(Coordinate worldLocation) {
    return getChunk(new Coordinate(
      worldLocation.x - location.x * World.WORLD_NEXUS_LENGTH,
      worldLocation.y - location.y * World.WORLD_NEXUS_LENGTH,
      worldLocation.z - location.z * World.WORLD_NEXUS_LENGTH
    ).chunkLocation);
  }

  /// <summary>
  /// create a jagged chunk array
  /// </summary>
  /// <returns></returns>
  Chunk[][][] createJaggedArray() {
    Chunk[][][] chunkArray = new Chunk[widthInChunks][][];
    for (int x = 0; x < widthInChunks; x++) {
      var tmp1 = chunkArray[x] = new Chunk[heightInChunks][];
      for (int y = 0; y < heightInChunks; y++) {
        var tmp2 = tmp1[y] = new Chunk[depthInChunks];
        for (int z = 0; z < depthInChunks; z++) {
          tmp2[z] = null; 
        }
      }
    }
    return chunkArray;
  }

  /// <summary>
  /// Create a new chunk at the point
  /// use getChunk with forceload to be safer
  /// </summary>
  /// <param name="location"></param>
  /// <returns>The new chunk</returns>
  Chunk createNewChunk(Coordinate location) {
    Chunk newChunk = new Chunk(location, this);
    chunks[location.x][location.y][location.z] = newChunk;
    return newChunk;
  }
}