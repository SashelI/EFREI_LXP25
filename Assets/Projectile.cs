using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    private Rigidbody rb;

    void Start()
    {
        // R�cup�re le composant Rigidbody de la sph�re
        rb = GetComponent<Rigidbody>();

        // Donne une impulsion initiale � la sph�re pour la faire avancer
        rb.linearVelocity = transform.forward * speed;

        // Optionnel : D�truire la sph�re apr�s un certain temps
        Destroy(gameObject, 5f); // D�truit la sph�re apr�s 5 secondes
    }
}