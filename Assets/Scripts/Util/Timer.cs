using System;

[Serializable]
public class Timer
{
    public float Elapsed { get; private set; }

    public float Goal { get; private set; }

    private bool _started = false;

    private Action _finished;

    public Timer(float goal, Action finshedCallback)
    {
       Goal = goal;
       _finished = finshedCallback;
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
        if (_started == true)
        {
            Elapsed += deltaTime;
        }

        if (Elapsed >= Goal)
        {
            Pause();
            Elapsed = Goal;
            _finished?.Invoke();
        }
    }

    public void Start()
    {
        _started = true;
    }

    public void Stop()
    {
        _started = false;
        Elapsed = 0;
    }

    public void Pause()
    {
        _started = false;
    }
}
