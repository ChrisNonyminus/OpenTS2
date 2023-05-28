﻿using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.UI
{
    public class UILayout : AbstractAsset
    {
        public List<UIElement> Elements = new List<UIElement>();
        public List<UIElement> AllElements
        {
            get
            {
                var elementList = new List<UIElement>();
                foreach(var element in Elements)
                {
                    elementList.AddRange(GetElements(element));
                }
                return elementList;
                List<UIElement> GetElements(UIElement element)
                {
                    var elementList = new List<UIElement>();
                    foreach (var child in element.Children)
                    {
                        elementList.Add(child);
                        elementList.AddRange(GetElements(child));
                    }
                    return elementList;
                }
            }
        }

        public GameObject[] Instantiate(Transform parent)
        {
            var elementArray = new GameObject[Elements.Count];
            for (var i = 0; i < Elements.Count; i++)
            {
                var gameObject = Elements[i].Instantiate(parent);
                elementArray[i] = gameObject;
            }
            return elementArray;
        }
    }
}
