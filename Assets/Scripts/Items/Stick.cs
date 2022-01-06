using UnityEngine;

public class Stick : Item
{
    public Stick(int _itemID, string _stringID, string _name, string _sprite) : base(_itemID, _stringID, _name, _sprite)
    {

    }

    public override void OnUse(PlayerInstance playerInstance)
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Debug.Log("I am a stick.");
        }
    }
}
