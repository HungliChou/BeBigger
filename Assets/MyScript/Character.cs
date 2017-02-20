using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;

public enum Mode{Alive, Dead, Win, Pause, BeingHit}
public enum Buffname {Invincible,}

[System.Serializable]
public class Buff
{
    public Buffname name;
    public float duration;

    public Buff(Buffname _name, float _duration)
    {
        name = _name;
        duration = _duration;
    }
}

public class Character : MonoBehaviour {

    //record current scale to determine if it is alive
    public float myScale;
    //movement speed
    public float speed;
    //bigger value
    public float value_bigger;
    //smaller value
    public float value_small;
    //transform of goal sign
    public Transform goalTransform;
    //scale value of goal transform;
    public float goalScale;
    //value to reach
    public float goalValue;
    //SpriteRenderer of this gameobject
    public SpriteRenderer myRender;
    //current mode
    public Mode mode;
    //current Buff list
    public List<Buff> buffs = new List<Buff>();

    //Get var of mode
    public Mode MyMode{get{return mode;} set{mode = value;}}

    private Coroutine glowingCo;

    public Shape currentShape;

    void Awake()
    {
        mode = Mode.Pause;
        currentShape = Shape.Round;
    }
	// Use this for initialization
	void Start () {
        goalTransform = transform.FindChild("circle_hollow").transform;
        goalScale = goalValue/12.19f;
        goalTransform.localScale = new Vector3(goalScale,goalScale,1);
        myRender = GetComponent<SpriteRenderer>();
        myScale = transform.localScale.x;
        value_bigger = 0.3f;
        value_small = 0.5f;
        speed = 0.03f;
	}

	// Update is called once per frame
	void FixedUpdate () {
        if(mode == Mode.Dead || mode == Mode.Pause)
            return;

        #if UNITY_IOS || UNITY_ANDROID
            Vector3 moveVec = new Vector3(CrossPlatformInputManager.GetAxis("Horizontal"),CrossPlatformInputManager.GetAxis("Vertical"), 0) * speed;
            transform.localPosition += moveVec;
        #endif
        if (Input.GetAxis("Vertical")>0)
            transform.localPosition +=  new Vector3(0,speed,0);
        if (Input.GetAxis("Vertical")<0)
            transform.localPosition +=  new Vector3(0,-speed,0);
        if (Input.GetAxis("Horizontal")<0)
            transform.localPosition +=  new Vector3(-speed,0,0);
        if (Input.GetAxis("Horizontal")>0)
            transform.localPosition +=  new Vector3(speed,0,0);
        

        UpdateBuffs();
       
	}
        
    public void UpdateBuffs()
    {
        if(buffs.Count>0)
        {
            foreach(Buff buff in buffs)
            {
                if(buff.duration>0)
                {
                   buff.duration -= Time.deltaTime;
                }
                else
                {
                    buffs.Remove(buff);
                    if(buff.name == Buffname.Invincible)
                    {
                        StopCoroutine(glowingCo);
                        myRender.color = Color.blue;
                    }
                }
            }
        }
    }

    public bool CkeckIfhasBuff(Buffname name)
    {
        bool returnValue = false;
        if(buffs.Count>0)
        {
            foreach(Buff buff in buffs)
            {
                if(buff.name == name)
                {
                    returnValue = true;
                    break;
                }
            }
        }
        return returnValue;
    }

    public void DoAction(GameObject coll)
    {
        if(mode!= Mode.Alive)
            return;

        switch(GameManager.GetInstance.gameLevel)
        {
            case GameLevel.Normal:
                switch(coll.tag)
                {
                    case "absorb":
                        StartCoroutine(ColorSign(Color.blue));
                        ChangeScale(true,1);
                        Destroy(coll.gameObject);
                        break;
                    case "triangle":
                        Vector3 dif = coll.transform.position - transform.position;
                        coll.gameObject.GetComponent<Objects>().ChangeState(ObjectState.bouncing);
                        coll.gameObject.GetComponent<Objects>().MoveDir = dif.normalized;
                        break;
                    case "bomb":
                        if(!CkeckIfhasBuff(Buffname.Invincible))
                        {
                            StartCoroutine(ColorSign(Color.red));
                            ChangeScale(false,1);
                        }
                        Destroy(coll.gameObject);
                        break;
                    case "special1":
                        Buff invincible = new Buff(Buffname.Invincible, 5);
                        buffs.Add(invincible);
                        glowingCo = StartCoroutine(GlowingColorSign());
                        Destroy(coll.gameObject);
                        break;
                }
                break;
            case GameLevel.Color:
                Objects objInfo = coll.GetComponent<Objects>();
                SpriteRenderer objRenderer = coll.GetComponent<SpriteRenderer>();
                bool sameColor = false;
                bool sameShape = false;

                if(myRender.color == Color.white)
                {
                    myRender.color = objRenderer.color;
                    myRender.sprite = objRenderer.sprite;
                    ChangeCollider(objInfo.objShape);
                }
                else
                {
                    if(myRender.color==objRenderer.color)
                        sameColor = true;
                    if(currentShape==objInfo.objShape)
                        sameShape = true;

                    if(GameManager.GetInstance.gameMode==GameMode.Shape)
                    {
                        if(sameShape)
                        {
                            if(sameColor)
                                ChangeScale(true,2);
                            else
                            {
                                ChangeScale(true,1);
                                myRender.color = objRenderer.color;
                            }
                        }
                        else
                        {
                            if(sameColor)
                                ChangeScale(false,1);
                            else
                                ChangeScale(false,2);
                        }
                    }
                    else if(GameManager.GetInstance.gameMode==GameMode.Color)
                    {
                        if(sameColor)
                        {
                            if(sameShape)
                                ChangeScale(true,2);
                            else
                            {
                                ChangeScale(true,1);
                                myRender.color = objRenderer.color;
                                myRender.sprite = objRenderer.sprite;
                                ChangeCollider(objInfo.objShape);
                            }
                        }
                        else
                        {
                            if(sameShape)
                                ChangeScale(false,1);
                            else
                                ChangeScale(false,2);
                        }
                    }
                }
                Destroy(coll.gameObject);
                break;
        }
    }
        
    public void ChangeCollider(Shape shape)
    {
        if(shape == Shape.Round)
        {
            GetComponent<CircleCollider2D>().enabled = true;
            GetComponent<PolygonCollider2D>().enabled = false;
        }
        else if(shape == Shape.Rectangle && shape == Shape.Triangle)
        {
            GetComponent<CircleCollider2D>().enabled = false;
            GetComponent<PolygonCollider2D>().enabled = true;
        }
        currentShape = shape;
        goalTransform.GetComponent<SpriteRenderer>().sprite = GameManager.GetInstance.objectsGoal_shape[(int)shape];
    }

    private void ChangeScale(bool active, float BonusValue)
    {
        float ratio = 0;
        float changeValue = 0;
        bool enableChange = false;
        if(active)
        {
            changeValue = value_bigger*BonusValue;
            ratio = (myScale+changeValue) / myScale;
            enableChange = true;
        }
        else
        {
            if(!CkeckIfhasBuff(Buffname.Invincible))
            {
                changeValue = value_small*BonusValue;
                ratio = (myScale-changeValue) / myScale;
                changeValue *= -1;
                enableChange = true;
            }
        }

        if(enableChange)
        {
            transform.localScale += new Vector3(changeValue,changeValue,0);
            if(ratio!=0)
                goalTransform.localScale /= ratio;
            myScale += changeValue;
            string signString = "";
            if(active)
                signString += "+";
            else
                signString += "-";   
            signString += BonusValue;
            GameObject sign = Instantiate(GameManager.GetInstance.signPrefab,transform.position,Quaternion.identity) as GameObject;
            sign.GetComponent<Sign>().ChangeText(signString);
            sign.transform.SetParent (GameObject.Find("Canvas").transform, false);
            sign.transform.position = transform.position;
            CheckMode();
        }
    }

    private void CheckMode()
    {
        if(myScale<= 0.5f)
        {
            transform.localScale = Vector3.zero;
            mode = Mode.Dead;
            GameManager.GetInstance.GameResult(mode);
        }
        else if(myScale>= goalValue)
        {
            mode = Mode.Win;
            GameManager.GetInstance.GameResult(mode);
        }
        else
        {
            StartCoroutine(Behit());
        }
    }

    public void RestartGame()
    {
        //"Local"
        transform.localScale = Vector3.one * 2;
        transform.localPosition = Vector3.zero;
        transform.FindChild("circle_hollow").localScale = Vector3.one;
        myScale = 2;
        mode = Mode.Pause;
        myRender.color = Color.white;
        goalTransform.localScale = Vector3.one * goalScale;

        //"GameManager"
        GameManager.GetInstance.RestartLevel();
    }

    //Collide with objects that will affect the size of player
    IEnumerator Behit()
    {
        mode = Mode.BeingHit;
        myRender.enabled = false;
        yield return new WaitForSeconds(0.1f);
        myRender.enabled = true;
        mode = Mode.Alive;
        yield return 0;
    }

    IEnumerator ColorSign(Color color)
    {
        myRender.color = color;
        yield return new WaitForSeconds(0.1f);
        myRender.color = Color.white;
        yield return 0;
    }

    IEnumerator GlowingColorSign()
    {
        while(true)
        {
            myRender.color = Color.blue;
            yield return new WaitForSeconds(0.05f);
            myRender.color = Color.green;
            yield return new WaitForSeconds(0.05f);
            myRender.color = Color.yellow;
            yield return new WaitForSeconds(0.05f);
        }
    }
}
