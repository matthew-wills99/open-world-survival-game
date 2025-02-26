using UnityEngine;

[CreateAssetMenu(fileName = "newBlockScriptable", menuName = "Block Scriptable")]

public class BlockScriptable : ScriptableObject
{
    public string blockName;
    [ReadOnly] public int id;

    public Texture2D topFaceTexture;
    public Texture2D sideFaceTexture;

    public void SetID(int id)
    {
        this.id = id;
    }
}
