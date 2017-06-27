using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: removal of backgrounds are not accurately telling their left and right buddies about it
[RequireComponent (typeof(SpriteRenderer))] //always make sure we have a sprite renderer
public class Tiling : MonoBehaviour {

    //offset so we dont get clipping errors
    public int offsetX = 2;
    //background specific offset for position - each background sprite is slightly different TODO: fix this so all background sprites are at least consistent - kinda an art thing
    public int positionOffset = 0;
    //determines if after instantiation we move the background
    public bool shouldParallax;
    //used for checking if we need to instantiate backgrounds
    public bool hasRightBuddy = false, hasLeftBuddy = false;

    private Transform leftBuddy, rightBuddy;

    //used for background elements who're not tilable
    public bool reversScale = false;
    //width of our sprite
    private float spriteWidth = 0f;
    //camera ref
    private Camera _cam;

    private void Awake() {
        //store Camera ref
        _cam = Camera.main;
    }

    // Use this for initialization
    void Start () {
        //Grab the sprite renderer
        SpriteRenderer sRenderer = GetComponent<SpriteRenderer>();
        //gives width of element no matter how we size it
        spriteWidth = sRenderer.sprite.bounds.size.x;
	}
	
	// Update is called once per frame
	void Update () {
        float dir = transform.position.x - _cam.transform.position.x;
        if (Mathf.Abs(dir) > spriteWidth * 2) {
            if(rightBuddy != null) {
                rightBuddy.GetComponent<Tiling>().hasLeftBuddy = false;
            }
            if(leftBuddy != null) {
                leftBuddy.GetComponent<Tiling>().hasRightBuddy = false;
            }

            GameMaster.instance.GetComponent<Parallaxing>().RemoveParallax(transform);

            Destroy(gameObject);
        }

        //check if a buddy is needed
        if (!hasLeftBuddy || !hasRightBuddy) {
            //Calculate hald the width of what the camera can see in world coordinates
            float camHorizontalExtend = _cam.orthographicSize * (Screen.width / Screen.height); //center of cam to right bar = half the cam view

            //calculate the x position where the camera can see the edge of the sprite
            float visibleEdgePosRight = (transform.position.x + ((spriteWidth - positionOffset) / 2f) - camHorizontalExtend);
            float visibleEdgePosLeft = (transform.position.x - ((spriteWidth - positionOffset) / 2f) + camHorizontalExtend);

            //check if tthe position of the camera is >- to where the element is visible
            if(_cam.transform.position.x >= visibleEdgePosRight - offsetX && !hasRightBuddy) {//don't instantiate if right buddy already exists
                CreateBuddy(1); //right
                hasRightBuddy = true;
            }else if (_cam.transform.position.x <= visibleEdgePosLeft + offsetX && !hasLeftBuddy) {
                CreateBuddy(-1);//left
                hasLeftBuddy = true;
            }
        }
	}

    private void CreateBuddy(int direction) {  //-1 <----  ---> 1
        //calculating the new position for the new buddy
        Vector3 newPosition = new Vector3(transform.position.x + (spriteWidth - positionOffset) * direction, transform.position.y, transform.position.z);
        Transform newBuddy = Instantiate(transform, newPosition, transform.rotation) as Transform; //close the object

        //flips the background sprite so it matches the seam perfectly. Thanks Brackeys!
        if (reversScale) {//placeholder atm
            newBuddy.localScale = new Vector3(newBuddy.localScale.x * -1, newBuddy.localScale.y, newBuddy.localScale.z);//invert the x scale of the new buddy to perfectly loop
        }

        //set clones parent
        newBuddy.parent = transform.parent;
        if(direction > 0) {
            leftBuddy = newBuddy;
            Debug.Log("This object: " + gameObject.name + "'s left buddy is: " + leftBuddy.gameObject.name);
            newBuddy.GetComponent<Tiling>().hasLeftBuddy = true;//the opposite side we're creating a buddy on already has a buddy so tell it so
        }else {
            rightBuddy = newBuddy;
            Debug.Log("This object: " + gameObject.name + "'s right buddy is: " + rightBuddy.gameObject.name);
            newBuddy.GetComponent<Tiling>().hasRightBuddy = true;//same as above
        }
        if (shouldParallax) {//only some backgorund elements should parallax, the back sky should not
            GameMaster.instance.GetComponent<Parallaxing>().AddParallax(newBuddy);
        }
    }
}
