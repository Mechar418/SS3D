﻿using FishNet.Object;
using SS3D.Core;
using SS3D.Substances;
using SS3D.Systems.Health;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CirculatoryController;

public class Lungs : BodyPart
{

    public enum BreathingState
    {
        Nice,
        Difficult,
        Suffocating
    }

    public BreathingState breathing;

    // Number of inspiration and expiration per minutes
    private float _breathFrequency = 60f;

    public event EventHandler OnBreath;

    [SerializeField]
    private CirculatoryController _circulatoryController;

    private float _timer = 0f;

    // TODO : remove this and replace with oxygen taken from atmos when possible
    [SerializeField]
    private float OxygenConstantIntake = 0.4f;

    public float SecondsBetweenBreaths => _breathFrequency > 0 ? 60f / _breathFrequency : float.MaxValue;

    public float MaxOxygenAmount => 10;

    [Server]
    protected override void AddInitialLayers()
    {
        TryAddBodyLayer(new MuscleLayer(this));
        TryAddBodyLayer(new CirculatoryLayer(this, 3f));
        TryAddBodyLayer(new NerveLayer(this));
        TryAddBodyLayer(new OrganLayer(this));
    }

    void Update()
    {
        if (!IsServer) return;
        _timer += Time.deltaTime;

        if (_timer > SecondsBetweenBreaths)
        {
            _timer = 0f;
            Breath();
        }
    }

    [Server]
    private void Breath()
    {
        OnBreath?.Invoke(this, EventArgs.Empty);
        SubstancesSystem registry = Subsystems.Get<SubstancesSystem>();
        Substance oxygen = registry.FromType(SubstanceType.Oxygen);
        if (_circulatoryController.Container.GetSubstanceQuantity(oxygen) > MaxOxygenAmount)
        {
            return;
        }
        else
        {
            _circulatoryController.Container.AddSubstance(oxygen, OxygenConstantIntake);
        }
    }

    // should set that to private
    [Server]
    public void SetBreathingState(float availableOxygen, float sumNeeded)
    {
        if (availableOxygen > HealthConstants.SafeOxygenFactor * sumNeeded)
        {
            breathing = BreathingState.Nice;
        }
        else if (availableOxygen > sumNeeded)
        {
            breathing = BreathingState.Difficult;
        }
        else
        {
            breathing = BreathingState.Suffocating;
        }
    }

    [Server]
    protected override void AfterSpawningCopiedBodyPart()
    {
        return;
    }

    [Server]
    protected override void BeforeDestroyingBodyPart()
    {
        return;
    }
}
