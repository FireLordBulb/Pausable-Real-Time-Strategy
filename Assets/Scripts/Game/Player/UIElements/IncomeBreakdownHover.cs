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
            List<string> valueColumn = new();
            List<string> sourceColumn = new();
            switch (resource){
                case Resource.Gold:
                    valueColumn.Add(Bold(FormatNumber(Player.GoldIncome)));
                    sourceColumn.Add(Bold("Net Monthly Income"));
                    AddRows(valueColumn, sourceColumn, Player.MonthlyGoldChanges, FormatNumber, (a, b) => a+b);
                    break;
                case Resource.Manpower:
                    valueColumn.Add(Bold(FormatNumber(Player.ManpowerIncome)));
                    sourceColumn.Add(Bold("Net Monthly Reserves"));
                    AddRows(valueColumn, sourceColumn, Player.MonthlyManpowerChanges, FormatNumber, (a, b) => a+b);
                    break;
                case Resource.Sailors:
                    valueColumn.Add(Bold(FormatNumber(Player.SailorsIncome)));
                    sourceColumn.Add(Bold("Net Monthly Reserves"));
                    AddRows(valueColumn, sourceColumn, Player.MonthlySailorsChanges, FormatNumber, (a, b) => a+b);
                    break;
                default: return;
            }
            breakdown.Generate(Source, sourceColumn.ToArray());
            breakdown.UpdateColumn(Value, valueColumn.ToArray());
        }
        
        private void AddRows<T>(List<string> valueColumn, List<string> sourceColumn, IReadOnlyList<(T, string, Type)> monthlyChanges, Func<T, string> formatter, Func<T, T, T> adder) where T : struct {
            valueColumn.Add("-------------------------------");
            sourceColumn.Add("");
            int provinceTotalValueIndex = valueColumn.Count;
            T provinceTotal = new();
            valueColumn.Add("[province_total]");
            sourceColumn.Add("From Provinces");
            int i = 0;
            for (; i < monthlyChanges.Count; i++){
                (T value, string source, Type type) = monthlyChanges[i];
                if (type != typeof(Land)){
                    i--;
                    break;
                }
                valueColumn.Add(Indent(formatter(value)));
                sourceColumn.Add(source);
                provinceTotal = adder(provinceTotal, value);
            }
            valueColumn[provinceTotalValueIndex] = formatter(provinceTotal);
            for (; i < monthlyChanges.Count; i++){
                (T value, string source, _) = monthlyChanges[i];
                valueColumn.Add(formatter(value));
                sourceColumn.Add(source);
            }
        }

        private string Bold(string text){
            return $"<b>{text}</b>";
        }
        private string Indent(string text){
            return $"    {text}";
        }
        private string FormatNumber(float value){
            return Format.FormatLargeNumberWithSign(value, maxNumberCharacters);
        }
        private string FormatNumber(int value){
            return Format.FormatLargeNumberWithSign(value, maxNumberCharacters);
        }
    }
}