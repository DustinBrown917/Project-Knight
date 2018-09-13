using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTester : MonoBehaviour {

    private SpriteRenderer focusSR;
    private SpriteRenderer backgroundSR;
    private SpriteRenderer backsideSR;

    [SerializeField]
    private MovementCardDef cardDef;

	// Use this for initialization
	void Start () {
        focusSR = transform.Find("FocusImage").GetComponent<SpriteRenderer>();
        backgroundSR = transform.Find("BackgroundImage").GetComponent<SpriteRenderer>();
        backsideSR = transform.Find("BacksideImage").GetComponent<SpriteRenderer>();


        focusSR.sprite = Resources.Load<Sprite>(cardDef.FocusImage);
        backgroundSR.sprite = Resources.Load<Sprite>(cardDef.BackgroundImage);
        backsideSR.sprite = Resources.Load<Sprite>(cardDef.BacksideImage);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
