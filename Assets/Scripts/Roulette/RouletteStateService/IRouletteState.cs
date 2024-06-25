using System;

public interface IRouletteState<T>
{
	event Action<ERouletteState> StateFinishedEvent;
	void EnterState(ERouletteState nextState);
	T ExitState();
}