﻿using System;
using Blocks;

/// <summary>
/// The groups of tiles that render together in the world
/// </summary>
[Serializable]
public class Chunk {

  /// <summary>
  /// The default diameter of a chunk
  /// </summary>
  public const int CHUNK_DIAMETER = 25;

  /// <summary>
  /// The height of chunks
  /// </summary>
  public const int CHUNK_HEIGHT = CHUNK_DIAMETER;

  /// <summary>
  /// If this is the spawn chunk
  /// </summary>
  public bool isSpawn = false;

  /// <summary>
  /// If this chunk is empty and hasn't had any blocks updated
  /// </summary>
  public bool isEmpty = true;

  /// <summary>
  /// If this chunk has been visibly rendered already
  /// </summary>
  public bool hasBeenRendered = false;

  /// <summary>
  /// If this chunk is currently rendered visibly
  /// </summary>
  public bool isRendered = false;

  /// <summary>
  /// If this chunk's terrain has been generated by the island
  /// </summary>
  public bool hasBeenGenerated = false;

  /// <summary>
  /// The location in the level x, y of this chunk
  /// </summary>
  public Coordinate location;

  /// <summary>
  /// The controller of this chunk's gameobject
  /// </summary>
  public ChunkController controller;

  /// <summary>
  /// The chunk's render mesh
  /// </summary>
  public ChunkMeshGenerator.ChunkMesh renderMesh;

  /// <summary>
  /// For quick access to the chunks neighboring this one
  /// </summary>
  [NonSerialized]
  Chunk[] neighbors;

  /// <summary>
  /// The blocks in this chunk
  /// </summary>
  Block[][][] blocks;

  /// <summary>
  /// The level this chunk is in
  /// </summary>
  public Level level { get; private set; }

  /// <summary>
  /// Unique ID of this chunk
  /// </summary>
  public int id { get; private set; }

  /// <summary>
  /// The absolute world location of this chunk
  /// </summary>
  public Coordinate worldLocation {
    get {
      return new Coordinate(
        location.x + level.location.x * World.WORLD_NEXUS_LENGTH,
        location.y + level.location.y * World.WORLD_NEXUS_LENGTH,
        location.z + level.location.z * World.WORLD_NEXUS_LENGTH
      );
    }
  }

  /// <summary>
  /// The chunk to the north
  /// </summary>
  public Chunk north {
    get { return getNeighbor(Directions.north); }
    set { neighbors[(int)Directions.north] = value; }
  }

  /// <summary>
  /// The chunk to the south
  /// </summary>
  public Chunk east {
    get { return getNeighbor(Directions.east); }
    set { neighbors[(int)Directions.east] = value; }
  }

  /// <summary>
  /// The chunk to the east
  /// </summary>
  public Chunk south {
    get { return getNeighbor(Directions.south); }
    set { neighbors[(int)Directions.south] = value; }
  }

  /// <summary>
  /// The chunk to the west
  /// </summary>
  public Chunk west {
    get { return getNeighbor(Directions.west); }
    set { neighbors[(int)Directions.west] = value; }
  }

  /// <summary>
  /// The chunk above
  /// </summary>
  public Chunk up {
    get { return getNeighbor(Directions.up); }
    set { neighbors[(int)Directions.up] = value; }
  }

  /// <summary>
  /// The chunk below
  /// </summary>
  public Chunk down {
    get { return getNeighbor(Directions.down); }
    set { neighbors[(int)Directions.down] = value; }
  }

  /// <summary>
  /// Create a chunk
  /// </summary>
  /// <param name="location"></param>
  /// <param name="level"></param>
  public Chunk(Coordinate location, Level level) {
    this.location = location.copy;
    this.level = level;
    id = level.world.currentChunkID++;
    neighbors = new Chunk[6];
    blocks = createJaggedArray();
  }

  /// <summary>
  /// Preform a function on each block in this chunk
  /// </summary>
  /// <param name="action">The action (function) to preform on each block</param>
  public void forEach(Action<Block> action) {
    Coordinate location = new Coordinate(0, 0, 0);
    for (location.x = 0; location.x < CHUNK_DIAMETER; location.x++) {
      for (location.y = 0; location.y < CHUNK_HEIGHT; location.y++) {
        for (location.z = 0; location.z < CHUNK_DIAMETER; location.z++) {
          action(getBlock(location));
        }
      }
    }
  }

  /// <summary>
  /// Get the chunk in the specified direction.
  /// </summary>
  /// <param name="direction"></param>
  /// <param name=forceLoadNeighbor">If a neighbor is null, force load it as a new chunk</param>
  /// <returns>The chunk in the direction from this chunk or null if it fails to find one</returns>
  public Chunk toThe(Directions direction, bool forceLoadNeighbor = false) {
    return getNeighbor(direction, forceLoadNeighbor);
  }

  /// <summary>
  /// Set a neighboring chunk at the given direction
  /// </summary>
  /// <param name="direction"></param>
  /// <param name="chunk"></param>
  public void setNeighbor(Directions direction, Chunk chunk) {
    neighbors[(int)direction] = chunk;
  }

  /// <summary>
  /// Get block at location
  /// </summary>
  /// <param name="blockLocation">the location of the block relative to this chunk</param>
  /// <returns>The block at location, or an uninitialized block (!Block.isValid) if one wasn't found</returns>
  public Block getBlock(Coordinate blockLocation) {
    // if it's in this chunk
    if (blockLocation.isWithinChunkBounds) {
      // if it's invalid set it to air
      if (!blocks[blockLocation.x][blockLocation.y][blockLocation.z].isValid) {
        setBlock(new Block(blockLocation, this, BlockTypes.Air));
        return blocks[blockLocation.x][blockLocation.y][blockLocation.z];
      }
      return blocks[blockLocation.x][blockLocation.y][blockLocation.z];
    // if it's to the north
    } else if (blockLocation.z >= CHUNK_DIAMETER) {
      blockLocation.z -= CHUNK_DIAMETER;
      return neighbors[(int)Directions.north] != null ? north.getBlock(blockLocation) : new Block();
    // if it's to the east
    } else if (blockLocation.x >= CHUNK_DIAMETER) {
      blockLocation.x -= CHUNK_DIAMETER;
      return neighbors[(int)Directions.east] != null ? east.getBlock(blockLocation) : new Block();
    // if it's to the south
    } else if (blockLocation.z < 0) {
      blockLocation.z += CHUNK_DIAMETER;
      return neighbors[(int)Directions.south] != null ? south.getBlock(blockLocation) : new Block();
    // if it's to the west
    } else if (blockLocation.x < 0) {
      blockLocation.x += CHUNK_DIAMETER;
      return neighbors[(int)Directions.west] != null ? west.getBlock(blockLocation) : new Block();
    // if it's above
    } else if (blockLocation.y >= CHUNK_HEIGHT) {
      blockLocation.y -= CHUNK_HEIGHT;
      return neighbors[(int)Directions.up] != null ? up.getBlock(blockLocation) : new Block();
    // if it's below
    } else if (blockLocation.y < 0) {
      blockLocation.y += CHUNK_HEIGHT;
      return neighbors[(int)Directions.down] != null ? down.getBlock(blockLocation) : new Block();
    } else {
      return new Block();
    }
  }

  /// <summary>
  /// Cange the block at location in a chunk
  /// </summary>
  /// <param name="newBlock">The block to replace the current one in the chunk with</param>
  /// <param name="updateNeighbors">Whether or not to update this block and all it's neighbors as well</param>
  public void updateBlock(Block newBlock) {
    if (newBlock.isValid) {
      // first, see if this chunk is still empty
      isEmpty = !isEmpty ? isEmpty : BlockTypes.isEmpty(newBlock.type);
      newBlock.setParent(this);
      Block oldBlock = getBlock(newBlock.location);
      // if the types are different, we need to update the neighbors of this new block, and surrounding blocks
      if (oldBlock.type != newBlock.type) {
        newBlock.copyNeighbors(oldBlock);
        foreach (Directions direction in Coordinate.DIRECTIONS) {
          // for each neighboring block, if it's valid, set it's neighbor in the oposite direction to this block type
          Block neighbor = getBlock(newBlock.location.go(direction));
          if (neighbor.isValid) {
            neighbor.setNeighbor(Coordinate.reverseDirection(direction), newBlock.type);
            // if it's not from the current chunk, we need to ask the chunk in that direction from this one to update it.
            if (neighbor.chunkId != newBlock.chunkId) {
              getNeighbor(direction).setBlock(neighbor);
            } else {
              setBlock(neighbor);
            }
          }
        }
        // set the new block
        setBlock(newBlock);
      }
    }
  }

  /// <summary>
  /// Remove the block from the chunk, replacing it with air
  /// </summary>
  /// <param name="block">The block to remove</param>
  public void destroyBlock(Block block) {
    setBlock(new Block(block.location, this, BlockTypes.Air));
  }

  /// <summary>
  /// Override tostring for chunks
  /// </summary>
  /// <returns>The type of chunk and it's world location</returns>
  public override string ToString() {
    return (isEmpty ? "Empty @ " : "Land @ ") + location.ToString();
  }

  /// <summary>
  /// set the block at it's location
  /// </summary>
  /// <param name="newBlock"></param>
  void setBlock(Block newBlock) {
    blocks[newBlock.location.x][newBlock.location.y][newBlock.location.z] = newBlock;
  }

  /// <summary>
  /// Get the chunk in the direction specified
  /// </summary>
  /// <param name="direction">The direction from this chunk of the chunk you want</param>
  /// <param name=forceLoadNeighbor">If a neighbor is null, force load it as a new chunk before checking</param>
  /// <returns>The requested neighnoring chunk</returns>
  Chunk getNeighbor(Directions direction, bool forceLoadNeighbor = false) {
    int directionIndex = (int)direction;
    if (neighbors[directionIndex] == null) {
      neighbors[directionIndex] = level.getChunk(location.go(direction), forceLoadNeighbor);
      return neighbors[directionIndex];
    } else {
      return neighbors[directionIndex];
    }
  }

  /// <summary>
  /// Goes through each direction and sets this neighbor to what it is in the world for quick lookup
  /// This also sets the neighbors of existing neighbors as this to save time.
  /// </summary>
  void setNeighbors() {
    foreach(Directions direction in Coordinate.DIRECTIONS) {
      int directionalIndex = (int)direction;
      if (neighbors[directionalIndex] == null) {
        neighbors[directionalIndex] = level.getChunk(location.go(direction));
        if (neighbors[directionalIndex] != null) {
          switch (direction) {
            case Directions.north:
              neighbors[directionalIndex].south = this;
              break;
            case Directions.east:
              neighbors[directionalIndex].west = this;
              break;
            case Directions.south:
              neighbors[directionalIndex].north = this;
              break;
            case Directions.west:
              neighbors[directionalIndex].east = this;
              break;
            case Directions.up:
              neighbors[directionalIndex].up = this;
              break;
            case Directions.down:
              neighbors[directionalIndex].down = this;
              break;
            default:
              break;
          }
        }
      }
    }
  }

  /// <summary>
  /// create a jagged chunk array
  /// </summary>
  /// <returns></returns>
  Block[][][] createJaggedArray() {
    Block[][][] blockArray = new Block[CHUNK_DIAMETER][][];
    for (int x = 0; x < CHUNK_DIAMETER; x++) {
      var tmp1 = blockArray[x] = new Block[CHUNK_HEIGHT][];
      for (int y = 0; y < CHUNK_HEIGHT; y++) {
        var tmp2 = tmp1[y] = new Block[CHUNK_DIAMETER];
        for (int z = 0; z < CHUNK_DIAMETER; z++) {
          tmp2[z] = new Block();
        }
      }
    }
    return blockArray;
  }
}
