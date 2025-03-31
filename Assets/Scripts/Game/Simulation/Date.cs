using System;

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

		public Date(Date date) : this(date.year, date.month, date.day){}
		public Date(int startingYear, int startingMonth, int startingDay){
			year = startingYear;
			month = startingMonth;
			day = startingDay;
			Validate();
		}
		private void Validate(){
			if (day < 1){
				do {
					month--;
					while (month < 0){
						year--;
						month += Months.Length;
					}
					day += MonthLength();
				} while (day < 1);
				return;
			}
			while (MonthLength() < day){
				day -= MonthLength();
				month++;
				while (Months.Length <= month){
					month -= Months.Length;
					year++;
				}
			}
		}
		
		// YearLength is always the same amount of months so it's static, MonthLength depends on the current month so it's not static.
		public static int YearLength(){
			return Months.Length;
		}
		public int MonthLength(){
			// Leap year calculation is just a bunch of magic numbers. 97 leap years per 400 years.
			if (month == February && year%4 == 0 && (year%100 != 0 || year%400 == 0)){
				return Months[month].days+1;
			}
			return Months[month].days;
		}
		
		public override bool Equals(object obj){
			return obj is Date other && Equals(other);
		}
		public bool Equals(Date other){
			return this == other;
		}
		public static bool operator==(Date left, Date right){
			return left.year == right.year && left.month == right.month && left.day == right.day;
		}
		public static bool operator!=(Date left, Date right){
			return !(left == right);
		}
		public static bool operator>(Date left, Date right){
			return right < left;
		}
		public static bool operator<(Date left, Date right){
			return left.year < right.year || left.year == right.year && (left.month < right.month || left.month == right.month && left.day < right.day);
		}
		
		public override int GetHashCode(){
			return HashCode.Combine(year, month, day);
		}
		public override string ToString(){
			return $"{day} {Months[month].name} {year}";
		}
	}
}
