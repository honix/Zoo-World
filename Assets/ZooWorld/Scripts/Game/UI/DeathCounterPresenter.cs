using System;
using VContainer.Unity;
using ZooWorld.Core.Stats;

namespace ZooWorld.Game.UI
{
    /// <summary>Pushes <see cref="DeathStats"/> into the <see cref="DeathCounterView"/>.</summary>
    public sealed class DeathCounterPresenter : IStartable, IDisposable
    {
        private readonly DeathStats _stats;
        private readonly DeathCounterView _view;

        public DeathCounterPresenter(DeathStats stats, DeathCounterView view)
        {
            _stats = stats;
            _view = view;
        }

        public void Start()
        {
            _stats.Changed += Refresh;
            Refresh();
        }

        public void Dispose() => _stats.Changed -= Refresh;

        private void Refresh() => _view.Show(_stats.DeadPrey, _stats.DeadPredators);
    }
}
