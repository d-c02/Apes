using Godot;
using System;

public partial class UnfinishedIdol : Project
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		m_MaxWork = 10;
		m_NumWorkPerRow = 5;
        ConfigureWork();

        AddWork((int)WorkAspects.Fervor, 2);
        AddWork((int)WorkAspects.Influence, 2);
        AddWork((int)WorkAspects.Insight, 2);
        AddWork((int)WorkAspects.Fervor, 3);
        RemoveWork(10);
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
