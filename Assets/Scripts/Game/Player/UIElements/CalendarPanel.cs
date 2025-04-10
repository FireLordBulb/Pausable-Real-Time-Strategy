using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player {
	public class CalendarPanel : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI speed;
		[SerializeField] private TextMeshProUGUI date;
		
		private Input.CalendarActions input;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		public DebugConsole DebugConsole {private get; set;}
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
				Calendar.Instance.ChangeSpeed(Mathf.RoundToInt(context.ReadValue<float>()));
				UpdateSpeed();
			};

			input.Pause.performed += _ => {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				if (DebugConsole.IsKeyboardBusy){
					return;
				}
#endif
				Calendar.Instance.TogglePause();
			};
		}
		private void Start(){
			EnableUpdate();
			UpdateDate();
			UpdateSpeed();
		}
		
		public void EnableUpdate(){
			Calendar.Instance.OnDayTick.AddListener(UpdateDate);
		}
		public void DisableUpdate(){
			Calendar.Instance.OnDayTick.RemoveListener(UpdateDate);
		}
		public void UpdateDate(){
			date.text = Calendar.Instance.CurrentDate.ToString();
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
