using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using PDollarGestureRecognizer;
using System.IO;
using UnityEngine.Events;
public class sc_MovementRecognizer : MonoBehaviour
{
    public XRNode inputSource;
    public InputHelpers.Button inputButton;
    public float inputThreshold = 0.1f;
    public Transform movementSource;

    public float newPositionThresholdDistance = 0.05f;
    public GameObject p_debugSpherePrefab;
    public bool creationMode = true;
    public string newGestureName;
    public ObjSpawner objSpawner;
    public float recognitionThreshold = 0.85f;

    [System.Serializable]
    public class UnityStringEvent : UnityEvent<string> { }
    public UnityStringEvent OnRecognized;

    private List<Gesture> trainingSet = new List<Gesture>();
    private bool isMoving = false;
    private List<Vector3> positionsList = new List<Vector3>();
    // Start is called before the first frame update
    void Start()
    {
        string[] gestureFiles = Directory.GetFiles(Application.persistentDataPath, "*.xml");
        foreach(var item in gestureFiles)
        {
            trainingSet.Add(GestureIO.ReadGestureFromFile(item));
        }
    }

    // Update is called once per frame
    void Update()
    {
        InputHelpers.IsPressed(InputDevices.GetDeviceAtXRNode(inputSource), inputButton, out bool isPressed, inputThreshold);

        //Start the movement
        if(!isMoving && isPressed)
        {
            StartMovement();
        }
        //ending the movement
        else if(isMoving && !isPressed)
        {
            EndMovement();
        }
        //updating the movement
        else if(isMoving && isPressed)
        {
            UpdateMovement();
        }
    }

    void StartMovement()
    {
        isMoving = true;
        positionsList.Clear();
        positionsList.Add(movementSource.position);

        if (p_debugSpherePrefab)
        {
            Destroy(Instantiate(p_debugSpherePrefab, movementSource.position, Quaternion.identity), 3);
        }
    }

    void EndMovement()
    {
        isMoving = false;

        //Create the gesture from the positions list
        Point[] pointArray = new Point[positionsList.Count];

        //Populating the pointArray
        for(int i = 0; i < positionsList.Count; i++)
        {
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(positionsList[i]);//projecting the 3D points on to the screen(2D plane)
            pointArray[i] = new Point(screenPoint.x, screenPoint.y, 0);
        }

        Gesture newGesture = new Gesture(pointArray);

        //Add a new gesture to the training set
        if (creationMode)
        {
            newGesture.Name = newGestureName;
            trainingSet.Add(newGesture);

            string fileName = Application.persistentDataPath + "/" + newGestureName + ".xml";
            GestureIO.WriteGesture(pointArray, newGestureName, fileName);
        }
        //recognize gesture
        else
        {
            Result result = PointCloudRecognizer.Classify(newGesture, trainingSet.ToArray());
            Debug.Log(result.GestureClass + " : " + result.Score);
            if(result.Score >= recognitionThreshold)
            {
                Debug.Log("BEFORE" + result.GestureClass);
                //OnRecognized.Invoke(result.GestureClass);
                objSpawner.SpawnObjectHandler(result.GestureClass);
                Debug.Log("AFTER" + result.GestureClass + " : " + result.Score) ;
            }
        }

    }

    void UpdateMovement()
    {
        Vector3 lastPosition = positionsList[positionsList.Count - 1];
        if (Vector3.Distance(movementSource.position, lastPosition) > newPositionThresholdDistance)
        {
            positionsList.Add(movementSource.position);
            if (p_debugSpherePrefab)
            {
                Destroy(Instantiate(p_debugSpherePrefab, movementSource.position, Quaternion.identity), 3);
            }
        }
    }
}
