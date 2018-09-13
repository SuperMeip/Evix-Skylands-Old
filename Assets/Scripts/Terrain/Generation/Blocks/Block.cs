using System.Linq;

/// <summary>
/// All blocks are kept in this namespace
/// </summary>
namespace Blocks {

  /// <summary>
  /// The enum used to store the id for block types.
  /// </summary>
  public enum Type : byte { air, water, dirt, stone, grass, sand }

  /// <summary>
  /// Manager for singleton blocktypes
  /// </summary>
  public static class BlockTypes {
    /// <summary>
    /// The instance, there should only be one of each blocktype.
    /// </summary>
    private static BlockType[] blockTypes = new BlockType[] {
      new Air(),
      new Water(),
      new Dirt(),
      new Stone(),
      new Grass(),
      new Sand()
    };

    /// <summary>
    /// Types ignored in rendering/ and for rendering faces
    /// </summary>
    private static Type[] emptyTypes = new Type[] {
      Air.value
    };

    /// <summary>
    /// The number of blocktypes
    /// </summary>
    public static int count {
      get {
        return blockTypes.Length;
      }
    }

    public static BlockType Air {
      get {
        return blockTypes[(int)Blocks.Air.type];
      }
    }

    public static BlockType Water {
      get {
        return blockTypes[(int)Blocks.Water.type];
      }
    }

    public static BlockType Dirt {
      get {
        return blockTypes[(int)Blocks.Dirt.type];
      }
    }

    public static BlockType Stone {
      get {
        return blockTypes[(int)Blocks.Stone.type];
      }
    }

    public static BlockType Grass {
      get {
        return blockTypes[(int)Blocks.Grass.type];
      }
    }

    public static BlockType Sand {
      get {
        return blockTypes[(int)Blocks.Sand.type];
      }
    }

    /// <summary>
    /// Get the blocktype by ID
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static BlockType get(Type type) {
      return blockTypes[(byte)type];
    }

    /// <summary>
    /// Get the blocktype by ID
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static BlockType get(byte typeId) {
      return typeId < count ? blockTypes[typeId] : null;
    }

    /// <summary>
    /// Returns true if the type is empty and should be ignored in rendering.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool isEmpty(Type type) {
      return emptyTypes.Contains(type);
    }
  }

  /// <summary>
  /// A type of block of terain
  /// </summary>
  public abstract class BlockType {

    /// <summary>
    /// If this block is solid
    /// </summary>
    public bool isSolid;

    /// <summary>
    /// If this block is a liquid
    /// </summary>
    public bool isLiquid;

    /// <summary>
    /// The block type
    /// </summary>
    public static Type type { get; protected set; }

    /// <summary>
    /// The name of this block type
    /// </summary>
    public string name { get { return type.ToString(); } }

    /// <summary>
    /// The id of this block type
    /// </summary>
    // @todo: may have to set manually
    public byte id { get { return (byte)type; } }

    /// <summary>
    /// If this block has an alpha 
    /// </summary>
    public bool alpha { get; protected set; }

    /// <summary>
    /// The type value of this blocktype
    /// </summary>
    public Type value { get; protected set; }

    /// <summary>
    /// The UV sprite location of this block
    /// </summary>
    public Coordinate uvBase { get; protected set; }

    protected BlockType(Type type, bool isSolid = true, bool isLiquid = false) {
      value = type;
      this.isSolid = isSolid;
      this.isLiquid = isLiquid;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="blockType"></param>
    /// <returns></returns>
    public bool isA(Type blockType) {
      return (int)blockType == id;
    }


    /// <summary>
    /// If two blocks are of the same type
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj) {
      BlockType other = obj as BlockType;
      return other.id == id;
    }
  }

  /// <summary>
  /// A block in the chunk.
  /// </summary>
  public struct Block {

    /// <summary>
    /// If this is a selected block
    /// </summary>
    public bool isSelected;

    /// <summary>
    /// For quick access to the blocks neighboring this one without accessing world
    /// </summary>
    Type[] neighbors;

    /// <summary>
    /// The id of the parent chunk of this block
    /// </summary>
    public int chunkId { get; private set; }

    /// <summary>
    /// The blocktype information.
    /// </summary>
    public Type type { get; private set; }

    /// <summary>
    /// The location in the world x, y of this chunk
    /// </summary>
    public Coordinate location { get; private set; }

    /// <summary>
    /// If this is a valid, initialized block
    /// </summary>
    public bool isValid {
      get {
        return location.isInitialized 
          && location.isWithinChunkBounds 
          && neighbors != null;
      }
    }

    /// <summary>
    /// If this block is empty and should be ignored in rendering
    /// </summary>
    public bool isEmpty {
      get {
        return !isValid || BlockTypes.isEmpty(type);
      }
    }

    /// <summary>
    /// The block to the north
    /// </summary>
    public Type north {
      get { return getNeighbor(Directions.north); }
      set { neighbors[(int)Directions.north] = value; }
    }

    /// <summary>
    /// The block to the south
    /// </summary>
    public Type east {
      get { return getNeighbor(Directions.east); }
      set { neighbors[(int)Directions.east] = value; }
    }

    /// <summary>
    /// The block to the east
    /// </summary>
    public Type south {
      get { return getNeighbor(Directions.south); }
      set { neighbors[(int)Directions.south] = value; }
    }

    /// <summary>
    /// The block to the west
    /// </summary>
    public Type west {
      get { return getNeighbor(Directions.west); }
      set { neighbors[(int)Directions.west] = value; }
    }

    /// <summary>
    /// The block above
    /// </summary>
    public Type up {
      get { return getNeighbor(Directions.up); }
      set { neighbors[(int)Directions.up] = value; }
    }

    /// <summary>
    /// The block below
    /// </summary>
    public Type down {
      get { return getNeighbor(Directions.down); }
      set { neighbors[(int)Directions.down] = value; }
    }

    /// <summary>
    /// if the block is at the same location in the same chunk
    /// </summary>
    /// <param name="obj" type="Block">A block to compare</param>
    /// <returns></returns>
    public override bool Equals(object obj) {
      var other = (Block)obj;
      return location.Equals(other.location) 
        && chunkId == other.chunkId
        && type == other.type;
    }

    /// <summary>
    /// Make a new block
    /// </summary>
    /// <param name="location">The location in the chunk of this block</param>
    /// <param name="parent">The parent chunk of this block</param>
    /// <param name="type">The type of this block</param>
    /// <returns>A new block of the requested type</returns>
    public Block(Coordinate location, Chunk parent, BlockType type) {
      chunkId = parent.id;
      this.location = location;
      this.type = type.value;
      isSelected = false;
      neighbors = new Type[6];
    }
    
    /// <summary>
    /// Make a new block
    /// </summary>
    /// <param name="location">The location in the chunk of this block</param>
    /// <param name="type">The type of this block</param>
    /// <returns>A new block of the requested type</returns>
    public Block(Coordinate location, BlockType type) {
      chunkId = 0;
      this.location = location;
      this.type = type.value;
      isSelected = false;
      neighbors = new Type[6];
    }

    /// <summary>
    /// Copy neighbors from another block
    /// </summary>
    /// <param name="blockToCopyFrom"></param>
    public void copyNeighbors(Block blockToCopyFrom) {
      neighbors = blockToCopyFrom.neighbors;
    }

    /// <summary>
    /// Set the parent to the given chunk
    /// </summary>
    /// <param name="parent"></param>
    public void setParent(Chunk parent) {
      chunkId = parent.id;
    }

    /// <summary>
    /// set all the neighbors based on a parent chunk
    /// </summary>
    /// <param name="chunk"></param>
    public void setNeighbors(Chunk chunk) {
      foreach (Directions direction in Coordinate.DIRECTIONS) {
        int directionIndex = (int)direction;
        Block neighborBlock = chunk.getBlock(location.go(direction));
        neighbors[directionIndex] = neighborBlock.type;
      }
    }

    /// <summary>
    /// Set the neighbor by type in the given direction
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="neighborType"></param>
    public void setNeighbor(Directions direction, Type neighborType) {
      neighbors[(int)direction] = neighborType;
    }

    /// <summary>
    /// Get the chunk in the direction specified
    /// </summary>
    /// <param name="direction">The direction from this chunk of the chunk you want</param>
    /// <returns>The type of the block in the direction</returns>
    Type getNeighbor(Directions direction) {
      int directionIndex = (int)direction;
      return neighbors[directionIndex];
    }
  }
}