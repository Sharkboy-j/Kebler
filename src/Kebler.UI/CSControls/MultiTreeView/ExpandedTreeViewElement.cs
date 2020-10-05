using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Kebler.UI.CSControls.MuliTreeView
{
    public class ExpandedTreeViewElement
    {
        public ExpandedTreeViewElement(string name, ExpandedTreeViewElement[] children)
        {
            Name = name;
            Children = children;
        }

        public string Name { get; }

        public ExpandedTreeViewElement[] Children { get; }

        public static class Coder
        {
            public static ExpandedTreeViewElement Decode(JToken json)
            {
                if (json == null || !json.HasValues)
                    return null;
                var jtoken = json["Name"];
                var name = jtoken != null ? jtoken.Value<string>() : null;
                var children = DecodeExpandedTreeViewElementArray(json["Children"] as JArray);
                return Guard.NotNull(name, children) ? null : new ExpandedTreeViewElement(name, children);
            }

            private static ExpandedTreeViewElement[] DecodeExpandedTreeViewElementArray(
                JArray jsonArray)
            {
                if (jsonArray == null)
                    return null;
                var expandedTreeViewElementList = new List<ExpandedTreeViewElement>(jsonArray.Count);
                foreach (var json in jsonArray)
                {
                    var expandedTreeViewElement = Decode(json);
                    if (expandedTreeViewElement != null)
                        expandedTreeViewElementList.Add(expandedTreeViewElement);
                }

                return expandedTreeViewElementList.ToArray();
            }

            public static JObject Encode(ExpandedTreeViewElement element)
            {
                if (element == null)
                    return null;
                return new JObject
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
                var jarray = new JArray();
                foreach (var entry in entries)
                {
                    var jobject = Encode(entry);
                    if (jobject != null)
                        jarray.Add(jobject);
                }

                return jarray;
            }
        }
    }
}