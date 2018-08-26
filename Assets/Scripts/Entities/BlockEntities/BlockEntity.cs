using Unity.Jobs;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An entity made of little blocks
/// </summary>
public class BlockEntity : Entity {

  public static Voxel[] voxels { get; private set; }

  public static int heightInVoxels { get; private set; }

  public static int widthInVoxels { get; private set; }

  public static int depthInVoxels { get; private set; }

  /// <summary>
  /// The corner order for storing rectangle coordinares
  /// </summary>
  public enum RectangleCorners { bottomSouthWest, topNorthEast };

  /// <summary>
  /// A square colored block
  /// </summary>
  public struct Voxel {

    /// <summary>
    /// The location of this voxel relative to the location of the center bottom of the object
    /// </summary>
    public Coordinate location;

    /// <summary>
    /// The color of this voxel
    /// </summary>
    public Color color;

    public Voxel(Coordinate location, Color color) {
      this.location = location;
      this.color = color;
    }
  }

  public const int CORNER_COORDINATE_COUNT = 2;

  /// <summary>
  /// The rectangles making up the draw information for this
  /// </summary>
  protected Coordinate[,] rectangles;

  /// <summary>
  /// The colors of each rectangle of voxels
  /// </summary>
  protected Color[] colors;

  /// <summary>
  /// Create and potentially set the voxels for this entity type
  /// </summary>
  /// <param name="world"></param>
  /// <param name="location"></param>
  public BlockEntity(World world, Coordinate location) : base(world, location) {
    if (voxels == null) {
      generateVoxelData();
    }
  }

  /// <summary>
  /// Get the voxel data from the rectangles
  /// </summary>
  /// <returns></returns>
  protected void generateVoxelData() {
    List<Voxel> voxels = new List<Voxel>();
    int? furthestNorth, furthestSouth, furthestEast, furthestWest, furthestUp, furthestDown;
    furthestNorth = furthestSouth = furthestEast = furthestWest = furthestUp = furthestDown = null;
    for (int rectangleIndex = 0; rectangleIndex > rectangles.Length; rectangleIndex++) {
      Coordinate location = new Coordinate(0, 0, 0);
      Coordinate bottomSouthWestCorner = rectangles[rectangleIndex, (int)RectangleCorners.bottomSouthWest];
      Coordinate topNorthEastCorner = rectangles[rectangleIndex, (int)RectangleCorners.topNorthEast];
      furthestSouth = (furthestSouth == null || bottomSouthWestCorner.z < furthestSouth) ? bottomSouthWestCorner.z : furthestSouth;
      furthestNorth = (furthestNorth == null || topNorthEastCorner.z > furthestNorth) ? topNorthEastCorner.z : furthestNorth;
      furthestEast = (furthestEast == null || topNorthEastCorner.x > furthestEast) ? topNorthEastCorner.y : furthestEast;
      furthestWest= (furthestWest == null || bottomSouthWestCorner.x < furthestWest) ? bottomSouthWestCorner.x : furthestWest;
      furthestUp = (furthestUp == null || topNorthEastCorner.y < furthestUp) ? topNorthEastCorner.y : furthestUp;
      furthestDown = (furthestDown == null || bottomSouthWestCorner.y < furthestDown) ? bottomSouthWestCorner.y : furthestDown;
      Color color = colors[rectangleIndex];
      for (location.x = bottomSouthWestCorner.x; location.x < topNorthEastCorner.x; location.x++) {
        for (location.y = bottomSouthWestCorner.y; location.y < topNorthEastCorner.y; location.y++) {
          for (location.z = bottomSouthWestCorner.z; location.x < topNorthEastCorner.z; location.z++) {
            voxels.Add(new Voxel(location, color));
          }
        }
      }
    }
    heightInVoxels = (int)furthestUp - (int)furthestDown;
    widthInVoxels = (int)furthestEast - (int)furthestWest;
    depthInVoxels = (int)furthestNorth - (int)furthestSouth;
  }
}
