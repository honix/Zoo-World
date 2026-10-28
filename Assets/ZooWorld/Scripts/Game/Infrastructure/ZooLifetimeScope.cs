using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZooWorld.Game.Configs;
using ZooWorld.Game.UI;
using ZooWorld.Game.World;

namespace ZooWorld.Game.Infrastructure
{
    /// <summary>
    /// Composition root of the Zoo scene. The scene only holds content and references;
    /// every service and runtime object is created here, in code.
    /// </summary>
    public sealed class ZooLifetimeScope : LifetimeScope
    {
        [SerializeField] private WorldSettings _settings;
        [SerializeField] private Camera _camera;

        [Header("UI")]
        [SerializeField] private DeathCounterView _deathCounter;
        [SerializeField] private TastyLabel _tastyLabelPrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            var playArea = new CameraPlayArea(_camera, _settings.ScreenMargin, _settings.SpawnMargin);
            new ZooInstaller(_settings, playArea).Install(builder);

            builder.RegisterComponent(_deathCounter);
            builder.RegisterEntryPoint<DeathCounterPresenter>();
            builder.RegisterEntryPoint<TastyLabelPresenter>().WithParameter(_tastyLabelPrefab);
        }

        private void OnDrawGizmos()
        {
            if (_settings == null || _camera == null) return;

            var playArea = new CameraPlayArea(_camera, _settings.ScreenMargin, _settings.SpawnMargin);
            DrawRect(playArea.Visible(), new Color(1f, 1f, 1f, 0.35f));
            DrawRect(playArea.Bounds, new Color(1f, 0.6f, 0.1f, 0.9f));
            DrawRect(playArea.SpawnBounds, new Color(0.2f, 1f, 0.4f, 0.9f));
        }

        private static void DrawRect(GroundRect rect, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawWireCube(rect.Center, new Vector3(rect.Width, 0.01f, rect.Depth));
        }
    }
}
