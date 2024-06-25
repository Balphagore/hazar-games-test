using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RouletteView : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Button					_cooldownButton;
	[SerializeField]
	private TextMeshProUGUI			_cooldownButtonText;
	[SerializeField]
	private Button					_startButton;
	[SerializeField]
	private Transform				_wheel;
	[SerializeField]
	private TextMeshProUGUI[]		_sectors;
	[SerializeField]
	private Image					_icon;
	[SerializeField]
	private TextMeshProUGUI			_rewardCounter;
	[SerializeField]
	private Transform				_spawnPosition;

	[Header("Parameters")]
	[SerializeField]
	private int						_numberOfTurns;
	[SerializeField]
	private float					_rotationDuration;
	[SerializeField]
	private float					_iconScaleDuration;
	[SerializeField]
	private float					_r1;
	[SerializeField]
	private float					_r2;

	private RouletteController		_controller;

	public void Initialize(RouletteController controller)
	{
		_controller = controller;

		_controller.TimerStartedEvent += OnTimerStartedEvent;
		_controller.TimerStoppedEvent += OnTimerStoppedEvent;
		_controller.TimerUpdatedEvent += OnTimerUpdatedEvent;
		_controller.AnimationStartedEvent += OnAnimationStartedEvent;
		_controller.IconHidedEvent += OnIconHidedEvent;
		_controller.RewardValueUpdated += OnRewardValueUpdated;

		_controller.SpawnPositionRequestedEvent += OnSpawnPositionRequestedEvent;
		_controller.TargetPositionRequestedEvent += OnTargetPositionRequestedEvent;

		_startButton.onClick.AddListener(OnStartButtonClick);
	}

	private void OnDestroy()
	{
		_controller.TimerStartedEvent -= OnTimerStartedEvent;
		_controller.TimerStoppedEvent -= OnTimerStoppedEvent;
		_controller.TimerUpdatedEvent -= OnTimerUpdatedEvent;
		_controller.AnimationStartedEvent -= OnAnimationStartedEvent;
		_controller.IconHidedEvent -= OnIconHidedEvent;
		_controller.RewardValueUpdated -= OnRewardValueUpdated;

		_controller.SpawnPositionRequestedEvent -= OnSpawnPositionRequestedEvent;
		_controller.TargetPositionRequestedEvent -= OnTargetPositionRequestedEvent;

		_startButton.onClick.RemoveListener(OnStartButtonClick);
	}

	private void OnTimerStartedEvent()
	{
		_cooldownButton.gameObject.SetActive(true);
		_startButton.gameObject.SetActive(false);
		ChangeIconState(true);
		ChangeCounterState(false);
	}

	private void OnTimerStoppedEvent()
	{
		_cooldownButton.gameObject.SetActive(false);
		_startButton.gameObject.SetActive(true);
		_startButton.interactable = true;
	}

	private void OnTimerUpdatedEvent(string text, string[] values, Sprite sprite)
	{
		_cooldownButtonText.text = text;

		for (int i = 0; i < values.Length; i++)
		{
			_sectors[i].text = values[i];
		}

		_icon.sprite = sprite;
	}

	private void OnAnimationStartedEvent(int sector, Action callback)
	{
		_startButton.interactable = false;
		StartRotationToSector(sector, callback);
	}

	private void OnIconHidedEvent(Action callback)
	{
		ChangeIconState(false, callback);
	}

	private void OnRewardValueUpdated(string value)
	{
		_rewardCounter.text = value;
	}

	private Vector2 OnSpawnPositionRequestedEvent()
	{
		Vector2 spawnPosition2D = new Vector2(_spawnPosition.position.x, _spawnPosition.position.y);

		Vector2 randomOffset = Random.insideUnitCircle;

		randomOffset *= Random.Range(_r1, _r2);

		return spawnPosition2D + randomOffset;
	}

	private Vector2 OnTargetPositionRequestedEvent()
	{
		return _rewardCounter.transform.position;
	}

	private void ChangeIconState(bool isActive, Action callback = null)
	{
		if (isActive)
		{
			_icon.transform.DOScale(Vector3.one, 0);
		}
		else
		{
			_icon.transform.DOScale(Vector3.zero, _iconScaleDuration).OnComplete(
				() => 
					{
						ChangeCounterState(true, callback);
					}
				);
		}
	}

	private void ChangeCounterState(bool isActive, Action callback = null)
	{
		if (isActive)
		{
			_rewardCounter.gameObject.SetActive(true);
			_rewardCounter.transform.DOScale(Vector3.one, _iconScaleDuration).OnComplete(
					() => callback?.Invoke()
				);
		}
		else
		{
			_rewardCounter.transform.DOScale(Vector3.zero, 0);
			_rewardCounter.gameObject.SetActive(false);
		}
	}

	private void StartRotationToSector(int targetSector, Action callback)
	{
		float segmentAngle = 360f / _sectors.Length;
		float initialAngle = _wheel.transform.eulerAngles.z;
		float centerOffset = segmentAngle / 2;

		float targetAngle = segmentAngle * (_sectors.Length - targetSector) - centerOffset;

		_wheel.DORotate(new Vector3(0, 0, -targetAngle - ( 360 * _numberOfTurns )), _rotationDuration, RotateMode.FastBeyond360)
			.SetEase(Ease.OutCubic)
			.OnComplete(() => callback?.Invoke());
	}

	private void OnStartButtonClick()
	{
		_controller.StopWaiting();
	}
}
