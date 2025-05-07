using System;
using Mathematics;
using UnityEngine;

namespace Player {
    [RequireComponent(typeof(RectTransform))]
    public class ValueTable : MonoBehaviour {
        [SerializeField] private TableRow rowPrefab;
        
        private RectTransform rectTransform;
        private float defaultHeight;
        private TableRow[] rows = {};
        
        private void Awake(){
            rectTransform = ((RectTransform)transform);
            defaultHeight = rectTransform.rect.height;
        }

        public void Generate(int columnIndex, params string[] cellTexts){
            foreach (TableRow row in rows){
                DestroyImmediate(row.gameObject);
            }
            rows = new TableRow[cellTexts.Length];
            if (cellTexts.Length == 0){
                return;
            }
            columnIndex = ColumnModulo(columnIndex);
            float width = rectTransform.rect.width;

            rows[0] = Instantiate(rowPrefab, transform);
            VectorGeometry.SetRectWidth(rows[0].RectTransform, width);
            rows[0].SetCell(columnIndex, cellTexts[0]);
            
            Vector2 positionOffset = new(0, -rows[0].RectTransform.rect.height);
            Vector2 rowPosition = Vector2.zero;
            for (int i = 1; i < rows.Length; i++){
                rowPosition += positionOffset;
                rows[i] = Instantiate(rowPrefab, transform);
                rows[i].RectTransform.anchoredPosition = rowPosition;
                VectorGeometry.SetRectWidth(rows[i].RectTransform, width);
                rows[i].SetCell(columnIndex, cellTexts[i]);
            }
            float minHeight = -(rowPosition+positionOffset).y;
            VectorGeometry.SetRectHeight(rectTransform, Mathf.Max(minHeight, defaultHeight));
        }
        
        public void UpdateColumn<T>(int columnIndex, Func<T, string> formatter, params T[] cellTexts){
            columnIndex = ColumnModulo(columnIndex);
            for (int i = 0; i < rows.Length; i++){
                rows[i].SetCell(columnIndex, formatter.Invoke(cellTexts[i]));
            }
        }
        public void UpdateColumn(int columnIndex, params string[] cellTexts){
            columnIndex = ColumnModulo(columnIndex);
            for (int i = 0; i < rows.Length; i++){
                rows[i].SetCell(columnIndex, cellTexts[i]);
            }
        }

        public void UpdateCell(int columnIndex, int rowIndex, string text){
            columnIndex = ColumnModulo(columnIndex);
            rows[rowIndex].SetCell(columnIndex, text);
        }

        private int ColumnModulo(int index) => (index+rowPrefab.ColumnCount)%rowPrefab.ColumnCount;
    }
}
