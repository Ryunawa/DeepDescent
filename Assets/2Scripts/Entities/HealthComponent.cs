using System.Threading.Tasks;
using _2Scripts.Entities.AI;
using _2Scripts.Entities.Player;
using _2Scripts.Helpers;
using _2Scripts.Interfaces;
using _2Scripts.Manager;
using _2Scripts.UI;
using NaughtyAttributes;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

namespace _2Scripts.Entities
{
	public class HealthComponent : GameManagerSync<HealthComponent>
	{
        [Header("Health Settings")]
        [FormerlySerializedAs("MaxHealth")]
        [SerializeField]
        private float maxHealth = 100;
        [SerializeField]
        private NetworkVariable<float> _health = new NetworkVariable<float>();
        private HUD hudObject;

        [Header("Debug")]
        [SerializeField]
        private bool invincibleDebug;

        [Header("Components")]
        private EnemyData _enemyData;
        [SerializeField] private EnemyFeedback feedback;
        private StatComponent _statComponent;
        [SerializeField]
        private AIController aiController;

        [Header("Character Settings")]
        private int characterID;

        [Header("Events")]
        public UnityEvent OnDeath;
        public UnityEvent<float> OnDamaged;
        public UnityEvent<float> OnHealed;

        public float MaxHealth => maxHealth;

        private void _CheckForDeath(float iPrevVal, float iCurVal)
		{
			if (this == GameManager.playerBehaviour.Health)
			{
				HUD hud = GameManager.GetManager<InventoryUIManager>().HUD;
				
				hud.SetHp();
				hud.FlashDamageEffect(iCurVal, MaxHealth);
			}
			
			if (iPrevVal > 0 && iCurVal <= 0)
			{
				Debug.Log($"{gameObject.name} Die");
				OnDeath.Invoke();
				if (!_enemyData)
				{
                    addDeadPlayerRPC();
                }
				else
                {
                    GameManager.GetManager<AudioManager>().PlaySfx("MonsterDie", this, 1, 5);
                    GameManager.GetManager<EnemiesSpawnerManager>().EnemyDestroyed(_enemyData);
                }
			}
		}

        [Rpc(SendTo.Server)]
        private void addDeadPlayerRPC()
        {
            GameManager.instance.AddADeadPlayer();
        }

        // Start is called before the first frame update
        protected override void OnGameManagerChangeState(GameState gameState)
		{
			if (gameState != GameState.InLevel) return;

			if (_enemyData)
			{
                maxHealth = _enemyData.enemyStats.health;
				Heal(_enemyData.enemyStats.health);
                GameManager.GetManager<AudioManager>().PlaySfx("MonsterSpawn", this, 1, 5);
            }

			Heal(maxHealth);
            characterID = GameManager.GetManager<MultiManager>().GetSelectedCharacterID();
        }

		protected override void Start()
		{
			base.Start();

            if (OnDeath == null)
                OnDeath = new UnityEvent();
            if (OnDamaged == null)
                OnDamaged = new UnityEvent<float>();
            if (OnHealed == null)
                OnHealed = new UnityEvent<float>();
            
            
			_health.OnValueChanged += _CheckForDeath;

            _enemyData = GetComponent<EnemyData>();
            _statComponent = GetComponent<StatComponent>();

            //OnGameManagerChangeState(GameManager.GameState);
        }


		public void TakeDamage(float pDamage, float pArmorPenetration = 0, Transform attacker = null)
		{
			if (invincibleDebug) return;

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
				if (_enemyData)
				{
                    if (feedback) feedback.TakeHit();
                    else Debug.LogError("feedback is nul.");

                    float effectiveArmor = _enemyData.enemyStats.armor * (1 - pArmorPenetration / 100);
                    float damageReductionFactor = 1 - effectiveArmor / 100;
                    damage = pDamage * damageReductionFactor;
				}
				else
				{
					damage = pDamage;
				}
            }

            // play sound
            if (gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                GameManager.GetManager<AudioManager>().PlaySfx("MonsterHurt", this, 1, 5);

				// TODO: add visual effects

                AIController aiController = GetComponent<AIController>();
                if (aiController != null && attacker != null)
                {
                    aiController.SwitchToChaseMode(attacker);
                }
            }
			else
			{
				// if (!hudObject) hudObject = GameObject.Find("HUD").GetComponent<HUD>();
				// if (hudObject) hudObject.FlashDamageEffect(_health.Value, maxHealth);

				// else Debug.Log("no HUD found");
            }

            _health.Value -= damage;

            OnDamaged.Invoke(pDamage);
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
	                    GameManager.GetManager<AudioManager>().PlaySfx("FemaleSigh", this, 1, 5);
                        break;
                    case 1 | 3:
	                    GameManager.GetManager<AudioManager>().PlaySfx("MaleSigh", this, 1, 5);
                        break;
                }
            }

            _health.Value = Mathf.Min((int)_health.Value + (int) iHeal, maxHealth);

			// if (_hud)
			// {
   //              _hud.SetHp();
   //          }

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

		public void SetMaxHealth(float newMaxHealth)
		{
			maxHealth = newMaxHealth;
			_health.Value = newMaxHealth;
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
