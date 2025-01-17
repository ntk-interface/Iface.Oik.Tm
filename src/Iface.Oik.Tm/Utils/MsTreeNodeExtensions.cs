using System;
using System.Collections.Generic;
using System.Linq;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Utils
{
    public static class MsTreeNodeExtensions
    {
        public static IEnumerable<MSTreeNode> FindNodesByProps<T>(this MSTreeNode root) where T: BaseNodeProperties
        {
            var result = new List<MSTreeNode>();

            if (root.Properties is T)
            {
                result.Add(root);
            }

            if (root.Children == null)
            {
                return result;
            }
            
            foreach (var childResult in root.Children.Select(child => child.FindNodesByProps<T>()))
            {
                result.AddRange(childResult);
            }
            
            return result;
        }
    }
}