using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;

namespace InputObservable
{
    public class TouchInputObservable : InputObservableBase
    {
        int index;
        EventSystem eventSystem;

        protected override void Update()
        {
            if (Input.touchCount == 0)
            {
                return;
            }
            var touch = Input.GetTouch(this.index);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    {
                        if (eventSystem!=null && eventSystem.IsPointerOverGameObject(touch.fingerId))
                        {
                            return;
                        }
                        begin = true;
                        beginPos = touch.position;
                        var e = new InputEvent()
                        {
                            id = this.id,
                            type = InputEventType.Begin,
                            position = touch.position
                        };
                        beginStream.OnNext(e);
                    }
                    break;
                case TouchPhase.Moved:
                    {
                        if (!begin)
                            return;
                        var e = new InputEvent()
                        {
                            id = this.id,
                            type = InputEventType.Move,
                            position = touch.position
                        };
                        moveStream.OnNext(e);
                    }
                    break;
                case TouchPhase.Ended:
                    {
                        if (!begin)
                            return;
                        begin = false;
                        var e = new InputEvent()
                        {
                            id = this.id,
                            type = InputEventType.End,
                            position = touch.position
                        };
                        endStream.OnNext(e);
                        this.id++;
                    }
                    break;
                default:
                    break;
            }
        }

        public TouchInputObservable(MonoBehaviour behaviour, int index, EventSystem eventSystem) : base(behaviour)
        {
            this.index = index;
            this.eventSystem = eventSystem;
        }
    }
}
