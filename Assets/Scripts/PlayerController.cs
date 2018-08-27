using UnityEngine;

[RequireComponent (typeof(CharacterController))]
public class PlayerController : MonoBehaviour {

  /// <summary>
  /// The model for the select block outline
  /// </summary>
  public GameObject selectBlockOutlineObject;

  /// <summary>
  /// The character's head, for mouselook
  /// </summary>
  public GameObject headObject;

  /// <summary>
  /// The player model
  /// </summary>
  public Player player;

  /// <summary>
  /// The move speed of the player
  /// </summary>
  public float moveSpeed = 10;

  /// <summary>
  /// The current renderer managing chunks, if null they're in space probably
  /// </summary>
  public IslandRenderer currentRenderer;

  /// <summary>
  /// The mouselook clamp
  /// </summary>
  public Vector2 clampInDegrees = new Vector2(360, 180);

  /// <summary>
  /// Whether to lock cursor for mouselook
  /// </summary>
  public bool lockCursor;

  /// <summary>
  /// Sensitivity vector for mouselook
  /// </summary>
  public Vector2 sensitivity = new Vector2(2, 2);

  /// <summary>
  /// Smoothing vector for mouselook
  /// </summary>
  public Vector2 smoothing = new Vector2(3, 3);

  /// <summary>
  /// direction the camera is facing
  /// </summary>
  public Vector2 facingDirection;

  /// <summary>
  /// Direction the character is facing.
  /// </summary>
  public Vector2 targetCharacterDirection;

  /// <summary>
  /// The character controller unity component, used for movement.
  /// </summary>
  CharacterController controller;

  /// <summary>
  /// The absolute mouse mosition
  /// </summary>
  Vector2 mouseAbsolute;

  /// <summary>
  /// The smooth mouse position
  /// </summary>
  Vector2 smoothMouse;

  // Use this for initialization
  void Start () {
    controller = GetComponent<CharacterController>();
    // Set target direction to the camera's initial orientation.
    facingDirection = headObject.transform.localRotation.eulerAngles;
    selectBlockOutlineObject.transform.localScale = new Vector3(
      World.BLOCK_SIZE + 0.01f,
      World.BLOCK_SIZE + 0.01f,
      World.BLOCK_SIZE + 0.01f
    );
  }

  // Update is called once per frame
  void Update () {
    move();
    look();
    currentlySelectedBlock();
  }

  /// <summary>
  /// Player movement management
  /// </summary>
  void move() {
    if (Input.GetAxis("Vertical") == 0 && Input.GetAxis("Horizontal") == 0) {
      return;
    }
    Vector3 fwd = headObject.transform.forward * Input.GetAxis("Vertical") * moveSpeed;
    Vector3 rgt = headObject.transform.right * Input.GetAxis("Horizontal") * moveSpeed;
    // get the total vector and check if we're moving
    Vector3 move = fwd + rgt;
    if (move.magnitude > 0) {
      // move character
      controller.SimpleMove(move);
      // if we're on an island we want to check to see if we should render new chunks
      if (currentRenderer != null) {
        player.updateWorldLocation(gameObject.transform.position);
        // if at any point we move onto a boundry of a chunk, check our position for rendering new chunks around us.
        Coordinate localPosition = transform.position.getCoordinate().trimmed;
        if (localPosition.x == 0 
          || localPosition.y == 0 
          || localPosition.z == 0
          || localPosition.x == Chunk.CHUNK_DIAMETER - 1
          || localPosition.y == Chunk.CHUNK_HEIGHT - 1
          || localPosition.z == Chunk.CHUNK_DIAMETER - 1
        ) {
          Chunk newChunk = player.level.chunkAtWorldLocation(player.location);
          if (newChunk != null && newChunk != player.chunk && newChunk.level == player.level) {
            currentRenderer.renderPositionChange(newChunk, player.chunk);
            player.updateChunk(newChunk);
          }
        }
      }
    }
  }

  /// <summary>
  /// Player mouselook management
  /// </summary>
  void look() {
    // Ensure the cursor is always locked when set
    if (lockCursor) {
      Cursor.lockState = CursorLockMode.Locked;
    }

    // Allow the script to clamp based on a desired target value.
    var targetOrientation = Quaternion.Euler(targetCharacterDirection);
    var targetCharacterOrientation = Quaternion.Euler(targetCharacterDirection);

    // Get raw mouse input for a cleaner reading on more sensitive mice.
    var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

    // Scale input against the sensitivity setting and multiply that against the smoothing value.
    mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

    // Interpolate mouse movement over time to apply smoothing delta.
    smoothMouse.x = Mathf.Lerp(smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
    smoothMouse.y = Mathf.Lerp(smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

    // Find the absolute mouse movement value from point zero.
    mouseAbsolute += smoothMouse;

    // Clamp and apply the local x value first, so as not to be affected by world transforms.
    if (clampInDegrees.x < 360) {
      mouseAbsolute.x = Mathf.Clamp(mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);
    }

    // Then clamp and apply the global y value.
    if (clampInDegrees.y < 360) {
      mouseAbsolute.y = Mathf.Clamp(mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);
    }

    // Set the new look rotations
    headObject.transform.localRotation = Quaternion.AngleAxis(-mouseAbsolute.y, targetOrientation * Vector3.right) * targetOrientation;
    var yRotation = Quaternion.AngleAxis(mouseAbsolute.x, headObject.transform.InverseTransformDirection(Vector3.up));
    headObject.transform.localRotation *= yRotation;
    facingDirection = headObject.transform.localRotation.eulerAngles;
  }

  /// <summary>
  /// Hilight the currently viewed block
  /// </summary>
  void currentlySelectedBlock() {
    Ray ray = Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0));
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, 25)) {
      Vector3 hitBlockPosition = hit.point + (hit.normal * -(World.BLOCK_SIZE / 2));
      Coordinate hitCoordinate = Coordinate.fromWorldPosition(hitBlockPosition);
      selectBlockOutlineObject.transform.position = hitCoordinate.blockCenter;
      removeBlockOnClick(hitCoordinate);
    }
  }

  /// <summary>
  /// Remove a block on a button press
  /// </summary>
  /// <param name="hitBlock"></param>
  void removeBlockOnClick(Coordinate hitCoordinate) {
    if (Input.GetMouseButtonDown(0)) {
      ChunkController chunkController = player.level.chunkAtWorldLocation(hitCoordinate).controller;
      if (chunkController != null) {
        player.level.chunkAtWorldLocation(hitCoordinate).controller.destroyBlock(hitCoordinate);
      }
    }
  }
}
