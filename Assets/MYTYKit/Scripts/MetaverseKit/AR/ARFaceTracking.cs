using MYTYKit.MotionTemplates;
using UnityEngine;

namespace MYTYCamera.AR
{
    public class ARFaceTracking : MonoBehaviour
    {
        [SerializeField]
        MeshRenderer m_arBounds;
        
        private Vector3[] m_vertices;

        public Transform plane;
        public bool is2DRotation;

        private Quaternion m_planeLocalRot;
        public PointsTemplate m_pointsModel;
        
        public float scale;
        public float yOffset;
        public float xOffset;

        // Start is called before the first frame update
        private void Start()
        {
            m_vertices = new Vector3[468];

            m_planeLocalRot = plane.localRotation;
        }

        // Update is called once per frame
        private void Update()
        {
            if (m_pointsModel.points.Length < 468) return;
            var sumPosition = Vector3.zero;
            var bounds = m_arBounds.bounds;
            for (var i = 0; i < 468; i++)
            {
                m_vertices[i] = new Vector3(-(m_pointsModel.points[i].x+0.5f) * bounds.size.x,
                    (m_pointsModel.points[i].y+0.5f) * bounds.size.y,
                    m_pointsModel.points[i].z+0.5f);
                sumPosition += m_vertices[i];
            }

            sumPosition /= 468;
            sumPosition += new Vector3(xOffset, yOffset, 0);

            var up = (m_vertices[10] - m_vertices[152]).normalized;
            var left2right = (m_vertices[454] - m_vertices[234]).normalized;
            var lookAt = Vector3.Cross(up, left2right);

            plane.localPosition = new Vector3(sumPosition.x, sumPosition.y, -5);
            if (is2DRotation)
                plane.localRotation = Quaternion.LookRotation(Vector3.back, up) * m_planeLocalRot;
            else
                plane.localRotation = Quaternion.LookRotation(
                    new Vector3(lookAt.x * bounds.size.x, 
                        lookAt.y * bounds.size.y, lookAt.z), up) * m_planeLocalRot;

            var height = (m_vertices[10] - m_vertices[152]).magnitude;
            var width = (m_vertices[454] - m_vertices[234]).magnitude;
            var size = (width + height) / 2 * scale;

            plane.localScale = new Vector3(size, size, size);
        }
    }
}