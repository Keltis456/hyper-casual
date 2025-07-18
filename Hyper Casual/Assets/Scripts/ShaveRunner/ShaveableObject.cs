using UnityEngine;

namespace ShaveRunner
{
    public class ShaveableObject : MonoBehaviour
    {
        [Header("Shaving Settings")]
        public ParticleSystem shaveParticles; // Particle effect to play when shaved
        public Animator animator; // Animator for shaving animation
        public string shaveTrigger = "Shave"; // Animator trigger name

        private bool isShaved = false;

        // Detect collision with player
        private void OnTriggerEnter(Collider other)
        {
            if (isShaved) return;
            if (other.CompareTag("Player"))
            {
                Shave();
            }
        }

        // Triggers the shaving animation, particle effect, and changes state
        void Shave()
        {
            isShaved = true;
            if (animator != null)
            {
                animator.SetTrigger(shaveTrigger);
            }
            if (shaveParticles != null)
            {
                shaveParticles.Play();
            }
            // Optionally: Add logic here for score, sound, etc.
        }
    }
} 