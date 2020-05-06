using System;

[Serializable]
public class Timer
{
    private float _goal;

    private float _t = 0;
    private bool _started = false;

    private Action _finished;

    public Timer(float goal, Action finshedCallback)
    {
       _goal = goal;
       _finished = finshedCallback;
    }

    public void Update(float deltaTime)
    {
        if (_started == true)
        {
            _t += deltaTime;
        }

        if (_t >= _goal)
        {
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
        _t = 0;
    }

    public void Pause()
    {
        _started = false;
    }
}
