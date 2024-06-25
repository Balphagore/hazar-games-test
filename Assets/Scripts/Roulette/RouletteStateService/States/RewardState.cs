using System;
using VContainer;

public class RewardState : IRouletteState<RouletteController>
{
	private readonly RouletteController	_controller;

	public event Action<ERouletteState> StateFinishedEvent;

	[Inject]
	public RewardState(IObjectResolver resolver, RouletteController controller)
	{
		_controller = controller;
	}

	public void EnterState(ERouletteState nextState)
	{
		_controller.GiveReward(() => StateFinishedEvent?.Invoke(ERouletteState.Cooldown));
	}

	public RouletteController ExitState()
	{
		return _controller;
	}
}
