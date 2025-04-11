using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player {
	public class CalendarPanel : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI speed;
		[SerializeField] private TextMeshProUGUI date;
		
		private Input.CalendarActions input;
		internal Calendar Calendar {private get; set;}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		internal DebugConsole DebugConsole {private get; set;}
#endif
		
		private void Awake(){
			input = new Input().Calendar;
			input.Enable();
			
			InputAction[] speeds = {input.Speed1, input.Speed2, input.Speed3, input.Speed4, input.Speed5};
			for (int i = 0; i < speeds.Length; i++){
				int index = i;
				speeds[i].performed += _ => {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					if (DebugConsole.IsKeyboardBusy){
						return;
					}
#endif
					SetSpeed(index);
				};
			}
			
			input.ChangeSpeed.performed += context => {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				if (DebugConsole.IsKeyboardBusy){
					return;
				}
#endif
				Calendar.ChangeSpeed(Mathf.RoundToInt(context.ReadValue<float>()));
				UpdateSpeed();
			};

			input.Pause.performed += _ => {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				if (DebugConsole.IsKeyboardBusy){
					return;
				}
#endif
				Calendar.TogglePause();
			};
		}
		private void Start(){
			EnableUpdate();
			UpdateDate();
			UpdateSpeed();
		}
		
		public void EnableUpdate(){
			Calendar.OnDayTick.AddListener(UpdateDate);
		}
		public void DisableUpdate(){
			Calendar.OnDayTick.RemoveListener(UpdateDate);
		}
		public void UpdateDate(){
			date.text = Calendar.CurrentDate.ToString();
		}
		
		private void SetSpeed(int index){
			Calendar.SpeedIndex = index;
			UpdateSpeed();
		}
		private void UpdateSpeed(){
			speed.text = $"Speed: {Calendar.SpeedIndex+1}";
		}
	}
}
