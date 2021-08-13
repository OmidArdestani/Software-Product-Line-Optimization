using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPLAOptimization
{
    public interface PLAItems
    {
        string Id { get; set; }
        string Name { get; set; }
    }
    public class PLArchitecture
    {
        public PLArchitecture(List<PLAComponent> components)
        {
            this.Components = components;
        }
        public List<PLAComponent> Components{ get; set; }
    }
    public class PLAComponent : PLAItems
    {
        public List<PLAInterface> Interfaces { get; set; }
        public List<PLAInterface> DependedInterfaces { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class PLAInterface : PLAItems
    {
        public PLAComponent OwnerComponent { get; set; }
        public List<PLAOperator> Operators { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class PLAOperator : PLAItems
    {
        public List<Object> Arguments { get; set; }
        public PLAInterface OwnerInterface { get; set; }
        public string Id { get; set ; }
        public string Name { get ; set ; }
    }

}
