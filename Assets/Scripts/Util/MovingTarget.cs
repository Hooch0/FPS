using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingTarget : MonoBehaviour
{
    public GameObject Point1;
    public GameObject Point2;

    public float MovementTime = 5;
    private Timer _timeToPoint;

    private bool _goingToPoint1;

    private Vector3 _currentStart;
    private Vector3 _currentEnd;


    private void Awake()
    {
        _currentStart = transform.position;
        _currentEnd = Point2.transform.position;
        _timeToPoint = new Timer(MovementTime, OnPointTimerFinished);
        _timeToPoint.Start();
    }

    private void Update()
    {
        _timeToPoint.Update(Time.deltaTime);
        transform.position = Vector3.Lerp(_currentStart, _currentEnd, _timeToPoint.Elapsed / _timeToPoint.Goal);
    }

    private void OnPointTimerFinished()
    {
        _goingToPoint1 = !_goingToPoint1;

        if (_goingToPoint1 == false)
        {
            _currentStart = Point1.transform.position;
            _currentEnd = Point2.transform.position;
        }
        else
        {
            _currentStart = Point2.transform.position;
            _currentEnd = Point1.transform.position;
        }

        _timeToPoint.Restart();
    }
}
