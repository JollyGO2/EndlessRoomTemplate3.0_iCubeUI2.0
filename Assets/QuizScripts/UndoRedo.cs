using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UndoRedo : MonoBehaviour
{
    public static UndoRedo instance;
    public SlidesData slidesData;

    public List<SaveSlot> undoStack = new List<SaveSlot>();
    public List<SaveSlot> redoStack = new List<SaveSlot>();

    public class SaveSlot
    {
        public List<SlideManager.Slides> wallL = new List<SlideManager.Slides>();
        public List<SlideManager.Slides> wallLC = new List<SlideManager.Slides>();
        public List<SlideManager.Slides> wallRC = new List<SlideManager.Slides>();
        public List<SlideManager.Slides> wallR = new List<SlideManager.Slides>();

        public int currentWall;
        public int currentSlide;
        public AudioClip bgmfile;
        public int timer; //Addon feature
    }

    public List<SlideManager> allManagers;
    bool firstundo;


    public Button undobut;
    public Button redobut;

    private void Start()
    {
        if (instance!= null)
        {
            if (instance != this)
            {
                Destroy(this);
            }
        }

        instance = this;

        undobut.interactable = false;
        redobut.interactable = false;
    }

    public void Action()
    {
        if (undoStack.Count >= 50)
        {
            undoStack.RemoveAt(0);
        }


        Debug.Log("Action saving to undoredo");

        SaveSlot slot = new SaveSlot();
        
        for(int i = 0; i<allManagers.Count;i++)
        {
            switch (i)
            {
                case 0:
                    SaveWalls(i, slot.wallL);
                    break;

                case 1:
                    SaveWalls(i, slot.wallLC);
                    break;

                case 2:
                    SaveWalls(i, slot.wallRC);
                    break;

                case 3:
                    SaveWalls(i, slot.wallR);
                    break;
            }
        }


        slot.currentWall = FindObjectOfType<WallManager>().current;
        slot.currentSlide = allManagers[slot.currentWall].currentSlide;
        slot.bgmfile = FindObjectOfType<EditorManager>().bgm;
        slot.timer = FindObjectOfType<EditorManager>().totalTime;

        undobut.interactable = true;
        redobut.interactable = false;
        undoStack.Add(slot);
        redoStack.Clear();
    }

    public void SaveWalls(int i, List<SlideManager.Slides> slidesList)
    {


        for (int slideNum = 0; slideNum < allManagers[i].slidesList.Count; slideNum++)
        {

            SlideManager.Slides man = allManagers[i].slidesList[slideNum];

            slidesList.Add(new SlideManager.Slides());


            slidesList[slideNum].bgSprite = man.bgSprite;


            if (man.slideType == SlideManager.Slides.Type.text)
            {
                slidesList[slideNum].slideType = SlideManager.Slides.Type.text;
            }
            else if (man.slideType == SlideManager.Slides.Type.image)
            {
                slidesList[slideNum].slideType = SlideManager.Slides.Type.image;
            }
            else if (man.slideType == SlideManager.Slides.Type.both)
            {
                slidesList[slideNum].slideType = SlideManager.Slides.Type.both;
            }
            else if (man.slideType == SlideManager.Slides.Type.blank)
            {
                slidesList[slideNum].slideType = SlideManager.Slides.Type.blank;

            }

            slidesList[slideNum].correctAnswers = new(man.correctAnswers);
            slidesList[slideNum].textAnswers = new (man.textAnswers);
            slidesList[slideNum].videoPath = new(man.videoPath);
            slidesList[slideNum].imageSprites = new(man.imageSprites);
            slidesList[slideNum].choiceCount = man.choiceCount;
            slidesList[slideNum].questionText = man.questionText;


        }

    }





    public void Undo()
    {
        Debug.Log("Before undo Value of timer is: " + FindObjectOfType<EditorManager>().totalTime);
        Debug.Log("Undoing Action");

        
        if (undoStack.Count == 0)
        {
            Debug.LogError("No more to undo!");
            return;
        }

        SaveSlot slot = new SaveSlot();
        for (int i = 0; i < allManagers.Count; i++)
        {
            switch (i)
            {
                case 0:
                    SaveWalls(i, slot.wallL);
                    break;

                case 1:
                    SaveWalls(i, slot.wallLC);
                    break;

                case 2:
                    SaveWalls(i, slot.wallRC);
                    break;

                case 3:
                    SaveWalls(i, slot.wallR);
                    break;
            }
        }
        slot.currentWall = FindObjectOfType<WallManager>().current;
        slot.currentSlide = allManagers[slot.currentWall].currentSlide;
        slot.bgmfile = FindObjectOfType<EditorManager>().bgm;
        slot.timer = FindObjectOfType<EditorManager>().totalTime;
        redoStack.Add(slot);
        redobut.interactable = true;
        

        
        SaveSlot undid = undoStack[undoStack.Count-1];


        Debug.Log("Still have " + undoStack.Count + " undos");

        if (undoStack.Count == 0)
        {
            undobut.interactable = false;
        }


        for (int i = 0; i < allManagers.Count; i++)
        {
            if (allManagers[i] == null)
            {
                Debug.LogError("No manager " + i);
                continue;
            }

            switch (i)
            {
                case 0:
                    EditWalls(i, undid.wallL);
                    break;

                case 1:
                    EditWalls(i, undid.wallLC);
                    break;

                case 2:
                    EditWalls(i, undid.wallRC);
                    break;

                case 3:
                    EditWalls(i, undid.wallR);
                    break;
            }
        }

        allManagers[undid.currentWall].currentSlide = undid.currentSlide;
        FindObjectOfType<WallManager>().current = undid.currentWall;

        allManagers[undid.currentWall].skipaction = true;

        FindObjectOfType<WallManager>().OnOffWall();

        if (undid.bgmfile)
        {
            FindObjectOfType<EditorManager>().bgmFileName = undid.bgmfile.name;
            FindObjectOfType<EditorManager>().bgm = undid.bgmfile;
        }
        else
        {
            allManagers[0].RemoveAudio();
        }

        FindObjectOfType<EditorManager>().totalTime = undid.timer; //Undo Time set
        Debug.Log("After undo Value of timer is: " + FindObjectOfType<EditorManager>().totalTime);

        undoStack.RemoveAt(undoStack.Count - 1);
    }

    public void Redo()
    {
        Debug.Log("Before Redo Value of timer is: " + FindObjectOfType<EditorManager>().totalTime);
        Debug.Log("Redoing action");

        if (redoStack.Count == 0)
        {
            Debug.LogError("No more to redo!");
            return;
        }
        SaveSlot slot = new SaveSlot();
        for (int i = 0; i < allManagers.Count; i++)
        {
            switch (i)
            {
                case 0:
                    SaveWalls(i, slot.wallL);
                    break;

                case 1:
                    SaveWalls(i, slot.wallLC);
                    break;

                case 2:
                    SaveWalls(i, slot.wallRC);
                    break;

                case 3:
                    SaveWalls(i, slot.wallR);
                    break;
            }
        }
        slot.currentWall = FindObjectOfType<WallManager>().current;
        slot.currentSlide = allManagers[slot.currentWall].currentSlide;
        slot.bgmfile = FindObjectOfType<EditorManager>().bgm;
        slot.timer = FindObjectOfType<EditorManager>().totalTime;
        undoStack.Add(slot);
        undobut.interactable = true;


        SaveSlot redid = redoStack[redoStack.Count-1];

        if (redoStack.Count == 0)
        {
            redobut.interactable = false;
        }

        for (int i = 0; i < allManagers.Count; i++)
        {
            if (allManagers[i] == null)
            {
                Debug.LogError("No manager " + i);
                continue;
            }

            switch (i)
            {
                case 0:
                    EditWalls(i, redid.wallL);
                    break;

                case 1:
                    EditWalls(i, redid.wallLC);
                    break;

                case 2:
                    EditWalls(i, redid.wallRC);
                    break;

                case 3:
                    EditWalls(i, redid.wallR);
                    break;
            }
        }
        allManagers[redid.currentWall].currentSlide = redid.currentSlide;
        //  Debug.Log("Undo to wall " + redid.currentWall);
        FindObjectOfType<WallManager>().current = redid.currentWall;
        allManagers[redid.currentWall].skipaction = true;
        FindObjectOfType<WallManager>().OnOffWall();

        if (redid.bgmfile)
        {
            FindObjectOfType<EditorManager>().bgmFileName = redid.bgmfile.name;
            FindObjectOfType<EditorManager>().bgm = redid.bgmfile;
        }
        else
        {
            allManagers[0].RemoveAudio();
        }

        FindObjectOfType<EditorManager>().totalTime = redid.timer; //redo Time set
        Debug.Log("After Redo Value of timer is: " + FindObjectOfType<EditorManager>().totalTime);

        redoStack.RemoveAt(redoStack.Count - 1);
    }

    public void EditWalls(int i, List<SlideManager.Slides> slidesList)
    {
       // Debug.Log("undoredo slidelist for manager " + i + " has " + slidesList.Count);

        allManagers[i].slidesList.Clear();


        for (int slideNum = 0; slideNum < slidesList.Count; slideNum++)
        {
           //Debug.Log("Currently adding slide " + slideNum + " for manager " + i);

            SlideManager.Slides man = slidesList[slideNum];


            allManagers[i].slidesList.Add(new SlideManager.Slides());

            //Debug.Log("added, manager " + i + " total has " + allManagers[i].slidesList.Count);

            allManagers[i].slidesList[slideNum].bgSprite = man.bgSprite;


            if (man.slideType == SlideManager.Slides.Type.text)
            {
                allManagers[i].slidesList[slideNum].slideType = SlideManager.Slides.Type.text;
            }
            else if (man.slideType == SlideManager.Slides.Type.image)
            {
                allManagers[i].slidesList[slideNum].slideType = SlideManager.Slides.Type.image;
            }
            else if (man.slideType == SlideManager.Slides.Type.both)
            {
                allManagers[i].slidesList[slideNum].slideType = SlideManager.Slides.Type.both;
            }
            else if (man.slideType == SlideManager.Slides.Type.blank)
            {
                allManagers[i].slidesList[slideNum].slideType = SlideManager.Slides.Type.blank;

            }

            allManagers[i].slidesList[slideNum].correctAnswers = man.correctAnswers;
            allManagers[i].slidesList[slideNum].textAnswers = man.textAnswers;
            allManagers[i].slidesList[slideNum].videoPath = man.videoPath;
            allManagers[i].slidesList[slideNum].imageSprites = man.imageSprites;
            allManagers[i].slidesList[slideNum].choiceCount = man.choiceCount;
            allManagers[i].slidesList[slideNum].questionText = man.questionText;

            //if (allManagers[i].slideButtonsList.Count <= slideNum)
            //{
            //    allManagers[i].AddSlide(man.slideType == SlideManager.Slides.Type.blank);
            //    allManagers[i].sk
            //}



        }


       // Debug.Log("manager " + i + " total has " + allManagers[i].slidesList.Count);
    }
}
