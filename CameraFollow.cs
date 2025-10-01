using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Il target che la camera seguir� (il nostro giocatore)
    public Transform target;

    // Velocit� con cui la camera si "aggancia" al giocatore. Valori pi� bassi = movimento pi� lento e fluido.
    public float smoothSpeed = 0.125f;

    // L'offset della camera rispetto al giocatore (soprattutto per l'asse Z)
    public Vector3 offset;

    void LateUpdate()
    {
        // Controlla se il target (giocatore) � stato assegnato
        if (target != null)
        {
            // Posizione desiderata della camera
            Vector3 desiredPosition = target.position + offset;

            // Interpola linearmente dalla posizione attuale a quella desiderata per un movimento fluido
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // Applica la nuova posizione alla camera
            transform.position = smoothedPosition;
        }
    }
}