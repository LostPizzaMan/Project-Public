using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceBlockCollider : MonoBehaviour
{
    [SerializeField] TerrainModifier terrainModifier;

    void OnTriggerEnter(Collider other)
    {
        terrainModifier.colliding = true;
    }

    void OnTriggerExit(Collider other)
    {
        terrainModifier.colliding = false;
    }
}
