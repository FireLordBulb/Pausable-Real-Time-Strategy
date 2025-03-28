using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player {
	public class CalendarPanel : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI speed;
		[SerializeField] private TextMeshProUGUI date;
		
		private Input.CalendarActions input;
		
		private void Awake(){
			input = new Input().Calendar;
			input.Enable();
			
			InputAction[] speeds = {input.Speed1, input.Speed2, input.Speed3, input.Speed4, input.Speed5};
			for (int i = 0; i < speeds.Length; i++){
				int index = i;
				speeds[i].performed += _ => SetSpeed(index);
			}
			
			input.ChangeSpeed.performed += context => {
				Calendar.Instance.ChangeSpeed(Mathf.RoundToInt(context.ReadValue<float>()));
				UpdateSpeed();
			};

			input.Pause.performed += _ => Calendar.Instance.TogglePause();
		}
		private void Start(){
			Calendar.Instance.OnDayTick.AddListener(UpdateDate);
			UpdateDate();
			UpdateSpeed();
		}
		
		private void UpdateDate(){
			date.text = Calendar.Instance.Date;
		}
		private void SetSpeed(int index){
			Calendar.Instance.SpeedIndex = index;
			UpdateSpeed();
		}
		private void UpdateSpeed(){
			speed.text = $"Speed: {Calendar.Instance.SpeedIndex+1}";
		}
	}
}
