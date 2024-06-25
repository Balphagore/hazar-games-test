using UnityEngine;
using VContainer;

public class RouletteStateService
{
	private IObjectResolver						_resolver;
	private ERouletteState						_initialState = ERouletteState.Initialization;

	private IRouletteState<RouletteController>  _currentState;

	[Inject]
	public RouletteStateService(IObjectResolver resolver)
	{
		_resolver = resolver;
		ActivateState(_initialState);
	}

	public void ActivateState(ERouletteState state)
	{
		switch (state)
		{
			case ERouletteState.None:

				break;

			case ERouletteState.Initialization:

				_currentState = new InitializationState(_resolver);
				_currentState.StateFinishedEvent += OnStateFinishedEvent;
				_currentState.EnterState(ERouletteState.Cooldown);

				break;

			case ERouletteState.Cooldown:

				RouletteController controller = _currentState.ExitState();
				_currentState = new CooldownState(_resolver, controller);
				_currentState.StateFinishedEvent += OnStateFinishedEvent;
				_currentState.EnterState(ERouletteState.Active);

				break;

			case ERouletteState.Active:

				controller = _currentState.ExitState();
				_currentState = new ActiveState(_resolver, controller);
				_currentState.StateFinishedEvent += OnStateFinishedEvent;
				_currentState.EnterState(ERouletteState.Reward);

				break;

			case ERouletteState.Reward:

				controller = _currentState.ExitState();
				_currentState = new RewardState(_resolver, controller);
				_currentState.StateFinishedEvent += OnStateFinishedEvent;
				_currentState.EnterState(ERouletteState.Cooldown);

				break;

			default:

				Debug.LogError("Invalid state");

				break;
		}
	}

	private void OnStateFinishedEvent(ERouletteState nextState)
	{
		_currentState.StateFinishedEvent -= OnStateFinishedEvent;
		ActivateState(nextState);
	}
}
