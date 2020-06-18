using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Kebler.UI.CSControls.MuliTreeView
{
    public class ExpandedTreeViewElement
    {
        public string Name { get; }

        public ExpandedTreeViewElement[] Children { get; }

        public ExpandedTreeViewElement(string name, ExpandedTreeViewElement[] children)
        {
            this.Name = name;
            this.Children = children;
        }

        public static class Coder
        {
            public static ExpandedTreeViewElement Decode(JToken json)
            {
                if (json == null || !json.HasValues)
                    return null;
                JToken jtoken = json["Name"];
                string name = jtoken != null ? jtoken.Value<string>() : null;
                ExpandedTreeViewElement[] children = DecodeExpandedTreeViewElementArray(json["Children"] as JArray);
                return Guard.NotNull(name, children) ? null : new ExpandedTreeViewElement(name, children);
            }

            private static ExpandedTreeViewElement[] DecodeExpandedTreeViewElementArray(
              JArray jsonArray)
            {
                if (jsonArray == null)
                    return null;
                List<ExpandedTreeViewElement> expandedTreeViewElementList = new List<ExpandedTreeViewElement>(jsonArray.Count);
                foreach (JToken json in jsonArray)
                {
                    ExpandedTreeViewElement expandedTreeViewElement = Decode(json);
                    if (expandedTreeViewElement != null)
                        expandedTreeViewElementList.Add(expandedTreeViewElement);
                }
                return expandedTreeViewElementList.ToArray();
            }

            public static JObject Encode(ExpandedTreeViewElement element)
            {
                if (element == null)
                    return null;
                return new JObject()
                {
                    {
                      "Name",
                       new JValue(element.Name)
                    },
                    {
                      "Children",
                       EncodeEncodeExpandedTreeViewElementArray(element.Children)
                    }
                };
            }

            private static JArray EncodeEncodeExpandedTreeViewElementArray(ExpandedTreeViewElement[] entries)
            {
                if (entries == null)
                    return null;
                JArray jarray = new JArray();
                foreach (ExpandedTreeViewElement entry in entries)
                {
                    JObject jobject = Encode(entry);
                    if (jobject != null)
                        jarray.Add(jobject);
                }
                return jarray;
            }
        }
    }
}
