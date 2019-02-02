using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gun script that fires particles. Applies damage to rocket.cs when particles collide with it.
/// Also plays an animation to show that it's firing particles.
/// 
/// @Author Michael Frye
/// </summary>
public class TurretGun : MonoBehaviour {

    private float damagePerParticle = 7f;
    private ParticleSystem particleBullets;
    private List<ParticleCollisionEvent> collisionEvents;
    private Animation animationComponent;

    public void Start()
    {
        particleBullets = GetComponent<ParticleSystem>();
        animationComponent = transform.parent.GetChild(1).GetComponent<Animation>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    public bool AnimationComponentIsPlaying()
    {
        return animationComponent.isPlaying;
    }

    /// <summary>
    /// Plays an animation on the gun to make it look like it's shooting
    /// </summary>
    public void PlayFireAnimation()
    {
        animationComponent.Play();
    }
    
    /// <summary>
    /// Plays a particle system (Shoots one particle)
    /// </summary>
    public void Fire()
    {
        particleBullets.Play(true);
    }

    /// <summary>
    /// Sends damage to the rocket if there were any particle collision events with the rocket
    /// </summary>
    /// <param name="other"></param>
    private void OnParticleCollision(GameObject other)
    {
        int numberOfCollisions = particleBullets.GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < numberOfCollisions; i++)
        {
            if (collisionEvents[i].colliderComponent.transform.tag.Equals("Rocket", System.StringComparison.Ordinal))
            {
                collisionEvents[i].colliderComponent.gameObject.GetComponent<RocketDamageSender>().ApplyDamage(damagePerParticle);
            }

        }
    }

}
