using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DictionaryIntInt: SerializableDictionary<int,int>
{
    
}

[System.Serializable]
public class StateDict: SerializableDictionary<Vector3Int, int>
{
    
}

[System.Serializable]
public class StateActionDict: SerializableDictionary<StateDict, PlayerManager.Action>
{
    
}

[System.Serializable]
public class QValueDict : SerializableDictionary<StateActionDict, float>
{
    
}