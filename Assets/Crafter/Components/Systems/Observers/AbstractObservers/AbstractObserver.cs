using Assets.Crafter.Components.SkillAndAttackIndicatorsRemake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.Systems.Observers.AbstractObservers
{
    public class AbstractObserver<P> where P : AbstractObserverProps
    {
        protected readonly P Props;
        public ObserverStatus ObserverStatus = ObserverStatus.Active;
        protected AbstractObserver(P props)
        {
            Props = props;
        }
        protected void CompleteObserver()
        {
            ObserverStatus = ObserverStatus.Remove;
        }
    }
}
