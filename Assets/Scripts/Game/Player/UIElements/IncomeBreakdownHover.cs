using System.Collections.Generic;
using Simulation;
using Text;
using UnityEngine;

namespace Player {
    [RequireComponent(typeof(RectTransform))]
    public class IncomeBreakdownHover : HoverPanel<ValueTable> {
        private const int Value = 0, Source = 1;
        
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
                breakdown.Generate(0, "N/A");
                return;
            }
            List<string>[] cellText = {new(), new()};
            foreach ((float value, string source) in Player.MonthlyGoldChanges){
                cellText[Value].Add(Format.FormatLargeNumber(value, maxNumberCharacters));
                cellText[Source].Add(source);
            }
            breakdown.Generate(Source, cellText[1].ToArray());
            breakdown.UpdateColumn(Value, cellText[Value].ToArray());
        }
    }
}