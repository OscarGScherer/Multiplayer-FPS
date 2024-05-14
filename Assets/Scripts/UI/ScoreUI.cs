using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
	public TextMeshProUGUI s1, s2;
	void Update()
	{
		s1.text = Mathf.RoundToInt(CapturePoint.scoreWhite.Value).ToString();
		s2.text = Mathf.RoundToInt(CapturePoint.scoreBlack.Value).ToString();
	}
}
