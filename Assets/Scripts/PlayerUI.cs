using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
	public PlayerController playerController;
	public RectTransform healthbarfill;

	void Awake()
	{
		playerController.health.OnValueChanged += UpdateHealth;
	}
	void UpdateHealth(float previous, float current)
	{
		healthbarfill.sizeDelta = new Vector2(current/playerController.maxHealth * 302, healthbarfill.sizeDelta.y);
	}
}
