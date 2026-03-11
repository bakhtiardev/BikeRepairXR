// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

namespace MRBike
{
    public class BikeAppStarter : MonoBehaviour
    {
        [SerializeField] private TaskVOPlayer m_voPlayer;

        

        [SerializeField] private float m_delay = 10;

private void Start() { m_voPlayer.PlayOnce(0); Invoke("PlayDelay", m_delay); }

private void PlayDelay() { }
    }
}
