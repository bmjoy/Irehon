using UnityEngine;

// This script spins the propeller of the WindmillBase object spin along its local Z axis.
public class PropellerSpin : MonoBehaviour
{

    public float spinSpeed = 13;

    // Update is called once per frame
    private void Update()
    {
        this.transform.RotateAround(this.transform.position, this.transform.forward, Time.deltaTime * -this.spinSpeed);
    }
}
