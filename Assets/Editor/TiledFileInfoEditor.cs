using UnityEngine;
using System.Collections;
using UnityEditor;


[CustomEditor(typeof(TiledFileInfo))]
public class TiledFileInfoEditor : Editor
{

    TiledFileInfo tiledImporter = null;
			
    public override void OnInspectorGUI()
    {
        if (tiledImporter == null)
        {
            tiledImporter = (TiledFileInfo)target;
        }
        if (GUILayout.Button("Import"))
        {
            tiledImporter.ImportTilemap();
        }
        base.OnInspectorGUI();
    }
}
