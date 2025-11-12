using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;


// MoveCube manages cube movement. WASD + Cursor keys rotate the cube in the
// selected direction. If the cube is not grounded (has a tile under it), it falls.
// Some events trigger corresponding sounds.


public class MoveCube : MonoBehaviour
{
  private InputAction _moveAction; 		// Input action to capture player movement (WASD + cursor keys)

  private bool _bMoving;  // Is the object in the middle of moving?
  private bool _bFalling; // Is the object falling?
    
	public float rotSpeed; 			// Rotation speed in degrees per second
    public float fallSpeed; 		// Fall speed in the Y direction

    private Vector3 _rotPoint, _rotAxis; // Rotation movement is performed around the line formed by rotPoint and rotAxis
    private float _rotRemainder;         // The angle that the cube still has to rotate before the current movement is completed
    private float _rotDir;               // Has rotRemainder to be applied in the positive or negative direction?
    private LayerMask _layerMask;        // LayerMask to detect raycast hits with ground tiles only

    public AudioClip[] sounds; 		// Sounds to play when the cube rotates
    public AudioClip fallSound; 	// Sound to play when the cube starts falling
	
	
	// Determine if the cube is grounded by shooting a ray down from the cube location and 
	// looking for hits with ground tiles

  private bool IsGrounded()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.0f, _layerMask))
            return true;

        return false;
    }

    // Start is called once after the MonoBehaviour is created
    private void Start()
    {
		// Find the move action by name. Done once in the Start method to avoid doing it every Update call.
        _moveAction = InputSystem.actions.FindAction("Move");
		
		// Create the layer mask for ground tiles. Done once in the Start method to avoid doing it every Update call.
        _layerMask = LayerMask.GetMask("Ground");
    }

    // Update is called once per frame
    private void Update()
    {
        if(_bFalling)
        {
			// If we have fallen, we just move down
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
        }
        else if (_bMoving)
        {
			// If we are moving, we rotate around the line formed by rotPoint and rotAxis an angle depending on deltaTime
			// If this angle is larger than the remainder, we stop the movement
            float amount = rotSpeed * Time.deltaTime;
            if(amount > _rotRemainder)
            {
                transform.RotateAround(_rotPoint, _rotAxis, _rotRemainder * _rotDir);
                _bMoving = false;
            }
            else
            {
                transform.RotateAround(_rotPoint, _rotAxis, amount * _rotDir);
                _rotRemainder -= amount;
            }
        }
        else
        {
			// If we are not falling, nor moving, we check first if we should fall, then if we have to move
            if (!IsGrounded())
            {
                _bFalling = true;
				
				// Play sound associated to falling
                AudioSource.PlayClipAtPoint(fallSound, transform.position, 1.5f);
            }
			
			// Read the move action for input
            Vector2 dir = _moveAction.ReadValue<Vector2>();
            if(Math.Abs(dir.x) > 0.99 || Math.Abs(dir.y) > 0.99)
            {
				// If the absolute value of one of the axis is larger than 0.99, the player wants to move in a non diagonal direction
                _bMoving = true;
				
				// We play a random movemnt sound
                int iSound = Random.Range(0, sounds.Length);
                AudioSource.PlayClipAtPoint(sounds[iSound], transform.position, 1.0f);
				
				// Set rotDir, rotRemainder, rotPoint, and rotAxis according to the movement the player wants to make
                if (dir.x > 0.99)
                {
                    _rotDir = -1.0f;
                    _rotRemainder = 90.0f;
                    _rotAxis = new Vector3(0.0f, 0.0f, 1.0f);
                    _rotPoint = transform.position + new Vector3(0.5f, -0.5f, 0.0f);
                }
                else if (dir.x < -0.99)
                {
                    _rotDir = 1.0f;
                    _rotRemainder = 90.0f;
                    _rotAxis = new Vector3(0.0f, 0.0f, 1.0f);
                    _rotPoint = transform.position + new Vector3(-0.5f, -0.5f, 0.0f);
                }
                else if (dir.y > 0.99)
                {
                    _rotDir = 1.0f;
                    _rotRemainder = 90.0f;
                    _rotAxis = new Vector3(1.0f, 0.0f, 0.0f);
                    _rotPoint = transform.position + new Vector3(0.0f, -0.5f, 0.5f);
                }
                else if (dir.y < -0.99)
                {
                    _rotDir = -1.0f;
                    _rotRemainder = 90.0f;
                    _rotAxis = new Vector3(1.0f, 0.0f, 0.0f);
                    _rotPoint = transform.position + new Vector3(0.0f, -0.5f, -0.5f);
                }
            }
        }
    }

}
