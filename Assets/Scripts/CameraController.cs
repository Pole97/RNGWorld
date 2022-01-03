using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public float moveSpeed = 1f;
    public float scrollSpeed = 100f;
    public Camera cam;

    private BoxCollider2D cameraBox;
    public BoxCollider2D boundary;

    private float leftPivot;
    private float rightPivot;
    private float topPivot;
    private float botPivot;
    private float maxCameraSize;
    private float yBoundary;

    void Start() {
        cam = Camera.main;
        cameraBox = cam.GetComponent<BoxCollider2D>();
    }

    void AspectRatioBoxChange() {
        float height = 2 * cam.orthographicSize;
        float width = height * cam.aspect;
        cameraBox.size = new Vector2(width, height);
    }

    void CalculateCameraPivot() {
        botPivot = boundary.bounds.min.y + cameraBox.size.y / 2;
        topPivot = boundary.bounds.max.y - cameraBox.size.y / 2;
        leftPivot = boundary.bounds.min.x + cameraBox.size.x / 2;
        rightPivot = boundary.bounds.max.x - cameraBox.size.x / 2;
    }

    void CalculateCameraBoundary() {
        maxCameraSize = boundary.size.x / 2f / 16f * 9f;
    }

    void Update() {
        if (boundary.size.x < cameraBox.size.x) {
            cameraBox.size = boundary.size;
        }
        AspectRatioBoxChange();
        CalculateCameraPivot();
        Vector3 targetPosition = transform.position;
        float tagetSize = cam.orthographicSize;
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) {
            targetPosition = transform.position + moveSpeed * new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0) {
            tagetSize = cam.orthographicSize - scrollSpeed * Input.GetAxis("Mouse ScrollWheel");
        }
        transform.position = new Vector3(
            Mathf.Clamp(targetPosition.x, leftPivot, rightPivot),
            Mathf.Clamp(targetPosition.y, botPivot, topPivot),
            transform.position.z);
        CalculateCameraBoundary();
        cam.orthographicSize = Mathf.Clamp(tagetSize, 100, maxCameraSize);
    }
}
