using UnityEngine;

public class Glove : MonoBehaviour
{
    public System.Action OnHit;

    private void OnTriggerEnter(Collider other)
    {
        Dummy dummy = other.GetComponent<Dummy>();

        if (dummy != null)
        {
            dummy.Hit();

            if (OnHit != null)
            {
                OnHit();
            }
        }
    }
}
