using UnityEngine;
using UnityEngine.Events;

namespace Simulation {
	public class Calendar : MonoBehaviour {
		private const float NoProgress = 0;
		private const float FullProgress = 1;
		
		[SerializeField] public float[] speedTimeSteps;
		[SerializeField] public int startingSpeed;
		[SerializeField] private Date startDate;
		[SerializeField] private string[] callbackOrder;
		
		private Date currentDate;
		private float speed;
		private float tickProgress;
		
		private int speedIndex;
		
		public Date CurrentDate => currentDate;
		public bool IsPaused {get; private set;}
		
		public int SpeedIndex {
			get => speedIndex;
			set {
				speedIndex = value;
				speed = 1/speedTimeSteps[value];
			}
		}
		public TickEvent OnDayTick {get; private set;}
		public TickEvent OnMonthTick {get; private set;}
		public TickEvent OnYearTick {get; private set;}
		public UnityEvent<bool> OnPauseToggle {get; private set;}
		
		private void Awake(){
			IsPaused = true;
			currentDate = new Date(startDate);
			tickProgress = NoProgress;
			SpeedIndex = startingSpeed;

			CallbackSorter sorter = new(callbackOrder);
			OnDayTick = new TickEvent(sorter);
			OnMonthTick = new TickEvent(sorter);
			OnYearTick = new TickEvent(sorter);
			OnPauseToggle = new UnityEvent<bool>();
		}
		private void Update(){
			if (IsPaused){
				return;
			}
			tickProgress += Time.deltaTime*speed;
			if (tickProgress < FullProgress){
				return;
			}
			tickProgress = NoProgress;
			ToNextDay();
		}
		public void ToNextDay(){
			currentDate.day++;
			if (currentDate.MonthLength() < currentDate.day){
				currentDate.day = 1;
				currentDate.month++;
				if (Date.YearLength() <= currentDate.month){
					currentDate.month = 0;
					currentDate.year++;
					OnYearTick.Invoke();
				}
				OnMonthTick.Invoke();
			}
			OnDayTick.Invoke();
		}

		public void SetDate(Date date){
			currentDate = date;
		}
		
		public void TogglePause(){
			IsPaused = !IsPaused;
			OnPauseToggle.Invoke(IsPaused);
		}
		
		public void ChangeSpeed(int change){
			SpeedIndex = Mathf.Clamp(SpeedIndex+change, 0, speedTimeSteps.Length-1);
		}
	}
}
