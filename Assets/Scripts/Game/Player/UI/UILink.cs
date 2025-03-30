using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Player {
    public class UILink : MonoBehaviour, IPointerClickHandler {
        private Action action;

        public void Link(Action clickAction){
            action = clickAction;
        }
        public void OnPointerClick(PointerEventData eventData){
            if (eventData.button == PointerEventData.InputButton.Left){
                action();
            }
        }
    }
}