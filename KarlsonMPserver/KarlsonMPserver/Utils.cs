using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarlsonMPserver
{
    class Utils
    {
        public static string RemoveRichText(string input)
        {

            input = RemoveRichTextDynamicTag(input, "color");
            input = RemoveRichTextTag(input, "a");

            input = RemoveRichTextTag(input, "b");
            input = RemoveRichTextTag(input, "i");


            // TMP
            input = RemoveRichTextDynamicTag(input, "align");
            input = RemoveRichTextDynamicTag(input, "size");
            input = RemoveRichTextDynamicTag(input, "cspace");
            input = RemoveRichTextDynamicTag(input, "font");
            input = RemoveRichTextDynamicTag(input, "indent");
            input = RemoveRichTextDynamicTag(input, "line-height");
            input = RemoveRichTextDynamicTag(input, "line-indent");
            input = RemoveRichTextDynamicTag(input, "link");
            input = RemoveRichTextDynamicTag(input, "margin");
            input = RemoveRichTextDynamicTag(input, "margin-left");
            input = RemoveRichTextDynamicTag(input, "margin-right");
            input = RemoveRichTextDynamicTag(input, "mark");
            input = RemoveRichTextDynamicTag(input, "mspace");
            input = RemoveRichTextDynamicTag(input, "noparse");
            input = RemoveRichTextDynamicTag(input, "nobr");
            input = RemoveRichTextDynamicTag(input, "page");
            input = RemoveRichTextDynamicTag(input, "pos");
            input = RemoveRichTextDynamicTag(input, "space");
            input = RemoveRichTextDynamicTag(input, "sprite index");
            input = RemoveRichTextDynamicTag(input, "sprite name");
            input = RemoveRichTextDynamicTag(input, "sprite");
            input = RemoveRichTextDynamicTag(input, "style");
            input = RemoveRichTextDynamicTag(input, "voffset");
            input = RemoveRichTextDynamicTag(input, "width");

            input = RemoveRichTextTag(input, "u");
            input = RemoveRichTextTag(input, "s");
            input = RemoveRichTextTag(input, "sup");
            input = RemoveRichTextTag(input, "sub");
            input = RemoveRichTextTag(input, "allcaps");
            input = RemoveRichTextTag(input, "smallcaps");
            input = RemoveRichTextTag(input, "uppercase");
            // TMP end


            return input;

        }

        private static string RemoveRichTextDynamicTag(string input, string tag)
        {
            int index = -1;
            while (true)
            {
                index = input.IndexOf($"<{tag}=");
                if (index != -1)
                {
                    int endIndex = input.Substring(index, input.Length - index).IndexOf('>');
                    if (endIndex > 0)
                        input = input.Remove(index, endIndex + 1);
                    continue;
                }
                input = RemoveRichTextTag(input, tag, false);
                return input;
            }
        }
        private static string RemoveRichTextTag(string input, string tag, bool isStart = true)
        {
            while (true)
            {
                int index = input.IndexOf(isStart ? $"<{tag}>" : $"</{tag}>");
                if (index != -1)
                {
                    input = input.Remove(index, 2 + tag.Length + (!isStart).GetHashCode());
                    continue;
                }
                if (isStart)
                    input = RemoveRichTextTag(input, tag, false);
                return input;
            }
        }
    }
}
