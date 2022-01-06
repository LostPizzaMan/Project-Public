using UnityEngine;

public class LightSource : Item
{
    LayerMask layerMask = LayerMask.GetMask("Terrain") + LayerMask.GetMask("TransparentTerrain");

    public LightSource(int _itemID, string _stringID, string _name, string _sprite) : base(_itemID, _stringID, _name, _sprite)
    {

    }

    public override void OnUse(PlayerInstance playerInstance)
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            RaycastHit hitInfo = PlayerUtilities.ShootRaycast(playerInstance.cam, 4, layerMask);

            if (hitInfo.transform == null)
            {
                return;
            }

            Vector3 pointInTargetBlock = hitInfo.point - playerInstance.cam.transform.forward * .01f;

            PlayerUtilities.PlaceLightSourceAt(pointInTargetBlock, 11);
        }
    }
}
