using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A basic tree
/// </summary>
public class Tree : BlockEntity {
  public Tree(World world, Coordinate location) : base(world, location) {

    rectangles = new Coordinate[2, CORNER_COORDINATE_COUNT] {
      // Trunk
      {
        new Coordinate(-1, 0, -1),
        new Coordinate(1, 12, 1)
      },
      // Leaves
      {
        new Coordinate(-4, 8 , -4),
        new Coordinate(4, 16, 4),
      }
    };

    colors = new Color[2] {
      // trunk - brown
      new Color(160, 82, 45),
      // leaves - light green
      new Color(154, 205, 50)
    };
  }
}
