using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCore.Template
{
    public interface IClassFactory
    {
        T Create<T>(IEnumerable<object> extraArgs = null);

        object Create(Type type, IEnumerable<object> extraArgs = null);

        T Create<T>(Type type, IEnumerable<object> extraArgs = null) where T : class;

        T CreateMonoBehaviour<T>(T prefab, Transform parent) where T : Component;

        T CreateMonoBehaviour<T>(GameObject prefab, Transform parent) where T : Component;

        void Inject(GameObject gameObject);

        void Inject<T>(T instance);

        List<T> CreateByBaseClass<T>(Predicate<T> extraPredicate = null) where T : class;
    }
}
