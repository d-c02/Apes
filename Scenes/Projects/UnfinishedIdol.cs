using Godot;
using System;

public partial class UnfinishedIdol : Project
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		m_MaxWork = 20;
		ConfigureWork();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		BillboardWork();
	}

    public override void OnFinish()
    {
        throw new NotImplementedException();
    }
}
