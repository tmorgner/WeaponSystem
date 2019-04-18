using System;
using UnityEngine;

namespace UnityTools
{
  [ExecuteInEditMode]
  [AddComponentMenu("Unity Tools/Prefab/BlenderFix")]
  public class BlenderFbxFix : MonoBehaviour
  {
    public GameObject Prefab;
    public Vector3 Translate;
    public Vector3 Rotation;
    public Vector3 Scale;
    public bool HideInHierarchy;
    GameObject instance;

    void Reset()
    {
      Scale = Vector3.one;
      Rotation = new Vector3(-90, 0, 0);
    }

    void LogDebug(string message)
    {

    }

    void OnEnable()
    {
      if (Prefab != null)
      {
        if (!ReferenceEquals(instance, null))
        {
          return;
        }

        LogDebug(string.Format("Instantiate prefab for FbxFix at {0}", this.gameObject.name));

        instance = Instantiate(Prefab);
        instance.layer = this.gameObject.layer;
        instance.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
        if (HideInHierarchy)
        {
          instance.hideFlags |= HideFlags.HideInHierarchy;
        }
        instance.transform.SetParent(this.transform);
        instance.transform.localPosition = Translate;
        instance.transform.localScale = Scale;
        var rot = instance.transform.localRotation;
        rot.eulerAngles = Rotation;
        instance.transform.localRotation = rot;
      }
      else
      {
        LogDebug(string.Format("No prefab for FbxFix at {0}", this.gameObject.name));
      }

    }

    void OnDisable()
    {
      if (Application.isEditor && !ReferenceEquals(instance, null))
      {
        DestroyImmediate(instance);
        instance = null;
      }
    }

    void OnDestroy()
    {
      if (!ReferenceEquals(instance, null))
      {
        if (Application.isEditor)
        {
          DestroyImmediate(instance);
        }
        else
        {
          Destroy(instance);
        }

        instance = null;
      }
    }
  }
}
