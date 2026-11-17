# Hierarchical State Machine (HSM) Framework Sample

>**_NOTE FOR WHEN OPENING THE PROJECT_**
>- If you open the project and get a message from FMOD about the banks being broken, press repair on the dialogue window that popped up to fix the issue.
>- To test the HSM in action, open the testing level located at: `/Assets/Scenes/Testing Scene.unity` then press play.

## OVERVIEW

In game development, logic complexity tends to grow rapidly—whether in character behavior, gameplay systems, or UI flows. What often begins as simple logic can evolve into a tangled web of states, each dependent on others, leading to tight coupling and fragile code.

To manage this growing complexity, developers commonly adopt the State design pattern, isolating logic into self-contained "states." One popular implementation is the Finite State Machine (FSM)—like Unity’s Animator system—where each state transitions to others based on rules.

However, traditional FSMs introduce several challenges:

- State Explosion: Flat state machines become unmanageable as complexity grows
- Code Duplication: Similar state behaviors require repetitive implementations
- Transition Management: Complex state switching logic leads to bugs and maintenance issues
- Debugging Complexity: Difficulty tracking state hierarchies across multiple entities
- Rigid Architecture: Adding new states requires modifying existing code structures

A better alternative for managing complex, layered behaviors is a Hierarchical State Machine (HSM). The framework in this project provides a robust, flexible solution using hierarchical composition and separation of concerns.

### How is a HSM different from other state machines?

Below is an example of how a HSM would be structured:
```
StateMachine
|
├── SuperState (manages broad behavior categories)
    ├── SubState A (default: entered automatically with its SuperState)
    |   |
    |   └── SubState A-A (A state within the substate)
    |
    └── SubState B (handles specific behaviors)
```
**Key Advantage**: Rather than being restricted to a single active state, HSMs allow for layered states—superstates managing broad logic and substates handling finer details. Substates can even contain other substates, enabling deep composition. This reduces flat-state complexity and allows shared behavior at different hierarchy levels.

At any time the machine has one **active path**, from a root state down to a leaf (e.g. `Grounded > Run`). Entering a superstate automatically enters its default substate, and leaving a superstate always exits its active substate first.

### Technical Implementation

The framework lives in `Assets/Scripts/HSM/Core` and is made of four pieces: `BaseState`, `Transition`, `StateMachine` and `HsmBehaviour`.

#### BaseState

All states inherit from the abstract `BaseState`. A state only contains **behaviour**: it doesn't switch its own substates or decide its own transitions. Its position in the tree is fixed when it is registered, and every lifecycle hook is optional, so you only override what you need.
```csharp
public abstract class BaseState
{
    // Set once at registration, read-only afterwards
    public StateMachine Machine { get; }
    public BaseState Parent { get; }
    public BaseState DefaultChild { get; }
    public int Depth { get; }
    public bool IsActive { get; }

    protected virtual void OnEnter() { }      // before its default child is entered
    protected virtual void OnExit() { }       // after its active child has been exited
    protected virtual void OnUpdate() { }     // every tick, parents before children
    protected virtual void OnLateUpdate() { } // every tick, children before parents

    // Imperative escape hatch, applied at the start of the next tick
    protected void RequestTransition<T>(bool allowReenter = false) where T : BaseState;
}
```
The hooks are called only by the state machine. That guarantees a consistent order: children are always exited before their parents, and parents are always entered before their children.

#### Transition

A transition is a guarded edge to a target state:
```csharp
public sealed class Transition
{
    public BaseState Target { get; }
    public Func<bool> Condition { get; }
    public bool AllowReenter { get; } // exit and re-enter the target if it is already active
}
```
Transitions are declared during registration instead of being written into each state, so the whole transition graph of an entity can be read in one place.

#### StateMachine

The `StateMachine` owns the state tree, the active path and all transitions. States are stored in a `Dictionary<Type, BaseState>`, which gives O(1) type-safe lookups. Asking for a state that was never registered throws a clear error instead of returning `null`.
```csharp
public sealed class StateMachine
{
    public T AddState<T>(T state, BaseState parent = null, bool isDefault = false) where T : BaseState;
    public void AddTransition<TFrom, TTo>(Func<bool> condition, bool allowReenter = false);
    public void AddAnyTransition<TTo>(Func<bool> condition, bool allowReenter = false);
    public T GetState<T>() where T : BaseState;

    public void Start<T>() where T : BaseState;
    public void Update();
    public void RequestTransition<T>(bool allowReenter = false) where T : BaseState;

    public IReadOnlyList<BaseState> ActivePath { get; }
    public BaseState ActiveLeaf { get; }
    public event Action<BaseState, BaseState> OnTransition; // (previous leaf, new leaf)
    public string GetActivePathDescription();
}
```

**What happens every `Update()`:**
1. **Find a transition.** "Any" transitions are checked first, then the transitions of each active state from root to leaf. The first condition that passes wins, and at most one transition happens per tick. Because parents are checked first, a transition declared on a superstate applies to all of its substates.
2. **Perform it** (see below).
3. **Tick the active path.** `OnUpdate` runs root → leaf, then `OnLateUpdate` runs leaf → root.

**How a transition is performed:**
The machine finds the *lowest common ancestor* (LCA) of the current active path and the target, then:
1. exits from the active leaf up to, but not including, the LCA;
2. enters from below the LCA down to the target;
3. keeps entering `DefaultChild` states until it reaches a leaf.

Using the player's tree as an example:

| Transition | Calls |
|---|---|
| `Idle → Run` | Exit Idle, Enter Run (`Grounded` stays active) |
| `Run → Jump` | Exit Run, Exit Grounded, Enter Jump |
| `Falling → Grounded` | Exit Falling, Enter Grounded, Enter Idle (default child) |

**Built-in safety:**
- A transition to a state that is already active is ignored, unless it was declared with `allowReenter: true`.
- A transition requested from inside `OnEnter`/`OnExit` is queued and runs as soon as the current transition finishes, so the tree is never changed while it is still being entered or exited.
- More than 8 chained transitions in a row throws an error, to catch transition loops.
- `Start<T>()` validates that every state with substates has a default substate.

#### HsmBehaviour

`HsmBehaviour` is the `MonoBehaviour` that hosts a `StateMachine`. Inherit from it on any object that needs states (player, enemies, environment objects). It was previously named `StateMachineBehaviour` and was renamed to avoid clashing with Unity's own `UnityEngine.StateMachineBehaviour`.
```csharp
public abstract class HsmBehaviour : MonoBehaviour
{
    protected StateMachine Machine { get; }
    public Animator Animator { get; }
    public string GetStatesDebugText { get; } // shown in the inspector while playing

    protected abstract void RegisterStates(); // called from Awake

    protected T AddState<T>(T state, BaseState parent = null, bool isDefault = false) where T : BaseState;
    protected void AddTransition<TFrom, TTo>(Func<bool> condition, bool allowReenter = false);
    protected void AddAnyTransition<TTo>(Func<bool> condition, bool allowReenter = false);
}
```

### How a state is added to the state machine

**1. Create the state.** Give it the context it needs through its constructor, and override only the hooks it uses:
```csharp
public class ExampleState : BaseState
{
    private readonly PlayerCharacter _ctx;

    public ExampleState(PlayerCharacter ctx)
    {
        _ctx = ctx;
    }

    protected override void OnEnter()
    {
        // Enter logic
    }

    protected override void OnUpdate()
    {
        // Update logic
    }

    protected override void OnExit()
    {
        // Exit logic
    }
}
```

**2. Register the states and transitions** in `RegisterStates()` of the class inheriting from `HsmBehaviour`. This is the player's setup from `PlayerCharacter.cs`:
```csharp
protected override void RegisterStates()
{
    // Tree
    var grounded = AddState(new GroundedBaseState(this));
    AddState(new IdleState(), grounded, isDefault: true);
    AddState(new RunState(this), grounded);
    AddState(new JumpState(this));
    AddState(new FallingState(this));

    // Evaluated parent-first, first match wins: Grounded's exits apply while in Idle or Run.
    AddTransition<GroundedBaseState, JumpState>(() => JumpPressed);
    AddTransition<GroundedBaseState, FallingState>(() => !IsGrounded);
    AddTransition<IdleState, RunState>(() =>
        PlayerInput.magnitude > 0 || MovementComponent.GetCurrentVelocityWithoutGravity().magnitude > 0);
    AddTransition<RunState, IdleState>(() =>
        MovementComponent.GetCurrentVelocityWithoutGravity() == Vector3.zero);
    AddTransition<JumpState, FallingState>(() => MovementComponent.GetCurrentVelocity().y < 0);
    AddTransition<FallingState, GroundedBaseState>(() => IsGrounded);
}
```

Transitions that should fire from any state can be declared with `AddAnyTransition`. This is an illustrative example; `DeadState` is not part of the sample:
```csharp
AddAnyTransition<DeadState>(() => Health <= 0);
```

**3. Start and tick the machine.** Start it after `base.Awake()` has registered the states, and update it every frame:
```csharp
protected override void Awake()
{
    base.Awake();
    // ... set up anything the states need ...
    Machine.Start<GroundedBaseState>();
}

private void Update()
{
    Machine.Update();
}
```

### Key Notes

This implementation of the HSM allows for the following benefits:

- **Reduced code complexity**
- **Loose coupling**: states hold behaviour only. Transitions are declared in one place, so an entity's whole state graph is visible at a glance.
- **Hierarchical transitions**: a transition can go from any level to any other level, and a transition declared on a superstate covers all of its substates.
- **Guaranteed enter/exit order**: substates are always exited before their superstate and entered after it, so no state is left half-active.
- **Consistent patterns**
- **Clear separation of logic**
- **Easy extending**: new states can be made with complex logic without modifying the HSM framework.
- **Fail-fast validation**: unknown states, duplicate registrations, missing default substates and transition loops are reported with clear errors.
- **Dependency injection**: each state receives its dependencies beforehand. `BaseState`, `Transition` and `StateMachine` have no Unity dependency, so the machine itself can be unit tested outside Unity.
- **Isolated states**: since each state only cares about itself, logic issues are much easier to debug because they are restricted to the state in question.
- **Shared behaviour through superstates**: each state type is registered once, under a single parent. Behaviour and transitions that several states have in common belong in their shared superstate (e.g. gravity and the jump/fall transitions in `GroundedBaseState`).
