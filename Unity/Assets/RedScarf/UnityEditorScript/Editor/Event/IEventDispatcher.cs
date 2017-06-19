using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IUESEventDispatcher
{
    string GUID { get; }

    void AddEventListener(string type, Action<UESEvent> callback);
    void RemoveEventListener(string type, Action<UESEvent> callback);

}
