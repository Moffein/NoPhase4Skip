using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx;
using System;
using UnityEngine.AddressableAssets;

namespace R2API.Utils
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ManualNetworkRegistrationAttribute : Attribute
    {
    }
}

namespace NoPhase4Skip
{
    [BepInPlugin("com.Moffein.NoPhase4Skip", "NoPhase4Skip", "1.0.0")]
    public class NoPhase4Skip : BaseUnityPlugin
    {
        private static BodyIndex brotherHurtIndex;
        private void Awake()
        {
            GameObject brotherHurtPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherHurtBody.prefab").WaitForCompletion();
            brotherHurtPrefab.AddComponent<Phase4ImmuneComponent>();

            RoR2Application.onLoad += OnLoad;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.EntityStates.BrotherMonster.SpellBaseState.OnExit += SpellBaseState_OnExit;
        }

        private void SpellBaseState_OnExit(On.EntityStates.BrotherMonster.SpellBaseState.orig_OnExit orig, EntityStates.BrotherMonster.SpellBaseState self)
        {
            Phase4ImmuneComponent p4 = self.GetComponent<Phase4ImmuneComponent>();
            if (p4) Destroy(p4);

            orig(self);
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (NetworkServer.active && self.body.bodyIndex == brotherHurtIndex)
            {
                if (self.GetComponent<Phase4ImmuneComponent>())
                {
                    damageInfo.rejected = true;
                }
            }
            orig(self, damageInfo);
        }

        private void OnLoad()
        {
            brotherHurtIndex = BodyCatalog.FindBodyIndex("BrotherHurtBody");
        }
    }

    //Having this on BrotherHurt prevents him from taking damage
    public class Phase4ImmuneComponent : MonoBehaviour
    {
        public float spawnImmuneDuration = 10f; //Failsafe in case this doesnt get deleted for whatever reason.

        private void Awake()
        {
            if (!NetworkServer.active) Destroy(this);
        }

        private void FixedUpdate()
        {
            spawnImmuneDuration -= Time.fixedDeltaTime;
            if (spawnImmuneDuration <= 0f)
            {
                Destroy(this);
                return;
            }
        }
    }
}
