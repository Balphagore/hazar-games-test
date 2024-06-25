using System;
using VContainer;

public class CooldownState : IRouletteState<RouletteController>
{
	private readonly RouletteController	_controller;

	public event Action<ERouletteState>	StateFinishedEvent;

	[Inject]
	public CooldownState(IObjectResolver resolver, RouletteController controller)
	{
		_controller = controller;
	}

	public void EnterState(ERouletteState nextState)
	{
		_controller.StartTimer( () =>
		{
			StateFinishedEvent?.Invoke(ERouletteState.Active);
		});
	}

	public RouletteController ExitState()
	{
		return _controller;
	}
}
