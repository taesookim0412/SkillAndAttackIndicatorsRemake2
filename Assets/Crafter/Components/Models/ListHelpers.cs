using Assets.Crafter.Components.SkillAndAttackIndicatorsRemake;
using Assets.Crafter.Components.Systems.Observers;
using Assets.Crafter.Components.Systems.Observers.AbstractObservers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.Models
{
    public static class ListHelpers
    {
        // This is a lazy way to iterate observers, only created so the entire ObserversList is not copied -- do not use this in production
        public static void UpdateObservers<T, P>(this List<T> observers) where T: AbstractUpdateObserver<P> where P: AbstractObserverProps
        {
            if (observers.Count > 0)
            {
                List<int> removeIndices = new List<int>();
                for (int i = 0; i < observers.Count; i++)
                {
                    observers[i].OnUpdate();
                    if (observers[i].ObserverStatus == ObserverStatus.Remove)
                    {
                        removeIndices.Add(i);
                    }
                }
                // lazy way of removing for remake prototype -- do not use the in-built RemoveAt here in a serious project.
                if (removeIndices.Count > 0)
                {
                    for (int i = removeIndices.Count - 1; i >= 0; i--)
                    {
                        observers.RemoveAt(removeIndices[i]);
                    }
                }
            }
        }

    }
}
