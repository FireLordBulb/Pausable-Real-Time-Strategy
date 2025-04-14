using Simulation;
using UnityEngine;

namespace AI {
	[RequireComponent(typeof(Country))]
	public class AIController : MonoBehaviour {
		[SerializeField] private PeaceAcceptance peaceAcceptance;
		
		private Calendar calendar;
		
		public Country Country {get; private set;}
		
		public void Init(){
			Country = GetComponent<Country>();
			calendar = Country.Map.Calendar;
			enabled = true;
		}
		private void OnEnable(){
			calendar.OnDayTick.AddListener(DayTick);
			calendar.OnMonthTick.AddListener(MonthTick);
			calendar.OnYearTick.AddListener(YearTick);
		}
		private void OnDisable(){
			calendar.OnDayTick.RemoveListener(DayTick);
			calendar.OnMonthTick.RemoveListener(MonthTick);
			calendar.OnYearTick.RemoveListener(YearTick);
		}

		private void DayTick(){
			
		}
		private void MonthTick(){
			
		}
		private void YearTick(){
			
		}

		public int EvaluatePeaceOffer(PeaceTreaty treaty){
			return peaceAcceptance.EvaluatePeaceOffer(treaty);
		}
	}
}
