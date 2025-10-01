
using UnityEngine;

public class Repeat : MonoBehaviour
{
    private float length;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        // calcola la larghezza dello sprite
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        float camX = cam.transform.position.x;

        // quando la camera supera il bordo destro dello sprite,
        // lo sposta in avanti di due volte la sua lunghezza
        if (camX >= transform.position.x + length)
        {
            transform.position = new Vector3(
                transform.position.x + 2 * length,
                transform.position.y,
                transform.position.z
            );
        }
    }
}


/*using UnityEngine;

public class GroundRepeat : MonoBehaviour
{
    private float length;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        float camX = cam.transform.position.x;
        if (camX >= transform.position.x + length)
        {
            transform.position = new Vector3(transform.position.x + 2 * length,
                                             transform.position.y,
                                             transform.position.z);
        }
    }
}
*/