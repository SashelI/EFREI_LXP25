using UnityEngine;

public class GunShooting : MonoBehaviour
{
    public GameObject projectilePrefab; // Pour lier ton Prefab de sphère
    public Transform firePoint;       // L'endroit où la sphère va apparaître

    void Update()
    {
        // Vérifie si le bouton de tir est pressé (par exemple, le bouton principal de la manette droite)
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // Vérifie si le Prefab de projectile est bien assigné
        if (projectilePrefab != null && firePoint != null)
        {
            // Crée une instance de la sphère à la position et à la rotation du point de tir
            GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        }
        else
        {
            Debug.LogError("Projectile Prefab ou Fire Point non assigné au pistolet !");
        }
    }
}