﻿using FishNet.Object;
using SS3D.Core;
using SS3D.Systems.Entities;
using SS3D.Systems.Inventory.Items;

namespace SS3D.Systems.Health
{
	/// <summary>
	/// Body part for a human head.
	/// </summary>
	public class HeadBodyPart : BodyPart
	{
		public Brain brain;

		public override void Init(BodyPart parent)
		{
			base.Init(parent);
		}

    public override void OnStartServer()
    {
        base.OnStartServer();
		AddInternalBodyPart(brain);
    }

		protected override void AddInitialLayers()
		{
			TryAddBodyLayer(new MuscleLayer(this));
			TryAddBodyLayer(new BoneLayer(this));
			TryAddBodyLayer(new CirculatoryLayer(this, 5f));
			TryAddBodyLayer(new NerveLayer(this));
			InvokeOnBodyPartLayerAdded();
		}

		protected override void DetachBodyPart()
		{
			if (_isDetached) return;
			DetachChildBodyParts();
			HideSeveredBodyPart();

			// When detached, spawn a head and set player's mind to be in the head,
			// so that player can still play as a head (death is near though..).
			BodyPart head = SpawnDetachedBodyPart();
			MindSystem mindSystem = Subsystems.Get<MindSystem>();

            var EntityControllingHead = GetComponentInParent<Entity>();
            if(EntityControllingHead.Mind != null)
            {
                mindSystem.SwapMinds(GetComponentInParent<Entity>(), head.GetComponent<Entity>());
                head.GetComponent<NetworkObject>().RemoveOwnership();

                var entitySystem = Subsystems.Get<EntitySystem>();
                entitySystem.TryTransferEntity(GetComponentInParent<Entity>(), head.GetComponent<Entity>());
            }

			InvokeOnBodyPartDetached();
			_isDetached = true;
            // For now simply set unactive the whole body. In the future, should instead put the body in ragdoll mode
            // and disable a bunch of components.
            //DeactivateWholeBody();

            Dispose(false);
		}

        protected override void DestroyBodyPart()
        {
            base.DestroyBodyPart();
        }

        /// <summary>
        /// Deactivate this game object, should run for all observers, and for late joining (hence bufferlast = true).
        /// </summary>
        [ObserversRpc(RunLocally = true, BufferLast = true)]
		protected void DeactivateWholeBody()
		{
			GetComponentInParent<Human>().gameObject.SetActive(false);
		}
	}
}
