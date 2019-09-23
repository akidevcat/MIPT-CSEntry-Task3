using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphy {
    public class CamView : MonoBehaviour
    {
        public float RadiusMultiplier = 1.5f;
        public float SensitivityX = 6f;
        public float SensitivityY = 6f;

        private Quaternion CamRotation = Quaternion.identity;

        public GyCore gyCore;

        private float mx, my;

        private float particleFarAvg;

        void Update()
        {
            particleFarAvg = Mathf.Lerp(particleFarAvg, GyCore.ParticleFar, Time.deltaTime * 1f);

            if (Input.GetKey(KeyCode.Mouse0))
            {
                mx += Input.GetAxis("Mouse X") * Time.deltaTime * SensitivityX;
                my += Input.GetAxis("Mouse Y") * Time.deltaTime * SensitivityY;
                mx = Mathf.Min(mx, 1f);
                my = Mathf.Min(my, 1f);
            }
            mx = Mathf.Lerp(mx, 0, Time.deltaTime * 2f);
            my = Mathf.Lerp(my, 0, Time.deltaTime * 2f);
            Vector3 dir = Vector3.up * mx + Vector3.left * my;

            transform.LookAt(GyCore.AverageParticlePosition);
            transform.RotateAround(GyCore.AverageParticlePosition, transform.rotation * dir, Time.deltaTime * dir.magnitude * 100f);
            transform.position = (transform.position - GyCore.AverageParticlePosition).normalized * Mathf.Max(1f, particleFarAvg) * RadiusMultiplier + GyCore.AverageParticlePosition;
        }

        void OnPostRender()
        {
            if (!gyCore.DrawLines)
                return;

            for (int i = 0; i < GyCore.pLines.Count; i += 2)
            {
                GL.Begin(GL.LINES);
                GyCore.pLineMaterial.SetPass(0);
                GL.Color(Color.white);
                GL.Vertex(GyCore.pLines[i]);
                GL.Vertex(GyCore.pLines[i + 1]);
                GL.End();
            }
        }
    }
}