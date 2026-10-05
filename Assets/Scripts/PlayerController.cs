using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public VirtualJoystick joystick;

    CharacterController _cc;
    float _verticalSpeed;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector2 input = joystick != null ? joystick.Value : Vector2.zero;
        Vector3 move = new Vector3(input.x, 0f, input.y);

        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, 15f * Time.deltaTime);
        }

        _verticalSpeed = _cc.isGrounded ? -1f : _verticalSpeed + Physics.gravity.y * Time.deltaTime;

        Vector3 velocity = move * moveSpeed;
        velocity.y = _verticalSpeed;
        _cc.Move(velocity * Time.deltaTime);
    }
}