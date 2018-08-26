/// <summary>
/// All blocks are kept in this namespace
/// </summary>
namespace Blocks {

  /// <summary>
  /// A block of terrain
  /// </summary>
  public abstract class Block {

    public enum Type { air, water, dirt, stone, grass, sand }

    /// <summary>
    /// The block type
    /// </summary>
    public Type type { get; protected set; }

    /// <summary>
    /// The location in the world x, y of this chunk
    /// </summary>
    public Coordinate location;

    /// <summary>
    /// The chunk this block resides in
    /// </summary>
    public Chunk chunk { get; private set; }

    /// <summary>
    /// The name of this block type
    /// </summary>
    public string name { get { return type.ToString(); } }

    /// <summary>
    /// The id of this block type
    /// </summary>
    public int id { get { return (int)type; } }

    /// <summary>
    /// If this block has an alpha 
    /// </summary>
    public bool alpha { get; protected set; }

    /// <summary>
    /// The UV sprite location of this block
    /// </summary>
    public Coordinate uvBase { get; protected set; }

    /// <summary>
    /// If this is a selected block
    /// </summary>
    public bool isSelected;

    /// <summary>
    /// The block to the north
    /// </summary>
    public Block north {
      get { return getNeighbor(Directions.north); }
      set { neighbors[(int)Directions.north] = value; }
    }

    /// <summary>
    /// The block to the south
    /// </summary>
    public Block east {
      get { return getNeighbor(Directions.east); }
      set { neighbors[(int)Directions.east] = value; }
    }

    /// <summary>
    /// The block to the east
    /// </summary>
    public Block south {
      get { return getNeighbor(Directions.south); }
      set { neighbors[(int)Directions.south] = value; }
    }

    /// <summary>
    /// The block to the west
    /// </summary>
    public Block west {
      get { return getNeighbor(Directions.west); }
      set { neighbors[(int)Directions.west] = value; }
    }

    /// <summary>
    /// The block above
    /// </summary>
    public Block up {
      get { return getNeighbor(Directions.up); }
      set { neighbors[(int)Directions.up] = value; }
    }

    /// <summary>
    /// The block below
    /// </summary>
    public Block down {
      get { return getNeighbor(Directions.down); }
      set { neighbors[(int)Directions.down] = value; }
    }

    /// <summary>
    /// For quick access to the chunks neighboring this one without accessing world
    /// </summary>
    private Block[] neighbors;

    protected Block(Coordinate location, Chunk parent, Type type = Type.air) {
      this.location = location.copy;
      chunk = parent;
      this.type = type;
      isSelected = false;
      neighbors = new Block[6];
    }

    /// <summary>
    /// Make a new block
    /// </summary>
    /// <param name="location">The location in the chunk of this block</param>
    /// <param name="parent">The parent chunk of this block</param>
    /// <param name="type">The type of this block</param>
    /// <returns>A new block of the requested type</returns>
    public static Block make(Coordinate location, Chunk parent, Type type = Type.air) {
      switch (type) {
        case Type.air:
          return new Air(location, parent);
        case Type.water:
          return new Water(location, parent);
        case Type.grass:
          return new Grass(location, parent);
        case Type.dirt:
          return new Dirt(location, parent);
        case Type.sand:
          return new Sand(location, parent);
        case Type.stone:
          return new Stone(location, parent);
        default:
          return null;
      }
    }

    /// <summary>
    /// Copy neighbors from another block
    /// </summary>
    /// <param name="blockToCopyFrom"></param>
    public void copyNeighbors(Block blockToCopyFrom) {
      neighbors = blockToCopyFrom.neighbors;
    }

    // @todo: this seems broken between chunks, make sure this works with loading and unloading chunks for determining faces
    /// <summary>
    /// Get the chunk in the direction specified
    /// </summary>
    /// <param name="direction">The direction from this chunk of the chunk you want</param>
    /// <returns>The requested neighnoring chunk</returns>
    private Block getNeighbor(Directions direction) {
      int directionIndex = (int)direction;
      if (neighbors[directionIndex] == null) {
        switch (direction) {
          case Directions.north:
            neighbors[directionIndex] = chunk.getBlock(new Coordinate(location.x, location.y, location.z + 1));
            if (neighbors[directionIndex] != null) {
              neighbors[directionIndex].south = this;
            }
            break;
          case Directions.east:
            neighbors[directionIndex] = chunk.getBlock(new Coordinate(location.x + 1, location.y, location.z));
            if (neighbors[directionIndex] != null) {
              neighbors[directionIndex].west = this;
            }
            break;
          case Directions.south:
            neighbors[directionIndex] = chunk.getBlock(new Coordinate(location.x, location.y, location.z - 1));
            if (neighbors[directionIndex] != null) {
              neighbors[directionIndex].north = this;
            }
            break;
          case Directions.west:
            neighbors[directionIndex] = chunk.getBlock(new Coordinate(location.x - 1, location.y, location.z));
            if (neighbors[directionIndex] != null) {
              neighbors[directionIndex].east = this;
            }
            break;
          case Directions.up:
            neighbors[directionIndex] = chunk.getBlock(new Coordinate(location.x, location.y + 1, location.z));
            if (neighbors[directionIndex] != null) {
              neighbors[directionIndex].down = this;
            }
            break;
          case Directions.down:
            neighbors[directionIndex] = chunk.getBlock(new Coordinate(location.x, location.y - 1, location.z));
            if (neighbors[directionIndex] != null) {
              neighbors[directionIndex].up = this;
            }
            break;
          default:
            return null;
        }
      }
      return neighbors[directionIndex];
    }
  }
}