using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphy
{
    public class GyConsts : MonoBehaviour
    {
        // Pre-defined (Default) constants
        public float Vacuum_Permittivity = 8.8541878128E-12f;
        public float HookesLaw_K = 0.1f;
        public float HookesLaw_L = 2f;
        public float Friction = 0.1f;
        public float Particle_Mass = 9.10938356E-31f;
        public float Particle_Charge = 1.60217662E-19f;

        public static float fParticle_Charge { get; private set; }

        public static float fParticle_Mass { get; private set; }

        public static float fVacuum_Permittivity { get; private set; }

        public static float fCoulombsLaw_K { get; private set; }

        public static float fHookesLaw_K { get; private set; }
        public static float fHookesLaw_L { get; private set; }

        public static float fFriction { get; private set; }

        private void Start() => Update();

        private void Update()
        {
            fParticle_Charge = Particle_Charge;
            fParticle_Mass = Particle_Mass;
            fVacuum_Permittivity = Vacuum_Permittivity;
            fCoulombsLaw_K = 1.0f / (4.0f * Mathf.PI * fVacuum_Permittivity);
            fHookesLaw_K = HookesLaw_K;
            fHookesLaw_L = HookesLaw_L;
            fFriction = Friction;
        }
    }
}