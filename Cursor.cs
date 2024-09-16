using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    public Transform mCursorVisual;
    public Vector3 mDisplacement;
    public GameObject cursoryo;
    void Start()
    {
        cursoryo.gameObject.SetActive(true);
        // this sets the base cursor as invisible
        Cursor.visible = false;
    }

    void Update()
    {
        mCursorVisual.position = Input.mousePosition + mDisplacement;

    }
}