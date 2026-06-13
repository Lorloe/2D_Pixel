using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField] private GameObject destroyVFX;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Throwable"))
        {
            Destroy(gameObject);
            Instantiate(destroyVFX, transform.position, Quaternion.identity);
        }
    }
}
