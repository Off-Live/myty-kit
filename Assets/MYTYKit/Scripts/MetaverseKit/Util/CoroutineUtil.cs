using System;
using System.Collections;

namespace MYTYKit.Scripts.MetaverseKit.Util
{
    public static class CoroutineUtil
    {
        public static IEnumerator RunThrowingIterator(
            IEnumerator enumerator,
            Action<Exception> done)
        {
            while (true)
            {
                object current;
                try
                {
                    if (enumerator.MoveNext() == false)
                    {
                        break;
                    }
                    current = enumerator.Current;
                }
                catch (Exception e)
                {
                    done?.Invoke(e);
                    yield break;
                }
                yield return current;
            }
        }
    }
}