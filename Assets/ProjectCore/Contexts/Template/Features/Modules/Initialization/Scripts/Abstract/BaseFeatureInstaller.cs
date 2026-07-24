using System.Collections.Generic;
using Zenject;

namespace ProjectCore.Template
{
    public abstract class BaseFeatureInstaller : MonoInstaller
    {
        private readonly List<IBaseFeature> _features = new List<IBaseFeature>();

        public override void InstallBindings()
        {
            AddFeatures();
            BindFeatures();

            var featureInitializationFlow = new FeatureInitializationFlow(_features, Container);

            Container.Bind<IFeatureInitializationFlow>().FromInstance(featureInitializationFlow)
                .WhenInjectedInto<BaseContextInitializer>();
        }

        protected abstract void AddFeatures();

        protected void AddFeature<T>() where T : IBaseFeature, new()
        {
            _features.Add(new T());
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
