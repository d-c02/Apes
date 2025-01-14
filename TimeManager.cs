using Godot;
using System;

public partial class TimeManager : Node3D
{
	private int m_Time = 0;
	private int m_MaxTime = 5;
	[Export] private DirectionalLight3D m_Sun;
	[Export] private DirectionalLight3D m_Moon;
	[Export] private WorldEnvironment m_WorldEnvironment;

	[Export] private Gradient m_SunColor;
    [Export] private Curve m_SunIntensity;

	[Export] private Gradient m_SkyTopColor;
    [Export] private Gradient m_SkyHorizonColor;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		UpdateSky();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void UpdateSky()
	{
		switch (m_Time)
		{
			case 0:
				//Morning
				GlobalRotation = new Vector3(Mathf.DegToRad(-80), 0, 0);

				m_Moon.Visible = false;
                m_Sun.Visible = true;
				m_Moon.LightEnergy = 0.0f;

				break;

			case 1:
                GlobalRotation = new Vector3(Mathf.DegToRad(-30), 0, 0);

                break;

			case 2:
                GlobalRotation = new Vector3(Mathf.DegToRad(30), 0, 0);
                break;

			case 3:
                GlobalRotation = new Vector3(Mathf.DegToRad(80), 0, 0);
                break;

			case 4:
                GlobalRotation = new Vector3(Mathf.DegToRad(180), 0, 0);

                m_Sun.Visible = false;
                m_Moon.Visible = true;
				m_Moon.LightEnergy = 1.0f;
                
				break;
		}

		float curTime = (float) m_Time / (m_MaxTime - 1);
		m_Sun.LightColor = m_SunColor.Sample(curTime);
		m_Sun.LightEnergy = m_SunIntensity.Sample(curTime);
		m_WorldEnvironment.Environment.Sky.SkyMaterial.Set("sky_top_color", m_SkyTopColor.Sample(curTime));
        m_WorldEnvironment.Environment.Sky.SkyMaterial.Set("sky_horizon_color", m_SkyHorizonColor.Sample(curTime));
        m_WorldEnvironment.Environment.Sky.SkyMaterial.Set("ground_bottom_color", m_SkyTopColor.Sample(curTime));
        m_WorldEnvironment.Environment.Sky.SkyMaterial.Set("ground_horizon_color", m_SkyHorizonColor.Sample(curTime));
    }

	public void IncrementTime()
	{
		m_Time++;
		if (m_Time >= m_MaxTime)
		{
			m_Time = 0;
		}

		UpdateSky();
	}
}