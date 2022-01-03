using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasSettingsManager : MonoBehaviour {

    public Settings settings;
    public TMP_Text mapSizeText;

    void Update() {

    }

    public CanvasSettingsManager(Settings settings) {
        this.settings = settings;
    }

    public int getMapSize() {
        return settings.mapSize;
    }

    public void setMapSize(float newMapSize) {
        settings.mapSize = (int)newMapSize;
        mapSizeText.text = settings.mapSize.ToString();
    }
}
