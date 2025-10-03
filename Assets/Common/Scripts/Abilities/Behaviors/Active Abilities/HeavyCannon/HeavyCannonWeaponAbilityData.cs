using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Heavy Cannon Data", menuName = "October/Abilities/Active/Heavy Cannon")]
    public class HeavyCannonWeaponAbilityData : GenericAbilityData<HeavyCannonWeaponAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.HeavyCannon;
            isActiveAbility = true;
        }

        private void OnValidate()
        {
            type = AbilityType.HeavyCannon;
            isActiveAbility = true;
        }
    }

    [System.Serializable]
    public class HeavyCannonWeaponAbilityLevel : AbilityLevel
    {
        [Tooltip("Time between cannon shots (base value, affected by player cooldown multiplier)")]
        [SerializeField] float abilityCooldown;
        public float AbilityCooldown => abilityCooldown;

        [Tooltip("Projectile speed")]
        [SerializeField] float projectileSpeed;
        public float ProjectileSpeed => projectileSpeed;

        [Tooltip("Damage multiplier (Player.Damage * Damage)")]
        [SerializeField] float damage;
        public float Damage => damage;

        [Tooltip("Base size of the cannon shell")]
        [SerializeField] float projectileSize;
        public float ProjectileSize => projectileSize;

        [Tooltip("Lifetime (seconds)")]
        [SerializeField] float projectileLifetime;
        public float ProjectileLifetime => projectileLifetime;
    }
}