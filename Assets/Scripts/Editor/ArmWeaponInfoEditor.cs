using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArmWeaponInfoSO))]
public class ArmWeaponInfoEditor : Editor
{
    private Transform _armsHolder;


    public override void OnInspectorGUI()
    {
        GUI.enabled = false;
        DrawDefaultInspector();
        GUI.enabled = true;

        _armsHolder = (Transform)EditorGUILayout.ObjectField("Arms Holder", _armsHolder, typeof(Transform), true);

        if (GUILayout.Button("Save"))
        {
            ArmWeaponInfoSO awi = target as ArmWeaponInfoSO;

            Transform left = null;
            Transform right = null;

            Transform _lUpperArm = null;
            Transform _lForearm = null;
            Transform _lHand = null;

            Transform _rUpperArm = null;
            Transform _rForearm = null;
            Transform _rHand = null;

            foreach(Transform trans in _armsHolder)
            {
                if (trans.CompareTag("LeftArm"))
                {
                    left = trans;
                }
                if (trans.CompareTag("RightArm"))
                {
                    right = trans;
                }
            }

            foreach(Transform trans in left)
            {
                if (trans.CompareTag("UpperArm"))
                {
                    _lUpperArm = trans;
                }
                if (trans.CompareTag("Forearm"))
                {
                    _lForearm = trans;
                }
                if (trans.CompareTag("Hand"))
                {
                    _lHand = trans;
                }
            }

            foreach(Transform trans in right)
            {
                if (trans.CompareTag("UpperArm"))
                {
                    _rUpperArm = trans;
                }
                if (trans.CompareTag("Forearm"))
                {
                    _rForearm = trans;
                }
                if (trans.CompareTag("Hand"))
                {
                    _rHand = trans;
                }
            }

            SetInfo(awi.LeftArm, _lUpperArm, _lForearm, _lHand);
            SetInfo(awi.RightArm, _rUpperArm, _rForearm, _rHand);



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
