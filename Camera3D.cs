using Godot;
using System;

public partial class Camera3D : Godot.Camera3D
{
	private float rotationX = 0f;
	private float rotationY = 0f;
	private float mouseSens = 0.001f;
	private float moveSpeed = 0.02f;

	private bool isSwimming = true;

	private MeshInstance3D player;
	private RigidBody3D playerRigid;

	private CollisionShape3D playercoll;

	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		playerRigid = GetParent<RigidBody3D>();
		player = playerRigid.GetChild<MeshInstance3D>(0);

		Gravity();
		Resistance();
	}

	public override void _Process(double delta)
	{
		Movement();
	}

	public override void _PhysicsProcess(double delta)
	{
		Collision();
	}

	private void Collision()
	{
		foreach (Node3D node in playerRigid.GetCollidingBodies())
		{
			if (node.Name == "Floor")
			{
				playerRigid.ApplyCentralForce(new Vector3(0f, playerRigid.GravityScale * 9.8f, 0f));
			}
		}
	}

	private void Resistance()
	{
		playerRigid.LinearDamp = 2.0f;
	}

	private void Gravity()
	{
		playerRigid.GravityScale = 0.5f;
		if (isSwimming)
		{
			playerRigid.GravityScale = 0f;
		}
	}

	private void Movement()
	{
		//Swimming Movement
		Vector3 input = Vector3.Zero;
		input.X = Input.GetAxis("move_left", "move_right");
		input.Z = Input.GetAxis("move_forward", "move_backwards");
		input.Y = Input.GetAxis("move_down", "move_up");

		//Rotate the vector based upon the facing of the player
		if (isSwimming)
		{
			input = input.Rotated(new Vector3(1f, 0f, 0f), rotationY);
		}
		else
		{
			input = input * new Vector3(1f, 0f, 1f);
		}
		input = input.Rotated(new Vector3(0f, 1f, 0f), rotationX);

		playerRigid.ApplyCentralImpulse(moveSpeed * input);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			// modify accumulated mouse rotation
			rotationX -= mouseMotion.Relative.X * mouseSens;
			rotationY -= mouseMotion.Relative.Y * mouseSens;

			// Stop deformation
			Transform3D transform = Transform;
			transform.Basis = Basis.Identity;
			Transform = transform;

			RotateObjectLocal(new Vector3(0, 1, 0), rotationX); // first rotate about Y

			if (rotationY <= Mathf.Pi / 2.0f && rotationY > -Mathf.Pi / 2.0f)
			{
				RotateObjectLocal(new Vector3(1, 0, 0), rotationY); // then rotate about X
			}
			else
			{
				rotationY = Mathf.Sign(rotationY) * Mathf.Pi / 2.0f;
				RotateObjectLocal(new Vector3(1, 0, 0), Mathf.Sign(rotationY) * Mathf.Pi / 2.0f);
			}
		}

		if (@event is InputEvent input)
		{
			//This activates twice for press and release of the key
			if (input.IsActionPressed("swim_toggle"))
			{
				isSwimming = !isSwimming;
				Gravity();
			}
		}
	}
}
