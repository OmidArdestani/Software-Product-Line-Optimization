﻿using read_feature_model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPLAOptimization
{
    public abstract class PLAItems
    {
        public string Id { get; set; }
        public string Name { get; set; }

        private Dictionary<string, ValueType> propertie = new Dictionary<string, ValueType>();
        public PLAItems()
        {

        }
        public PLAItems(PLAItems cp)
        {
            Id = cp.Id;
            Name = cp.Name;
            propertie = cp.propertie;
        }
        public ValueType Propertie(string key)
        {
            if (propertie.ContainsKey(key))
                return propertie[key];
            else
                return null;
        }

        public void SetPropertie(string key, ValueType value)
        {
            if (propertie.ContainsKey(key))
                propertie[key] = value;
            else
                propertie.Add(key, value);
        }
    }
    public class PLArchitecture
    {
        public PLArchitecture(List<PLAComponent> components)
        {
            this.Components = components;
        }
        public List<PLAComponent> Components { get; set; }
        public int ComponentCount { get; set; }
        public int InterfaceCount { get; set; }
        public int OperatorCount { get; set; }
    }
    public class PLAComponent : PLAItems
    {
        public List<PLAInterface> Interfaces { get; set; }
        public List<PLAInterface> DependedInterfaces { get; set; }
    }
    public class PLAInterface : PLAItems
    {
        public PLAComponent OwnerComponent { get; set; }
        public List<PLAOperation> Operations { get; set; }
    }
    public class PLAOperation : PLAItems
    {
        public PLAOperation()
        {

        }
        public PLAOperation(PLAOperation cp) : base(cp)
        {
            Arguments = cp.Arguments;
        }
        public List<Object> Arguments { get; set; }
        public PLAInterface OwnerInterface { get; set; }
    }

    public class FeatureRelationship
    {
        public PLAOperation RelatedOperation { get; set; }
        public FeatureTreeNode RelatedFeature { get; set; }
    }

}
