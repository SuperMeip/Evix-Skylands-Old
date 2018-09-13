namespace Blocks {
  public class Water : BlockType {
    public static new Type type = Type.water;

    public Water() : base(type, false, true) {
      uvBase = new Coordinate(1, 2);
    }
  }
}