using OctoberStudio.Easing;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class PentaHeavyCannonWeaponAbilityBehavior : AbilityBehavior<PentaHeavyCannonWeaponAbilityData, PentaHeavyCannonWeaponAbilityLevel>
    {
        public static readonly int HEAVY_CANNON_LAUNCH_HASH = "Wand Projectile Launch".GetHashCode();

        [SerializeField] GameObject projectilePrefab;
        public GameObject ProjectilePrefab => projectilePrefab;

        private PoolComponent<SimplePlayerProjectileBehavior> projectilePool;
        private readonly List<SimplePlayerProjectileBehavior> projectiles = new();

        IEasingCoroutine projectileCoroutine;
        Coroutine abilityCoroutine;

        private float AbilityCooldown => AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier;

        private void Awake()
        {
            // Augmenté pour supporter 5 projectiles simultanés par tir sur plusieurs salves
            projectilePool = new PoolComponent<SimplePlayerProjectileBehavior>("Penta Heavy Cannon Projectile", ProjectilePrefab, 30);
        }

        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);

            if (abilityCoroutine != null) Disable();

            abilityCoroutine = StartCoroutine(AbilityCoroutine());
        }

        private IEnumerator AbilityCoroutine()
        {
            var lastTimeSpawned = Time.time - AbilityCooldown;

            while (true)
            {
                while (lastTimeSpawned + AbilityCooldown < Time.time)
                {
                    var spawnTime = lastTimeSpawned + AbilityCooldown;

                    // Détermination de la direction de base (cible prioritaire)
                    var closestEnemy = StageController.EnemiesSpawner.GetClosestEnemy(PlayerBehavior.CenterPosition);

                    var baseDir = PlayerBehavior.Player.LookDirection;
                    if (closestEnemy != null)
                    {
                        baseDir = closestEnemy.Center - PlayerBehavior.CenterPosition;
                        baseDir.Normalize();
                    }

                    float baseAngleDeg = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

                    const int PROJECTILES_PER_SHOT = 5;
                    const float ANGLE_STEP = 360f / PROJECTILES_PER_SHOT; // 72°

                    for (int i = 0; i < PROJECTILES_PER_SHOT; i++)
                    {
                        var projectile = projectilePool.GetEntity();

                        float angleDeg = baseAngleDeg + ANGLE_STEP * i;
                        float rad = angleDeg * Mathf.Deg2Rad;
                        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                        // Position initiale directe (pas d'extrapolation de temps de vol comme la Wand)
                        var position = PlayerBehavior.CenterPosition;

                        projectile.Init(position, dir);
                        projectile.Speed = AbilityLevel.ProjectileSpeed * PlayerBehavior.Player.ProjectileSpeedMultiplier;
                        projectile.transform.localScale =
                            Vector3.one *
                            AbilityLevel.ProjectileSize *
                            PlayerBehavior.Player.SizeMultiplier;

                        projectile.LifeTime = AbilityLevel.ProjectileLifetime * PlayerBehavior.Player.DurationMultiplier;
                        projectile.DamageMultiplier = AbilityLevel.Damage;

                        TryMakeProjectilePierce(projectile);

                        projectile.onFinished += OnProjectileFinished;
                        projectiles.Add(projectile);
                    }

                    lastTimeSpawned += AbilityCooldown;

                    GameController.AudioManager.PlaySound(HEAVY_CANNON_LAUNCH_HASH);
                }

                yield return null;
            }
        }

        private void TryMakeProjectilePierce(SimplePlayerProjectileBehavior projectile)
        {
            var field = typeof(SimplePlayerProjectileBehavior).GetField("selfDestructOnHit",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

            if (field != null)
            {
                field.SetValue(projectile, false);
            }
        }

        private void OnProjectileFinished(SimplePlayerProjectileBehavior projectile)
        {
            projectile.onFinished -= OnProjectileFinished;
            projectiles.Remove(projectile);
        }

        private void Disable()
        {
            projectileCoroutine.StopIfExists();

            for (int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].gameObject.SetActive(false);
            }

            projectiles.Clear();

            if (abilityCoroutine != null) StopCoroutine(abilityCoroutine);
            abilityCoroutine = null;
        }

        public override void Clear()
        {
            Disable();
            base.Clear();
        }
    }
}