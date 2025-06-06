using System;
using UnityEngine;

namespace TransformGizmos
{
    public class GizmoController : MonoBehaviour
    {
        public Action updateMetadata;
        [SerializeField] Rotation m_rotation;
        [SerializeField] Translation m_translation;


        [SerializeField] Material m_clickedMaterial;
        [SerializeField] Material m_transparentMaterial;

        [Header("Adjustable Variables")]
        [SerializeField] GameObject m_targetObject;
        [SerializeField] float m_gizmoSize = 1;
        [SerializeField] private float fixedScale = 3.0f;

        Transformation m_transformation = Transformation.None;

        public enum Transformation
        {
            None,
            Rotation,
            Translation
        }

        void Start()
        {
          m_translation.updateMetadata += UpdateMetadataHandler;
          m_rotation.updateMetadata += UpdateMetadataHandler;
        }

        private void UpdateMetadataHandler()
        {
            updateMetadata?.Invoke();
        }

        void Update()
        {
            if (!m_targetObject) return;
            transform.SetPositionAndRotation(m_targetObject.transform.position, m_targetObject.transform.rotation);
            m_rotation.SetGizmoSize(m_gizmoSize);
            m_translation.SetGizmoSize(m_gizmoSize);
            transform.localScale = new Vector3(fixedScale, fixedScale, fixedScale);
        }

        private void ChangeTransformationState(Transformation transformation)
        {
            m_rotation.gameObject.SetActive(false);
            m_translation.gameObject.SetActive(false);

            switch (transformation)
            {
                case Transformation.None:
                    break;

                case Transformation.Rotation:
                    m_rotation.gameObject.SetActive(true);
                    break;

                case Transformation.Translation:
                    m_translation.gameObject.SetActive(true);
                    break;
            }
            m_transformation = transformation;
        }

        public void ChangeTransformationState()
        {
            switch (m_transformation)
            {
                case Transformation.None:
                    ChangeTransformationState(Transformation.Translation);
                    break;

                case Transformation.Rotation:
                    ChangeTransformationState(Transformation.None);
                    break;

                case Transformation.Translation:
                    ChangeTransformationState(Transformation.Rotation);
                    break;
            }
        }

        public void ToggleFixedScale()
        {
            fixedScale = fixedScale == 3.0f ? 1.0f : 3.0f;
            transform.localScale = new Vector3(fixedScale, fixedScale, fixedScale);
        }

        public bool CurrentStateIsNone()
        {
            return m_transformation == Transformation.None;
        }

        public void SetTargetObject(GameObject targetObject)
        {
            m_targetObject = targetObject;
            transform.SetPositionAndRotation(m_targetObject.transform.position, m_targetObject.transform.rotation);
            // transform.localScale = m_targetObject.transform.localScale;
            m_rotation.Initialization(m_targetObject, m_clickedMaterial, m_transparentMaterial);
            m_translation.Initialization(m_targetObject, m_clickedMaterial, m_transparentMaterial);
        }

        public GameObject GetTargetObject()
        {
            return m_targetObject;
        }

        public void ChangeTransformationToNone()
        {
            ChangeTransformationState(Transformation.None);
        }
  }
}
