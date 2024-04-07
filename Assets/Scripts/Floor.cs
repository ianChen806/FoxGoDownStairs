using UnityEngine;
using UnityEngine.Serialization;

public class Floor : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("MoveSpeed")]
    private int moveSpeed = 2;

    private void Update()
    {
        transform.Translate(0, moveSpeed * Time.deltaTime, 0);
        if (transform.position.y > 6f)
        {
            Destroy(gameObject);
            transform.parent.GetComponent<FloorManager>().CreateFloor();
        }
    }
}
