using System;
using System.Collections.Generic;
using System.Linq;
using AI;
using Simulation;
using Text;
using UnityEngine;

namespace Player {
    [RequireComponent(typeof(RectTransform))]
    public class PeaceAcceptanceHover : HoverPanel<ValueTable> {
        private const int Value = 0, Source = 1;
        
        [SerializeField] private int maxNumberCharacters;
        
        private ValueTable breakdown;
        private readonly List<string> valueColumn = new();
        private readonly List<string> sourceColumn = new();
        
        public Country Player {private get; set;}
        public AIController AI {private get; set;}
        
        protected override void InitPanel(ValueTable valueTable){
            breakdown = valueTable;
            Refresh();
        }
        public void Refresh(){
            if (breakdown == null){
                return;
            }
            valueColumn.Clear();
            sourceColumn.Clear();
            foreach ((int value, string reason) in AI.PeaceAcceptanceReasons){
                valueColumn.Add(FormatAndColor(value));
                sourceColumn.Add(reason);
            }
            breakdown.Generate(Source, sourceColumn.ToArray());
            breakdown.UpdateColumn(Value, valueColumn.ToArray());
        }
        
        private string FormatAndColor(int value){
            string formattedNumber = Format.FormatLargeNumberWithSign(value, maxNumberCharacters);
            if (value > 0){
                return $"<color=green>{formattedNumber}</color>";
            }
            if (value < 0){
                return $"<color=red>{formattedNumber}</color>";
            }
            return formattedNumber;
        }
    }
}
