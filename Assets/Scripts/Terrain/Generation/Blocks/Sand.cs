namespace Blocks {
  public class Sand : BlockType {
    public static new Type type = Type.sand;

    public Sand() : base(type) {
      uvBase = new Coordinate(2, 3);
    }
  }
}