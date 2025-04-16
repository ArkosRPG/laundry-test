using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header(nameof(CameraControls))]
    [SerializeField] private Transform _camera;
    [Range(0, 90)]
    [SerializeField] private int _cameraXMaxUp = 30;
    [Range(0, 90)]
    [SerializeField] private int _cameraXMaxDown = 60;

    [Header(nameof(Movement))]
    [Min(1)]
    [SerializeField] private int _speed = 5;
    [Min(1)]
    [SerializeField] private int _sprint = 12;
    [SerializeField] private CapsuleCollider _capsule;
    [SerializeField] private LayerMask _wallsMask;
    [Min(1)]
    [SerializeField] private int _hitsCount = 2;

    [Header("Items")]
    [Min(1)]
    [SerializeField] private float _pickDistance = 2f;
    [SerializeField] private LayerMask _itemsMask;


    private void Update()
    {
        // TODO: remove debug crutch
        if (Time.time < 0.1f)
            return;

        CameraControls();
        Movement();
        HandleItems();
    }


    private void CameraControls()
    {
        var mouse = Input.mousePositionDelta;

        var player = transform.localEulerAngles;
        var camera = _camera.localEulerAngles;
        if (camera.x > 180f)
            camera.x -= 360f;

        player.y += mouse.x;
        camera.x = Mathf.Clamp(camera.x - mouse.y, -_cameraXMaxUp, _cameraXMaxDown);

        transform.localEulerAngles = player;
        _camera.localEulerAngles = camera;
    }

    // TODO: use new input system or Rewired etc
    private void Movement()
    {
        var input = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
            input.z++;
        if (Input.GetKey(KeyCode.S))
            input.z--;
        if (Input.GetKey(KeyCode.A))
            input.x--;
        if (Input.GetKey(KeyCode.D))
            input.x++;
        var speed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? _sprint : _speed;

        var tf = transform;
        var desiredDistance = Time.deltaTime * speed;
        var desiredLocalMove = input * desiredDistance;
        var desiredMove = tf.forward * desiredLocalMove.z + tf.right * desiredLocalMove.x;
        tf.position += desiredMove;
    }

    private bool HandleItems()
    {
        var tf = _camera.transform;

        // TODO: non-alloc
        if (!Physics.Raycast(tf.position, tf.forward, out var hit, _pickDistance, _itemsMask))
            return false;

        var item = hit.transform.parent.GetComponent<PlaceableItem>();
        if (!item)
            return false;

        item.Pick();
        return false;
    }
}