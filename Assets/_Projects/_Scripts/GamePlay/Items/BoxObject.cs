using System.Collections;
using System.Collections.Generic;
using _Projects.GamePlay;
using UnityEngine;

public class BoxObject : DisposableObject
{
    public GameObject boxPrefab;
    public GameObject openedBoxPrefab;

    public bool isOpened = false;
    
    public override void ChangeState()
    {
        if (!isOpened)
        {
            boxPrefab.SetActive(false);
            openedBoxPrefab.SetActive(true);
            isOpened = true;
        }
        base.ChangeState();
        
    }

    public override void Recycle()
    {
        if (isOpened)
        {
            boxPrefab.SetActive(true);
            openedBoxPrefab.SetActive(false);
            isOpened = false;
        }
        base.Recycle();
    }
}
