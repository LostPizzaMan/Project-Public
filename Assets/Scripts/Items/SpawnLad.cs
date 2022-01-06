using UnityEngine;

public class SpawnLad : Item
{
    LayerMask layerMask = LayerMask.GetMask("Terrain") + LayerMask.GetMask("TransparentTerrain");
    GameObject prefab;

    public SpawnLad(int _itemID, string _stringID, string _name, string _sprite) : base(_itemID, _stringID, _name, _sprite)
    {

    }

    public override void OnUse(PlayerInstance playerInstance)
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            RaycastHit hitInfo = PlayerUtilities.ShootRaycast(playerInstance.cam, 4, layerMask);

            Vector3 pointInTargetBlock = hitInfo.point;
            pointInTargetBlock.y++;

            prefab = GameObject.Find("Seeker");
            GameObject instantiated = Object.Instantiate(prefab, pointInTargetBlock, Quaternion.identity);
            instantiated.GetComponent<Unit>().enabled = true;
        }
    }
}
