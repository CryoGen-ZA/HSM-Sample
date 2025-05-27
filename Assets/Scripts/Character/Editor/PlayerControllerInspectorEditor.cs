using Character;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerCharacter))]
public class PlayerControllerInspectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (!Application.isPlaying)
        {
            base.OnInspectorGUI();
            return;
        }
     
        var ctx = (PlayerCharacter)target;
        GUILayout.Label(ctx.GetStatesDebugText);
        
        base.OnInspectorGUI();
    }
}
