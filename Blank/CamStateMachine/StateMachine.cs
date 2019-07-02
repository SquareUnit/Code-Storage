// Création du script : Felix Desrosiers-Dorval
// Date : 02-04-2019 16:15

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public IStates currentState;
    public IStates previousState;

    /// <summary>Change the current state of the state machine</summary>
    /// <param name="newState">The desired state you wish the FSm to change to </param>
    public void ChangeState(IStates newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
            previousState = currentState;
        }
        currentState = newState;
        currentState.Enter();
    }

    /// <summary>Contains all logic that verify if current state should be playing it's execute or change to a new state</summary>
    public void CheckIfStateChange()
    {
        if (currentState != null)
            currentState.IfStateChange();
    }

    /// <summary>Update loop for the states of the state machine</summary>
    public void CurrentStateUpdate()
    {
        if (currentState != null)
            currentState.StateUpdate();
    }

    /// <summary>Revert current state to previous state</summary>
    public void SwitchToPreviousState()
    {
        currentState.Exit();
        currentState = previousState;
        currentState.Enter();
    }
}
