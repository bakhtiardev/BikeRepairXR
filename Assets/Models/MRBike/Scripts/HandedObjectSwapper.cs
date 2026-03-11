// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MRBike
{
    public class HandedObjectSwapper : MonoBehaviour
    {
        [FormerlySerializedAs("rightHandObject")][SerializeField] private GameObject m_rightHandObject;
        [FormerlySerializedAs("leftHandObject")][SerializeField] private GameObject m_leftHandObject;
        [SerializeField] private UnityEvent m_onTrigger;

        private bool m_currentHandednessIsRight = true;

        public void SwapObjects()
        {
            if (m_rightHandObject.activeSelf)
            {
                m_rightHandObject.SetActive(false);
                m_leftHandObject.SetActive(true);
            }
            else
            {
                m_rightHandObject.SetActive(true);
                m_leftHandObject.SetActive(false);
            }
        }

public void SetRight() { if (!m_currentHandednessIsRight) { m_currentHandednessIsRight = true; m_onTrigger?.Invoke(); } }

public void SetLeft() { if (m_currentHandednessIsRight) { m_currentHandednessIsRight = false; m_onTrigger?.Invoke(); } }
    }
}
