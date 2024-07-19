using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.Systems.Observers.AbstractObservers
{
    public abstract class AbstractUpdateObserver<P> : AbstractObserver<P> where P : AbstractObserverProps
    {
        protected AbstractUpdateObserver(P props) : base(props)
        {
        }

        public abstract void OnUpdate();
    }
}
