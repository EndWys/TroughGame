using System;
using UnityEngine;

namespace ProjectCore.Template
{
    public interface IContainerBindingFactory
    {
        T CreateAndBind<T>();

        object CreateAndBind(Type type);

        T CreateMonoBehaviourAndBind<T>(T prefab, Transform parent) where T : Component;

        T BindFromInstanceAsSingle<T>(T instance);

        T BindComponentFromGameObjectAsSingle<T>(GameObject gameObject) where T : Component;
    }
}
