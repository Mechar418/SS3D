﻿using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using System.Collections.Generic;
using UnityEngine;
using SS3D.Core;
using SS3D.Logging;
using SS3D.Systems.Permissions;
using Cysharp.Threading.Tasks;
using UnityEditor;
using SS3D.Interactions;
using SS3D.Interactions.Interfaces;
using SS3D.Systems.Health;
using System.Linq;
using System.Collections.ObjectModel;
using FishNet;

/// <summary>
/// Class to handle all networking stuff related to a body part, there should be only one on a given game object.
/// There should always be a network object component everywhere this component is.
/// </summary>
public abstract class BodyPart : InteractionTargetNetworkBehaviour
{

    [SyncVar]
    private BodyPart _parentBodyPart;

    [SerializeField]
    private SkinnedMeshRenderer _skinnedMeshRenderer;

	[SerializeField]
	protected GameObject _bodyPartItem;


	private readonly List<BodyPart> _childBodyParts = new List<BodyPart>();

    public readonly List<BodyLayer> _bodyLayers = new List<BodyLayer>();

	[SerializeField]
	private Collider _bodyCollider;

	public Collider BodyCollider => _bodyCollider;

    public string Name => gameObject.name;




    public ReadOnlyCollection<BodyLayer> BodyLayers
    {
        get { return _bodyLayers.AsReadOnly(); }
    }


	public ReadOnlyCollection<BodyPart> ChildBodyParts
    {
        get { return _childBodyParts.AsReadOnly(); }
    }

	public bool IsDestroyed => TotalDamage > MaxDamage;

	public bool IsSevered => GetBodyLayer<BoneLayer>().IsDestroyed();


	/// <summary>
	/// The parent bodypart is the body part attached to this body part, closest from the brain. 
	/// For lower left arm, it's higher left arm. For neck, it's head.
	/// Be careful, it doesn't necessarily match the game object hierarchy
	/// </summary>
	public BodyPart ParentBodyPart
    {
        get { return _parentBodyPart; }
        set
        {
            if (value == null)
                return;

            if (_childBodyParts.Contains(value))
            {
                Punpun.Error(this, "trying to set up {bodypart} bodypart as both child and" +
                    " parent of {bodypart} bodypart.", Logs.Generic, value, this);
                return;
            }

            Punpun.Debug(this, "value of parent body part {bodypart}", Logs.Generic, value);
            _parentBodyPart = value;
            _parentBodyPart._childBodyParts.Add(this);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        ParentBodyPart = _parentBodyPart;
        AddInitialLayers();
    }

    public virtual void Init(BodyPart parent)
    {
        ParentBodyPart = parent;
    }

    public virtual void Init(BodyPart parentBodyPart, List<BodyPart> childBodyParts, List<BodyLayer> bodyLayers)
    {
        ParentBodyPart = parentBodyPart;
        _childBodyParts.AddRange(childBodyParts);
        _bodyLayers.AddRange(bodyLayers);
        foreach (var bodylayer in BodyLayers)
        {
            bodylayer.BodyPart = this;
        }
    }

    /// <summary>
    /// The body part is not destroyed, it's simply detached from the entity.
    /// </summary>
    protected virtual void DetachBodyPart()
    {
		//Spawn a detached body part from the entity, and destroy this one with all childs.
		// Maybe better in body part controller.
		//throw new NotImplementedException();

		GameObject go = Instantiate(_bodyPartItem);
		InstanceFinder.ServerManager.Spawn(go, null);
	}

	/// <summary>
	/// The body part took so much damages that it's simply destroyed.
	/// Think complete crushing, burning to dust kind of stuff.
	/// All child body parts are detached.
	/// </summary>
	/// <exception cref="NotImplementedException"></exception>
	public void DestroyBodyPart()
    {
        // destroy this body part with all childs on the entity, detach all childs.
        // Maybe better in body part controller.
        //throw new NotImplementedException();
    }



    public override IInteraction[] CreateTargetInteractions(InteractionEvent interactionEvent)
    {
        return new IInteraction[] { new KillInteraction() };
    }

    /// <summary>
    /// Add a body layer if none of the same type are already present on this body part.
    /// TODO : use generic to check type, actually check if only one body layer of each kind.
    /// </summary>
    /// <returns> The body layer was added.</returns>
    public virtual bool TryAddBodyLayer(BodyLayer layer)
    {
        layer.BodyPart = this;
        _bodyLayers.Add(layer);
        return true;
    }

    public float TotalDamage => _bodyLayers.Sum(layer => layer.TotalDamage);
    public float MaxDamage => _bodyLayers.Sum(layer => layer.MaxDamage);


    /// <summary>
    /// Remove a body layer from the body part.
    /// TODO : check if it exists first.
    /// </summary>
    /// <param name="layer"></param>
    public virtual void RemoveBodyLayer(BodyLayer layer)
    {
         _bodyLayers.Remove(layer);
    }

    /// <summary>
    /// Add a new body part as a child of this one. 
    /// </summary>
    /// <param name="bodyPart"></param>
    public virtual void AddChildBodyPart(BodyPart bodyPart)
    {
        _childBodyParts.Add(bodyPart);
    }

    /// <summary>
    /// Inflic damages of a certain kind on a certain body layer type if the layer is present.
    /// </summary>
    /// <returns>True if the damage could be inflicted</returns>
    public virtual bool TryInflictDamage(BodyLayerType type, DamageTypeQuantity damageTypeQuantity)
    {
		BodyLayer layer = FirstBodyLayerOfType(type);
		if (!BodyLayers.Contains(layer)) return false;
		layer.InflictDamage(damageTypeQuantity);
		if (IsSevered) RemoveBodyPart();
		return true;	
    }

	/// <summary>
	/// inflict same type damages to all layers present on this body part.
	/// </summary>
	public virtual void InflictDamageToAllLayer(DamageTypeQuantity damageTypeQuantity)
    {
        foreach (var layer in BodyLayers)
        {
            layer.InflictDamage(damageTypeQuantity);
        }

        if (IsSevered) RemoveBodyPart();
    }

    /// <summary>
    /// inflict same type damages to all layers present on this body part except one.
    /// </summary>
    public virtual void InflictDamageToAllLayerButOne<T>(DamageTypeQuantity damageTypeQuantity)
    {
        foreach (var layer in BodyLayers)
        {
            if (!(layer is T))
                layer.InflictDamage(damageTypeQuantity);
        }

        if (IsSevered) RemoveBodyPart();
    }

    /// <summary>
    /// Check if this body part contains a given layer type.
    /// </summary>
    public bool ContainsLayer(BodyLayerType layerType)
    {
        return BodyLayers.Any(x => x.LayerType == layerType);
    }

	public BodyLayer FirstBodyLayerOfType(BodyLayerType layerType)
	{
		return BodyLayers.Where(x => x.LayerType == layerType).First();
	}


    /// <summary>
    /// GetBodyLayer of type T on this bodypart.
    /// Todo : change that with TryGetBody.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public BodyLayer GetBodyLayer<T>()
    {
        foreach (var layer in BodyLayers)
        {
            if (layer is T)
            {
                return layer;
            }
        }

        return null;
    }

    /// <summary>
    /// Describe extensively the bodypart.
    /// </summary>
    /// <returns></returns>
    public string Describe()
    {
        var description = "";
        foreach (var layer in BodyLayers)
        {
            description += "Layer " + layer.GetType().ToString() + "\n";
        }
        description += "Child connected body parts : \n";
        foreach (var part in _childBodyParts)
        {
            description += part.gameObject.name + "\n";
        }
        description += "Parent body part : \n";
        description += ParentBodyPart.name;
        return description;
    }

    public override string ToString()
    {
        return Name;
    }

	private void RemoveBodyPart()
	{
		RemoveSingleBodyPart();
		for(int i= _childBodyParts.Count-1; i>=0;i--)
		{
			_childBodyParts[i].RemoveBodyPart();
		}
	}

	protected virtual void RemoveSingleBodyPart()
	{
		HideSeveredBodyPart();
		DetachBodyPart();
	}

	private void HideSeveredBodyPart()
    {
        _skinnedMeshRenderer.enabled = false;
    }

    protected abstract void AddInitialLayers();


}
