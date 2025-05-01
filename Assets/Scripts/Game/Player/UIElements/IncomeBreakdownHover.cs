using System;
using System.Collections.Generic;
using System.Linq;
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
                    AddRows(Player.GoldIncome, "Income", Player.MonthlyGoldChanges, FormatNumber, Add, Compare);
                    break;
                case Resource.Manpower:
                    AddRows(Player.ManpowerIncome, "Reserves", Player.MonthlyManpowerChanges, FormatNumber, Add, Compare);
                    break;
                case Resource.Sailors:
                    AddRows(Player.SailorsIncome, "Reserves", Player.MonthlySailorsChanges, FormatNumber, Add, Compare);
                    break;
                default: return;
            }
            breakdown.Generate(Source, sourceColumn.ToArray());
            breakdown.UpdateColumn(Value, valueColumn.ToArray());
        }
        
        private void AddRows<T>(T income, string nameOfTotal, IReadOnlyList<(T, string, Type)> monthlyChanges, Func<T, string> format, Func<T, T, T> add, Func<T, T, int> compare) where T : new() {
            valueColumn.Add(Bold(format(income)));
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
                SumUpDuplicate(value, source, provinceDuplicates, add);
                provinceTotal = add(provinceTotal, value);
            }
            AddDuplicates(provinceDuplicates, n => Indent(format(n)), compare);
            
            Dictionary<string, (T, int)> otherDuplicates = new();
            valueColumn[provinceTotalValueIndex] = format(provinceTotal);
            for (; i < monthlyChanges.Count; i++){
                (T value, string source, _) = monthlyChanges[i];
                SumUpDuplicate(value, source, otherDuplicates, add);
            }
            AddDuplicates(otherDuplicates, format, compare);
        }
        private static void SumUpDuplicate<T>(T value, string source, Dictionary<string, (T, int)> duplicates, Func<T, T, T> add){
            if (duplicates.TryGetValue(source, out (T total, int count) duplicate)){
                duplicates[source] = (add(duplicate.total, value), duplicate.count+1);
            } else{
                duplicates[source] = (value, 1);
            }
        }
        private void AddDuplicates<T>(Dictionary<string, (T, int)> duplicates, Func<T, string> format, Func<T, T, int> compare){
            KeyValuePair<string, (T total, int count)>[] duplicateArray = duplicates.ToArray();
            Array.Sort(duplicateArray, (pairA, pairB) => compare(pairA.Value.total, pairB.Value.total));
            foreach ((string source, (T total, int count) duplicate) in duplicateArray){
                valueColumn.Add(format(duplicate.total));
                sourceColumn.Add(duplicate.count > 1 ? $"{source} (\u00d7{duplicate.count})" : source);
            }
        }
        
        private static string Bold(string text){
            return $"<b>{text}</b>";
        }
        private static string Indent(string text){
            return $"  {text}";
        }
        private string FormatNumber(float value){
            return Format.FormatLargeNumberWithSign(value, maxNumberCharacters);
        }
        private string FormatNumber(int value){
            return Format.FormatLargeNumberWithSign(value, maxNumberCharacters);
        }
        private static int Compare(float a, float b) => Mathf.CeilToInt(Mathf.Abs(b)-Mathf.Abs(a));
        private static int Compare(int a, int b) => Mathf.Abs(b)-Mathf.Abs(a);
        private static float Add(float a, float b) => a+b;
        private static int Add(int a, int b) => a+b;
    }
}
