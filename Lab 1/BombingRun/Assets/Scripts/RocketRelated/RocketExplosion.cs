using UnityEngine;

/// <summary>
/// The explosion object in of the Rocket itself
/// When the rocket hits an object, the rocket will then leave this object after it's 'death'
/// Explosion affects: Turret.cs, PlayerController.cs, DestructableObject.cs and Rigidbody
/// 
/// @Author Michael Frye
/// </summary>
public class RocketExplosion : MonoBehaviour {

    public float explosionRadius = 5f;
    public float explosionForce = 50f;
    private Transform ParentOfOrigin;
    private Collider[] CollidersFoundInExplosion;

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.localPosition, explosionRadius);
    }

    /// <summary>
    /// When the explosion particles settle and die, return to parent of origin and disable itself.
    /// </summary>
    private void OnParticleSystemStopped()
    {
        transform.parent = ParentOfOrigin;
        transform.localPosition = Vector3.zero;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Unparents the explosion from the rocket object, taking into account what parent it came from
    /// And triggers the explosion
    /// </summary>
    /// <param name="originalParent"></param>
    public void DecupleAndTriggerExplosion(Transform originalParent)
    {
        ParentOfOrigin = originalParent;
        transform.parent = null;
        TriggerExplosion();
    }

    /// <summary>
    /// Creates an OverlapSphere and ApplyDamage and force to whatever is in range
    /// Explosion will affect: turret, player, destructableObject and rigidbody
    /// </summary>
    public void TriggerExplosion()
    {
        CollidersFoundInExplosion = Physics.OverlapSphere(transform.localPosition, explosionRadius);

        for(int i = 0; i < CollidersFoundInExplosion.Length; i++)
        {
            Turret turretObject = CollidersFoundInExplosion[i].GetComponent<Turret>();
            PlayerController playerController = CollidersFoundInExplosion[i].GetComponent<PlayerController>();
            DestructableObject destructableObjectScript = CollidersFoundInExplosion[i].GetComponent<DestructableObject>();
            Rigidbody objectRigidbody = CollidersFoundInExplosion[i].GetComponent<Rigidbody>();

            if(turretObject != null)
            {
                turretObject.KillPlayer();
            }

            if(playerController != null)
            {
                playerController.Die();
            }

            if(destructableObjectScript != null)
            {
                destructableObjectScript.DestroyObject();
            }

            if (objectRigidbody != null)
            {
                objectRigidbody.AddExplosionForce(explosionForce, transform.localPosition, explosionRadius, explosionForce / 2, ForceMode.Force);
            }
        }
    }
}
