using UnityEngine;

public class 개발자루트 : MonoBehaviour
{
    public GameObject tp;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        collision.transform.position = tp.transform.position;

    }
}
