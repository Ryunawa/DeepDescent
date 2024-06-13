using _2Scripts.Manager;
using _2Scripts.UI;
using NaughtyAttributes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;

namespace _2Scripts.Entities
{
	public class HealthComponent : NetworkBehaviour
	{
		[FormerlySerializedAs("MaxHealth")] [SerializeField] private float maxHealth = 100;

		private NetworkVariable<float> _health = new NetworkVariable<float>();

		public UnityEvent OnDeath;

		public UnityEvent<float> OnDamaged;

		public UnityEvent<float> OnHealed;

		private EnemyData _enemyData;
	
		private StatComponent _statComponent;

        private int characterID;


        private void Awake()
		{
			if(OnDeath == null)
				OnDeath = new UnityEvent();
			if(OnDamaged == null)
				OnDamaged = new UnityEvent<float>();
			if (OnHealed == null)
				OnHealed = new UnityEvent<float>();

			_health.OnValueChanged += _CheckForDeath;

			_enemyData = GetComponent<EnemyData>();
            _statComponent = GetComponent<StatComponent>();
        }

		private void _CheckForDeath(float iPrevVal, float iCurVal)
		{
			if (iPrevVal > 0 && iCurVal <= 0)
			{
				OnDeath.Invoke();
			}
		}

		// Start is called before the first frame update
		void Start()
		{
			if (_enemyData)
			{
				maxHealth = _enemyData.enemyStats.health;
				Heal(_enemyData.enemyStats.health);
			}
			Heal(maxHealth);
            characterID = MultiManager.instance.GetSelectedCharacterID();
        }

		public void TakeDamage(float pDamage, float pArmorPenetration = 0)
		{
			if(!IsServer)
			{
				TakeDamageServerRpc(pDamage);
				return;
			}

			if(pDamage <= 0 || _health.Value <= 0)
				return;

			float damage = 0;

			if (_statComponent)
			{
				damage = _statComponent.CalcDamageReceived(pDamage);
			}
			else
			{
                float effectiveArmor = _enemyData.enemyStats.armor * (1 - pArmorPenetration / 100);
                float damageReductionFactor = 1 - effectiveArmor / 100;
				damage = pDamage * damageReductionFactor;
            }

            // play sound
            if (gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                AudioManager.instance.PlaySfx("MonsterDie", this, 1, 5);
            }


            _health.Value -= damage;

			HUD.instance.SetHp(_health.Value / maxHealth);

            OnDamaged.Invoke(pDamage);
		}

		[Button]
		public void InflictDamageTest()
		{
			if (!IsServer)
			{
				TakeDamageServerRpc(10.0f);
				return;
			}
			_health.Value -= 10.0f;
			OnDamaged.Invoke(10.0f);
		}

		public void Heal(float iHeal)
		{
			if(!IsServer) 
			{
				HealServerRPC(iHeal);
				return;
			}

			if (iHeal <= 0 || _health.Value >= maxHealth)
			{
				return;
			}

            // play sound
            if (gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                switch (characterID)
                {
                    case 0 | 2:
                        AudioManager.instance.PlaySfx("FemaleSigh", this, 1, 5);
                        break;
                    case 1 | 3:
                        AudioManager.instance.PlaySfx("MaleSigh", this, 1, 5);
                        break;
                }
            }

            _health.Value = Mathf.Min((int)_health.Value + (int) iHeal, maxHealth);

            HUD.instance.SetHp(_health.Value / maxHealth);

            OnHealed.Invoke(iHeal);
		}

		[Rpc(SendTo.Server)]
		private void TakeDamageServerRpc(float iDamage, float iArmorPenetration = 0)
		{
			TakeDamage(iDamage);
		}

		[Rpc(SendTo.Server, RequireOwnership = false)]
		private void HealServerRPC(float iHeal)
		{
			Heal(iHeal);
		}


		public float GetHealth()
		{
			return _health.Value;
		}

		public void OnValueChanged(NetworkVariable<float>.OnValueChangedDelegate iListener)
		{
			_health.OnValueChanged += iListener;
		}
	}
}
