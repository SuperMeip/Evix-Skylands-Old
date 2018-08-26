public class Dirt : Blocks.Block {
  public Dirt(Coordinate location, Chunk parent) : base(location, parent, Type.dirt) {
    uvBase = new Coordinate(0, 2);
  }

  protected void growGrass() {
    type = Type.grass;
  }
}

public class Grass : Dirt {
  public Grass(Coordinate location, Chunk parent) : base(location, parent) {
    uvBase = new Coordinate(0, 3);
    growGrass();
  }
}
