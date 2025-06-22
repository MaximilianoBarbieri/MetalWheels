using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSM;
using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("Interacción")] public float interactionRange = 2f;
    private HashSet<GameObject> _interactablesInRange = new();

    private float _life;

    public Transform target;
    public Animator animator;
    public float speed;
    public float speedRotation;

    private FiniteStateMachine _fsm;

    [SerializeField] private Idle_NPC idleNpc;
    [SerializeField] private Walk_NPC walkNpc;
    [SerializeField] private Escape_NPC escapeNpc;
    [SerializeField] private Talk_NPC talkNpc;
    [SerializeField] private Sitdown_NPC sitdownNpc;
    [SerializeField] private Death_NPC deathNpc;


    private void Start()
    {
        _fsm = new FiniteStateMachine(idleNpc, StartCoroutine);

        // Activamos la FSM
        _fsm.Active = true;

        // Disparamos el primer plan
        StartCoroutine(RunPlanLoop());

        #region OldFSM

        // _fsm.AddTransition(Utils.ToIdleNpc, walkNpc, idleNpc);
        // _fsm.AddTransition(Utils.ToIdleNpc, escapeNpc, idleNpc);
        // _fsm.AddTransition(Utils.ToIdleNpc, talkNpc, idleNpc);
        // _fsm.AddTransition(Utils.ToIdleNpc, sitdownNpc, idleNpc);
        //
        // _fsm.AddTransition(Utils.ToWalkNpc, idleNpc, walkNpc);
        // _fsm.AddTransition(Utils.ToWalkNpc, escapeNpc, walkNpc);
        // _fsm.AddTransition(Utils.ToWalkNpc, talkNpc, walkNpc);
        // _fsm.AddTransition(Utils.ToWalkNpc, sitdownNpc, walkNpc);
        //
        // _fsm.AddTransition(Utils.ToEscapeNpc, idleNpc, escapeNpc);
        // _fsm.AddTransition(Utils.ToEscapeNpc, walkNpc, escapeNpc);
        // _fsm.AddTransition(Utils.ToEscapeNpc, talkNpc, escapeNpc);
        // _fsm.AddTransition(Utils.ToEscapeNpc, sitdownNpc, escapeNpc);
        //
        // _fsm.AddTransition(Utils.ToTalkNpc, idleNpc, talkNpc);
        // _fsm.AddTransition(Utils.ToTalkNpc, walkNpc, talkNpc);
        // _fsm.AddTransition(Utils.ToTalkNpc, escapeNpc, talkNpc);
        // _fsm.AddTransition(Utils.ToTalkNpc, sitdownNpc, talkNpc);
        //
        // _fsm.AddTransition(Utils.ToSitdownNpc, idleNpc, sitdownNpc);
        // _fsm.AddTransition(Utils.ToSitdownNpc, walkNpc, sitdownNpc);
        // _fsm.AddTransition(Utils.ToSitdownNpc, escapeNpc, sitdownNpc);
        // _fsm.AddTransition(Utils.ToSitdownNpc, talkNpc, sitdownNpc);
        //
        // _fsm.AddTransition(Utils.ToDeathNpc, idleNpc, deathNpc);
        // _fsm.AddTransition(Utils.ToDeathNpc, walkNpc, deathNpc);
        // _fsm.AddTransition(Utils.ToDeathNpc, escapeNpc, deathNpc);
        // _fsm.AddTransition(Utils.ToDeathNpc, talkNpc, deathNpc);
        // _fsm.AddTransition(Utils.ToDeathNpc, sitdownNpc, deathNpc);

        #endregion
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable"))
            _interactablesInRange.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable"))
            _interactablesInRange.Remove(other.gameObject);
    }

    public GameObject GetClosestInteractable()
    {
        return _interactablesInRange
            .Where(obj => obj != null)
            .OrderBy(obj => Vector3.Distance(transform.position, obj.transform.position))
            .FirstOrDefault();
    }

    public WorldState GetCurrentWorldState()
    {
        var closest = GetClosestInteractable();
        var type = closest ? closest.GetComponent<InteractableNPC>()?.type : null;

        return new WorldState
        {
            isAlive = _life > 0f,
            life = _life,
            carInRange = closest != null,
            interactionType = type
        };
    }

    public List<GoapAction> GetAvailableActions()
    {
        return new List<GoapAction>
        {
            new GoapAction
            {
                Name = "Idle",
                Precondition = s => !s.carInRange,
                Effect = s => s.Clone(),
                Execute = () => (GoToState(idleNpc))
            },
            new GoapAction
            {
                Name = "Walk",
                Precondition = s => !s.carInRange, //TODO: Agregar otra condicion, por ejemplo, vecinos cerca
                Effect = s => s.Clone(),
                Execute = () => (GoToState(walkNpc))
            },
            new GoapAction
            {
                Name = "Talk",
                Precondition = s => !s.carInRange && s.interactionType == InteractionType.Talk,
                Effect = s => s.Clone(),
                Execute = () => (GoToState(talkNpc))
            },
            new GoapAction
            {
                Name = "Sitdown",
                Precondition = s => !s.carInRange && s.interactionType == InteractionType.Sit,
                Effect = s => s.Clone(),
                Execute = () => (GoToState(sitdownNpc))
            },
            new GoapAction
            {
                Name = "Escape",
                Precondition = s => s.life < 30f,
                Effect = s =>
                {
                    var ns = s.Clone();
                    ns.life += 20f;
                    return ns;
                },
                Execute = () => (GoToState(escapeNpc))
            },
            new GoapAction
            {
                Name = "Death",
                Precondition = s => s.life <= 0f,
                Effect = s => s.Clone(),
                Execute = () => (GoToState(deathNpc))
            },
        };
    }

    public IEnumerator RunPlan()
    {
        var current = GetCurrentWorldState();
        var goal = new System.Func<WorldState, bool>(s => s.life >= 110f); // ejemplo

        var plan = GoapPlanner.Plan(current, goal, GetAvailableActions()).FirstOrDefault();
        if (plan != null)
        {
            foreach (var action in plan)
                yield return StartCoroutine(action.Execute());
        }
        else
        {
            Debug.Log("No plan found.");
        }
    }

    private IEnumerator RunPlanLoop()
    {
        while (true)
        {
            yield return RunPlan();
            yield return new WaitForSeconds(1f);
        }
    } 

    private IEnumerator GoToState(IState nextState)
    {
        _fsm.TransitionTo(nextState);
        yield break;
    }
}