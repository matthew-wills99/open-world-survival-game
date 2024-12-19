using UnityEngine;

[CreateAssetMenu(fileName = "newBlockScriptable", menuName = "Block Scriptable")]

public class BlockScriptable : ScriptableObject
{
    public string blockName;

    public Texture2D topFaceTexture;
    public Texture2D sideFaceTexture;
}
