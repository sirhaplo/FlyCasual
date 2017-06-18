﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SubPhases;

public enum TriggerTypes
{
    None,
    OnShipMovementFinish,
    OnCombatPhaseStart,
    OnDamageCardIsDealt
}

public static partial class Triggers
{
    class Trigger
    {
        public string Name { get; private set; }
        public TriggerTypes TriggerType { get; private set; }
        public EventHandler TriggerExecution { get; private set; }
        public object Sender { get; private set; }
        public int Id { get; private set; }

        public Trigger(string name, TriggerTypes triggerType, EventHandler triggerExecution, object sender)
        {
            Name = name;
            TriggerType = triggerType;
            TriggerExecution = triggerExecution;
            Sender = sender;
            Id = counter++;
        }
    };

    private static int counter = 0;

    public static bool Empty
    {
        get { return simultaneousTriggers.Count == 0; }
    }

    public static Players.PlayerNo CurrentPlayer { get; private set; }

    private static Dictionary<int, Trigger> simultaneousTriggers = new Dictionary<int, Trigger>();
    private static List<Dictionary<int, Trigger>> stackedTriggers = new List<Dictionary<int, Trigger>>();

    public static void AddTrigger(string name, TriggerTypes triggerType, EventHandler triggerExecution, object sender)
    {
        Debug.Log("Trigger \"" + name + "\" is registered. Id " + counter + ". Active: " + (simultaneousTriggers.Count+1));
        simultaneousTriggers.Add(counter, new Trigger(name, triggerType, triggerExecution, sender));
    }

    public static void RemoveTrigger(int id)
    {
        Debug.Log("Trigger \"" + simultaneousTriggers[id].Name + "\" is unregistered. Id " + id + ". Active: " + (simultaneousTriggers.Count-1));
        simultaneousTriggers.Remove(id);
    }

    public static IEnumerator ResolveAllTriggers(TriggerTypes triggerType)
    {
        Debug.Log("Are triggers empty? : " + Triggers.Empty);
        while (!Triggers.Empty)
        {
            Debug.Log("I want to resolve all trigers with type: " + triggerType);

            Dictionary<int, Trigger> filteredTriggers = GetAllTriggersByType(triggerType);

            if (filteredTriggers.Count != 0)
            {
                if (stackedTriggers.Count == 0)
                {
                    stackedTriggers.Add(filteredTriggers);
                    Debug.Log("Initial level of stack: " + filteredTriggers.Last().Value.TriggerType + " (LEVEL " + stackedTriggers.Count + ")");
                }
                else if (stackedTriggers[stackedTriggers.Count-1].Last().Value.TriggerType != filteredTriggers.Last().Value.TriggerType)
                {
                    stackedTriggers.Add(filteredTriggers);
                    Debug.Log("New level of stack: " + filteredTriggers.Last().Value.TriggerType + " (LEVEL " + stackedTriggers.Count + ")");
                }
            }

            if (filteredTriggers.Count == 0)
            {
                Debug.Log("But all triggers with this type is already resolved!");
                Debug.Log("Current level of stack: " + stackedTriggers.Count);
                stackedTriggers.Remove(stackedTriggers.Last());
                Debug.Log("Changed level of stack: " + stackedTriggers.Count);

                triggerType = stackedTriggers[stackedTriggers.Count - 1].Last().Value.TriggerType;
                Debug.Log("Return to previous level of triggers: " + triggerType);
            }

            yield return Triggers.CallTrigger(triggerType);
        }
    }

    private static IEnumerator CallTrigger(TriggerTypes triggerType)
    {
        while (GetAllTriggersByTypeAndPlayer(Phases.PlayerWithInitiative, triggerType).Count > 0)
        {
            yield return CallTriggerForPlayer(Phases.PlayerWithInitiative, triggerType);
        }

        while (GetAllTriggersByTypeAndPlayer(Roster.AnotherPlayer(Phases.PlayerWithInitiative), triggerType).Count > 0)
        {
            yield return CallTriggerForPlayer(Roster.AnotherPlayer(Phases.PlayerWithInitiative), triggerType);
        }
    }

    private static IEnumerator CallTriggerForPlayer(Players.PlayerNo playerNo, TriggerTypes triggerType)
    {
        CurrentPlayer = playerNo;

        Dictionary<int, Trigger> results = GetAllTriggersByTypeAndPlayer(playerNo, triggerType);

        Debug.Log("Trigger + \"" + triggerType + "\" is called. Subscribed by: " + results.Count);
        if (results.Count == 1)
        {
            RemoveTrigger(results.First().Value.Id);
            results.First().Value.TriggerExecution.Invoke(results.First().Value.Sender, null);
        }
        else if (results.Count > 1)
        {
            Debug.Log("Start phase to show windows with results: " + results.Count);
            Phases.StartTemporarySubPhase("Triggers Order", typeof(TriggersOrderSubPhase));
            yield return Phases.WaitForTemporarySubPhasesFinish();
        }
    }

    //TODO: Rewrite two next methods into one

    private static Dictionary<int, Trigger> GetAllTriggersByTypeAndPlayer(Players.PlayerNo playerNo, TriggerTypes type)
    {
        var rawResults =
            from n in simultaneousTriggers
            where n.Value.TriggerType == type
            where (n.Value.Sender as Ship.GenericShip).Owner.PlayerNo == playerNo
            select n;
        Dictionary<int, Trigger> results = rawResults.ToDictionary(n => n.Key, n => n.Value);

        return results;
    }

    private static Dictionary<int, Trigger> GetAllTriggersByType(TriggerTypes type)
    {
        var rawResults =
            from n in simultaneousTriggers
            where n.Value.TriggerType == type
            select n;
        Dictionary<int, Trigger> results = rawResults.ToDictionary(n => n.Key, n => n.Value);

        return results;
    }

    private class TriggersOrderSubPhase : DecisionSubPhase
    {

        public override void Prepare()
        {
            int counter = 2;
            infoText = "Select a trigger to resolve";

            foreach (var trigger in simultaneousTriggers)
            {
                if ((trigger.Value.Sender as Ship.GenericShip).Owner.PlayerNo == Triggers.CurrentPlayer)
                {
                    string name = trigger.Value.Name;
                    while (decisions.ContainsKey(name))
                    {
                        name = trigger.Value.Name + " #" + counter++;
                    }
                    decisions.Add(name, delegate {
                        Phases.FinishSubPhase(this.GetType());
                        RemoveTrigger(trigger.Value.Id);
                        trigger.Value.TriggerExecution.Invoke(trigger.Value.Sender, null);
                    });

                }
            }

            Debug.Log(decisions.Count);
            defaultDecision = decisions.First().Key;
            
        }

    }

}

