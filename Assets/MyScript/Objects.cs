using UnityEngine;
using System.Collections;

public enum ObjectState
{
    moving, bouncing, 
}

public enum Shape
{
    Round, Rectangle, Triangle
}

public class Objects : MonoBehaviour {

    public Vector2[] startDir;
    public Transform target;
    private Character targetScript;
    private Vector3 move;
    public ObjectState state;
    private bool canDestroy;
    private float canDestroyTimer;

    //
    public Shape objShape;

    public Vector3 MoveDir{get{return move;} set{move = value;}}
	// Use this for initialization
	void Start () {
        canDestroy = false;
        canDestroyTimer = 0;
        state = ObjectState.moving;
        targetScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Character>();
        target = targetScript.transform;
        facingPlayer();
	}
	
	// Update is called once per frame
	void Update () {
        if(GameManager.GetInstance.gameLevel!=GameLevel.Menu)
        {
            if(targetScript.MyMode==Mode.Pause || targetScript.MyMode==Mode.Win || targetScript.MyMode==Mode.Dead)
                return;
            Moving();
        }
        else
            Moving();
	}

    void Moving()
    {
        transform.position += move*2;
        CheckIsOut();
    }
    void facingPlayer()
    {
        Vector3 dif = target.position - transform.position;
        move = dif.normalized;

        if(tag=="bomb")
        {
            float angle = Vector3.Angle(dif,transform.up);
            if(transform.position.x<0)
                transform.Rotate(0,0,-angle);
            else
                transform.Rotate(0,0,angle);
        }

    }

    public void ChangeState(ObjectState _state)
    {
        state = _state;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject!=gameObject && col.tag!= "bound")
        {
            if(col.tag=="Player")
            {
                col.GetComponent<Character>().DoAction(gameObject);
            }
            else
            {
                if(GameManager.GetInstance.gameLevel==GameLevel.Normal)
                {
                    if(tag=="triangle")
                    {
                        Vector3 dif = col.transform.position - transform.position;
                        Objects targetObj = col.gameObject.GetComponent<Objects>();
                        if(targetObj!=null)
                        {
                            targetObj.ChangeState(ObjectState.bouncing);
                            targetObj.MoveDir = dif.normalized;
                        }
                     }
                }
            }
        }
    }

    void CheckIsOut()
    {
        if(!canDestroy)
        {
            if(canDestroyTimer>1)
            {
                canDestroy = true;
            }
            else
                canDestroyTimer += Time.deltaTime;
        }
        else
        {
            if(transform.position.x>200||transform.position.x<-200||transform.position.y>200||transform.position.y<-200)
                Destroy(gameObject);
        }
    }
            
    public void ChangeCollider(string col)
    {
        if(col == "round")
        {
            GetComponent<CircleCollider2D>().enabled = true;
            GetComponent<PolygonCollider2D>().enabled = false;
        }
        else if(col == "poly")
        {
            GetComponent<CircleCollider2D>().enabled = false;
            GetComponent<PolygonCollider2D>().enabled = true;
        }
    }
}
