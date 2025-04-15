using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private const float MIN_DISTANCE = 0.1f;


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


    private void Update()
    {
        // TODO: remove debug crutch
        if (Time.time < 0.1f)
            return;

        CameraControls();
        Movement();
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
        for (var i = 0; i < _hitsCount; i++)
        {
            var point1 = tf.position + Vector3.up * _capsule.radius;
            var point2 = point1 + Vector3.up * _capsule.height;

            var desiredLocalMove = input * desiredDistance;
            var desiredMove = tf.forward * desiredLocalMove.z + tf.right * desiredLocalMove.x;
            var desiredDirection = desiredMove.normalized;

            // TODO: non-alloc
            if (!Physics.CapsuleCast(point1, point2, _capsule.radius, desiredDirection, out var hit, desiredDistance, _wallsMask))
            {
                tf.position += desiredMove;
                break;
            }

            var moveDistance = hit.distance;
            if (moveDistance < MIN_DISTANCE)
                break;

            moveDistance -= MIN_DISTANCE;
            var move = desiredDirection * moveDistance;
            tf.position += move;

            input = Vector3.Cross(hit.normal, desiredMove.normalized).normalized;
        }
    }
}