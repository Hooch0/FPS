using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugOverlay : MonoBehaviour
{
    public static DebugOverlay Instance { get; private set; }

    private Dictionary<string, VectorLabel> _customVectorLabel = new Dictionary<string, VectorLabel>();
    private Dictionary<string, BoolLabel> _customBoolLabel = new Dictionary<string, BoolLabel>();
    private Dictionary<string, StringLabel> _customStringLabel = new Dictionary<string, StringLabel>();


    private float _distanceTraveled = 0;
    private Vector3 _lastPo;

    private Timer _distanceTimer;
    private Timer _frameUpdate;

    private float _maxDistance;
    private float _fps;
    private float _maxFPS;
    private float _minFPS = 99999999;

    private bool _display = true;

    private void Awake()
    {
        _distanceTimer = new Timer(1, () => 
        { 
            _lastPo = PlayerController.Instance.transform.position;
            _distanceTimer.Restart();
        });

        _frameUpdate = new Timer(1, () => { });

        _frameUpdate.Start();

        _distanceTimer.Start();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            Debug.Log("here");
            _display = !_display;
        }

        FPS();
        PlayerDistance();
        ChangeVectorValue("Player Rotation",PlayerController.Instance.GetRotation().eulerAngles);
    }

    private void OnEnable()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void FPS()
    {

        _frameUpdate.Update(Time.deltaTime);


        if (_frameUpdate.Elapsed >= _frameUpdate.Goal)
        {
            _fps = 1 / Time.deltaTime;
            _frameUpdate.Restart();

            if (_fps > _maxFPS)
            {
                _maxFPS = _fps;
            }

            if (_fps < _minFPS)
            {
                _minFPS = _fps;
            }
        }

        
    }

    private void PlayerDistance()
    {
        _distanceTimer.Update(Time.deltaTime);
        _distanceTraveled = (PlayerController.Instance.transform.position - _lastPo).magnitude;

        if (_distanceTraveled > _maxDistance)
        {
            _maxDistance = _distanceTraveled;
        }

    }

    private void OnGUI()
    {
        if (_display == false)
        {
            return;
        }

        GUILayout.BeginVertical(GUI.skin.box,GUILayout.Width(250));//(new Rect(0,0,150,500));

        GUILayout.Label("----------DEBUG----------");
        GUI.color = Color.green;
        GUILayout.Label("FPS: " + Mathf.RoundToInt(_fps).ToString());
        GUILayout.Label("Max FPS: " + Mathf.RoundToInt(_maxFPS).ToString());
        GUILayout.Label("Min FPS: " + (_minFPS == 99999999 ? "0" : Mathf.RoundToInt(_minFPS).ToString()));


        GUI.color = Color.white;
        GUILayout.Label("Distance Traveled: " + _distanceTraveled.ToString());
        GUILayout.Label("Max Distance Traveled: " + _maxDistance.ToString());
        if (GUILayout.Button("Refil Current Weapon Ammo"))
        {
            PlayerController.Instance.inventory.Ammo.AddAmmo(PlayerController.Instance.inventory.CurrentWeapon.Data.AmmoType
            ,PlayerController.Instance.inventory.Ammo.GetAmmo(PlayerController.Instance.inventory.CurrentWeapon.Data.AmmoType).MaxAmmo);
        }

        foreach(string reff in _customBoolLabel.Keys)
        {
            GUILayout.Label(_customBoolLabel[reff].ToString());
        }

        foreach(string reff in _customStringLabel.Keys)
        {
            GUILayout.Label(_customStringLabel[reff].ToString());
        }

        foreach(string reff in _customVectorLabel.Keys)
        {
            GUILayout.Label(_customVectorLabel[reff].ToString());
        }

        GUILayout.EndVertical();
    }

    public void ChangeVectorValue(string reff, Vector3 val)
    {
        if (_customVectorLabel.ContainsKey(reff) == false)
        {
            _customVectorLabel.Add(reff, new VectorLabel(reff,reff,val));
            return;
        }

        _customVectorLabel[reff].ChangeValue(val);
    }

    public void ChangeBoolValue(string reff, bool val)
    {
        if (_customBoolLabel.ContainsKey(reff) == false)
        {
            _customBoolLabel.Add(reff, new BoolLabel(reff,reff,val));
            return;
        }

        _customBoolLabel[reff].ChangeValue(val);
    }

    public void ChangeStringValue(string reff, string val)
    {
        if (_customStringLabel.ContainsKey(reff) == false)
        {
            _customStringLabel.Add(reff, new StringLabel(reff,reff,val));
            return;
        }

        _customStringLabel[reff].ChangeValue(val);
    }
}



public class VectorLabel 
{
    public string Key { get; protected set; }
    public string Label { get; protected set; }
    public Vector3 Value { get; protected set; }


    public VectorLabel(string key, string label, Vector3 vec)
    {
        Key = key;
        Label = label;
        Value = vec;
    }

    public void ChangeValue(Vector3 value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Label.ToString() + ": " +"\n[x: " + Value.x.ToString() + "]\n[y: " + Value.y.ToString() + "]\n[z: " + Value.z.ToString() + "]";
    }
}

public class BoolLabel 
{
    public string Key { get; protected set; }
    public string Label { get; protected set; }
    public bool Value { get; protected set; }


    public BoolLabel(string key, string label, bool value)
    {
        Key = key;
        Label = label;
        Value = value;
    }

    public void ChangeValue(bool value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Label.ToString() + ": " + Value.ToString();
    }
}

public class StringLabel 
{
    public string Key { get; protected set; }
    public string Label { get; protected set; }
    public string Value { get; protected set; }


    public StringLabel(string key, string label, string value)
    {
        Key = key;
        Label = label;
        Value = value;
    }

    public void ChangeValue(string value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Label.ToString() + ": " + Value.ToString();
    }
}

