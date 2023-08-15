using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Match3_Evo
{
    public class AnimationEventTrigger : MonoBehaviour
    {
        public List<AnimationEventData> animationEvent;

        void TrigerEvent(string _eventID)
        {
            animationEvent.Find(x => x.EventID == _eventID).Event.Invoke();
        }
    }

    [System.Serializable]
    public class AnimationEventData
    {
        public string EventID;
        public UnityEvent Event;
    }
}