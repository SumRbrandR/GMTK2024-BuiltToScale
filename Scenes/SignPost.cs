using Godot;
using System;

public partial class SignPost : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_body_entered(Node2D node)
	{
        var ht = GetTree().Root.GetNode<HowTo>("LevelField/HowTo");
		ht.appear = true;
	}

	public void _on_body_exited(Node2D node)
	{
        var ht = GetTree().Root.GetNode<HowTo>("LevelField/HowTo");
		ht.appear = false;
	}
}
