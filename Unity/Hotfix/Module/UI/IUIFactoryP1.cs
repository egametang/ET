using UnityEngine;

namespace ETHotfix
{
    public interface IUIFactoryP1
    {

        UI Create<K>(Scene scene, string type, GameObject parent, K k);
        void Remove(string type);
    }
}