using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KarlsonMP
{
    class TableGui
    {
        public static TableView TableView(Rect position, string title, TableStyle style)
        {
            TableView view = new TableView(position, title, style, tables.Count);
            tables.Add(view);
            return view;
        }

        public static void DrawTables()
        {
            tables.ForEach(t => t.Draw());
        }


        internal static List<TableView> tables = new List<TableView>();

        public enum TableStyle
        {
            alterBg        = 0b1,        // alternate background between rows
            lineColumn     = 0b10,       // draw a line at each column
            dynamicColumns = 0b100,      // make user able to resize row width
            window         = 0b1000,     // put the table in a window instead of a standard box
            dynamicHeight  = 0b10000,    // tables have dynamic height, based on number of elements
            dragWindow     = 0b100000,   // allow user to drag the window; only works if window is set
        }
    }

    class TableView
    {
        public TableView(Rect _position, string _title, TableGui.TableStyle _style, int _windowId)
        {
            pos = _position;
            title = _title;
            maxHeight = _position.height;
            if ((_style & TableGui.TableStyle.alterBg) == TableGui.TableStyle.alterBg)
                alterBg = true;
            if ((_style & TableGui.TableStyle.lineColumn) == TableGui.TableStyle.lineColumn)
                lineColumn = true;
            if ((_style & TableGui.TableStyle.dynamicColumns) == TableGui.TableStyle.dynamicColumns)
                dynamicColumns = true;
            if ((_style & TableGui.TableStyle.window) == TableGui.TableStyle.window)
                window = true;
            if ((_style & TableGui.TableStyle.dynamicHeight) == TableGui.TableStyle.dynamicHeight)
                dynamicHeight = true;
            if ((_style & TableGui.TableStyle.dragWindow) == TableGui.TableStyle.dragWindow)
                dragWindow = true;
            windowId = _windowId;
        }

        private Rect pos;
        private int windowId;
        private readonly string title;
        private readonly bool alterBg = false;
        private readonly bool lineColumn = false;
        private readonly bool dynamicColumns = false; // TODO: implement them
        private readonly bool window = false;
        private readonly bool dynamicHeight = false;
        private readonly bool dragWindow = false;
        private readonly float maxHeight = 0f; // if dynamicHeight is used

        private Vector2 scrollPos = Vector2.zero;

        private string header = "";
        private float[] width = Array.Empty<float>();
        public readonly List<string> items = new List<string>();

        public void SetHeader(string _header, float[] width = null)
        {
            this.width = width;
            header = _header;
        }

        public void Draw()
        {
            string[] headers = header.Split('\t');
            if (width == null || width.Length != headers.Length)
            {
                width = new float[headers.Length];
                for (int i = 0; i < headers.Length; i++)
                    width[i] = GUI.skin.label.CalcSize(new GUIContent(headers[i])).x;
            }
            pos.height = maxHeight + 3f;
            if (dynamicHeight)
                pos.height = Mathf.Min(35f + 40f * items.Count + 3f, pos.height);

            if (window)
            {
                pos = GUI.Window(windowId, new Rect(pos.x, pos.y, pos.width, pos.height), DrawWindow, title);
                return;
            }
        }

        private void DrawWindow(int wID)
        {
            if (dragWindow)
                GUI.DragWindow();
            GUI.BeginGroup(new Rect(3f, 15f, pos.width - 6f, pos.height - 15f));
            string[] headerelemes = header.Split('\t');
            for (int i = 0; i < headerelemes.Length; i++)
                GUI.Label(new Rect(PrevWidth(i), 0f, width[i], 20f), headerelemes[i]);
            //GUI.Box(new Rect(0f, 20f, pos.width - 6f, pos.height - 35f), "");
            scrollPos = GUI.BeginScrollView(new Rect(0f, 20f, pos.width - 6f, pos.height - 35f - 3f), scrollPos, new Rect(0f, 0f, pos.width - 6f - 20f, 40f * items.Count));
            GUIStyle bg1 = new GUIStyle();
            bg1.normal.background = GetTextureFromColor(17, 17, 17);
            GUIStyle bg2 = new GUIStyle();
            bg2.normal.background = GetTextureFromColor(60, 60, 60);
            foreach (string element in items)
            {
                GUI.Box(new Rect(0f, 40f * items.IndexOf(element), pos.width - 6f, 40f), "", bg1); // background
                string[] elempart = element.Split('\t');
                for (int i = 0; i < Math.Min(headerelemes.Length, elempart.Length); i++)
                    GUI.Label(new Rect(PrevWidth(i), 40f * items.IndexOf(element), width[i], 40f), "<size=30>" + elempart[i] + "</size>");
                if (alterBg)
                {
                    GUIStyle temp = bg1;
                    bg1 = bg2;
                    bg2 = temp;
                }
            }
            // draw lines
            if (lineColumn)
            {
                GUIStyle line = new GUIStyle();
                line.normal.background = GetTextureFromColor(100, 100, 100);
                for (int i = 1; i < headerelemes.Length; i++)
                    GUI.Box(new Rect(PrevWidth(i) - 5f, 0f, 2f, 40f * items.Count), "", line);
            }
            GUI.EndScrollView(true);
            GUI.EndGroup();
        }

        internal float PrevWidth(int i)
        {
            float ret = 0f;
            for (int j = 0; j < i; j++)
                ret += width[j];
            return ret;
        }
        internal float PrevWidth(float[] width, int i)
        {
            float ret = 0f;
            for (int j = 0; j < i; j++)
                ret += width[j];
            return ret;
        }
        internal static Texture2D GetTextureFromColor(int r, int g, int b)
        {
            return GetTextureFromColor(new Color(r / 255f, g / 255f, b / 255f));
        }
        internal static Texture2D GetTextureFromColor(Color color)
        {
            Texture2D d = new Texture2D(1, 1);
            var arr = d.GetPixels();
            for (var i = 0; i < arr.Length; i++)
                arr[i] = color;
            d.SetPixels(arr);
            d.Apply();
            return d;
        }
    }
}
