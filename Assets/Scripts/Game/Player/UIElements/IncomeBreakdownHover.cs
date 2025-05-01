using System;
using System.Collections.Generic;
using Simulation;
using Text;
using UnityEngine;

namespace Player {
    [RequireComponent(typeof(RectTransform))]
    public class IncomeBreakdownHover : HoverPanel<ValueTable> {
        private const int Value = 0, Source = 1;
        
        [SerializeField] private Resource resource;
        [SerializeField] private int maxNumberCharacters;
        
        private ValueTable breakdown;
        private readonly List<string> valueColumn = new();
        private readonly List<string> sourceColumn = new();
        
        public Country Player {private get; set;}
        
        protected override void InitPanel(ValueTable valueTable){
            breakdown = valueTable;
            Refresh();
        }
        public void Refresh(){
            if (breakdown == null){
                return;
            }
            if (Player == null){
                breakdown.Generate(Value, Bold("N/A"));
                breakdown.UpdateColumn(Source, "");
                return;
            }
            valueColumn.Clear();
            sourceColumn.Clear();
            switch (resource){
                case Resource.Gold:
                    AddRows(Player.GoldIncome, "Income", Player.MonthlyGoldChanges, FormatNumber, Add);
                    break;
                case Resource.Manpower:
                    AddRows(Player.ManpowerIncome, "Reserves", Player.MonthlyManpowerChanges, FormatNumber, Add);
                    break;
                case Resource.Sailors:
                    AddRows(Player.SailorsIncome, "Reserves", Player.MonthlySailorsChanges, FormatNumber, Add);
                    break;
                default: return;
            }
            breakdown.Generate(Source, sourceColumn.ToArray());
            breakdown.UpdateColumn(Value, valueColumn.ToArray());
        }
        
        private void AddRows<T>(T income, string nameOfTotal, IReadOnlyList<(T, string, Type)> monthlyChanges, Func<T, string> formatter, Func<T, T, T> adder) where T : new() {
            valueColumn.Add(Bold(formatter(income)));
            sourceColumn.Add(Bold($"Net Monthly {nameOfTotal}"));
            valueColumn.Add("-----------------------------------");
            sourceColumn.Add("");
            int provinceTotalValueIndex = valueColumn.Count;
            T provinceTotal = new();
            valueColumn.Add("[province_total]");
            sourceColumn.Add("From Provinces");
            int i = 0;
            Dictionary<string, (T, int)> provinceDuplicates = new();
            for (; i < monthlyChanges.Count; i++){
                (T value, string source, Type type) = monthlyChanges[i];
                if (type != typeof(Land)){
                    break;
                }
                SumUpDuplicate(value, source, provinceDuplicates, adder);
                provinceTotal = adder(provinceTotal, value);
            }
            AddDuplicates(provinceDuplicates, n => Indent(formatter(n)));
            
            Dictionary<string, (T, int)> otherDuplicates = new();
            valueColumn[provinceTotalValueIndex] = formatter(provinceTotal);
            for (; i < monthlyChanges.Count; i++){
                (T value, string source, _) = monthlyChanges[i];
                SumUpDuplicate(value, source, otherDuplicates, adder);
            }
            AddDuplicates(otherDuplicates, formatter);
        }
        private static void SumUpDuplicate<T>(T value, string source, Dictionary<string, (T, int)> duplicates, Func<T, T, T> adder){
            if (duplicates.TryGetValue(source, out (T total, int count) duplicate)){
                duplicates[source] = (adder(duplicate.total, value), duplicate.count+1);
            } else{
                duplicates[source] = (value, 1);
            }
        }
        private void AddDuplicates<T>(Dictionary<string, (T, int)> duplicates, Func<T, string> formatter){
            foreach ((string source, (T total, int count) duplicate) in duplicates){
                valueColumn.Add(formatter(duplicate.total));
                sourceColumn.Add($"{source}{(duplicate.count > 1 ? $" (\u00d7{duplicate.count})" : "")}");
            }
        }
        
        private string Bold(string text){
            return $"<b>{text}</b>";
        }
        private string Indent(string text){
            return $"  {text}";
        }
        private string FormatNumber(float value){
            return Format.FormatLargeNumberWithSign(value, maxNumberCharacters);
        }
        private string FormatNumber(int value){
            return Format.FormatLargeNumberWithSign(value, maxNumberCharacters);
        }
        private static int Add(int a, int b) => a+b;
        private static float Add(float a, float b) => a+b;
    }
}
