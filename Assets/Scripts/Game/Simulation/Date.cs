using System;
using UnityEngine.Events;

namespace Simulation {
	[Serializable]
	public struct Date {
		// 0-indexed months means Jan is 0 and Feb is 1.
		private static readonly int February = 1;
		private static readonly (string name, int days)[] Months = {
			("January"  , 31), ("February", 28), ("March"   , 31), ("April"   , 30),
			("May"      , 31), ("June"    , 30), ("July"    , 31), ("August"  , 31),
			("September", 30), ("October" , 31), ("November", 30), ("December", 31)
		};
		
		public int year;
		public int month;
		public int day;

		public readonly UnityEvent OnDayTick;
		public readonly UnityEvent OnMonthTick;
		public readonly UnityEvent OnYearTick;

		public Date(Date date){
			year = date.year;
			month = date.month;
			day = date.day;
			OnDayTick = new UnityEvent();
			OnMonthTick = new UnityEvent();
			OnYearTick = new UnityEvent();
			Validate();
		}
		private void Validate(){
			while (MonthLength() < day){
				day -= MonthLength();
				month++;
				while (Months.Length <= month){
					month -= Months.Length;
					year++;
				}
			}
		}
		
		public void ToNextDay(){
			day++;
			if (MonthLength() < day){
				day = 1;
				month++;
				if (Months.Length <= month){
					month = 0;
					year++;
					OnYearTick.Invoke();
				}
				OnMonthTick.Invoke();
			}
			OnDayTick.Invoke();
		}
		
		public int MonthLength(){
			// Leap year calculation is just a bunch of magic numbers. 97 leap years per 400 years.
			if (month == February && year%4 == 0 && (year%100 != 0 || year%400 == 0)){
				return Months[month].days+1;
			}
			return Months[month].days;
		}
		
		public override string ToString(){
			return $"{day} {Months[month].name} {year}";
		}
	}
}
