using UnityEngine;
using VContainer;
using VContainer.Unity;

public class SceneInstaller : LifetimeScope
{
	[SerializeField] 
	private Transform		_canvas;
	[SerializeField]
	private CoroutineRunner	_coroutineRunner;

	[Header("Configurations")]
	[SerializeField]
	private RouletteConfig	_rouletteConfig;

	protected override void Configure(IContainerBuilder builder)
	{
		builder.RegisterInstance(_canvas).AsSelf();

		builder.RegisterInstance(_coroutineRunner).AsSelf();

		builder.RegisterInstance(_rouletteConfig).AsImplementedInterfaces();

		builder.Register<RouletteStateService>(Lifetime.Singleton).AsSelf();

		builder.RegisterBuildCallback(resolver =>
		{
			var service = resolver.Resolve<RouletteStateService>();
		});
	}
}
