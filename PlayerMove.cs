using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{

    // serializefields will show in the inspector even if private
    [SerializeField] private string horizontalInputName; //taking in the input of the horizontal Axis (A,D <- ->) - keybind specified in Unity settings
    [SerializeField] private string verticalInputName; //taking in the input of the vertical Axis (W,S ^ v)
    [SerializeField] private float movementSpeed;
    [SerializeField] private float runningSpeed;
    private float tempSpeed;

    [SerializeField] private float slopeForce;
    [SerializeField] private float slopeForceRayLength;

    public float stamina;
    [SerializeField] private float maxStamina;
    private float StaminaRegenTimer = 0.0f;
    private const float StaminaDecreasePerFrame = 1.0f;
    private const float StaminaIncreasePerFrame = 5.0f;
    private const float StaminaTimeToRegen = 3.0f;




    //initializing the character's controller
    private CharacterController charController;

    //variable to know if the character is jumping at x point in time
    private bool isJumping;
    private bool isRunning;

    [SerializeField] private AnimationCurve jumpFallOff; //Animation of the jump, needed for a smooth jump
    [SerializeField] private float jumpMultiplier; //Jumping force
    [SerializeField] private KeyCode jumpKey; //Jumping key

    private void Awake() //Awake is called ONCE after all objects have been initialized

    {
        charController = GetComponent<CharacterController>(); //Setting the charController to the ingame Player's CharacterController object
        tempSpeed = movementSpeed; //Temporary regular movement speed to go back to after sprinting
        stamina = maxStamina; //Initializing current stamina with the value of the max one
    }

    private void Update() //Updating the Player movement on every frame
    {
        PlayerMovement();
        isRunning = Input.GetKey(KeyCode.LeftShift);

        if (isRunning && !(isJumping))
        {
            stamina = Mathf.Clamp(stamina - (StaminaDecreasePerFrame * Time.deltaTime), 0.0f, maxStamina);
            StaminaRegenTimer = 0.0f;
        }
        else if (stamina < maxStamina)
        {
            if (StaminaRegenTimer >= StaminaTimeToRegen)
                stamina = Mathf.Clamp(stamina + (StaminaIncreasePerFrame * Time.deltaTime), 0.0f, maxStamina);
            else
                StaminaRegenTimer += Time.deltaTime;
        }
    }


    private void PlayerMovement()
    {
        //Setting a numerical value for the horizontal and vertical inputs
        float vertInput = Input.GetAxis(verticalInputName); 
        float horizInput = Input.GetAxis(horizontalInputName);

        //Creating vectors from the Input numerical values
        Vector3 forwardMovement = transform.forward * vertInput;
        Vector3 rightMovement = transform.right * horizInput;

        //checking if running
        if ((Input.GetKey("left shift") || Input.GetKey("right shift")) && stamina > 0)
        {
            movementSpeed = runningSpeed;
        }


        //Movement script
        charController.SimpleMove(Vector3.ClampMagnitude(forwardMovement + rightMovement, 1.0f) * movementSpeed );  //Fixes the diagonal movement -vector elements dont exceed 1

        if ((vertInput != 0 || horizInput != 0) && OnSlope())
        {
            charController.Move(Vector3.down * charController.height / 2 * slopeForce * Time.deltaTime);
        }
        movementSpeed = tempSpeed;  //Back to regular movement speed after sprinting

        //Jumping function
        JumpInput();
    }

    //Returns true if the character is walking on a slope
    private bool OnSlope()
    {
        if (isJumping)
            return false;

        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, charController.height / 2 * slopeForceRayLength))
            if (hit.normal != Vector3.up)
                return true;

        return false;
    }

    private void JumpInput()
    {
        if (Input.GetKeyDown(jumpKey) && !isJumping) 
        {
            isJumping = true;
            StartCoroutine(JumpEvent()); //The execution of a coroutine can be paused at any point using the yield statement
        }
    }

    private IEnumerator JumpEvent()
    {

        charController.slopeLimit = 90.0f;
        float timeInAir = 0.0f;
        do
        {
            float jumpForce = jumpFallOff.Evaluate(timeInAir);
            charController.Move(Vector3.up * jumpForce * jumpMultiplier * Time.deltaTime);
            timeInAir += Time.deltaTime;

            yield return null;
        } while (!charController.isGrounded && charController.collisionFlags != CollisionFlags.Above);

        charController.slopeLimit = 45.0f;
        isJumping = false;

    }
}
