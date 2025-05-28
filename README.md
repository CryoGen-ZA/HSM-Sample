# Hierarchical State Machine (HSM) Framework Sample

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
Statmachine
|
├── SuperState (manages broad behavior categories)
    ├── SubState A (handles specific behaviors, often reusable between super behaviours)
    |   |
    |   └── SubState A-A (A state within the substate)
    |
    └── SubState B (handles specific behaviors, often reusable between super behaviours)
```
**Key Advantage**: Rather than being restricted to a single active state, HSMs allow for layered states—superstates managing broad logic and substates handling finer details. Substates can even contain other substates, enabling deep composition. This reduces flat-state complexity and allows shared behavior at different hierarchy levels.

### Techinal Implementation:

All States inherit from an abstract Base state, conveniently called "BaseState"
```csharp
public abstract class BaseState
{
    protected readonly StateGenerator Generator;
    private BaseState _subState;
    private BaseState _superState;

    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void UpdateState();
    public abstract bool CheckStateSwitch(out string newState);
}
```
The abstract base class enforces a consistent interface while allowing flexible implementations.
All states also have access to the StateGenerator, allowing them to manage their own substate transitions.

The State Generator handles the tracking of existing states, allowing us to reuse them instead of reimplementing them each time a state is changed.
```csharp
public class StateGenerator
{
    private Dictionary<string, BaseState> _states = new();
    
    public BaseState GetState(string key) => _states.GetValueOrDefault(key);
    public void AddState(string key, BaseState state) => _states[key] = state;
}
```
This provides us with centralized state management with O(1) lookup performance using the Dictionary. This is important as a HSM could potentially have many states active at a time.


The State machine is in charge of handling the super states and allow the states and their substates to tick;
```csharp
public class StateMachine
{
    private BaseState _currentBaseState;
    private StateGenerator _generator;

    public void Initialize()
    {
    }

    public void DoUpdate()
    {
    }

    private void CheckStateSwitching()
    {
    }

    public void SetState(BaseState newState)
    {
    }
    public string GetStateAndSubStateNames()
    {
    }
 }
```
This is a very simple focused controller that delegates state management to the states themselves, following the single responsibility principle. This approach makes it very reusable between use cases. Examples would be to use this not only for the player but for enemies and even environment objects that may need some form of state management.


### How a state is added to that state machine:

First a state would be created, example of the state class below:
```csharp
 public class ExampleState : BaseState
    {
        private readonly PlayerCharacter _ctx;

        public IdleState(StateGenerator generator, PlayerCharacter ctx) : base(generator)
        {
            _ctx = ctx;
        }

        public override void EnterState()
        {
          //Enter Logic
        }

        public override void ExitState()
        {
          //Exit logic
        }

        public override void UpdateState()
        {
          //Update logic
        }

        public override bool CheckStateSwitch(out string newState)
        {
            if (CheckisTrue)
            {
                newState = nameof(NextStateClass);
                return true;
            }
            
            newState = null;
            return false;
        }
    }
```

Once the state is created, we can register the state using the Register state wrapper function available in the StateMachineBehaviour class:
```csharp
public abstract class StateMachineBehaviour : MonoBehaviour
{
    protected StateMachine _stateMachine;
    
    protected void RegisterState<T>(T state) where T : BaseState
    {
        _stateMachine.Generator.AddState(typeof(T).Name, state);
    }
    
    protected abstract void RegisterStates();
}
```
We would register like the following within the class inheriting from this abstract class:

```csharp
protected override void RegisterStates()
        {
            RegisterState(new GroundedBaseState(_stateMachine.Generator, this));
            RegisterState(new IdleState(_stateMachine.Generator, this));
            RegisterState(new RunState(_stateMachine.Generator, this));
            RegisterState(new JumpState(_stateMachine.Generator, this));
            RegisterState(new FallingState(_stateMachine.Generator, this));
        }
```

Then we need to set the first state to be used, this can be done anywhere in your code as long as its after the awake function of the StateMachineBehaviour abstract class:
```csharp
_stateMachine.SetState(_stateMachine.Generator.GetState(nameof(GroundedBaseState)));
```


### Key Notes

This implementation of the HSM allows for the following benefits:

- **Reduced code complexity**
- **Loose coupling**, states manages themselves and interact within their interface restrictions
- **Consistent patterns**
- **Clear separation of logic**
- **Easy extending** as new states can be made with complex logic, without the need of modifying the HSM framework
- **Dependency Injection**, each states receive their dependencies beforehand, with some changes, unit testing could be implemented too.
- **Isolated States**, Since each state cares about itself, it makes it much easier to debug logic issues as you know they are restricted to the state in question.
- **Reusable Substates**, depending on the need, substates could be used between super states, removing the need for duplicate states.
