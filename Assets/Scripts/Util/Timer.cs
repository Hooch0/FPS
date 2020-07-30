using System;
using UnityEngine;

[Serializable]
public class Timer
{
    public float Elapsed { get; private set; }

    public float Goal { get; private set; }

    public bool IsRunning { get; private set; }

    public bool IsFinished { get { return Elapsed >= Goal; } }

    private Action _finished;

    public Timer(float goal, Action finshedCallback)
    {
       Goal = goal;
       _finished += finshedCallback;
    }

    public void SetGoal(float goal)
    {
        Goal = goal;
    }

    public void SetElapsedTime(float elapsed)
    {
        Elapsed = elapsed;
    }

    public void Update(float deltaTime)
    {
        if (IsRunning == true)
        {
            Elapsed += deltaTime;

            if (Elapsed >= Goal)
            {
                Pause();
                Elapsed = Goal;
                _finished?.Invoke();
            }
        }
    }

    public void Start()
    {
        IsRunning = true;
    }

    public void Stop()
    {
        IsRunning = false;
        Elapsed = 0;
    }

    public void Restart()
    {
        Elapsed = 0;
        IsRunning = true;
    }

    public void Pause()
    {
        IsRunning = false;
    }
}
