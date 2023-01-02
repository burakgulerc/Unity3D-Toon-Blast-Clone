using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDName : MonoBehaviour
{
    public int ID;

    public enum CubeType
    {
        Default,
        Rocket,
        Bomb,
        Discoball
    }

    public CubeType type;  // shapes
    public Sprite[] sprites;  // colors
    public bool isRunningChangeSprites;

    //Change sprite shapes
    void Update()
    {
        if (isRunningChangeSprites)
        {
            switch (type)
            {
                case CubeType.Default:
                    GetComponent<SpriteRenderer>().sprite = sprites[0];
                    break;
                case CubeType.Rocket:
                    GetComponent<SpriteRenderer>().sprite = sprites[1];
                    break;
                case CubeType.Bomb:
                    GetComponent<SpriteRenderer>().sprite = sprites[2];
                    break;
                case CubeType.Discoball:
                    GetComponent<SpriteRenderer>().sprite = sprites[3];
                    break;
                default:
                    break;
            }
        }
        
    }
}
