using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class WorldSelector: MonoBehaviour
{
    [SerializeField] private TerrainGenerator terrainGenerator;
    [SerializeField] private Grid3D grid3D;

    public GameObject UI;
    public GameObject inventoryUI;
    public GameObject terrainModifier;

    public Text inputField;

    // Scrollview

    public GameObject Prefab;
    public Transform Container;
    public List<string> files = new List<string>();

    void Start()
    {
        GetFiles();

        for (int i = 0; i < files.Count; i++)
        {
            GameObject go = Instantiate(Prefab);
            go.GetComponentInChildren<Text>().text = files[i];
            go.transform.SetParent(Container);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            int buttonIndex = i;
            go.GetComponent<Button>().onClick.AddListener(() => LoadWorld(buttonIndex));
        }
    }

    public void LoadWorld(int index)
    {
        string file = files[index];

        terrainGenerator.worldName = file;
        terrainGenerator.LoadSave();
        terrainGenerator.LoadChunks(true);
        terrainGenerator.loadingChunksAllowed = true;

        grid3D.StartAI();

        UI.SetActive(false);
        terrainModifier.SetActive(true);
        inventoryUI.SetActive(true);
    }

    public void CreateWorld()
    {
        terrainGenerator.worldName = inputField.text;
        terrainGenerator.LoadSave();
        terrainGenerator.LoadChunks(true);
        terrainGenerator.loadingChunksAllowed = true;

        grid3D.StartAI();

        UI.SetActive(false);
        terrainModifier.SetActive(true);
        inventoryUI.SetActive(true);
    }

    void GetFiles()
    {
        var info = new DirectoryInfo(Application.persistentDataPath + " Saves/");
        DirectoryInfo[] fileInfo = info.GetDirectories();
        foreach (DirectoryInfo file in fileInfo) 
        {
            files.Add(file.Name);
        }
    }
}
