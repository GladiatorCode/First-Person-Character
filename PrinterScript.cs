using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrinterScript : MonoBehaviour
{

    private float displayedStamina;
    private float displayedS;

    void Start()
    
    {
        displayedStamina = GameObject.Find("Player").GetComponent<PlayerMove>().stamina;
    }

    void Update()
    {
        displayedStamina = GameObject.Find("Player").GetComponent<PlayerMove>().stamina;
        displayedS = displayedStamina * 100;
    }

    void OnGUI()
    {
        
        GUI.Label(new Rect(0, 0, 120, 50),"Stamina:" + displayedS.ToString(), "box");
    }
}
