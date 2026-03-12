// BikeStepAnimator.cs
// Drives Unity Animator components at each TaskManager step transition.
//
// Inspector setup — assign one StepAnimationSet per step (index matches RepairStep index):
//   Step 0 (Seat)        → assign arrow guide Animator(s) to onStepStart
//   Step 1 (FrontWheel)  → assign Axle_Animated Animator; ArrowParent Animator variants
//   Step 2 (Pedal)       → assign ArmsParent Animator, crank/pedal Animators
//
// The Animator must have its controller assigned (e.g. "Axle Animated.controller").
// The component plays the default state (index 0) on the base layer when the step fires.
// If you want a named trigger instead, set startTrigger / completeTrigger on the set.

using System;
using UnityEngine;

namespace MRBike
{
    [Serializable]
    public class StepAnimationSet
    {
        [Tooltip("Animators to enable + play when this step starts.")]
        public Animator[] onStepStart;

        [Tooltip("Optional Animator trigger name to set instead of Play(0). Leave empty to use Play(0).")]
        public string startTrigger = "";

        [Tooltip("Animators to enable + play when this step completes.")]
        public Animator[] onStepComplete;

        [Tooltip("Optional Animator trigger name to set on completion. Leave empty to use Play(0).")]
        public string completeTrigger = "";
    }

    /// <summary>
    /// Wire to a TaskManager and configure one StepAnimationSet per repair step.
    /// Animators are automatically enabled when the step fires and disabled when
    /// the step is complete (so they don't keep looping).
    /// </summary>
    public class BikeStepAnimator : MonoBehaviour
    {
        [SerializeField] private TaskManager m_taskManager;

        [Tooltip("One entry per repair step (same order as TaskManager.m_steps).")]
        [SerializeField] private StepAnimationSet[] m_stepAnimations;

        private void OnEnable()
        {
            if (m_taskManager == null) return;
            m_taskManager.OnStepStarted.AddListener(OnStepStarted);
            m_taskManager.OnStepCompleted.AddListener(OnStepCompleted);
        }

        private void OnDisable()
        {
            if (m_taskManager == null) return;
            m_taskManager.OnStepStarted.RemoveListener(OnStepStarted);
            m_taskManager.OnStepCompleted.RemoveListener(OnStepCompleted);
        }

        private void OnStepStarted(int stepIndex)
        {
            if (!TryGetSet(stepIndex, out var set)) return;
            PlayAnimators(set.onStepStart, set.startTrigger);
        }

        private void OnStepCompleted(int stepIndex)
        {
            if (!TryGetSet(stepIndex, out var set)) return;
            PlayAnimators(set.onStepComplete, set.completeTrigger);

            // Stop the "start" animators so guidance arrows don't keep looping.
            StopAnimators(set.onStepStart);
        }

        // ─────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────

        private bool TryGetSet(int index, out StepAnimationSet set)
        {
            set = null;
            if (m_stepAnimations == null || index < 0 || index >= m_stepAnimations.Length)
                return false;
            set = m_stepAnimations[index];
            return set != null;
        }

        private static void PlayAnimators(Animator[] animators, string trigger)
        {
            if (animators == null) return;
            foreach (var anim in animators)
            {
                if (anim == null) continue;
                anim.enabled = true;
                if (!string.IsNullOrEmpty(trigger))
                    anim.SetTrigger(trigger);
                else
                    anim.Play(0, 0, 0f);   // restart default state on base layer
            }
        }

        private static void StopAnimators(Animator[] animators)
        {
            if (animators == null) return;
            foreach (var anim in animators)
            {
                if (anim == null) continue;
                anim.enabled = false;
            }
        }
    }
}
