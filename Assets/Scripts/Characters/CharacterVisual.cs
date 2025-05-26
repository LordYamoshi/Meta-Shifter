using UnityEngine;
using System.Collections.Generic;

namespace MetaBalance.Characters
{
    /// <summary>
    /// Visual representation of a character on the 2.5D board
    /// </summary>
    public class CharacterVisual : MonoBehaviour
    {
        [Header("Visual Elements")]
        [SerializeField] private Renderer mainRenderer;
        [SerializeField] private ParticleSystem effectParticles;
        [SerializeField] private Transform winRateIndicator;
        [SerializeField] private Transform popularityIndicator;
        
        [Header("State Colors")]
        [SerializeField] private Color balancedColor = Color.white;
        [SerializeField] private Color overpoweredColor = Color.red;
        [SerializeField] private Color underpoweredColor = Color.blue;
        
        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private float bobHeight = 0.2f;
        [SerializeField] private float bobSpeed = 1f;
        
        // Reference to character
        private GameCharacter _character;
        
        // Original position for animation
        private Vector3 _originalPosition;
        
        private void Awake()
        {
            if (mainRenderer == null)
            {
                mainRenderer = GetComponentInChildren<Renderer>();
            }
            
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
            
            // Store original position
            _originalPosition = transform.position;
        }
        
        public void Initialize(GameCharacter character)
        {
            _character = character;
            
            // Initial update
            UpdateVisual();
        }
        
        private void Update()
        {
            if (_character == null)
                return;
                
            // Bobbing animation
            if (_character.GetCurrentState() is OverpoweredState)
            {
                float newY = _originalPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
            else
            {
                // Smoothly return to original height if not overpowered
                transform.position = Vector3.Lerp(transform.position, 
                    new Vector3(transform.position.x, _originalPosition.y, transform.position.z), 
                    Time.deltaTime * 3f);
            }
            
            // Update win rate indicator rotation
            if (winRateIndicator != null)
            {
                float winRate = _character.GetStat(CharacterStat.WinRate);
                float rotationSpeed = (winRate - 50f) * 0.1f;
                winRateIndicator.Rotate(0, rotationSpeed, 0);
            }
        }
        
        public void UpdateVisual()
        {
            if (_character == null)
                return;
                
            // Update color based on state
            UpdateStateColor();
            
            // Update particles
            UpdateParticleEffects();
            
            // Update indicators
            UpdateIndicators();
            
            // Update animation state
            UpdateAnimation();
        }
        
        private void UpdateStateColor()
        {
            if (mainRenderer != null && mainRenderer.material.HasProperty("_Color"))
            {
                ICharacterState state = _character.GetCurrentState();
                
                if (state is OverpoweredState)
                {
                    mainRenderer.material.color = overpoweredColor;
                }
                else if (state is UnderpoweredState)
                {
                    mainRenderer.material.color = underpoweredColor;
                }
                else
                {
                    mainRenderer.material.color = balancedColor;
                }
            }
        }
        
        private void UpdateParticleEffects()
        {
            if (effectParticles == null)
                return;
                
            float winRate = _character.GetStat(CharacterStat.WinRate);
            
            var emission = effectParticles.emission;
            var main = effectParticles.main;
            
            if (winRate > 60f)
            {
                // Very overpowered - lots of particles
                emission.rateOverTime = 50f;
                main.startColor = overpoweredColor;
                
                if (!effectParticles.isPlaying)
                {
                    effectParticles.Play();
                }
            }
            else if (winRate < 40f)
            {
                // Very underpowered - some particles
                emission.rateOverTime = 30f;
                main.startColor = underpoweredColor;
                
                if (!effectParticles.isPlaying)
                {
                    effectParticles.Play();
                }
            }
            else
            {
                // Balanced - no particles
                if (effectParticles.isPlaying)
                {
                    effectParticles.Stop();
                }
            }
        }
        
        private void UpdateIndicators()
        {
            // Update win rate indicator
            if (winRateIndicator != null)
            {
                float winRate = _character.GetStat(CharacterStat.WinRate);
                
                // Scale indicator based on win rate
                float scale = winRate / 50f;
                winRateIndicator.localScale = new Vector3(scale, scale, scale);
            }
            
            // Update popularity indicator
            if (popularityIndicator != null)
            {
                float popularity = _character.GetStat(CharacterStat.Popularity);
                
                // Enable/disable based on popularity threshold
                popularityIndicator.gameObject.SetActive(popularity > 60f);
                
                // Scale based on popularity
                float scale = popularity / 100f;
                popularityIndicator.localScale = new Vector3(scale, scale, scale);
            }
        }
        
        private void UpdateAnimation()
        {
            if (animator == null)
                return;
                
            ICharacterState state = _character.GetCurrentState();
            
            // Set animation parameters based on character state
            animator.SetBool("IsOverpowered", state is OverpoweredState);
            animator.SetBool("IsUnderpowered", state is UnderpoweredState);
            
            // Set win rate parameter for blend trees if used
            animator.SetFloat("WinRate", _character.GetStat(CharacterStat.WinRate) / 100f);
        }
    }
}