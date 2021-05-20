using System;

[Serializable]
public class RecoilPattern
{
    public RecoilData CurrentPattern { get { return Pattern.RecoilPattern[_currentIndex]; }}

    public RecoilSO Pattern;
    
    private int _currentIndex;

    public void ShotFired()
    {
        if (Pattern.RecoilPattern.Length == 0)
        {
            return;
        }

        CurrentPattern.Apply();

        if (CurrentPattern.IsFinished == true)
        {
            CurrentPattern.Reset();
            _currentIndex = _currentIndex == Pattern.RecoilPattern.Length - 1 ? 0 : _currentIndex + 1;
        }
    }

    public void Reset()
    {
        
        if (Pattern.RecoilPattern.Length == 0)
        {
            return;
        }

        CurrentPattern.Reset();
        _currentIndex = 0;
    }
}