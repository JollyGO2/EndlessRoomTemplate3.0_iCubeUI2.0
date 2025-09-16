using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class DuplicateWall : MonoBehaviour
{

    public SlideManager copyFromManager;
    public List<SlideManager> slideManagers;
    public List<Button> duplicateButtons;

    public void CopyFrom(SlideManager m)
    {
        gameObject.SetActive(true);
        copyFromManager = m;
        foreach(Button b in duplicateButtons)
        {
            b.interactable = true;
        }

        duplicateButtons[slideManagers.IndexOf(m)].interactable = false;
            
    }

    public void CopyTo(Button b)
    {
        UndoRedo.instance.Action();
        SlideManager copyToManager = slideManagers[duplicateButtons.IndexOf(b)];
        FindObjectOfType<WallManager>().current = duplicateButtons.IndexOf(b);
        copyToManager.currentSlide = 0;

        SlidesData.instance.Duplicate(copyFromManager, copyToManager);

    }

}
