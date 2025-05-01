using UnityEngine;

namespace Player {
    [RequireComponent(typeof(RectTransform))]
    public class IncomeBreakdownHover : HoverPanel<ValueTable> {
        protected override void InitPanel(ValueTable valueTable){
            valueTable.Generate(-1, "From Provinces");
        }
    }
}