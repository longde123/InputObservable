﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using InputObservable;

public class TestMain : MonoBehaviour
{
    public DrawTargetView draw;
    public UICanvasView ui;

    CompositeDisposable disposables = new CompositeDisposable();

    void log(string msg)
    {
        Debug.Log(msg);
    }

    void clear()
    {
        disposables.Clear();
    }

    void TraceBasic(IInputObservable io)
    {
        io.OnBegin.Subscribe(e =>
        {
            log(e.ToString());
            draw.Put(e, Color.red);
        }).AddTo(disposables);

        io.OnMove.Subscribe(e =>
        {
            log(e.ToString());
            draw.Put(e, Color.black);
        }).AddTo(disposables);

        io.OnEnd.Subscribe(e =>
        {
            log(e.ToString());
            draw.Put(e, Color.yellow);
        }).AddTo(disposables);
    }

    void TraceLumpPoints(IInputObservable io)
    {
        io.Any().Buffer(io.OnEnd).Subscribe(es =>
        {
            log($"<color=red>Seqeuence {es.Count} points</color>");
            log($"  {es[0]}");
            if (es.Count > 2)
            {
                log($"  {string.Join(",", es.Select((e, i) => (e, i)).Where((e, i) => 1 <= i && i < es.Count - 1).Select(x => x.e.position.ToString()))}");
            }
            log($"  {es[es.Count - 1]}");
            foreach (var e in es)
            {
                switch (e.type)
                {
                    case InputEventType.Begin:
                        draw.Put(e, Color.red);
                        break;
                    case InputEventType.Move:
                        draw.Put(e, Color.black);
                        break;
                    case InputEventType.End:
                        draw.Put(e, Color.yellow);
                        break;
                }
            }
        }).AddTo(disposables);
    }

    void TraceDoubleTap(IInputObservable io)
    {
        io.DoubleSequence(250).Subscribe(e =>
        {
            var msg = $"<color=blue>double click/tap ({e})</color>";
            log(msg);
            draw.Put(e, Color.blue);
        }).AddTo(disposables);
    }

    void TraceLongPress(IInputObservable io)
    {
        io.LongSequence(500).Subscribe(e =>
        {
            log($"<color=green>long press at {e}</color>");
            draw.Put(e, Color.green);
        }).AddTo(disposables);
    }

    void TraceDragAndDrop(IInputObservable io)
    {
        io.LongSequence(500).Subscribe(begin =>
        {
            log($"<color=green>Drag begin at. {begin}</color>");
            draw.DragBegin(begin, Color.green, begin.ToString());

            io.MoveThrottle(100).TakeUntil(io.OnEnd).Subscribe(drag =>
            {
                log($"Dragging. {drag}");
                draw.Dragging(drag, Color.green, drag.ToString());
            }).AddTo(disposables);

            io.OnEnd.First().Subscribe(drop =>
            {
                log($"<color=green>Drop at. {drop}</color>");
                draw.DragEnd(drop, Color.green, drop.ToString());
            }).AddTo(disposables);
        }).AddTo(disposables);
    }

    void TraceLastVerocity(IInputObservable io, int count)
    {
        io.Any().Subscribe(e =>
        {
            draw.Put(e, Color.blue);
        }).AddTo(disposables);

        io.TakeLastVerocities(count).Subscribe(vs =>
        {
            log(string.Join(", ", vs.Select(vi => vi.ToString())));
            foreach (var v in vs)
            {
                draw.Put(v, Color.magenta);
            }
        }).AddTo(disposables);
    }

    void TraceAllVerocity(IInputObservable io)
    {
        io.Any().Subscribe(e =>
        {
            Debug.Log(e);
            draw.Put(e, Color.blue);
        }).AddTo(disposables);

        io.Verocity().Subscribe(v =>
        {
            Debug.Log($">> {v}");
            draw.Put(v, Color.magenta);
        }).AddTo(disposables);
    }

    void Start()
    {
        var io = this.DefaultInputContext().GetObservable(0);

        ui.SelectedToggle.Subscribe(type =>
        {
            log(type);
            clear();

            if (type == "Basic")
                TraceBasic(io);
            else if (type == "Lump")
                TraceLumpPoints(io);
            else if (type == "Double")
                TraceDoubleTap(io);
            else if (type == "LongPress")
                TraceLongPress(io);
            else if (type == "DragAndDrop")
                TraceDragAndDrop(io);
            else if (type == "Last4V")
                TraceLastVerocity(io, 4);
            else if (type == "Last16V")
                TraceLastVerocity(io, 16);
            else if (type == "Verocity")
                TraceAllVerocity(io);
            else
                Debug.LogError($"not found: {type}");
        }).AddTo(this);
    }
}
