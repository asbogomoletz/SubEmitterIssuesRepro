using System.Collections.Generic;

using UnityEngine;

public class BulletsEmitterBehaviour : MonoBehaviour
{
	public float MaxRange = 500;
	public float Speed = 700;
	public float ShotInterval = 0.5f;

	private ParticleSystem _particleSystem;

	private uint _currentProjectileId;
	private float _emitTimeElapsed;

	private List<Projectile> _projectiles = new List<Projectile>();
	private List<Projectile> _emitProjectiles = new List<Projectile>();

	private ParticleSystem.Particle[] _particles = new ParticleSystem.Particle[2048];

	private void Start()
	{
		_particleSystem = GetComponent<ParticleSystem>();
	}

	private void Update()
	{
		EmitProjectiles();
		UpdateProjectilePositions();
		UpdateProjectileParticles();
		EmitProjectileParticles();
		_projectiles.RemoveAll(p => !p.IsAlive);
	}

	private void EmitProjectiles()
	{
		_emitTimeElapsed += Time.deltaTime;

		while (_emitTimeElapsed > ShotInterval)
		{
			var projectile = new Projectile
			{
				Id = _currentProjectileId++,
				IsAlive = true,
				Position = transform.position,
				Velocity = transform.forward * Speed,
				Lifetime = MaxRange / Speed
			};

			_emitProjectiles.Add(projectile);

			_emitTimeElapsed -= ShotInterval;
		}
	}

	private void UpdateProjectilePositions()
	{
		for (var i = 0; i < _projectiles.Count; i++)
		{
			var projectile = _projectiles[i];
			var deltaPosition = projectile.Velocity * Time.deltaTime;
			var oldPosition = projectile.Position;

			if (Physics.Raycast(oldPosition, projectile.Velocity.normalized, out var hit, deltaPosition.magnitude))
			{
				projectile.Position = hit.point;
				projectile.IsAlive = false;
			}
			else
			{
				projectile.Position += deltaPosition;
				projectile.Lifetime -= Time.deltaTime;

				if (projectile.Lifetime <= 0)
				{
					projectile.IsAlive = false;
				}
			}

			_projectiles[i] = projectile;
		}
	}

	private void EmitProjectileParticles()
	{
		foreach (var projectile in _emitProjectiles)
		{
			var emitParams = new ParticleSystem.EmitParams
			{
				position = projectile.Position,
				velocity = projectile.Velocity,
				randomSeed = projectile.Id,
				startLifetime = projectile.Lifetime
			};

			_particleSystem.Emit(emitParams, 1);

			_projectiles.Add(projectile);
		}

		_emitProjectiles.Clear();
	}

	private void UpdateProjectileParticles()
	{
		var particlesCount = _particleSystem.GetParticles(_particles);
		
		for (var i = 0; i < particlesCount; i++)
		{
			var particle = _particles[i];
			var projectile = _projectiles.Find(p => p.Id == particle.randomSeed);

			particle.position = projectile.Position;
			if (projectile.IsAlive)
			{
				particle.velocity = projectile.Velocity;
				particle.remainingLifetime = projectile.Lifetime;
			}
			else
			{
				particle.velocity = Vector3.zero;
				particle.remainingLifetime = -1;
			}

			_particles[i] = particle;
		}

		_particleSystem.SetParticles(_particles, particlesCount);
	}
}

public struct Projectile
{
	public uint Id;
	public Vector3 Position;
	public Vector3 Velocity;
	public float Lifetime;
	public bool IsAlive;
}