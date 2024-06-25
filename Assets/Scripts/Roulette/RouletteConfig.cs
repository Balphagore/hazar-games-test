using UnityEngine;

[CreateAssetMenu]
public class RouletteConfig : ScriptableObject
{
	[Min(0)]
	public int		SectorsCount = 12;
	[Min(0)]
	public int		CooldownTimer = 10;
	public Sprite[]	CurrencySprites;
	[Min(0)]
	public float	MinDelay = 1;
	[Min(0)]
	public float	MaxDelay = 2.5f;
	[Min(0)]
	public float	MoveDuration = 1;
	[Min(0)]
	public float	FinalDelay = 2;
	[Min(0)]
	public int		MaxObjects = 20;
}
