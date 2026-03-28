using UnityEngine;

public enum ItemType {

    Heal,
    PermBuff,
    TempBuff,
    Ability,
    Gun

}

[CreateAssetMenu(fileName = "BasicItem", menuName = "Scriptable Objects/BasicItem")]
public class BasicItem : ScriptableObject
{
    public string description;
    public ItemType type;
    public int price;

    public int id; // ID can be used as a sort of value integer;
    public float weight;

    public Sprite sprite;
}
