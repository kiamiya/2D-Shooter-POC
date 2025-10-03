using OctoberStudio.Easing;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class HeavyCannonWeaponAbilityBehavior : AbilityBehavior<HeavyCannonWeaponAbilityData, HeavyCannonWeaponAbilityLevel>
    {
        public static readonly int HEAVY_CANNON_LAUNCH_HASH = "Wand Projectile Launch".GetHashCode(); // Réutilise un son existant (à changer si besoin)

        [SerializeField] GameObject projectilePrefab;
        public GameObject ProjectilePrefab => projectilePrefab;

        // Pool plus petit (tirs lents)
        private PoolComponent<SimplePlayerProjectileBehavior> projectilePool;
        private readonly List<SimplePlayerProjectileBehavior> projectiles = new();

        IEasingCoroutine projectileCoroutine;
        Coroutine abilityCoroutine;

        private float AbilityCooldown => AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier;

        private void Awake()
        {
            projectilePool = new PoolComponent<SimplePlayerProjectileBehavior>("Heavy Cannon Projectile", ProjectilePrefab, 12);
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

                    var projectile = projectilePool.GetEntity();

                    // Ciblage : ennemi le plus proche sinon tir vers LookDirection
                    var closestEnemy = StageController.EnemiesSpawner.GetClosestEnemy(PlayerBehavior.CenterPosition);

                    var direction = PlayerBehavior.Player.LookDirection;
                    if (closestEnemy != null)
                    {
                        direction = closestEnemy.Center - PlayerBehavior.CenterPosition;
                        direction.Normalize();
                    }

                    var aliveDuration = Time.time - spawnTime;
                    var position = PlayerBehavior.CenterPosition + direction *
                                   aliveDuration *
                                   AbilityLevel.ProjectileSpeed *
                                   PlayerBehavior.Player.ProjectileSpeedMultiplier;

                    projectile.Init(position, direction);
                    projectile.Speed = AbilityLevel.ProjectileSpeed * PlayerBehavior.Player.ProjectileSpeedMultiplier;
                    projectile.transform.localScale =
                        Vector3.one *
                        AbilityLevel.ProjectileSize *
                        PlayerBehavior.Player.SizeMultiplier;

                    projectile.LifeTime = AbilityLevel.ProjectileLifetime * PlayerBehavior.Player.DurationMultiplier;
                    projectile.DamageMultiplier = AbilityLevel.Damage;

                    // Rendre le projectile perçant (ne pas se détruire en touchant)
                    TryMakeProjectilePierce(projectile);

                    projectile.onFinished += OnProjectileFinished;
                    projectiles.Add(projectile);

                    lastTimeSpawned += AbilityCooldown;

                    GameController.AudioManager.PlaySound(HEAVY_CANNON_LAUNCH_HASH);
                }

                yield return null;
            }
        }

        private void TryMakeProjectilePierce(SimplePlayerProjectileBehavior projectile)
        {
            // Tente de forcer selfDestructOnHit = false via reflection (champ protected)
            var field = typeof(SimplePlayerProjectileBehavior).GetField("selfDestructOnHit",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

            if (field != null)
            {
                field.SetValue(projectile, false);
            }
            else
            {
                // En cas d'échec, fallback possible: augmenter LifeTime et laisser le comportement normal
                // Debug.LogWarning("HeavyCannon: impossible de modifier 'selfDestructOnHit'.");
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