using System;
using System.Collections.Generic;

namespace ConnectSdk.Windows.Discovery.Provider.ssdp
{
    public class Action
    {
        private String name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public List<Argument> ArgumentList { get; set; }

        public Action(String name)
        {
            Name = name;
        }

    }
}