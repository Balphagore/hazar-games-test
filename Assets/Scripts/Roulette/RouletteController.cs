using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using VContainer;
using Random = UnityEngine.Random;

public class RouletteController
{
	private readonly RouletteModel                  _model;
	private readonly CoroutineRunner                _coroutineRunner;
	private readonly Transform                      _canvas;

	private List<(Transform transform, int value)>  _flyingImages;

	public event Action                             TimerStartedEvent;
	public event Action                             TimerStoppedEvent;
	public event Action<string, string[], Sprite>   TimerUpdatedEvent;
	public event Action<int, Action>                AnimationStartedEvent;
	public event Action<Action>                     IconHidedEvent;
	public event Action<Action>                     RewardAddedEvent;
	public event Action<string>						RewardValueUpdated;

	public Func<Vector2>                            SpawnPositionRequestedEvent;
	public Func<Vector2>							TargetPositionRequestedEvent;

	private event Action                            _awaitingCallback;

	[Inject]
	public RouletteController(IObjectResolver resolver)
	{
		RouletteConfig rouletteConfig = resolver.Resolve<RouletteConfig>();
		_model = new(rouletteConfig);
		_coroutineRunner = resolver.Resolve<CoroutineRunner>();
		_canvas = resolver.Resolve<Transform>();
	}

	public void StartTimer(Action callback)
	{
		_model.CurrentTime = _model.CooldownTimer;
		_flyingImages = new();
		_model.RewardValue = 0;
		RewardValueUpdated?.Invoke(_model.RewardValue.ToString());
		TimerStartedEvent?.Invoke();
		_coroutineRunner.StartCoroutine(StartTimerCoroutine(_model.CooldownTimer, callback));
	}

	public void StartWaiting(Action callback)
	{
		_awaitingCallback = callback;
	}

	public void StopWaiting()
	{
		_awaitingCallback?.Invoke();
	}

	public void GiveReward(Action callback)
	{
		AnimationStartedEvent?.Invoke(
			_model.SelectedSector,
			() => IconHidedEvent?.Invoke(
				() => SpawnRewards(
						_model.SelectedValue,
						_model.CurrencySprites[_model.LastSpriteIndex],
						() => MoveRewards(() => callback?.Invoke())
					)
				)
			);
	}

	public void SpawnRewards(int summary, Sprite sprite, Action callback)
	{
		(int count, List<int> values) result = CalculateValues(summary);
		int completedTasks = 0;
		for (int i = 0; i < result.count; i++)
		{
			int localIndex = i;
			AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync("FlyingImage", SpawnPositionRequestedEvent.Invoke(), Quaternion.identity, _canvas);
			handle.Completed += (AsyncOperationHandle<GameObject> obj) =>
			{
				if (obj.Status == AsyncOperationStatus.Succeeded)
				{
					GameObject instance = obj.Result;
					instance.GetComponent<Image>().sprite = sprite;
					_flyingImages.Add((instance.transform, result.values[localIndex]));
					instance.transform.localScale = Vector3.zero;
					instance.transform.DOScale(1, 0.5f)
						.OnComplete(() =>
						{
							completedTasks++;
							if (completedTasks == result.count)
							{
								callback?.Invoke();
							}
						});
				}
			};
		}
	}

	public (int count, List<int> values) CalculateValues(int summary)
	{
		int count = Math.Min(_model.MaxObjects, summary);
		int baseValue = summary / count;
		int extraValueCount = summary % count;

		List<int> values = new List<int>();

		for (int i = 0; i < count; i++)
		{
			values.Add(baseValue);
		}

		for (int i = 0; i < extraValueCount; i++)
		{
			values[i]++;
		}

		return (count, values);
	}

	private void MoveRewards(Action callback)
	{
		int completedAnimations = 0;
		Vector2 targetPosition = TargetPositionRequestedEvent.Invoke();
		float initialDistance = Vector2.Distance(_flyingImages[0].transform.position, targetPosition);

		foreach ((Transform transform, int value) in _flyingImages)
		{
			float delay = Random.Range(_model.MinDelay, _model.MaxDelay);

			transform.DOMove(targetPosition, _model.MoveDuration)
				.SetDelay(delay)
				.OnStart(() =>
				{
					transform.localScale = Vector3.one;
				})
				.OnUpdate(() =>
				{
					float currentDistance = Vector2.Distance(transform.position, targetPosition);
					float scaleValue = currentDistance / initialDistance;
					transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
				})
				.OnComplete(() =>
				{
					UnityEngine.Object.Destroy(transform.gameObject);
					_model.RewardValue += value;
					RewardValueUpdated?.Invoke(_model.RewardValue.ToString());

					completedAnimations++;
					if (completedAnimations == _flyingImages.Count)
					{
						_coroutineRunner.StartCoroutine(DelayedCallback(callback));
					}
				});
		}
	}

	private IEnumerator DelayedCallback(Action callback)
	{
		yield return new WaitForSeconds(_model.FinalDelay);
		callback?.Invoke();
	}

	private string[] GetValues(int valuesCount)
	{
		List<int> availableValues = new List<int>();

		for (int i = 5; i <= 100; i += 5)
		{
			availableValues.Add(i);
		}

		int n = availableValues.Count;
		while (n > 1)
		{
			n--;
			int k = UnityEngine.Random.Range(0, n + 1);
			int value = availableValues[k];
			availableValues[k] = availableValues[n];
			availableValues[n] = value;
		}

		int[] intValues = availableValues.Take(valuesCount).ToArray();

		int selectedIndex = UnityEngine.Random.Range(0, valuesCount);
		_model.SelectedSector = selectedIndex;
		_model.SelectedValue = intValues[selectedIndex];

		return intValues.Select(x => x.ToString()).ToArray();
	}

	private void SelectSpriteIndex()
	{
		int randomIndex;
		do
		{
			randomIndex = UnityEngine.Random.Range(0, _model.CurrencySprites.Length);
		}
		while (randomIndex == _model.LastSpriteIndex && _model.CurrencySprites.Length > 1);

		_model.LastSpriteIndex = randomIndex;
	}

	private IEnumerator StartTimerCoroutine(int time, Action callback)
	{
		for (int i = time; i > 0; i--)
		{
			_model.CurrentTime = i;
			SelectSpriteIndex();
			TimerUpdatedEvent?.Invoke(i.ToString(), GetValues(_model.SectorsCount), _model.CurrencySprites[_model.LastSpriteIndex]);
			yield return new WaitForSeconds(1);
		}

		Debug.Log(_model.SelectedValue);
		TimerStoppedEvent?.Invoke();
		callback?.Invoke();
	}
}
