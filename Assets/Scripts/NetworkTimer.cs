using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NetworkTimer
{
	private float timer;
	public int currentTick { get; private set;}
	public float minTimeBetweenTicks {get;}
	
	public NetworkTimer(float tickRate)
	{
		minTimeBetweenTicks = 1f / tickRate;
	}
	
	public void Update(float deltaTime)
	{
		timer += deltaTime;
	}
	
	public bool ShouldTick()
	{
		if(timer >= minTimeBetweenTicks)
		{
			timer -= minTimeBetweenTicks;
			currentTick++;
			return true;
		}
		return false;
	}
	
}
