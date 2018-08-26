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
  /// The level this chunk is in
  /// </summary>
  public Level level { get; private set; }

  /// <summary>
  /// Unique ID of this chunk
  /// </summary>
  public int id { get; private set; }

  /// <summary>
  /// The location in the level x, y of this chunk
  /// </summary>
  public Coordinate location;

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
  /// The controller of this chunk's gameobject
  /// </summary>
  public ChunkController controller;

  /// <summary>
  /// For quick access to the chunks neighboring this one
  /// </summary>
  [NonSerialized]
  private Chunk[] neighbors;

  /// <summary>
  /// The blocks in this chunk
  /// </summary>
  private Block[,,] blocks;

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
    initializeChunkBlocks();
  }

  /// <summary>
  /// Initialize all blocks in this chunk to default.
  /// </summary>
  public void initializeChunkBlocks() {
    blocks = new Block[CHUNK_DIAMETER, CHUNK_DIAMETER, CHUNK_HEIGHT];
    Coordinate location = new Coordinate(0, 0, 0);
    for (location.x = 0; location.x < CHUNK_DIAMETER; location.x++) {
      for (location.y = 0; location.y < CHUNK_HEIGHT; location.y++) {
        for (location.z = 0; location.z < CHUNK_DIAMETER; location.z++) {
          blocks[location.x, location.y, location.z] = new Air(location, this);
        }
      }
    }
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
          // @todo: uhhh? does this work with the changing types?
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
  /// <returns></returns>
  public Block getBlock(Coordinate blockLocation) {
    // if it's in this chunk
    if ((blockLocation.x < CHUNK_DIAMETER && blockLocation.x >= 0 )
      && (blockLocation.y < CHUNK_HEIGHT && blockLocation.y >= 0)
      && (blockLocation.z < CHUNK_DIAMETER && blockLocation.z >= 0)
    ) {
      return blocks[blockLocation.x, blockLocation.y, blockLocation.z];
    // if it's to the north
    } else if (blockLocation.z >= CHUNK_DIAMETER) {
      blockLocation.z -= CHUNK_DIAMETER;
      return neighbors[(int)Directions.north] != null ? north.getBlock(blockLocation) : null;
    // if it's to the east
    } else if (blockLocation.x >= CHUNK_DIAMETER) {
      blockLocation.x -= CHUNK_DIAMETER;
      return neighbors[(int)Directions.east] != null ? east.getBlock(blockLocation) : null;
    // if it's to the south
    } else if (blockLocation.z < 0) {
      blockLocation.z += CHUNK_DIAMETER;
      return neighbors[(int)Directions.south] != null ? south.getBlock(blockLocation) : null;
    // if it's to the west
    } else if (blockLocation.x < 0) {
      blockLocation.x += CHUNK_DIAMETER;
      return neighbors[(int)Directions.west] != null ? west.getBlock(blockLocation) : null;
    // if it's above
    } else if (blockLocation.y >= CHUNK_HEIGHT) {
      blockLocation.y -= CHUNK_HEIGHT;
      return neighbors[(int)Directions.up] != null ? up.getBlock(blockLocation) : null;
    // if it's below
    } else if (blockLocation.y < 0) {
      blockLocation.y += CHUNK_HEIGHT;
      return neighbors[(int)Directions.down] != null ? down.getBlock(blockLocation) : null;
    } else {
      return null;
    }
  }

  /// <summary>
  /// Cange the block at location in a chunk
  /// </summary>
  /// <param name="newBlock">The block to replace the current one in the chunk with</param>
  public void updateBlock(Block newBlock) {
    isEmpty = false;
    Block oldblock = getBlock(newBlock.location);
    if (oldblock != null && newBlock.location.isWithinChunkBounds) {
      // update this block's and the surrounding blocks neigbors
      newBlock.north = oldblock.north;
      if (newBlock.north != null) {
        oldblock.north.south = newBlock;
      }
      newBlock.east = oldblock.east;
      if (newBlock.east != null) {
        oldblock.east.west = newBlock;
      }
      newBlock.south = oldblock.south;
      if (newBlock.south != null) {
        oldblock.south.north = newBlock;
      }
      newBlock.west = oldblock.west;
      if (newBlock.west != null) {
        oldblock.west.east = newBlock;
      }
      newBlock.up = oldblock.up;
      if (newBlock.up != null) {
        oldblock.up.down = newBlock;
      }
      newBlock.down = oldblock.down;
      if (newBlock.down != null) {
        oldblock.down.up = newBlock;
      }
      oldblock.chunk.setBlock(newBlock);
    }
  }

  /// <summary>
  /// set the block at it's location
  /// </summary>
  /// <param name="newBlock"></param>
  private void setBlock(Block newBlock) {
    blocks[newBlock.location.x, newBlock.location.y, newBlock.location.z] = newBlock;
  }

  /// <summary>
  /// Get the chunk in the direction specified
  /// </summary>
  /// <param name="direction">The direction from this chunk of the chunk you want</param>
  /// <param name=forceLoadNeighbor">If a neighbor is null, force load it as a new chunk before checking</param>
  /// <returns>The requested neighnoring chunk</returns>
  private Chunk getNeighbor(Directions direction, bool forceLoadNeighbor = false) {
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
  public void setNeighbors() {
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
  /// Override tostring for chunks
  /// </summary>
  /// <returns>The type of chunk and it's world location</returns>
  public override string ToString() {
    return (isEmpty ? "Empty @ " : "Land @ ") + location.ToString();
  }
}