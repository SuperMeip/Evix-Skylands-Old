namespace Blocks {
  public class Stone : BlockType {
    public static new Type type = Type.stone;

    public Stone() : base(type) {
      uvBase = new Coordinate(1, 3);
    }
  }
}