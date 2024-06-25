using UnityEngine;

public class RouletteModel
{
	public readonly int         SectorsCount;
	public readonly int         CooldownTimer;
	public readonly Sprite[]    CurrencySprites;
	public readonly float       MinDelay;
	public readonly float       MaxDelay;
	public readonly float       MoveDuration;
	public readonly float		FinalDelay;
	public readonly int			MaxObjects;

	public int                  CurrentTime;
	public int                  SelectedSector;
	public int                  SelectedValue;
	public int                  LastSpriteIndex;
	public int					RewardValue;

	public RouletteModel(RouletteConfig rouletteConfig)
	{
		SectorsCount = rouletteConfig.SectorsCount;
		CooldownTimer = rouletteConfig.CooldownTimer;
		CurrencySprites = rouletteConfig.CurrencySprites;
		MinDelay = rouletteConfig.MinDelay;
		MaxDelay = rouletteConfig.MaxDelay;
		MoveDuration = rouletteConfig.MoveDuration;
		FinalDelay = rouletteConfig.FinalDelay;
		MaxObjects = rouletteConfig.MaxObjects;
	}
}
