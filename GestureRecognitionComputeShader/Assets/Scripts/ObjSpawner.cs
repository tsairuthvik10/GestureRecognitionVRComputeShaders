using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjSpawner : MonoBehaviour
{
    public List<GameObject> objects;

    public void SpawnObjectHandler(string objectName)
    {
        foreach(var item in objects)
        {
            Debug.Log("CHUTIYE");
            Debug.Log("objectName: "+ objectName);
            Debug.Log("iten.name: " + item.name);
            item.SetActive(objectName == item.name);
        }
    }
}
