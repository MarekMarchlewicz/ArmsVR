using UnityEngine;

public class Dummy : MonoBehaviour
{
    public void Hit()
    {
        GetComponent<Animation>().Play();
    }
}
