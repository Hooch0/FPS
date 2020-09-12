using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArmWeaponInfoSO))]
public class ArmWeaponInfoEditor : Editor
{
    private Transform _lUpperArm;
    private Transform _lForearm;

    private Transform _lHand;

    private Transform _rUpperArm;
    private Transform _rForearm;

    private Transform _rHand;


    public override void OnInspectorGUI()
    {
        GUI.enabled = false;
        DrawDefaultInspector();
        GUI.enabled = true;

        _lUpperArm = (Transform)EditorGUILayout.ObjectField("Left Upper Arm", _lUpperArm, typeof(Transform), true);
        _lForearm = (Transform)EditorGUILayout.ObjectField("Left Forearm", _lForearm, typeof(Transform), true);
        _lHand = (Transform)EditorGUILayout.ObjectField("Left Hand", _lHand, typeof(Transform), true);

        _rUpperArm = (Transform)EditorGUILayout.ObjectField("Right Upper Arm", _rUpperArm, typeof(Transform), true);
        _rForearm = (Transform)EditorGUILayout.ObjectField("Right Forearm", _rForearm, typeof(Transform), true);
        _rHand = (Transform)EditorGUILayout.ObjectField("Right Hand", _rHand, typeof(Transform), true);

        if (GUILayout.Button("Save"))
        {
            ArmWeaponInfoSO awi = target as ArmWeaponInfoSO;

            SetInfo(awi.LeftArm, _lUpperArm, _lForearm, _lHand);
            SetInfo(awi.RightArm, _rUpperArm, _rForearm, _rHand);

            _lUpperArm = null;
            _lForearm = null;
            _lHand = null;

            _rUpperArm = null;
            _rForearm = null;
            _rHand = null;

            EditorUtility.SetDirty(target);
        }
    }

    private void SetInfo(ArmSOInfo info, Transform upperArm, Transform forearm, Transform hand)
    {

        if (info == null || upperArm == null || forearm == null || hand == null)
        {
            return;
        }

        info.UpperArmPosition = upperArm.localPosition;
        info.UpperArmRotation = upperArm.localRotation;

        info.ForearmPosition = forearm.localPosition;
        info.ForearmRotation = forearm.localRotation;

        info.HandPosition = hand.localPosition;
        info.HandRotation = hand.localRotation;
    }
}
