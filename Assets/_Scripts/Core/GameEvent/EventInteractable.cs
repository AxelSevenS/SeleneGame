using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SeleneGame.Core {

    public class EventInteractable : MonoBehaviour, IInteractable{
        
        public List<GameEvent> interactEvents;
        [SerializeField] private string interactionText;


        public string InteractDescription {
            get {
                return interactionText;
            }
            set {
                interactionText = value;
            }
        }
        public bool IsInteractable {
            get {
                return true;
            }
            set {;}
        }
        

        public void Interact(Entity entity){
            foreach (GameEvent interactEvent in interactEvents){
                if (interactEvent.Evaluate())
                    interactEvent.Invoke(gameObject);
            }
        }
    }
}