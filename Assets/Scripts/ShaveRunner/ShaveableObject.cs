using UnityEngine;
using Common;

namespace ShaveRunner
{
    public class ShaveableObject : MonoBehaviour
    {
        [Header("Shaving Settings")]
        [SerializeField] private ParticleSystem shaveParticles;
        [SerializeField] private Animator animator;
        [SerializeField] private string shaveTrigger = "Shave";

        private bool isShaved = false;

        private void OnTriggerEnter(Collider other)
        {
            if (isShaved) return;
            if (other.CompareTag(GameConstants.PlayerTag))
            {
                Shave();
            }
        }

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
        }
    }
}
