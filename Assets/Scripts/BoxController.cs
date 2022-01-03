using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController : MonoBehaviour {
    public GameObject world;
    BoxCollider2D boundary;
    float sizeBoundary;

    void Start() {
        boundary = this.transform.gameObject.GetComponent<BoxCollider2D>();
        sizeBoundary = world.transform.localScale.x * 10;
        boundary.size = new Vector2(sizeBoundary, sizeBoundary);
    }

    void Update() {
        if (sizeBoundary != world.transform.localScale.x * 10) {
            sizeBoundary = world.transform.localScale.x * 10;
            boundary.size = new Vector2(sizeBoundary, sizeBoundary);
        }
    }
}
