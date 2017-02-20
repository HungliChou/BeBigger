using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameLevel{Menu, Normal, Color}

public enum GameMode{Color, Shape}

public class GameManager : MonoBehaviour {
    private static GameManager instance;
    public static GameManager GetInstance{get{return instance;}}
    public List<GameObject> objects = new List<GameObject>();
    public List<GameObject> objects_Special = new List<GameObject>();
    public List<GameObject> objects_shape = new List<GameObject>();
    public List<Sprite> objectsGoal_shape = new List<Sprite>();
    public Character character;

    //timer for creating objects
    private float timer;
    //create objects frequency
    public float releaseTime;

    //timer for changing mode
    private float timer_mode;
    //change mode frequency;
    public float modeChangeTime;

    //Record (Reset every challenge)
    public float timer_Record;

    public GameObject mobileControl;
    public GameLevel gameLevel;
    public GameMode gameMode;
    public GameObject signPrefab;
    public Text GameModeUI;
    public Text GameTimerUI;
    public GameObject HintPanel;
    public Text ReadyTimerUI;
    public GameObject PausePanel;
    public GameObject ResultPanel;

    void Awake()
    {
        instance = this;
    }

	// Use this for initialization
	void Start () {
        
        timer = 0;
        //releaseTime = 0.5f;

        timer_mode = 0;
        modeChangeTime = 5;

        timer_Record = 0;

        character = GameObject.FindGameObjectWithTag("Player").GetComponent<Character>();
        character.MyMode = Mode.Pause;

        #if UNITY_EDITOR
        #elif UNITY_IOS || UNITY_ANDROID
        mobileControl.SetActive(true);
        Debug.Log("mobile");
        #endif
	}
	
	// Update is called once per frame
	void Update () {
        if(gameLevel!=GameLevel.Menu)
        {
            if(character.MyMode==Mode.Alive || character.MyMode==Mode.BeingHit)
            {
                DoRecordTimer();
                DoTimer();
                if(gameLevel == GameLevel.Color)
                    DoChangeModeTimer();
            }     
        }
        else
        {
            DoTimer();
        }
	}

    public IEnumerator ReadyCountDown()
    {
        ReadyTimerUI.gameObject.SetActive(true);
        ReadyTimerUI.text = "3";
        yield return new WaitForSeconds(1);
        ReadyTimerUI.text = "2";
        yield return new WaitForSeconds(1);
        ReadyTimerUI.text = "1";
        yield return new WaitForSeconds(1);
        ReadyTimerUI.text = "Start!";
        character.MyMode = Mode.Alive;
        yield return new WaitForSeconds(0.3f);
        ReadyTimerUI.gameObject.SetActive(false);
        yield return 0;
    }

    void StartGame(bool enable)
    {
        if(enable)
        {
            HintPanel.SetActive(false);
            PausePanel.SetActive(false);
            ResultPanel.SetActive(false);
            StartCoroutine(ReadyCountDown());
        }
        else
        {
            HintPanel.SetActive(true);
            character.MyMode = Mode.Pause;
        }
    }

    public void PauseGame(bool enable)
    {
        if(enable)
        {
            if(character.MyMode==Mode.Pause || character.MyMode==Mode.Win || character.MyMode==Mode.Dead)
                return;
            character.MyMode = Mode.Pause;
            PausePanel.SetActive(true);
        }
        else
        {
            if(character.MyMode!=Mode.Pause)
                return;
            PausePanel.SetActive(false);
            StartCoroutine(ReadyCountDown());
        }
    }

    public void GameResult(Mode mode)
    {
        Transform nextLevel;
        ResultPanel.SetActive(true);
        if(mode == Mode.Win)
        {
            nextLevel = ResultPanel.transform.Find("NextLevel");
            if(nextLevel!=null)
                nextLevel.gameObject.SetActive(true);
            ResultPanel.transform.Find("ResultTitle").GetComponent<Text>().text = "Win!";
            ResultPanel.transform.Find("Result").GetComponent<Text>().text = "Your record:\n" + GetRecordTimerString();
        }
        else if(mode == Mode.Dead)
        {
            nextLevel = ResultPanel.transform.Find("NextLevel");
            if(nextLevel!=null)
                nextLevel.gameObject.SetActive(false);
            ResultPanel.transform.Find("ResultTitle").GetComponent<Text>().text = "Lose!";
            ResultPanel.transform.Find("Result").GetComponent<Text>().text = "No record";
        }
    }

    void ChangeGameMode()
    {
        if(gameMode==GameMode.Color)
        {
            gameMode = GameMode.Shape;
            GameModeUI.text = "Shape";
        }
        else
        {
            gameMode = GameMode.Color;
            GameModeUI.text = "Color";
        }
    }

    //Record Timer
    void DoRecordTimer()
    {
        timer_Record += Time.deltaTime;
        GameTimerUI.text = GetRecordTimerString();
    }
        
    public string GetRecordTimerString()
    {
        string timerString = "";
        timerString += Mathf.Floor(timer_Record/60).ToString("00");
        timerString += ":";
        timerString +=(timer_Record%60).ToString("00"); 
        return timerString;
    }
    //Change Mode Timer
    void DoChangeModeTimer()
    {
        if(timer_mode < modeChangeTime)
            timer_mode += Time.deltaTime;
        else
        {
            timer_mode = 0;
            ChangeGameMode();
        } 
    }
        
    //Create Objects Timer
    void DoTimer()
    {
        if(timer < releaseTime)
            timer += Time.deltaTime;
        else
        {
            timer = 0;
            CreateRandomObject();
        }
    }

    void CreateRandomObject()
    {
        int objRandom = 0;
        GameObject obj = gameObject;
        switch(gameLevel)
        {
            case GameLevel.Normal:
                objRandom =  Random.Range(0,objects.Count);
                obj = Instantiate(objects[objRandom]) as GameObject;
                break;
            case GameLevel.Color:
            case GameLevel.Menu:
                objRandom =  Random.Range(0,objects_shape.Count);
                obj = Instantiate(objects_shape[objRandom]) as GameObject;

                SpriteRenderer objRenderer = obj.GetComponent<SpriteRenderer>();
                int colorRandom = Random.Range(0,3);
                switch(colorRandom)
                {
                    case 0:
                        objRenderer.color = Color.red;
                        break;
                    case 1:
                        objRenderer.color = Color.green;
                        break;
                    case 2:
                        objRenderer.color = Color.blue;
                        break;
                }
                break;
        }
        SetObjPosition(obj);
    }

    void CreateSpecialObject()
    {
        GameObject obj = Instantiate(objects_Special[0]) as GameObject;
        SetObjPosition(obj);
    }

    void SetObjPosition(GameObject obj)
    {
        int x = 0;
        int y = 0;
        int pos = Random.Range(0,2);
        int pos2 = Random.Range(0,2);
        if(pos==0)
        {
            x = Random.Range(-200,200);
            if(pos2==0)
                y = 120;
            else
                y = -120;
        }
        else
        {
            y = Random.Range(-120,120);
            if(pos2==0)
                x = 200;
            else
                x = -200;  
        } 
        obj.transform.position = new Vector3(x,y,0);
    }

    public void LoadLevel(int level)
    {
        SceneManager.LoadScene(level);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    //Called by "Character" when the player click "Restart"
    public void RestartLevel()
    {
        StartGame(true);
        timer_Record = 0;
        GameTimerUI.text = GetRecordTimerString();
        Objects[] objs = FindObjectsOfType<Objects>();
        foreach(Objects obj in objs)
        {
            Destroy(obj.gameObject);
        }
    }


}
