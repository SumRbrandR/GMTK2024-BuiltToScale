using System;
using Godot;

public partial class player_char : RigidBody2D
{
	Vector2 vel = new Vector2();
	float airspd = 3500f;
	float grndspd = 5500f;
	float maxSpd = 500f;
	float maxjump = -1000f;
	float minjump = -600f;
	double ticker = 0;
	int tickerMax = 10;
	double tickRate = 0;
	int curFrame = 0;
	AnimatedSprite2D mySprite;
	Node2D holding = null;
	bool isHolding = false;
	Node2D pigArm;
	Camera2D cam;

	public override void _Ready()
	{
		vel = new Vector2();
		mySprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		cam = GetTree().Root.GetNode<Camera2D>("LevelField/Camera");
	}

    public override void _PhysicsProcess(double delta)
    {
		var spd = airspd;

		// Move camera
		if (cam.GlobalPosition.Y > GlobalPosition.Y)
			cam.GlobalPosition = new Vector2(cam.GlobalPosition.X, GlobalPosition.Y);

		// Vertical movement dampening
		var linvel = LinearVelocity;
		if (Input.IsActionJustReleased("Jump"))
		{
			linvel.Y = Math.Max(linvel.Y, minjump);
		}

		// Horizontal movement dampening
		linvel.X = Math.Clamp(linvel.X, -maxSpd, maxSpd);
		linvel.X /= 1.25f;
		LinearVelocity = linvel;

		// Jump movement
		if (MoveAndCollide(new Vector2(0,1), true) != null)
		{
			spd = grndspd;
			if (Input.IsActionJustPressed("Jump"))
				ApplyImpulse(new Vector2(0,maxjump), new Vector2(Position.X, Position.Y + 10));
		}

		// Horizontal movement
		if (Input.IsActionPressed("Right"))
		{
			if (holding == null) mySprite.FlipH = false;
			ApplyForce(new Vector2(spd,0));
		}
		if (Input.IsActionPressed("Left"))
		{
			if (holding == null) mySprite.FlipH = true;
			ApplyForce(new Vector2(-spd,0));
		}

		// Get animation tick rate
		tickRate = Math.Abs(linvel.X)/100;

		// Handle animation
		if (MoveAndCollide(new Vector2(0,1), true) == null)
			curFrame = 1;
		else if (Math.Abs(linvel.X) > 0.5)
		{
			if (ticker >= tickerMax)
			{
				ticker = 0;
				curFrame += 1;
				curFrame %= 4;
			}
			else
				ticker+=tickRate;
		}
		else
			curFrame = 0;
		mySprite.Frame = curFrame;

		// Clamp position
		var pos = Position;
		pos.X = Math.Clamp(pos.X,0,1920);
		Position = pos;

		// Test spawn object
		if (Input.IsActionJustPressed("Interact") && holding == null)
			SpawnObject("res://Scenes/PhysicsCardObjects/crate.tscn");

		// Holding object
		if (holding != null)
		{
			mySprite.Animation = "carry";

			// Handle pig arm position
			pigArm.GlobalPosition = GlobalPosition;

			// Get direction to mouse
			var dir = GlobalPosition.DirectionTo(GetGlobalMousePosition());
			var mag = (float)holding.GetMeta("HoldOffset");

			// Set object held position
			if (GetGlobalMousePosition().DistanceTo(GlobalPosition) < mag)
				holding.GlobalPosition = GetGlobalMousePosition();
			else
				holding.GlobalPosition = new Vector2(
					GlobalPosition.X + dir.X * mag,
					GlobalPosition.Y + dir.Y * mag
				);

			// Set facing sprite
			if (GetGlobalMousePosition().X > GlobalPosition.X)
			{
				mySprite.FlipH = false;
				pigArm.GetNode<Sprite2D>("PigArm").FlipH = false;
				pigArm.LookAt(GetGlobalMousePosition());
			}
			if (GetGlobalMousePosition().X < GlobalPosition.X)
			{
				mySprite.FlipH = true;
				pigArm.GetNode<Sprite2D>("PigArm").FlipH = true;
				pigArm.LookAt(GetGlobalMousePosition());
				pigArm.Rotation += (float)Math.PI;
			}

			// Rotate held object
			if (Input.IsActionPressed("RotateLeft"))
				holding.Rotate((float)(-2/(180/Math.PI)));
			if (Input.IsActionPressed("RotateRight"))
				holding.Rotate((float)(2/(180/Math.PI)));

			// Place held object
			if (Input.IsActionJustPressed("Interact") && isHolding == true)
			{
				var rigid = holding.GetNode<RigidBody2D>("RigidBody2D");
				rigid.SetCollisionMaskValue(3, true);
				if (rigid.MoveAndCollide(new Vector2(0,.1f), true) == null &&
					rigid.MoveAndCollide(new Vector2(0,-.1f), true) == null &&
					rigid.MoveAndCollide(new Vector2(.1f,0), true) == null && 
					rigid.MoveAndCollide(new Vector2(-.1f,0), true) == null &&
					rigid.MoveAndCollide(new Vector2(.1f,.1f), true) == null &&
					rigid.MoveAndCollide(new Vector2(.1f,-.1f), true) == null &&
					rigid.MoveAndCollide(new Vector2(-.1f,-.1f), true) == null && 
					rigid.MoveAndCollide(new Vector2(-.1f,.1f), true) == null)
				{
					rigid.Freeze = false;
					isHolding = false;
					holding = null;
					pigArm.QueueFree();
				}
				rigid.SetCollisionMaskValue(3, false);
			}
			else
				isHolding = true;
		}
		else
		{
			mySprite.Animation = "normal";
		}
	}

	public void SpawnObject(string path)
	{
		var ps = GD.Load<PackedScene>(path);
		var inst = ps.Instantiate<Node2D>();
		GetTree().Root.AddChild(inst);
		var rigid = inst.GetNode<RigidBody2D>("RigidBody2D");
		rigid.SetCollisionLayerValue(1, false);
		rigid.SetCollisionMaskValue(2, false);
		rigid.Freeze = true;
		holding = inst;
		
		ps = GD.Load<PackedScene>("res://Scenes/pig_hold_arm.tscn");
		inst = ps.Instantiate<Node2D>();
		GetTree().Root.AddChild(inst);
		pigArm = inst;
	}
}
