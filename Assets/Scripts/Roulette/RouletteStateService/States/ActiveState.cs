using System;
using VContainer;

public class ActiveState : IRouletteState<RouletteController>
{
	private readonly RouletteController _controller;

	public event Action<ERouletteState> StateFinishedEvent;

	[Inject]
	public ActiveState(IObjectResolver resolver, RouletteController controller)
	{
		_controller = controller;
	}

	public void EnterState(ERouletteState nextState)
	{
		_controller.StartWaiting(() =>
		{
			StateFinishedEvent?.Invoke(ERouletteState.Reward);
		});
	}

	public RouletteController ExitState()
	{
		return _controller;
	}
}
