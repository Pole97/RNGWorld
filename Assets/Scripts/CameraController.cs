using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public float moveSpeed = 1f;
    public float scrollSpeed = 100f;

    void Update() {
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) {
            transform.position += moveSpeed * new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0) {
            transform.position += scrollSpeed * new Vector3(0, 0, -Input.GetAxis("Mouse ScrollWheel"));
        }
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, -2500, 2500),
            Mathf.Clamp(transform.position.y, -2500, 2500),
            Mathf.Clamp(transform.position.z, -5000, -100));
    }
}
