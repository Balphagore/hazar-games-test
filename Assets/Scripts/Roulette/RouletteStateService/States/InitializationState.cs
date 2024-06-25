using UnityEngine.AddressableAssets;
using UnityEngine;
using VContainer;
using System;
using Object = UnityEngine.Object;

public class InitializationState : IRouletteState<RouletteController>
{
	private readonly RouletteController	_controller;
	private readonly Transform			_canvas;

	public event Action<ERouletteState> StateFinishedEvent;

	[Inject]
	public InitializationState(IObjectResolver resolver)
	{
		_controller = new(resolver);
		_canvas = resolver.Resolve<Transform>();
	}

	public void EnterState(ERouletteState nextState)
	{
		Addressables.LoadAssetAsync<GameObject>("RouletteView").Completed += handle =>
		{
			if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
			{
				GameObject prefab = handle.Result;
				RouletteView rouletteView = Object.Instantiate(prefab, _canvas).GetComponent<RouletteView>();
				rouletteView.Initialize(_controller);

				StateFinishedEvent?.Invoke(nextState);
			}
			else
			{
				Debug.LogError("Failed to load the asset from Addressables.");
			}
		};
	}

	public RouletteController ExitState()
	{
		return _controller;
	}
}
