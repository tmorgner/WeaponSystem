using System;
using System.Collections.Generic;

namespace UnityTools.SmartVars
{
  public class WeakEvent 
  {
    List<WeakReference> listeners;

    public WeakEvent()
    {
    }

    public void RemoveAllListeners()
    {
      if (listeners != null)
      {
        listeners.Clear();
      }
    }
     
    public void Invoke()
    {
      if (listeners == null)
      {
        return;
      }

      for (var i = listeners.Count - 1; i >= 0; i--)
      {
        var r = listeners[i].Target as Action;
        if (r != null)
        {
          r.Invoke();
        }
        else
        {
          listeners.RemoveAt(i);
        }
      }
    }

    public void AddListener(Action l)
    {
      if (listeners == null)
      {
        listeners = new List<WeakReference>();
      }

      listeners.Add(new WeakReference(l));
    }

    public void RemoveListener(Action l)
    {
      if (listeners == null)
      {
        return;
      }

      for (var i = listeners.Count - 1; i >= 0; i--)
      {
        var r = listeners[i].Target as Action;
        if (r != null)
        {
          if (ReferenceEquals(r, l))
          {
            listeners.RemoveAt(i);
            return;
          }
        }
        else
        {
          listeners.RemoveAt(i);
        }
      }
    }
  }  

  public class WeakEvent<TEvent> 
  {
    List<WeakReference> listeners;

    public WeakEvent()
    {
    }

    public void RemoveAllListeners()
    {
      if (listeners != null)
      {
        listeners.Clear();
      }
    }

    public void Invoke(TEvent eventArg)
    {
      if (listeners == null)
      {
        return;
      }

      for (var i = listeners.Count - 1; i >= 0; i--)
      {
        var r = listeners[i].Target as Action<TEvent>;
        if (r != null)
        {
          r.Invoke(eventArg);
        }
        else
        {
          listeners.RemoveAt(i);
        }
      }
    }

    public void AddListener(Action<TEvent> l)
    {
      if (listeners == null)
      {
        listeners = new List<WeakReference>();
      }

      listeners.Add(new WeakReference(l));
    }

    public void RemoveListener(Action<TEvent> l)
    {
      if (listeners == null)
      {
        return;
      }

      for (var i = listeners.Count - 1; i >= 0; i--)
      {
        var r = listeners[i].Target as Action<TEvent>;
        if (r != null)
        {
          if (ReferenceEquals(r, l))
          {
            listeners.RemoveAt(i);
            return;
          }
        }
        else
        {
          listeners.RemoveAt(i);
        }
      }
    }
  }
}
