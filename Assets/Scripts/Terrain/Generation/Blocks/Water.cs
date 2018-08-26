public class Water : Blocks.Block {
  public Water(Coordinate location, Chunk parent) : base(location, parent, Type.water) {
    uvBase = new Coordinate(1, 2);
  }
}