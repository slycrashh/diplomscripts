using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float movingspeed = 10f;
    private PlayerInputActions _actions;
    private Rigidbody2D _rb;
    private void Awake()
    {
        _actions = new PlayerInputActions();
        _rb = GetComponent<Rigidbody2D>();
        _actions.Enable();
    }
    private Vector2 GetMovementVector()
    {
        Vector2 inputVector = _actions.Player.Move.ReadValue<Vector2>();
        return inputVector;
    }
    private void FixedUpdate()
    {
        Vector2 vector = GetMovementVector();
        vector = vector.normalized;
        _rb.MovePosition(_rb.position + vector *(movingspeed * Time.fixedDeltaTime));
    }
}
