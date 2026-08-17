using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace ProjectCore.Template
{
    public abstract class BaseFeatureInstaller : MonoInstaller
    {
        private readonly List<IBaseFeature> _features = new List<IBaseFeature>();

        public override void InstallBindings()
        {
            Container.Bind(typeof(IDebugLogger<>))
                .To(typeof(FeatureDebugLoggerService<>))
                .AsCached();

            AddFeatures();
            BindFeatures();

            var featureInitializationFlow = new FeatureInitializationFlow(_features);

            Container.Bind<IFeatureInitializationFlow>().FromInstance(featureInitializationFlow)
                .WhenInjectedInto<BaseContextInitializer>();
        }

        protected abstract void AddFeatures();

        protected void AddFeature<T>() where T : IBaseFeature, new()
        {
            _features.Add(new T());
        }

        protected void AddFeatureFromComponent<T>() where T : Component, IBaseFeature
        {
            T[] features = GetComponents<T>();
            if (features.Length != 1)
            {
                throw new InvalidOperationException(
                    $"{GetType().Name} requires exactly one {typeof(T).Name} " +
                    "on the same GameObject.");
            }

            _features.Add(features[0]);
        }

        private void BindFeatures()
        {
            foreach (var feature in _features)
            {
                feature.InstallBindings(Container);
            }
        }
    }
}
