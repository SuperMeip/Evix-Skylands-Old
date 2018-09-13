namespace Blocks {
  public class Dirt : BlockType {
    public static new Type type = Type.dirt;

    public Dirt() : base(type) {
      uvBase = new Coordinate(0, 2);
    }
  }

  public class Grass : Dirt {
    public static new Type type = Type.grass;

    public Grass() : base() {
      value = type;
      uvBase = new Coordinate(0, 3);
    }
  }
}