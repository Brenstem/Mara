using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Timer {
    private float _time;
    private float _duration;

    public float Time {
        get { return _time; }
        set {
            if (value >= _time)
                _time = value;
        }
    }

    public float Duration {
        get { return _duration; }
    }

    public bool Expired() {
        return _time >= _duration;
    }

    public void Reset() {
        _time = 0;
    }

    public float Ratio() {
        return _time / _duration;
    }

    // Constructors
    public Timer(float duration, float time = 0) {
        if (duration <= 0)
            throw new Exception("Duration can not be 0 or less");
        else
            _duration = duration;
        if (time < 0)
            throw new Exception("Time can not be less than 0");
        else
            _time = time;
    }

    public Timer(Timer timer) {
        _time = timer._time;
        _duration = timer._duration;
<<<<<<< HEAD:Assets/Assets/Player/Prototyping/Timer.cs
=======
    }

    public override string ToString() {
        string s = "Time: " + _time + ", Time left: " + (Duration - _time) + ", Ratio: " + Ratio();
        return s;
>>>>>>> movement:Assets/Assets/Scripts/Timer.cs
    }
}
