using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class MapTransfer : MonoBehaviour, IMouseInputHandler
{
    [SerializeField]
    private string targetSceneName;

    public int targetPointID;

    public bool canSend;
    public bool canReceive;

    // public Dropdown test;

    // private void Awake()
    // {
    //     Debug.Log("hi");

    //     test.ClearOptions();
    //     test.AddOptions(EditorBuildSettings.scenes
    //         .Where(s => s.enabled)
    //         .Select(s => s.path)
    //         .ToList());

    //     Debug.Log(EditorBuildSettings.scenes
    //         .Where(s => s.enabled)
    //         .Select(s => s.path)
    //         .ToList());
    // }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetMouseButtonDown(0) && canSend)
        {
            GameManager.Instance.ChangeScene(targetSceneName, targetPointID);
        }
    }
}