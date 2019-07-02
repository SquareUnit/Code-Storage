/// Designed by Félix Desrosiers-Dorval
/// Last modification date : 2019-07-01
/// Last feature added : 
/// https://github.com/SquareUnit/Code-Storage

/// Used to change a or multiple materials of an mesh renderer. Extremely clean.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshRenderer))]
public class SwapMaterial : MonoBehaviour
{
    public enum meshToAffect { top, dissolve, bottom}
    public meshToAffect meshSelected;
    public enum matToSwap { mat00, mat01, mat02, mat03, mat04, mat05}
    public matToSwap matSelected;

    public List<Material> matList = new List<Material>();

    public void SetMatToMesh(Material mat)
    {
        if (meshSelected == meshToAffect.dissolve || meshSelected == meshToAffect.bottom)
        {
            Material[] mats = GetComponent<MeshRenderer>().sharedMaterials;
            mats[(int)meshSelected] = mat;
            GetComponent<MeshRenderer>().sharedMaterials = mats;
        }
        else Debug.Log("Change enum value please, the top mesh should not be changed.");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SwapMaterial))]
public class MultiMaterialEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SwapMaterial swapMaterial = (SwapMaterial)target;

        if (GUILayout.Button("Swap for : " + swapMaterial.matSelected.ToString()))
        {
            swapMaterial.SetMatToMesh(swapMaterial.matList[(int)swapMaterial.matSelected]);
        }
    }
}
#endif


