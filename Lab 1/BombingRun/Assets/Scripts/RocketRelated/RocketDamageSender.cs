using UnityEngine;

/// <summary>
/// Sends damage to the Rocket script
/// 
/// @Author Michael Frye
/// </summary>
public class RocketDamageSender : MonoBehaviour {

    public float health = 20f;
    private float originalHealth;
    private Rocket baseRocket;

    private void Start()
    {
        originalHealth = health;
        baseRocket = transform.parent.GetComponent<Rocket>();
    }

    /// <summary>
    /// Sends damage to the Rocket
    /// Gets called by TurretGun.cs
    /// </summary>
    /// <param name="amount"></param>
    public void ApplyDamage(float amount)
    {
        health -= amount;

        if(health <= 0)
        {
            baseRocket.DestroyRocket();
            health = originalHealth;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (baseRocket.IsBeingUsed() && collision.transform.tag.Equals("Untagged", System.StringComparison.Ordinal))
        {
            baseRocket.DestroyRocket();
        }
    }
}
