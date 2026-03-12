// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

namespace MRBike
{
    /// <summary>
    /// Plays the intro/welcome VO on app start and fires the workbench
    /// instruction clip after a configurable delay.
    ///
    /// Inspector setup:
    ///   m_voPlayer   → TaskVOPlayer on the same prefab
    ///   m_delay      → seconds between clip-0 end and clip-1 start (default 10 s)
    ///
    /// Expected TaskVOPlayer.m_clips order:
    ///   [0] MRMZ_BikeBuild_VO_0010_Intro.wav      ← welcome speech (plays immediately)
    ///   [1] MRMZ_BikeBuild_VO_0020_Workbench.wav  ← workbench instructions
    ///   … (remaining clips played by TaskManager per step)
    /// </summary>
    public class BikeAppStarter : MonoBehaviour
    {
        [SerializeField] private TaskVOPlayer m_voPlayer;

        [Tooltip("Extra wait time after the intro clip before playing the workbench clip.")]
        [SerializeField] private float m_delay = 2f;

        private void Start()
        {
            if (m_voPlayer == null) return;

            // Clip 0 = intro/welcome speech.
            // PlayOnce(0) also internally schedules clip 1 after clip-0's length,
            // so we don't need to call PlayOnce(1) separately.
            m_voPlayer.PlayOnce(0);
        }
    }
}
