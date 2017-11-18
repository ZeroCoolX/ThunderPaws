using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour {
    public string LevelFileName;
    //public Texture2D map;
    public ColorToPrefab[] ColorMappings;

	// Use this for initialization
	void Start () {
        LoadMap();
	}

    /// <summary>
    /// Find all our children and eliminate them
    /// </summary>
    private void EmptyMap() {
        while(transform.childCount > 0) {
            Transform child = transform.GetChild(0);
            child.SetParent(null); //Become batman
            Destroy(child.gameObject);
        }
    }

    private void LoadMap() {
        EmptyMap();

        //Read the image daata from the file
        string filePath = Application.dataPath + "/Sprites/LevelTileMaps/" + LevelFileName;
        byte[] bytes = System.IO.File.ReadAllBytes(filePath);
        Texture2D map = new Texture2D(2, 2);
        map.LoadImage(bytes);

        //Get the raw pixels from the level map
        Color32[] mapPixels = map.GetPixels32();
        int width = map.width;
        int height = map.height;

        for(int x = 0; x < width; ++x) {
            for (int y = 0; y < height; y++) {
                SpawnTileAt(mapPixels[x + (y * width)], x, y);
            }
        }
    }

    private void SpawnTileAt(Color32 color, int x, int y) {
        //If the pixel is transparent - then it's meaant to be empty
        if(color.a <= 155) {
            return;
        }

        //Find the right color in our map
        //TODO: Should use a dictionary for max speed
        foreach (var ctp in ColorMappings) {
            if(ctp.Color.Equals(color)) {
                //Spawn the prefab at the right location
                Instantiate(ctp.Prefab, new Vector3(x, y, 0), Quaternion.identity, transform);
                return;
            }
        }

        //If we got to this point then we did not find a matching color in our array
        Debug.LogError("No color to prefab found for: " + color.ToString());
    }
}
