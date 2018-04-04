﻿using UnityEngine;
using System;
using System.Collections.Generic;
using SubPhases;
using Players;

namespace GameModes
{
    public class LocalGame : GameMode
    {
        public override void RevertSubPhase()
        {
            (Phases.CurrentSubPhase as SubPhases.SelectShipSubPhase).CallRevertSubPhase();
        }

        public override void ConfirmCrit()
        {
            InformCrit.HidePanel();
            Triggers.FinishTrigger();
        }

        public override void DeclareTarget(int thisShipId, int anotherShipId)
        {
            Combat.DeclareIntentToAttack(thisShipId, anotherShipId);
        }

        public override void NextButtonEffect()
        {
            UI.NextButtonEffect();
        }

        public override void SkipButtonEffect()
        {
            UI.SkipButtonEffect();
        }

        public override void ConfirmShipSetup(int shipId, Vector3 position, Vector3 angles)
        {
            (Phases.CurrentSubPhase as SubPhases.SetupSubPhase).ConfirmShipSetup(shipId, position, angles);
        }

        public override void PerformStoredManeuver(int shipId)
        {
            ShipMovementScript.PerformStoredManeuver(Selection.ThisShip.ShipId);
        }

        public override void AssignManeuver(string maneuverCode)
        {
            ShipMovementScript.AssignManeuver(Selection.ThisShip.ShipId, maneuverCode);
        }

        public override void GiveInitiativeToRandomPlayer()
        {
            int randomPlayer = UnityEngine.Random.Range(1, 3);
            Phases.PlayerWithInitiative = Tools.IntToPlayer(randomPlayer);
            Triggers.FinishTrigger();
        }

        public override void ShowInformCritPanel()
        {
            InformCrit.ShowPanelVisible();
        }

        public override void StartBattle()
        {
            Global.StartBattle();
        }

        // BARREL ROLL

        public override void TryConfirmBarrelRollPosition(string templateName, Vector3 shipBasePosition, Vector3 movementTemplatePosition)
        {
            (Phases.CurrentSubPhase as SubPhases.BarrelRollPlanningSubPhase).TryConfirmBarrelRollPosition();
        }

        public override void StartBarrelRollExecution()
        {
            (Phases.CurrentSubPhase as SubPhases.BarrelRollPlanningSubPhase).StartBarrelRollExecution();
        }

        public override void FinishBarrelRoll()
        {
            (Phases.CurrentSubPhase as SubPhases.BarrelRollExecutionSubPhase).FinishBarrelRollAnimation();
        }

        public override void CancelBarrelRoll()
        {
            (Phases.CurrentSubPhase as SubPhases.BarrelRollPlanningSubPhase).CancelBarrelRoll();
        }

        // DECLOAK

        public override void TryConfirmDecloakPosition(Vector3 shipBasePosition, string decloakHelper, Vector3 movementTemplatePosition, Vector3 movementTemplateAngles)
        {
            (Phases.CurrentSubPhase as SubPhases.DecloakPlanningSubPhase).TryConfirmDecloakPosition();
        }

        public override void StartDecloakExecution(Ship.GenericShip ship)
        {
            (Phases.CurrentSubPhase as SubPhases.DecloakPlanningSubPhase).StartDecloakExecution(ship);
        }

        public override void FinishDecloak()
        {
            (Phases.CurrentSubPhase as SubPhases.DecloakExecutionSubPhase).FinishDecloakAnimation();
        }

        public override void CancelDecloak()
        {
            (Phases.CurrentSubPhase as SubPhases.DecloakPlanningSubPhase).CancelDecloak();
        }

        // BOOST

        public override void TryConfirmBoostPosition(string selectedBoostHelper)
        {
            (Phases.CurrentSubPhase as SubPhases.BoostPlanningSubPhase).TryConfirmBoostPosition();
        }

        public override void StartBoostExecution()
        {
            (Phases.CurrentSubPhase as SubPhases.BoostPlanningSubPhase).StartBoostExecution();
        }

        public override void FinishBoost()
        {
            Phases.FinishSubPhase(typeof(SubPhases.BoostExecutionSubPhase));
        }

        public override void CancelBoost()
        {
            (Phases.CurrentSubPhase as SubPhases.BoostPlanningSubPhase).CancelBoost();
        }

        public override void UseDiceModification(string effectName)
        {
            Combat.UseDiceModification(effectName);
        }

        public override void ConfirmDiceResults()
        {
            Combat.ConfirmDiceResultsClient();
        }

        public override void SwitchToOwnDiceModifications()
        {
            Combat.SwitchToOwnDiceModificationsClient();
        }

        public override void TakeDecision(Decision decision, GameObject button)
        {
            decision.ExecuteDecision(button);
        }

        public override void FinishMovementExecution()
        {
            Selection.ActiveShip.CallExecuteMoving(delegate { Phases.FinishSubPhase(typeof(SubPhases.MovementExecutionSubPhase)); });
        }

        // Swarm Manager

        public override void SetSwarmManagerManeuver(string maneuverCode)
        {
            SwarmManager.SetManeuver(maneuverCode);
        }

        public override void GenerateDamageDeck(PlayerNo playerNo, int seed)
        {
            DamageDecks.GetDamageDeck(playerNo).ShuffleDeck(seed);
        }

        public override void CombatActivation(int shipId)
        {
            Selection.ChangeActiveShip("ShipId:" + shipId);
            Selection.ThisShip.CallCombatActivation(delegate { (Phases.CurrentSubPhase as CombatSubPhase).ChangeSelectionMode(Team.Type.Enemy); });
        }
    }
}
