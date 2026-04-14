using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Domain
{
    public abstract class BaseFeatureInstaller : MonoInstaller
    {
        private List<IBaseFeature> _features = new List<IBaseFeature>();

        public override async void Start()
        {
            base.Start();

            await DoBeforeInitialization();
            await InitFeatures();
            await DoAfterInitialization();
        }

        public override void InstallBindings()
        {
            AddFeatures();
            BindFeatures();
        }

        protected abstract void AddFeatures();

        protected void AddFeature<T>() where T : IBaseFeature
        {
            T feature = Container.Instantiate<T>(new[] { Container });
            _features.Add(feature);
        }
        protected virtual UniTask DoBeforeInitialization()
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask DoAfterInitialization()
        {
            Application.targetFrameRate = 60;

            return UniTask.CompletedTask;
        }

        private async UniTask InitFeatures()
        {
            foreach (var feature in _features)
            {
                await feature.Init();
            }
        }

        private void BindFeatures()
        {
            foreach (var feature in _features)
            {
                feature.InstallBindings();
            }
        }
    }
}
