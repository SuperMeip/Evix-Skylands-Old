using System;

/// <summary>
/// A player exploring the world
/// </summary>
[Serializable]
public class Player : Entity {

  /// <summary>
  /// create a player at location in the world
  /// </summary>
  /// <param name="world"></param>
  /// <param name="location"></param>
  public Player(World world, Coordinate location) : base (world, location) {}
}
