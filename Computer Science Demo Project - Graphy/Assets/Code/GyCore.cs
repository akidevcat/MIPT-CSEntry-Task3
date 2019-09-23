using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphy
{
    public struct GyParticle
    {
        public float Velocity;
    }

    public class GyCore : MonoBehaviour
    {
        [Header("Calculations Parameters")]
        public string SeedString = "Anime";                     // Seed is being calculated using this magic word

        [Header("Particles (Vertices) Parameters")]
        public int ParticleCount = 10;                          // Amount of particles to simulate
        [Range(0, 1f)]
        public float ParticleConnections = 0.6f;
        [Range(0, 1f)]
        public float PositiveParticles = 0.5f;
        public float CreationSphereRadius = 100f;               // Particles spawn position limit

        [Header("Visualization Parameters")]
        public Color ParticlePositiveColor = Color.white;
        public Color ParticleNegativeColor = Color.blue;
        public float ParticleSize = 1f;                         // Only for the visualization goals
        public bool DrawLines = true;                           // Draw lines between particles
        public bool Freeze = false;
        public bool ShowFPS = false;                            // Show frames per second

        [Header("Links")]
        public RectTransform UIModifier;
        public GameObject UIRemoveButton;
        public GameObject UIAddButton;
        public GameObject UIChooseButton;

        public static Vector3 AverageParticlePosition;
        public static float ParticleFar;

        private int seed;
        private bool[] connections;

        private int modA = -1;
        private int modClosest = -1;

        private GameObject pSystemGO;
        private ParticleSystem pSystem;
        private ParticleSystemRenderer pRenderer;
        private ParticleSystem.Particle[] pSystemParticles;
        private ParticleSystem.MainModule pSystemMain;
        private ParticleSystem.EmissionModule pSystemEmission;
        private ParticleSystem.ShapeModule pSystemShape;
        internal static Material pSystemMaterial { get; private set; }
        internal static Material pLineMaterial { get; private set; }

        internal static List<Vector3> pLines { get; private set; }

        void Start()
        {
            seed = HashSeedString(SeedString);
            Random.InitState(seed);

            PrepareParticleSystem();
            GenerateConnections();
            SpawnParticles();
        }

        void Update()
        {
            pLines.Clear();
            Vector3 lastAvgPos = AverageParticlePosition;
            AverageParticlePosition = Vector3.zero;
            ParticleFar = 0;
            pSystem.GetParticles(pSystemParticles);

            var mousePos = Input.mousePosition;
            float closestMousePointDelta = Mathf.Infinity;
            Vector2 closestScreenPos = Vector2.zero;
            int closestMousePoint = -1;

            if (Freeze && !pSystem.isPaused)
                pSystem.Pause();
            if (!Freeze && pSystem.isPaused)
                pSystem.Play();

            for (int i = 0; i < ParticleCount; i++)
            {
                var posA = pSystemParticles[i].position;
                var F = new Vector3();

                Vector2 screenPos = Camera.main.WorldToScreenPoint(posA);
                float screenDelta = Vector2.Distance(screenPos, mousePos);
                if (screenDelta < closestMousePointDelta)
                {
                    closestMousePointDelta = screenDelta;
                    closestMousePoint = i;
                    closestScreenPos = screenPos;
                }

                #region CoulombsLaw and HookesLaw
                for (int j = 0; j < ParticleCount; j++)
                {
                    if (i == j)
                        continue;
                    if (!connections[i + j * ParticleCount])
                        continue;

                    var posB = pSystemParticles[j].position;
                    float ABDelta = Vector3.Distance(posA, posB);
                    float chargeA = GyConsts.fParticle_Charge * (pSystemParticles[i].startColor == ParticlePositiveColor ? 1f : -1f);
                    float chargeB = GyConsts.fParticle_Charge * (pSystemParticles[j].startColor == ParticlePositiveColor ? 1f : -1f);

                    //Coulomb's Law
                    float FDelta = (GyConsts.fCoulombsLaw_K * chargeA * chargeB * -1f) /
                         (ABDelta * ABDelta); //ABDelta * ABDelta
                    //Hooke's Law
                    FDelta += (ABDelta - GyConsts.fHookesLaw_L) * GyConsts.fHookesLaw_K;

                    F += FDelta * (posB - posA).normalized;

                    if (i > j)
                    {
                        pLines.Add(posA);
                        pLines.Add(posB);
                    }
                }
                #endregion

                var a = F / GyConsts.fParticle_Mass;

                if (!Freeze)
                {
                    pSystemParticles[i].velocity += Time.deltaTime * a;

                    #region Friction
                    pSystemParticles[i].velocity *= (1 - Time.deltaTime * GyConsts.fFriction);
                    #endregion
                }

                AverageParticlePosition += posA;
                ParticleFar = Mathf.Max(ParticleFar, Vector3.Distance(lastAvgPos, posA));
            }

            AverageParticlePosition /= ParticleCount;
            pSystem.SetParticles(pSystemParticles);

            modClosest = closestMousePoint;
            UpdateModifierPosition(closestMousePoint, closestScreenPos);
        }

        private void UpdateModifierPosition(int id, Vector2 screenPos)
        {
            UIModifier.anchoredPosition = screenPos - new Vector2(Screen.width, Screen.height) / 2f;
        }

        private void GenerateConnections()
        {
            connections = new bool[ParticleCount * ParticleCount];

            List<int> constructions = new List<int>();

            for (int i = 0; i < connections.Length; i++)
                connections[i] = true;

            for (int i = 0; i < ParticleCount; i++)
            {
                bool hasConstruction = false;

                for (int q = 0; q < ParticleCount - 1; q++)
                {
                    if (q == i)
                        continue;

                    if (constructions.Contains(i + q * ParticleCount))
                    {
                        hasConstruction = true;
                        break;
                    }
                }

                if (!hasConstruction)
                {
                    int rnd = Random.Range(0, ParticleCount - 1);
                    if (rnd == i)
                        rnd++;
                    connections[i + rnd * ParticleCount] = true;
                    connections[rnd + i * ParticleCount] = true;
                    constructions.Add(i + rnd * ParticleCount);
                    constructions.Add(rnd + i * ParticleCount);
                }

                //Has ParticleCount-1 connections
                for (int q = 0; q < ParticleCount - 1; q++)
                {
                    if (q == i)
                        continue;
                    if (constructions.Contains(i + q * ParticleCount))
                        continue;

                    if (Random.value >= ParticleConnections)
                    {
                        connections[i + q * ParticleCount] = false;
                        connections[q + i * ParticleCount] = false;
                    }
                }
            }
        }

        public void Generate()
        {
            pSystem.Clear();

            seed = HashSeedString(SeedString);
            Random.InitState(seed);

            pSystemParticles = new ParticleSystem.Particle[ParticleCount];
            pSystemMain.maxParticles = ParticleCount;

            GenerateConnections();
            SpawnParticles();
        }

        private void SpawnParticles()
        {
            pSystem.Emit(ParticleCount);

            pSystem.GetParticles(pSystemParticles);

            for (int i = 0; i < ParticleCount; i++)
            {
                var pos = Random.insideUnitSphere * CreationSphereRadius;
                pSystemParticles[i].position = pos;
                pSystemParticles[i].startColor = Random.value <= PositiveParticles ? ParticlePositiveColor : ParticleNegativeColor;
            }

            pSystem.SetParticles(pSystemParticles);
        }

        private void PrepareParticleSystem()
        {
            pLines = new List<Vector3>();

            pSystemGO = new GameObject("Particles");
            pSystemGO.transform.position = Vector3.zero;

            pSystemMaterial = Resources.Load<Material>("Materials/GyParticle");
            pLineMaterial = Resources.Load<Material>("Materials/GyLine");
            pLineMaterial.SetInt("_ZWrite", 1);
            pLineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Less);

            pSystem = pSystemGO.AddComponent<ParticleSystem>();
            pRenderer = pSystemGO.GetComponent<ParticleSystemRenderer>();
            pRenderer.sharedMaterial = pSystemMaterial;
            pSystemMain = pSystem.main;
            pSystemEmission = pSystem.emission;
            pSystemShape = pSystem.shape;

            pSystemMain.startLifetime = Mathf.Infinity;
            pSystemMain.startSpeed = 0;
            pSystemMain.startSize = ParticleSize;

            pSystemParticles = new ParticleSystem.Particle[ParticleCount];
            pSystemMain.maxParticles = ParticleCount;

            pSystemEmission.enabled = false;
            pSystemShape.enabled = false;
        }

        public void AddConnectionButton ()
        {
            if (modA != modClosest)
            {
                connections[modA + modClosest * ParticleCount] = true;
                connections[modClosest + modA * ParticleCount] = true;
            }
            modA = -1;

            UIChooseButton.SetActive(true);
            UIAddButton.SetActive(false);
            UIRemoveButton.SetActive(false);
        }

        public void RemoveConnectionButton()
        {
            if (modA != modClosest)
            {
                connections[modA + modClosest * ParticleCount] = false;
                connections[modClosest + modA * ParticleCount] = false;
            }
            modA = -1;

            UIChooseButton.SetActive(true);
            UIAddButton.SetActive(false);
            UIRemoveButton.SetActive(false);
        }

        public void ChooseButton()
        {
            modA = modClosest;
            UIChooseButton.SetActive(false);
            UIAddButton.SetActive(true);
            UIRemoveButton.SetActive(true);
        }

        private static int HashSeedString(string s)
        {
            int seed = 0;
            if (s.Length > 0)
            {
                seed = s[0];
                for (int i = 0; i < s.Length; i++)
                    seed += Pow(s[i], i);
            }
            return seed;
        }

        private static int Pow (int x, int p)
        {
            return p == 0 ? 1 : x * Pow(x, p - 1);
        }

        private static float Pow2(float x) => x * x;
    }
}
