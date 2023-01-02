using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int row;
    public int column;
    [SerializeField] int randomColorCount;
    [SerializeField] int paddingTop;
    [SerializeField] GameObject[] normalCubePrefabs;
    [SerializeField] Transform background, cube;   

    public static Dictionary<Tuple<int, int>, PickUp> AllItem = new Dictionary<Tuple<int, int>, PickUp>();
    
    static List<GameObject> deletedObjects = new List<GameObject>();  // used for clicked and neighbor objects which will be deleted objects 

    public static Dictionary<Tuple<int, int>, List<GameObject>> RelatedNeighbors = new Dictionary<Tuple<int, int>, List<GameObject>>();

    Vector2 backgroundColliderSize = new Vector2(0.5f, 0.5f);


    void Awake()
    {
        SpawnFillGrid();

        
        StartCoroutine(Wait(0.1f, () => {
            CheckGroupsForIconChange();
        }));
    }

    //Instantiate the cubes and background items
    void SpawnFillGrid()
    {
        // Setup Cubes
        for (int x = 0; x < row; x++) // Generate cubes in horizontal axis
        {
            for (int y = 0; y < column; y++) // Generate cubes in vertical axis 
            {
                var clone =Instantiate(normalCubePrefabs[UnityEngine.Random.Range(0, randomColorCount)], new Vector2(x,y), Quaternion.identity);
                clone.transform.SetParent(cube);
                clone.AddComponent<CapsuleCollider2D>(); 
                
                clone.tag = "Item";

                clone.AddComponent<PickUp>();
                clone.name = x.ToString() + " , "  + y.ToString();
                clone.GetComponent<PickUp>().x = x;
                clone.GetComponent<PickUp>().y = y;

                AllItem.Add(new Tuple<int,int>(x,y),clone.GetComponent<PickUp>());

                clone.GetComponent<IDName>().isRunningChangeSprites = true;
            }
        }

        //Setup Backgorund

        for (int x = 0; x < row; x++)
        {
            for (int y = 0; y < column; y++)
            {
                var clone = Instantiate(normalCubePrefabs[UnityEngine.Random.Range(0, normalCubePrefabs.Length)], new Vector2(x, y), Quaternion.identity);
                clone.transform.SetParent(background);
                Destroy(clone.GetComponent<SpriteRenderer>());
                clone.AddComponent<BoxCollider2D>();
                clone.GetComponent<BoxCollider2D>().isTrigger = true;  
                clone.GetComponent<BoxCollider2D>().enabled = false;
                
                clone.AddComponent<Change>();
                clone.name = x.ToString() + " , " + y.ToString();
                clone.GetComponent<Change>().x = x;
                clone.GetComponent<Change>().y = y;
                clone.GetComponent<Rigidbody2D>().gravityScale = 0f;
                clone.GetComponent<BoxCollider2D>().size = backgroundColliderSize;
            }
        }

        // Setup Background invisible item position
        Invoke("ChangeBackgroundPosition",0.5f); 
    }

    // Re-arrange the background empty items
    void ChangeBackgroundPosition()
    {
        for (int i = 0; i < background.childCount; i++)
        {
            var _change = background.GetChild(i).GetComponent<Change>();
            background.GetChild(i).transform.position = AllItem[new Tuple<int, int>(_change.x, _change.y)].transform.position;

        }
    }


    void CheckGroupsForIconChange()
    {
        foreach (var item in AllItem.Values)
        {
            RelatedNeighbors.Add(new Tuple<int, int>(item.x, item.y), new List<GameObject>());

            CalculateCubeCallBack(AllItem[new Tuple<int, int>(item.x, item.y)], AllItem[new Tuple<int, int>(item.x, item.y)].GetComponent<IDName>(), new Tuple<int, int>(item.x, item.y));
        }
        //Sprite change
        foreach (var item in RelatedNeighbors.Values)
        {
            if (item.Count == 1 || item.Count == 2 || item.Count == 3 || item.Count == 4)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    item[i].GetComponent<IDName>().type = IDName.CubeType.Default;
                }
            }
            else if(item.Count == 5 || item.Count == 6)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    item[i].GetComponent<IDName>().type = IDName.CubeType.Rocket;
                }
            }
            else if (item.Count == 7 || item.Count == 8)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    item[i].GetComponent<IDName>().type = IDName.CubeType.Bomb;
                }
            }
            else if (item.Count >= 9)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    item[i].GetComponent<IDName>().type = IDName.CubeType.Discoball;
                }
            }

        }
        // Clear square
        RelatedNeighbors.Clear();
    }

    IEnumerator Wait(float time, Action call)
    {
        if (call != null)
        {
            call.Invoke();
        }
        yield return new WaitForSeconds(time);
    }

    //When game starts and after every move find the same colored neighbors for changing sprite type
    public static void CalculateCubeCallBack(PickUp pickUp, IDName id,Tuple<int,int> tupleID)
    {
        // Neighbors
        var top = new Tuple<int, int>(pickUp.x, pickUp.y + 1);
        var down = new Tuple<int, int>(pickUp.x, pickUp.y - 1);
        var right = new Tuple<int, int>(pickUp.x + 1, pickUp.y);
        var left = new Tuple<int, int>(pickUp.x - 1, pickUp.y);

        //Control top
        if (AllItem.ContainsKey(top))
        {
            if(id.ID == AllItem[top].GetComponent<IDName>().ID)
            {
                // Check clicked pickup object first in the deleted objects List, if it is not in the list add
                if (!RelatedNeighbors[tupleID].Contains(id.gameObject)) RelatedNeighbors[tupleID].Add(id.gameObject);

                // Check neighbor pickup object in the deleted objects list, if it is not in the list add
                if (!RelatedNeighbors[tupleID].Contains(AllItem[top].gameObject)) 
                {
                    RelatedNeighbors[tupleID].Add(AllItem[top].gameObject);
                    AllItem[top].ContinueCallBack(tupleID);  // Continue for the neighbor's next neighbor
                }
                
            }
        }
        //Control down
        if (AllItem.ContainsKey(down))
        {
            if (id.ID == AllItem[down].GetComponent<IDName>().ID)
            {
                if (!RelatedNeighbors[tupleID].Contains(id.gameObject)) RelatedNeighbors[tupleID].Add(id.gameObject);
                if (!RelatedNeighbors[tupleID].Contains(AllItem[down].gameObject))
                {
                    RelatedNeighbors[tupleID].Add(AllItem[down].gameObject);
                    AllItem[down].ContinueCallBack(tupleID);
                }
            }
        }
        //Control right
        if (AllItem.ContainsKey(right))
        {
            if (id.ID == AllItem[right].GetComponent<IDName>().ID)
            {
                if (!RelatedNeighbors[tupleID].Contains(id.gameObject)) RelatedNeighbors[tupleID].Add(id.gameObject);
                if (!RelatedNeighbors[tupleID].Contains(AllItem[right].gameObject))
                {
                    RelatedNeighbors[tupleID].Add(AllItem[right].gameObject);
                    AllItem[right].ContinueCallBack(tupleID);
                }
            }
        }
        //Control left
        if (AllItem.ContainsKey(left))
        {
            if (id.ID == AllItem[left].GetComponent<IDName>().ID)
            {
                if (!RelatedNeighbors[tupleID].Contains(id.gameObject)) RelatedNeighbors[tupleID].Add(id.gameObject);
                if (!RelatedNeighbors[tupleID].Contains(AllItem[left].gameObject))
                {
                    RelatedNeighbors[tupleID].Add(AllItem[left].gameObject);
                    AllItem[left].ContinueCallBack(tupleID);
                }
            }
        }
    }

    //When clicked on the cube find the same colored neighbors
    public static void CalculateCubeCallBack(PickUp pickUp, IDName id)
    {
        // Neighbors
        var top = new Tuple<int, int>(pickUp.x, pickUp.y + 1);
        var down = new Tuple<int, int>(pickUp.x, pickUp.y - 1);
        var right = new Tuple<int, int>(pickUp.x + 1, pickUp.y);
        var left = new Tuple<int, int>(pickUp.x - 1, pickUp.y);

        //Control top neighbor item in the all Item Dictionary, if it is in the List with the right Tupple key;
        if (AllItem.ContainsKey(top))
        {
            // Check the neighbors ID, which is the same sprite(or color)
            if(id.ID == AllItem[top].GetComponent<IDName>().ID)
            {
                // Check clicked pickup object first in the deleted objects List, if it is not in the list add
                if (!deletedObjects.Contains(id.gameObject)) deletedObjects.Add(id.gameObject);


                // Check neighbor pickup object in the deleted objects list, if it is not in the list add and continue to do the same for the neighbors' next neighbor
                if (!deletedObjects.Contains(AllItem[top].gameObject)) 
                {
                    deletedObjects.Add(AllItem[top].gameObject);
                    AllItem[top].ContinueCallBack();  
                }
                
            }
        }
        //Control down
        if (AllItem.ContainsKey(down))
        {
            if (id.ID == AllItem[down].GetComponent<IDName>().ID)
            {
                if (!deletedObjects.Contains(id.gameObject)) deletedObjects.Add(id.gameObject);
                if (!deletedObjects.Contains(AllItem[down].gameObject))
                {
                    deletedObjects.Add(AllItem[down].gameObject);
                    AllItem[down].ContinueCallBack();
                }
            }
        }
        //Control right
        if (AllItem.ContainsKey(right))
        {
            if (id.ID == AllItem[right].GetComponent<IDName>().ID)
            {
                if (!deletedObjects.Contains(id.gameObject)) deletedObjects.Add(id.gameObject);
                if (!deletedObjects.Contains(AllItem[right].gameObject))
                {
                    deletedObjects.Add(AllItem[right].gameObject);
                    AllItem[right].ContinueCallBack();
                }
            }
        }
        //Control left
        if (AllItem.ContainsKey(left))
        {
            if (id.ID == AllItem[left].GetComponent<IDName>().ID)
            {
                if (!deletedObjects.Contains(id.gameObject)) deletedObjects.Add(id.gameObject);
                if (!deletedObjects.Contains(AllItem[left].gameObject))
                {
                    deletedObjects.Add(AllItem[left].gameObject);
                    AllItem[left].ContinueCallBack();
                }
            }
        }
    }

    //When clicked start the DeleteCubes method after the CalculateCallBack method
    public void DeleteCallBack()
    {
        Invoke("DeleteCubes", 0.1f);
        // Setup new cubes
        Invoke("EnableBoxCollider2DCallBack", 1f);
    }

    // Spawn new cubes at the destroyed cube's transform with a padding top, called in the DeleteCubes method
    void SpawnBack(PickUp pickUp)
    {
        var newCube =Instantiate(normalCubePrefabs[UnityEngine.Random.Range(0, randomColorCount)], new Vector2(pickUp.x,pickUp.y+paddingTop), Quaternion.identity);
        newCube.AddComponent<CapsuleCollider2D>();
        
        newCube.transform.SetParent(cube);
        newCube.tag = "Item";
        newCube.AddComponent<PickUp>();
        newCube.GetComponent<IDName>().isRunningChangeSprites = true;
    }

    // Destroy the clicked objects in the deletedObject List and remove from the allItem list, call spawnback
    void DeleteCubes()
    {
        for (int i = 0; i < deletedObjects.Count; i++)
        {
            SpawnBack(deletedObjects[i].GetComponent<PickUp>());
            Destroy(deletedObjects[i]);
        }
        AllItem.Clear();
        deletedObjects.Clear();
    }

    // Enable & Disable background invisible cubes for renaming(re-coordinating) the new and old cubes
    void EnableBoxCollider2DCallBack()
    {
        for (int i = 0; i < background.childCount; i++)
        {
            background.GetChild(i).GetComponent<BoxCollider2D>().enabled = true;
        }
        Invoke("DisableBoxCollider2DCallBack", 0.2f);
    }

    void DisableBoxCollider2DCallBack()
    {
        for (int i = 0; i < background.childCount; i++)
        {
            background.GetChild(i).GetComponent<BoxCollider2D>().enabled = false;
        }
        StartCoroutine(Wait(0.1f, () => {
            CheckGroupsForIconChange();
        }));
    }
}
