using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondBreakEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particle;
    [SerializeField] private Texture[] _colorParticles = new Texture[0];

    public int Color
    {
        set
        {
            var particleSystemRenderer = _particle.GetComponent<ParticleSystemRenderer>();
            var material = particleSystemRenderer.material;
            material.mainTexture = _colorParticles[value];
        }
    }
}
