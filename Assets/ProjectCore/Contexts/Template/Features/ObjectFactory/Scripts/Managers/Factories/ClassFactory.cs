using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace ProjectCore.Template
{
    public sealed class ClassFactory : IClassFactory, IContainerBindingFactory
    {
        private readonly DiContainer _container;

        public ClassFactory(DiContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public T Create<T>(IEnumerable<object> extraArgs = null)
        {
            return _container.Instantiate<T>(GetExtraArgs(extraArgs));
        }

        public object Create(Type type, IEnumerable<object> extraArgs = null)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return _container.Instantiate(type, GetExtraArgs(extraArgs));
        }

        public T Create<T>(Type type, IEnumerable<object> extraArgs = null) where T : class
        {
            object instance = Create(type, extraArgs);

            return instance as T
                ?? throw new InvalidOperationException(
                    $"Created type {type.Name} is not assignable to {typeof(T).Name}.");
        }

        public T CreateMonoBehaviour<T>(T prefab, Transform parent) where T : Component
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            return CreateMonoBehaviour<T>(prefab.gameObject, parent);
        }

        public T CreateMonoBehaviour<T>(GameObject prefab, Transform parent) where T : Component
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            T instance = _container
                .InstantiatePrefab(prefab, parent)
                .GetComponent<T>();

            if (instance == null)
            {
                throw new InvalidOperationException(
                    $"Prefab {prefab.name} does not contain {typeof(T).Name}.");
            }

            return instance;
        }

        public T CreateMonoBehaviourAndBind<T>(T prefab, Transform parent) where T : Component
        {
            T instance = CreateMonoBehaviour(prefab, parent);

            _container.Bind<T>()
                .FromInstance(instance)
                .AsSingle();

            return instance;
        }

        public T CreateAndBind<T>()
        {
            _container.Bind<T>().AsSingle();

            return _container.Resolve<T>();
        }

        public object CreateAndBind(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            _container.Bind(type).AsSingle();

            return _container.Resolve(type);
        }

        public T BindFromInstanceAsSingle<T>(T instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            _container.Bind<T>()
                .FromInstance(instance)
                .AsSingle();

            return instance;
        }

        public T BindComponentFromGameObjectAsSingle<T>(GameObject gameObject) where T : Component
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            T component = gameObject.GetComponent<T>();

            if (component == null)
            {
                throw new InvalidOperationException(
                    $"GameObject {gameObject.name} does not contain {typeof(T).Name}.");
            }

            _container.Bind<T>()
                .FromInstance(component)
                .AsSingle();

            return component;
        }

        public void Inject(GameObject gameObject)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            _container.InjectGameObject(gameObject);
        }

        public void Inject<T>(T instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            _container.Inject(instance);
        }

        public List<T> CreateByBaseClass<T>(Predicate<T> extraPredicate = null) where T : class
        {
            Type baseType = typeof(T);

            return baseType.Assembly
                .GetTypes()
                .Where(type => !type.IsAbstract && baseType.IsAssignableFrom(type))
                .Select(type => Create<T>(type))
                .Where(instance => extraPredicate == null || extraPredicate(instance))
                .ToList();
        }

        private static IEnumerable<object> GetExtraArgs(IEnumerable<object> extraArgs)
        {
            return extraArgs ?? Array.Empty<object>();
        }
    }
}
