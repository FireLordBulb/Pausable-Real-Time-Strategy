using System;
using TMPro;
using UnityEngine;

public class CalendarPanel : MonoBehaviour {
	[SerializeField] private TextMeshProUGUI speed;
	[SerializeField] private TextMeshProUGUI date;
	
	private void Start(){
		Calendar.Instance.OnDayTick.AddListener(() => {
			date.text = Calendar.Instance.Date;
		});
		UpdateSpeed();
	}
	public void UpdateSpeed(){
		speed.text = $"Speed: {Calendar.Instance.SpeedIndex+1}";
	}
}
