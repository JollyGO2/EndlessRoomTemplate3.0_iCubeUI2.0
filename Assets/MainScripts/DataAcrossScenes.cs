using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataAcrossScenes : MonoBehaviour
{
 public static DataAcrossScenes instance;
	public string projectName;
	public string projectDetails;
	public Sprite projectThumbnail;
	public enum WallsConfig { Three, Four};
	public WallsConfig wallsConfig;

	void Awake()
	{
		if (instance != null)
		{
			if (instance != this)
			{
				Destroy(this);
				return;
			}
		}
		instance = this;
		DontDestroyOnLoad(this);

		string savePath = Path.Combine(Application.persistentDataPath, "Config.txt");
		if (File.Exists(savePath))
        {
			int config = ES3.Load<int>("Config", savePath);

			switch (config)
            {
				case 3:
					wallsConfig = WallsConfig.Three;
					break;

				case 4:
					wallsConfig = WallsConfig.Four;
					break;
            }

        }


	}

    private void Update()
    {
		if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl))
        {
			Debug.Log("Pressing ctrl + alt keys");
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				Debug.Log("Pressing 3");
				wallsConfig = WallsConfig.Three;

				string savePath = Path.Combine(Application.persistentDataPath, "Config.txt");
				ES3.Save("Config", 3, savePath);
            }

			else if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				Debug.Log("Pressing 4");
				wallsConfig = WallsConfig.Four; 
				string savePath = Path.Combine(Application.persistentDataPath, "Config.txt");
				ES3.Save("Config", 4, savePath);
			}
		}
    }

}
