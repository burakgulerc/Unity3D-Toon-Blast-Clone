using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PickUp : MonoBehaviour
{
    public int x, y; //coordinates of pickups

    //When clicked on cube find same cubes as neighbors and destroy
    void OnMouseDown()
    {
        GameManager.CalculateCubeCallBack(GetComponent<PickUp>(), GetComponent<IDName>());
        FindObjectOfType<GameManager>().DeleteCallBack();
    }

    // for breaking cubes
    public void ContinueCallBack()
    {
        GameManager.CalculateCubeCallBack(GetComponent<PickUp>(),GetComponent<IDName>());
    }

    // for changing sprites
    public void ContinueCallBack(Tuple<int,int> id)
    {
        GameManager.CalculateCubeCallBack(GetComponent<PickUp>(), GetComponent<IDName>(),id);
    }

}
