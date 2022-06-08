using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace read_feature_model
{
    /// <summary>
    /// 
    /// </summary>
    public class FeatureTreeNode
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public FeatureTreeNode Parent { get; set; }
        private readonly List<FeatureTreeNode> Children = new List<FeatureTreeNode> { };
        public FeatureTreeNode(string name = "", string id = "")
        {
            this.ID = id;
            this.Name = name;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(FeatureTreeNode child)
        {
            Children.Add(child);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int ChildCount()
        {
            return Children.Count;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public FeatureTreeNode GetChildAt(int index)
        {
            return Children[index];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<FeatureTreeNode> GetChildren()
        {
            return Children;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public void RemoveChild(FeatureTreeNode node)
        {
            Children.Remove(node);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class RootNode : FeatureTreeNode
    {
        public RootNode(string name, string id) : base(name, id)
        {

        }
        private int NodeClildCount(FeatureTreeNode node)
        {
            if(node.ChildCount()>0)
            {
                int sum = node.ChildCount();
                for (int i = 0; i < node.ChildCount(); i++)
                {
                    sum += NodeClildCount(node.GetChildAt(i));
                }
                return sum;
            }
            return 0;
        }
        public int AllClildrenCount()
        {
            return NodeClildCount(this);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class FeatureGroup : FeatureTreeNode
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public FeatureGroup() : base("", "")
        {

        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class SolitaireFeature : FeatureTreeNode
    {
        public bool IsOptional { get; set; }
        public SolitaireFeature(string name, string id) : base(name, id)
        {

        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class FeatureModel
    {
        public RootNode Root { get; set; }
        protected List<PropositionalFormula> constraints = new List<PropositionalFormula> { };
        
        public FeatureModel(string fileName="")
        {

        }
        public List<FeatureTreeNode> GetAllChildrenOf(FeatureTreeNode root)
        {
            var returnVlaues = new List<FeatureTreeNode> { };
            var children = root.GetChildren();
            returnVlaues.AddRange(children);
            foreach (var item in children)
            {
                returnVlaues.AddRange(GetAllChildrenOf(item));
            }
            return returnVlaues;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<PropositionalFormula> GetConstraints()
        {
            return constraints;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void SetConstraints(List<PropositionalFormula> value)
        {
            constraints = value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void AddConstraints(PropositionalFormula value)
        {
            constraints.Add(value);
        }
        /// <summary>
        /// 
        /// </summary>
        public void LoadModel()
        {

        }

        public int NumberOfChildren()
        {
            return GetAllChildrenOf(Root).Count();
        }
    }

    public class PropositionalFormula
    {
        public string Name { get; set; }
        public string FirstOperator { get; set; }
        public string SecondOperator { get; set; }
        public FeatureTreeNode FirstItem { get; set; }
        public FeatureTreeNode SecondItem { get; set; }
        /// <summary>
        /// Return a string 
        /// </summary>
        /// <returns></returns>
        override public string ToString()
        {
            return FirstOperator + FirstItem.ID + " Or " + SecondOperator + SecondItem.ID;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class XMLFeatureModel : FeatureModel
    {
        private List<KeyValuePair<FeatureTreeNode, int>> nodeList;
        public Dictionary<string, string> Attributes { get; set; }
        /// <summary>
        /// Load header attributes for more detailes
        /// </summary>
        /// <param name="xmlData">The XML part of file</param>
        private void LoadAttributes(XmlNodeList xmlData)
        {
            Attributes = new Dictionary<string, string> { };
            foreach (XmlNode node in xmlData)
            {
                Attributes.Add(node.Attributes.GetNamedItem("name").Value, node.InnerText);
            }
        }
        /// <summary>
        /// Load all feature tree and add to root.
        /// </summary>
        /// <param name="xmlData">The XML part of file</param>
        private void LoadFeatureTree(XmlNodeList xmlData)
        {
            if (xmlData.Count == 0)
            {
                Debug.WriteLine("Feature tree is empty");
                return;
            }
            List<string> treeBody = xmlData.Item(0).InnerText.ToString().Trim().Split('\n').ToList();
            nodeList = new List<KeyValuePair<FeatureTreeNode, int>> { };
            int lastLevel = 0;
            foreach (string row in treeBody)
            {
                string name = "";
                string id = "";
                int currentLevel = row.Where(c => c == '\t').Count();
                string rowTemp = row.Replace("\t", "");
                FeatureTreeNode node = null;
                // root feature
                if (rowTemp.Contains(":r "))
                {
                    rowTemp = rowTemp.Replace(":r ", "");
                    int parantesIndex = rowTemp.IndexOf("(");
                    int parantesEndIndex = rowTemp.IndexOf(")");
                    name = rowTemp.Substring(0, parantesIndex);
                    id = rowTemp.Substring(parantesIndex + 1, parantesEndIndex - parantesIndex - 1);
                    node = new RootNode(name, id);
                }
                // optional feature
                else if (rowTemp.Contains(":o "))
                {
                    rowTemp = rowTemp.Replace(":o ", "");
                    int parantesIndex = rowTemp.IndexOf("(");
                    int parantesEndIndex = rowTemp.IndexOf(")");
                    name = rowTemp.Substring(0, parantesIndex);
                    id = rowTemp.Substring(parantesIndex + 1, parantesEndIndex - parantesIndex - 1);
                    node = new SolitaireFeature(name, id);
                    ((SolitaireFeature)node).IsOptional = true;
                }
                // mandatory feature
                else if (rowTemp.Contains(":m "))
                {
                    rowTemp = rowTemp.Replace(":m ", "");
                    int parantesIndex = rowTemp.IndexOf("(");
                    int parantesEndIndex = rowTemp.IndexOf(")");
                    name = rowTemp.Substring(0, parantesIndex);
                    id = rowTemp.Substring(parantesIndex + 1, parantesEndIndex - parantesIndex - 1);
                    node = new SolitaireFeature(name, id);
                    ((SolitaireFeature)node).IsOptional = false;
                }
                // inclusive-OR feature group with cardinality [1..*] ([1..3] also allowed)
                else if (rowTemp.Contains(":g "))
                {
                    rowTemp = rowTemp.Replace(":g ", "");
                    int parantesIndex = rowTemp.IndexOf("[");
                    int parantesEndIndex = rowTemp.IndexOf("]");
                    var range = rowTemp.Substring(parantesIndex + 1, parantesEndIndex - parantesIndex - 1).Split(',');
                    node = new FeatureGroup();
                    ((FeatureGroup)node).Min = Convert.ToInt32(range[0] != "*" ? range[0] : "25000");
                    ((FeatureGroup)node).Max = Convert.ToInt32(range[1] != "*" ? range[1] : "25000");
                }
                // grouped feature
                else if (rowTemp.Contains(": "))
                {
                    rowTemp = rowTemp.Replace(": ", "");
                    int parantesIndex = rowTemp.IndexOf("(");
                    int parantesEndIndex = rowTemp.IndexOf(")");
                    name = rowTemp.Substring(0, parantesIndex);
                    id = rowTemp.Substring(parantesIndex + 1, parantesEndIndex - parantesIndex - 1);
                    node = new FeatureTreeNode(name, id);
                }
                nodeList.Add(new KeyValuePair<FeatureTreeNode, int>(node, currentLevel));
                lastLevel = currentLevel;
            }
            // connect nodes
            while (nodeList.Count > 1)
            {
                int maxLevel = nodeList.Select(x => x.Value).Max();
                for (int i = 0; i < nodeList.Count; i++)
                {
                    if (nodeList[i + 1].Value > nodeList[i].Value && nodeList[i + 1].Value == maxLevel)
                    {
                        nodeList[i + 1].Key.Parent = nodeList[i].Key;
                        nodeList[i].Key.AddChild(nodeList[i + 1].Key);
                        nodeList.RemoveAt(i + 1);
                        break;
                    }
                }
            }
            // set single remaining node as Root
            this.Root = (RootNode)nodeList[0].Key;
        }
        /// <summary>
        /// find a node by Id in tree
        /// </summary>
        /// <param name="root"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public FeatureTreeNode GetNodeByID(FeatureTreeNode root, string id)
        {
            if (root.ID == id)
                return root;
            else if (root.ChildCount() > 0)
            {
                foreach (FeatureTreeNode node in root.GetChildren())
                {
                    var tempNode = GetNodeByID(node, id);
                    if (tempNode != null)
                        return tempNode;
                }
            }
            return null;
        }


        public static void ExportXMLFeatureModel(FeatureModel model, string path)
        {
            XmlDocument xdoc = new XmlDocument();

            var feature_model = xdoc.CreateElement("feature_model");
            var metaNode = xdoc.CreateElement("meta");
            var creatorDataNode = xdoc.CreateElement("data");
            var emailDataNode = xdoc.CreateElement("data");
            var organizationDataNode = xdoc.CreateElement("data");
            var dateDataNode = xdoc.CreateElement("data");
            var featureNode = xdoc.CreateElement("feature_tree");
            var constraintsNode = xdoc.CreateElement("constraints");

            xdoc.AppendChild(feature_model);
            metaNode.AppendChild(creatorDataNode);
            metaNode.AppendChild(emailDataNode);
            metaNode.AppendChild(organizationDataNode);
            metaNode.AppendChild(dateDataNode);
            feature_model.AppendChild(metaNode);
            feature_model.AppendChild(featureNode);
            feature_model.AppendChild(constraintsNode);
            //
            feature_model.SetAttribute("name", "GeneratedFeaturModel");

            creatorDataNode.SetAttribute("name", "creator");
            creatorDataNode.InnerText = "Omid Ardestani";

            dateDataNode.SetAttribute("name", "date");
            dateDataNode.InnerText = DateTime.Now.ToString();

            emailDataNode.SetAttribute("name", "email");
            emailDataNode.InnerText = "o.ardestani@hotmail.com";

            organizationDataNode.SetAttribute("name", "organization");
            organizationDataNode.InnerText = "Local";
            //

            string lines = GetNodeXMLString(model.Root, 0);
            featureNode.InnerText = lines;
            xdoc.Save(path);
        }

        private static string GetNodeXMLString(FeatureTreeNode node, int level)
        {
            int childCount = node.ChildCount();
            string str = "";
            if (node is RootNode)
            {
                str = "\n:r " + node.Name + "(" + node.ID + ")\n";
            }
            else
            {
                if (node is SolitaireFeature)
                {
                    if (((SolitaireFeature)node).IsOptional)
                        str = ":o " + node.Name + "(" + node.ID + ")\n";
                    else
                        str = ":m " + node.Name + "(" + node.ID + ")\n";
                }
                else if (node is FeatureGroup)
                {
                    str = ":g (" + node.ID + ") [" + ((FeatureGroup)node).Min + "," + ((FeatureGroup)node).Max + "]\n";
                }
                else
                {
                    str = ": " + node.Name + "(" + node.ID + ")\n";
                }
                for (int pad = 0; pad < level; pad++)
                {
                    str = "\t" + str;
                }
            }
            for (int i = 0; i < childCount; i++)
            {
                str += GetNodeXMLString(node.GetChildAt(i), level + 1);
            }
            return str;
        }
        /// <summary>
        /// find a node by Id in tree
        /// </summary>
        /// <param name="root"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public FeatureTreeNode GetNodeByName(FeatureTreeNode root, string name)
        {
            if (root.Name == name)
                return root;
            else if (root.ChildCount() > 0)
            {
                foreach (FeatureTreeNode node in root.GetChildren())
                {
                    var tempNode = GetNodeByName(node, name);
                    if (tempNode != null)
                        return tempNode;
                }
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlData">The XML part of file</param>
        private void LoadConstraints(XmlNodeList xmlData)
        {
            constraints = new List<PropositionalFormula> { };
            List<string> lines = xmlData.Item(0).InnerText.Trim().Split('\n').ToList();
            foreach (string line in lines)
            {
                if (line != "")
                {
                    string constraintName = line.Split(':')[0];
                    string constraintFormula = line.Split(':')[1];
                    string firstItemId = constraintFormula.Replace(" or ", ",").Split(',')[0];
                    string secondItemId = constraintFormula.Replace(" or ", ",").Split(',')[1];
                    bool firstOper = firstItemId.Contains("~");
                    bool secondOper = secondItemId.Contains("~");
                    firstItemId = firstOper ? firstItemId.Replace("~", "") : firstItemId;
                    secondItemId = secondOper ? secondItemId.Replace("~", "") : secondItemId;
                    firstItemId = firstItemId.Replace(" ", "");
                    secondItemId = secondItemId.Replace(" ", "");
                    PropositionalFormula formula = new PropositionalFormula();
                    formula.Name = constraintName;
                    formula.FirstOperator = firstOper ? "~" : "";
                    formula.SecondOperator = secondOper ? "~" : "";
                    var item = GetNodeByID(this.Root, firstItemId);
                    formula.FirstItem = item;
                    item = GetNodeByID(this.Root, secondItemId);
                    formula.SecondItem = item;
                    constraints.Add(formula);
                }
            }
        }
        public XMLFeatureModel() : base()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadFile(string fileName)
        {
            string fileData = File.ReadAllText(fileName);
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(fileData);
            XmlNodeList xmlAttributeList = xdoc.SelectNodes("//data");
            XmlNodeList xmlFeatureTree = xdoc.SelectNodes("//feature_tree");
            XmlNodeList xmlConstraints = xdoc.SelectNodes("//constraints");
            //
            LoadAttributes(xmlAttributeList);
            LoadFeatureTree(xmlFeatureTree);
            LoadConstraints(xmlConstraints);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="value"></param>
        public XMLFeatureModel(string fileName) : base(fileName)
        {
            LoadFile(fileName);
        }
    }
}
