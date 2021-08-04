using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPLAOptimization
{
    public interface XMIItems
    {
        int Id { get; set; }
        string Name { get; set; }
    }
    public class XMIComponent : XMIItems
    {
        public List<XMIInterface> Interfaces { get; set; }
        public List<XMIInterface> DependedInterfaces { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class XMIInterface : XMIItems
    {
        public XMIComponent OwnerComponent { get; set; }
        public List<XMIOperator> Operators { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class XMIOperator : XMIItems
    {
        public List<Object> Arguments { get; set; }
        public XMIInterface OwnerInterface { get; set; }
        public int Id { get; set ; }
        public string Name { get ; set ; }
    }

}
