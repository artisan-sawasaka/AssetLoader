using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineList
{
    List<IEnumerator> enumerators = new List<IEnumerator>();
    List<Coroutine> coroutines = new List<Coroutine>();

    public void Add(IEnumerator co)
    {
        enumerators.Add(co);
    }

    public void AddRange(IEnumerable<IEnumerator> cos)
    {
        enumerators.AddRange(cos);
    }

    public IEnumerator WaitForCoroutine(MonoBehaviour mb)
    {
        coroutines.Clear();
        foreach (var e in enumerators)
        {
            coroutines.Add(mb.StartCoroutine(e));
        }
        enumerators.Clear();

        foreach (var co in coroutines)
        {
            yield return co;
        }
    }
}
