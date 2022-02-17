using UnityEngine;
using UnityEngine.UI;

public class CameraScroll : MonoBehaviour
{

    public Slider speedSlider;
    public float moveSpeed = 0.5f;

    // Use this for initialization
    private void Start()
    {
        this.speedSlider.onValueChanged.AddListener(delegate { this.ChangeSpeed(); });
    }

    // Update is called once per frame
    private void Update()
    {
        //Move the camera to the left based on current speedSlider setting
        this.transform.Translate(Vector3.left * (Time.deltaTime * this.moveSpeed));

        //If the camera passes the last animation, loop to the beginning
        if (this.transform.position.x > 110)
        {
            this.transform.position = new Vector3(0f, this.transform.position.y, this.transform.position.z);
        }
    }

    private void ChangeSpeed()
    {
        this.moveSpeed = this.speedSlider.value;
    }
}
