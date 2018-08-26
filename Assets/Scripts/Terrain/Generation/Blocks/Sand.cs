public class Sand : Blocks.Block {
  public Sand(Coordinate location, Chunk parent) : base(location, parent, Type.sand) {
    uvBase = new Coordinate(2, 3);
  }
}