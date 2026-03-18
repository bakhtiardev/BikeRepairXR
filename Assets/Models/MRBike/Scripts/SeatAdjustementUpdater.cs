// Copyright (c) Meta Platforms, Inc. and affiliates.

using TMPro;
using UnityEngine;

namespace MRBike
{
    /// <summary>
    /// When adjusting the seat this will update the value indication of the height of the seat and update the fiducial ctrl
    /// </summary>
    public class SeatAdjustementUpdater : MonoBehaviour
    {
        [SerializeField] private TMP_Text m_valueLabel;
        [SerializeField] private FiducialCtrl m_fiducialCtrl;
        [SerializeField] private GameObject m_movingObject;
        [SerializeField] private float m_baseDistance = 0.7f;
        [SerializeField] private float m_coef = 100;
        [SerializeField] private string m_suffix = " cm";
        [SerializeField] private float m_moveCheck = 0.01f;

        [Header("Audio & Arrow")]
        [SerializeField] private AudioClip m_seatAdjustAudioClip;
        [SerializeField] private AudioClip m_seatAdjustVOClip;
        [SerializeField] private GameObject m_upDownArrow;
        [SerializeField] private float m_targetHeight = 0.65f;

        private float m_previousDistance;
        private Vector3 m_startPoint;
        private bool m_grabbed = false;
        private float m_travel;
        private bool m_arrowDisabled = false;

        private void Start()
        {
            m_startPoint = m_movingObject.transform.position;
            m_fiducialCtrl.Height = m_baseDistance * 100;
            
            // If arrow is not assigned, try to find it
            if (m_upDownArrow == null)
            {
                m_upDownArrow = gameObject;
            }
        }

        public void Grab(bool grabState)
        {
            m_grabbed = grabState;
        }

        public void PlaySnapAudio()
        {
            // Play the SFX (seatpost insert sound)
            if (m_seatAdjustAudioClip != null)
            {
                AudioSource.PlayClipAtPoint(m_seatAdjustAudioClip, transform.position);
            }
            
            // Play the VO (voice over - adjust height instruction)
            if (m_seatAdjustVOClip != null)
            {
                AudioSource.PlayClipAtPoint(m_seatAdjustVOClip, transform.position);
            }
        }

        private void Update()
        {
            var position = m_movingObject.transform.position;
            var hasMoved = m_moveCheck > 0 && Mathf.Abs(position.y - m_startPoint.y) >= m_moveCheck;
            if (!m_grabbed && !hasMoved)
            {
                return;
            }

            m_travel = (position.y - m_startPoint.y) * m_coef;

            m_startPoint = position;
            m_baseDistance -= m_travel;

            m_fiducialCtrl.Height = m_baseDistance * 100;
            m_valueLabel.text = m_baseDistance.ToString("F") + m_suffix;

            // Disable arrow when target height is reached
            if (m_baseDistance <= m_targetHeight && !m_arrowDisabled)
            {
                if (m_upDownArrow != null)
                {
                    m_upDownArrow.SetActive(false);
                    m_arrowDisabled = true;
                }
            }
            else if (m_baseDistance > m_targetHeight && m_arrowDisabled)
            {
                if (m_upDownArrow != null)
                {
                    m_upDownArrow.SetActive(true);
                    m_arrowDisabled = false;
                }
            }
        }
    }
}
