using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.UI.Components
{
    public class CounterComponentFactory : IComponentFactory
    {
        public string ComponentName
        {
            get { return "Counter"; }
        }

        public string Description
        {
            get { return "A labelled counter, allowing an initial value to be incremented/decremented by a specified amount."; }
        }

        public ComponentCategory Category
        {
            get { return ComponentCategory.Other; }
        }

        public IComponent Create(LiveSplitState state)
        {
            return new CounterComponent(state);
        }

        public string UpdateName
        {
            get { return ComponentName; }
        }

        public string XMLURL
        {
            get { return string.Empty; }
        }

        public string UpdateURL
        {
            get { return string.Empty; }
        }

        public Version Version
        {
            get { return Version.Parse("1.0"); }
        }
    }
}
