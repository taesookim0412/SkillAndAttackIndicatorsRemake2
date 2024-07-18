using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.Systems.Observers
{
    public class ObserverUpdateProps
    {
        public ObserverUpdateCache ObserverUpdateCache;

        public ObserverUpdateProps(ObserverUpdateCache observerUpdateCache)
        {
            ObserverUpdateCache = observerUpdateCache;
        }
    }
}
