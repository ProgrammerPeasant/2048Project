using UnityEngine; 
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private GameField gameField;
    private InputAction moveAction;
    private Vector2 swipeStart;
    private bool isSwiping = false;
    public float swipeThreshold = 50f;

    private void OnEnable()
    {
        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/s")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/a")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d")
            .With("Right", "<Keyboard>/rightArrow");
        moveAction.performed += OnMove;
        moveAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.performed -= OnMove;
        moveAction.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        if (input.magnitude < 0.5f) return;

        Vector2Int direction;
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            direction = input.x > 0 ? Vector2Int.right : Vector2Int.left;
        else
            direction = input.y > 0 ? Vector2Int.up : Vector2Int.down;
        Debug.Log("keyboard or arrow move: " + direction);
        gameField.Move(direction);
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            swipeStart = Mouse.current.position.ReadValue();
            isSwiping = true;
        }

        if (isSwiping && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Vector2 swipeEnd = Mouse.current.position.ReadValue();
            Vector2 swipeDelta = swipeEnd - swipeStart;
            if (swipeDelta.magnitude >= swipeThreshold)
            {
                Vector2Int direction;
                if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
                    direction = swipeDelta.x > 0 ? Vector2Int.right : Vector2Int.left;
                else
                    direction = swipeDelta.y > 0 ? Vector2Int.up : Vector2Int.down;
                Debug.Log("mouse swipe move: " + direction);
                gameField.Move(direction);
            }

            isSwiping = false;
        }

        if (Touchscreen.current != null)
        {
            if (Touchscreen.current.primaryTouch.press.isPressed)
            {
                if (!isSwiping)
                {
                    swipeStart = Touchscreen.current.primaryTouch.position.ReadValue();
                    isSwiping = true;
                }
            }

            if (isSwiping && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
            {
                Vector2 swipeEnd = Touchscreen.current.primaryTouch.position.ReadValue();
                Vector2 swipeDelta = swipeEnd - swipeStart;
                if (swipeDelta.magnitude >= swipeThreshold)
                {
                    Vector2Int direction;
                    if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
                        direction = swipeDelta.x > 0 ? Vector2Int.right : Vector2Int.left;
                    else
                        direction = swipeDelta.y > 0 ? Vector2Int.up : Vector2Int.down;
                    Debug.Log("touch swipe move: " + direction);
                    gameField.Move(direction);
                }

                isSwiping = false;
            }
        }
    }
}