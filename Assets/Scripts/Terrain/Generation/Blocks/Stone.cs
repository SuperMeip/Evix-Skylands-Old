public class Stone : Blocks.Block {
  public Stone(Coordinate location, Chunk parent) : base(location, parent, Type.stone) {
    uvBase = new Coordinate(1, 3);
  }
}