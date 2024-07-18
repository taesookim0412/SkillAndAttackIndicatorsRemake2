﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.Systems.Observers.AbstractObservers
{
    public abstract class AbstractObserverProps
    {
        public ObserverUpdateProps ObserverUpdateProps { get; set; }

        protected AbstractObserverProps(ObserverUpdateProps observerUpdateProps)
        {
            ObserverUpdateProps = observerUpdateProps;
        }
    }
}
