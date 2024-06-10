using NaughtyAttributes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

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

			float damageReceived = 0.0f;

			if (_enemyData)
			{
				float effectiveArmor = _enemyData.enemyStats.armor * (1 - pArmorPenetration / 100);
				float damageReductionFactor = 1 - effectiveArmor / 100;
				damageReceived = pDamage * damageReductionFactor;
			}
			else
				damageReceived = _statComponent.CalcDamageReceived(pDamage);
		
			_health.Value -= damageReceived;
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
				HealServerRpc(iHeal);
				return;
			}

			if (iHeal <= 0 || _health.Value >= maxHealth)
			{
				return;
			}

			_health.Value = Mathf.Min((int)_health.Value + (int) iHeal, maxHealth);
			OnHealed.Invoke(iHeal);
		}

		[Rpc(SendTo.Server)]
		private void TakeDamageServerRpc(float iDamage, float iArmorPenetration = 0)
		{
			TakeDamage(iDamage, iArmorPenetration);
		}

		[Rpc(SendTo.Server, RequireOwnership = false)]
		private void HealServerRpc(float iHeal)
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
