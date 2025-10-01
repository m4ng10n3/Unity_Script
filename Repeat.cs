using UnityEngine;

public class Repeat : MonoBehaviour
{
    private float length;
    private float prevCamX;
    private Camera cam;

    // Reference to the other two sprites (assign in inspector)
    [SerializeField] private Transform sprite2;
    [SerializeField] private Transform sprite3;

    private Transform[] sprites;

    void Start()
    {
        cam = Camera.main;
        // Calcola la larghezza dello sprite (assumendo stessa larghezza per tutti)
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        prevCamX = cam.transform.position.x;

        // Memorizza tutti e tre i sprite (il primo è quello con lo script)
        sprites = new Transform[] { transform, sprite2, sprite3 };
    }

    void Update()
    {
        float camX = cam.transform.position.x;
        float deltaX = camX - prevCamX;
        prevCamX = camX;

        // Calcola i limiti sinistro e destro della camera in coordinate mondo
        float camLeft = cam.ViewportToWorldPoint(new Vector3(0, 0.5f, 0)).x;
        float camRight = cam.ViewportToWorldPoint(new Vector3(1, 0.5f, 0)).x;

        // Trova il più a sinistra e il più a destra tra i tre sprite
        Transform leftmost = sprites[0];
        Transform rightmost = sprites[0];
        foreach (var s in sprites)
        {
            if (s != null)
            {
                if (s.position.x < leftmost.position.x) leftmost = s;
                if (s.position.x > rightmost.position.x) rightmost = s;
            }
        }

        // Se la camera si muove a destra, sposta l'oggetto più a sinistra dietro il più a destra
        if (deltaX > 0)
        {
            float leftmostRightEdge = leftmost.position.x + length * 0.5f;
            // Quando l'oggetto è completamente fuori dalla vista a sinistra della camera
            if (leftmostRightEdge < camLeft)
            {
                // Sposta il più a sinistra dopo il più a destra
                leftmost.position = new Vector3(
                    rightmost.position.x + length,
                    leftmost.position.y,
                    leftmost.position.z
                );
            }
        }
        // Se la camera si muove a sinistra, sposta l'oggetto più a destra davanti al più a sinistra
        else if (deltaX < 0)
        {
            float rightmostLeftEdge = rightmost.position.x - length * 0.5f;
            // Quando l'oggetto è completamente fuori dalla vista a destra della camera
            if (rightmostLeftEdge > camRight)
            {
                // Sposta il più a destra davanti al più a sinistra
                rightmost.position = new Vector3(
                    leftmost.position.x - length,
                    rightmost.position.y,
                    rightmost.position.z
                );
            }
        }
    }
}
