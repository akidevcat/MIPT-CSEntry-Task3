using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Graphy
{
    public class GyUI : MonoBehaviour
    {
        [Header("Links")]
        public InputField UISeed;
        public Slider UIParticleCount;
        public Slider UIParticleConnections;
        public Slider UIParticlePositive;
        public Toggle UIShowConnections;
        public Toggle UIFreezeParticles;

        private GyCore core;

        private void Start()
        {
            core = GetComponent<GyCore>();
        }

        public void Generate()
        {
            core.SeedString = UISeed.text;
            core.ParticleCount = (int)UIParticleCount.value;
            core.ParticleConnections = UIParticleConnections.value;
            core.PositiveParticles = UIParticlePositive.value;

            core.Generate();
        }

        private void Update()
        {
            core.Freeze = UIFreezeParticles.isOn;
            core.DrawLines = UIShowConnections.isOn;
        }

    }
}