using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if Unity_Editor
using UnityEditor;
#endif

[ExecuteInEditMode]
public class PathSetter : MonoBehaviour
{
    static PathSetter instance;

    float roadWidth = 2;

    // prefabs
    public GameObject rock; // border
    public GameObject peg;

    public Vector3 lastDot;

    private void Update()
    {
        // if move far enough from last placement, place a block
        if ((lastDot - transform.position).magnitude > .69f)
        {
            Vector3 spawnPos = (transform.position + lastDot) / 2;
            peg.transform.right = (transform.position - lastDot).normalized;
            Instantiate(rock, spawnPos + peg.transform.up * roadWidth, Quaternion.identity).transform.right = peg.transform.right;
            Instantiate(rock, spawnPos + peg.transform.up * -roadWidth, Quaternion.identity).transform.right = peg.transform.right;
            lastDot = transform.position;
        }
    }
#if Unity_Editor
    [MenuItem("Setter/SetInstance %i")]
    static void ReferenceInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<PathSetter>();
            if (instance != null)
            {
                Debug.Log("Succesfully referenced : " + instance.name);
            }
            else
            {
                Debug.Log("Cannot find a PathSetter in the scene");
            }
        }
        else
        {
            Debug.Log("Already have a path setter referenced");
        }
    }

    [MenuItem("Setter/Move %k")]
    static void MoveForward()
    {
        instance.transform.position += instance.transform.right * 6 * .02f;
    }

    [MenuItem("Setter/TurnRight %l")]
    static void TurnRight()
    {
        instance.transform.Rotate(new Vector3(0, 0, -1 * 90 * .02f));
        MoveForward();
    }

    [MenuItem("Setter/TurnLeft %j")]
    static void TurnLeft()
    {
        instance.transform.Rotate(new Vector3(0, 0, 90 * .02f));
        MoveForward();
    }
#endif
}
