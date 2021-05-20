using System;

[Serializable]
public class RecoilData
{
    public float XAmount;
    public float YAmount;

    public int BulletsTillFinish;

    public bool IsFinished { get { return _currentBullet == BulletsTillFinish; } }


    private int _currentBullet;

    public void Reset()
    {
        _currentBullet = 0;
    }

    public void Apply()
    {
        _currentBullet++;
    }
}
