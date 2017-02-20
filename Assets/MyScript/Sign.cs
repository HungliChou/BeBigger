using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Sign : MonoBehaviour {

    void Start()
    {}

    public void FinishAnimation()
    {
        Destroy(gameObject);
    }
        
    public void ChangeText(string text)
    {
        GetComponent<Text>().text = text;
    }
}
