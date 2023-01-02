using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Change : MonoBehaviour
{
    public int x, y; // Coordinates of background empty items

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Tagged cubes as "Item" from the GameManager script while creating the Tile and also new created cubes from SpawnBack method

        if (collision.gameObject.CompareTag("Item"))
        {
            collision.gameObject.name = x.ToString() + " , " + y.ToString();
            collision.gameObject.GetComponent<PickUp>().x = x;
            collision.gameObject.GetComponent<PickUp>().y = y;

            // Add new created objects to the all item Dictionary 
            if(!GameManager.AllItem.ContainsKey(new Tuple<int, int>(x, y)))
            {
                GameManager.AllItem.Add(new Tuple<int, int>(x, y), collision.gameObject.GetComponent<PickUp>());
            }
            
        }
    }

}
