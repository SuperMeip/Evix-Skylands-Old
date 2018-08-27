using System;
using UnityEngine;

[Serializable]
public enum Directions { north, east, south, west, up, down };

[Serializable]
public enum Corners { northEast, northWest, southEast, southWest };

[Serializable]
public struct Coordinate {

  /// <summary>
  /// All of the directions in order
  /// </summary>
  public static Directions[] DIRECTIONS = new Directions[6] { Directions.north, Directions.east, Directions.south, Directions.west, Directions.up, Directions.down };

  // The coordinate values
  /// <summary>
  /// east west
  /// </summary>
  public int x;

  /// <summary>
  /// up down
  /// </summary>
  public int y;

  /// <summary>
  /// north south
  /// </summary>
  public int z;

  /// <summary>
  /// If this coordinate is valid and was properly initialized
  /// </summary>
  public bool isInitialized;

  /// <summary>
  /// If this was initiated with a y coordinate
  /// </summary>
  bool notMadeWithY;

  /// <summary>
  /// This coordinate as a vector 2
  /// </summary>
  public Vector2 vec2 {
    get { return new Vector2(x, z); }
  }

  /// <summary>
  /// This coordinate as a vector 3
  /// </summary>
  public Vector3 vec3 {
    get { return new Vector3(x, y, z); }
  }

  /// <summary>
  /// If this coordinate is inside of it's home chunk
  /// </summary>
  public bool isWithinChunkBounds {
    get {
      return x < Chunk.CHUNK_DIAMETER
        && x >= 0
        && y < Chunk.CHUNK_HEIGHT
        && y >= 0
        && z < Chunk.CHUNK_DIAMETER
        && z >= 0;
    }
  }

  /// <summary>
  /// The coordinate trimmed to the local bounds of a chunk.
  /// </summary>
  public Coordinate trimmed {
    get {
      if (isWithinChunkBounds) {
        return this;
      }
      int ix = x % Chunk.CHUNK_DIAMETER;
      if (ix < 0) {
        ix = Chunk.CHUNK_DIAMETER + ix;
      }
      int iy = y % Chunk.CHUNK_HEIGHT;
      if (iy < 0) {
        iy = Chunk.CHUNK_HEIGHT + iy;
      }
      int iz = z % Chunk.CHUNK_DIAMETER;
      if (iz < 0) {
        iz = Chunk.CHUNK_DIAMETER + iz;
      }
      return new Coordinate(ix, iy, iz);
    }
  }

  /// <summary>
  /// The location of the chunk this world location is in
  /// </summary>
  public Coordinate chunkLocation {
    get {
      return new Coordinate(
        x / Chunk.CHUNK_DIAMETER,
        y / Chunk.CHUNK_HEIGHT,
        z / Chunk.CHUNK_DIAMETER,
        notMadeWithY
      );
    }
  }

  /// <summary>
  /// The center of the block at this coordinate
  /// </summary>
  public Vector3 blockCenter {
    get {
      return new Vector3(
        x + World.BLOCK_SIZE / 2,
        y + World.BLOCK_SIZE / 2,
        z + World.BLOCK_SIZE / 2
      );
    }
  }

  /// <summary>
  /// Get a new object that's a copy of this coordinate
  /// </summary>
  public Coordinate copy {
    get {
      return new Coordinate(x, y, z, notMadeWithY);
    }
  }

  /// <summary>
  /// Create a 2d coordinate
  /// </summary>
  /// <param name="x"></param>
  /// <param name="z"></param>
  public Coordinate(int x, int z) {
    this.x = x;
    this.z = z;
    y = 0;
    notMadeWithY = true;
    isInitialized = true;
  }

  /// <summary>
  /// Create a 3d coordinate
  /// </summary>
  /// <param name="x"></param>
  /// <param name="y"></param>
  /// <param name="z"></param>
  public Coordinate(int x, int y, int z) {
    this.x = x;
    this.y = y;
    this.z = z;
    notMadeWithY = false;
    isInitialized = true;
  }

  /// <summary>
  /// Create a 3d coordinate or 2d coordinate depending on notMadeWithY
  /// </summary>
  /// <param name="x"></param>
  /// <param name="y"></param>
  /// <param name="z"></param>
  /// <param name="notMadeWithY">whether to make and use y in calculations for this coordinate</param>
  private Coordinate(int x, int y, int z, bool notMadeWithY) {
    this.x = x;
    this.y = y;
    this.z = z;
    this.notMadeWithY = notMadeWithY;
    isInitialized = true;
  }

  /// <summary>
  /// Turn a world (rendered) location into a block coordinate
  /// </summary>
  /// <param name="worldPosition"></param>
  /// <returns></returns>
  public static Coordinate fromWorldPosition(Vector3 worldPosition) {
    return new Coordinate(
      (int)(worldPosition.x / World.BLOCK_SIZE),
      (int)(worldPosition.y / World.BLOCK_SIZE),
      (int)(worldPosition.z / World.BLOCK_SIZE)
    );
  }

  /// <summary>
  /// Get the coordinate one over in another direction.
  /// </summary>
  /// <param name="direction">The direction to move in</param>
  /// <param name="direction">The distance to move</param>
  /// <returns>The coordinate one over in the requested direction</returns>
  public Coordinate goTo(Directions direction, int magnitude) {
    switch(direction) {
      case Directions.north:
        return new Coordinate(x, y, z + magnitude, notMadeWithY);
      case Directions.east:
        return new Coordinate(x + magnitude, y, z, notMadeWithY);
      case Directions.south:
        return new Coordinate(x, y, z - magnitude, notMadeWithY);
      case Directions.west:
        return new Coordinate(x - magnitude, y, z, notMadeWithY);
      case Directions.up:
        return new Coordinate(x, y + magnitude, z);
      case Directions.down:
        return new Coordinate(x, y - magnitude, z);
      default:
        return this;
    }
  }

  /// <summary>
  /// Go one in the selected direction
  /// </summary>
  /// <param name="direction"></param>
  /// <returns>The coordinate one over in the direction</returns>
  public Coordinate go (Directions direction) {
    return goTo(direction, 1);
  }

  /// <summary>
  /// Override the hash code to prevent colisions when used as a key
  /// </summary>
  /// <returns></returns>
  public override int GetHashCode() {
    if (z == 0) {
      unchecked {
        int hash = 17;
        hash = hash * 31 + x.GetHashCode();
        hash = hash * 31 + y.GetHashCode();
        return hash;
      }
    }
    return unchecked(x + (31 * y) + (31 * 31 * z));
  }

  /// <summary>
  /// Equals override
  /// </summary>
  /// <param name="obj"></param>
  /// <returns></returns>
  public override bool Equals(object obj) {
    if (!(obj is Coordinate)) {
      return false;
    }
    Coordinate other = (Coordinate)obj;
    return x == other.x && y == other.y && z == other.z;
  }

  /// <summary>
  /// To string
  /// </summary>
  /// <returns></returns>
  public override string ToString() {
    return notMadeWithY
      ? "{" + x.ToString() + ", " + z.ToString() + "}"
      : "{" + x.ToString() + ", " + y.ToString() + ", " + z.ToString() + "}";
  }

  /// <summary>
  /// Get the distance between this and otherPoint
  /// </summary>
  /// <param name="otherPoint"></param>
  /// <returns></returns>
  public float distance(Coordinate otherPoint) {
    return Mathf.Sqrt(
      Mathf.Pow(x - otherPoint.x, 2)
      + Mathf.Pow(y - otherPoint.y, 2)
      + Mathf.Pow(z - otherPoint.z, 2)
    );
  }

  /// <summary>
  /// Get the oposite of the requested direction
  /// </summary>
  /// <param name="direction"></param>
  /// <returns></returns>
  public static Directions switchDirection(Directions direction) {
    switch (direction) {
      case Directions.north:
        return Directions.south;
      case Directions.east:
        return Directions.west;
      case Directions.south:
        return Directions.north;
      case Directions.west:
        return Directions.east;
      case Directions.up:
        return Directions.down;
      case Directions.down:
        return Directions.up;
      default:
        return direction;
    }
  }
}

public static class Vector2Extension {
  public static Vector2 Rotate(this Vector2 v, float degrees) {
    var x = Quaternion.Euler(0, 0, degrees) * v;
    return x;
  }

  public static Coordinate getCoordinate(this Vector2 v) {
    return new Coordinate((int)v.x, (int)v.y);
  }
}

public static class Vector3Extension {
  public static Coordinate getCoordinate(this Vector3 v) {
    return new Coordinate((int)v.x, (int)v.y, (int)v.z);
  }
}