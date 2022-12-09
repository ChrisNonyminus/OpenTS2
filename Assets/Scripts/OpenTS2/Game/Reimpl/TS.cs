using System;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTS2.Game.Reimpl
{
    public static class TS
    {
        public static bool IsCASLotName(string lotName)
        {
            return lotName.StartsWith("CAS!") || lotName.StartsWith("YACAS!");
        }
        
        public static void ShowMessage(string message)
        {
            // Use Unity's UI system to show a message to the user
            var canvas = GameObject.FindObjectOfType<Canvas>();
            var messageBox = new GameObject("MessageBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(Text));
            messageBox.transform.SetParent(canvas.transform, false);
            var rectTransform = messageBox.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            var image = messageBox.GetComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = true;
            var button = messageBox.GetComponent<Button>();
            button.onClick.AddListener(() => GameObject.Destroy(messageBox));
            var text = messageBox.GetComponent<Text>();
            text.text = message;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.alignment = TextAnchor.MiddleCenter;
        }

        private static TSGlobals globals;
        private static TSPersistSystem persistSystem;
        private static TSSimulator simulator;

        public static TSGlobals Globals()
        {
            if (globals == null)
            {
                globals = new TSGlobals();
            }
            return globals;
        }

        public static TSPersistSystem PersistSystem()
        {
            if (persistSystem == null)
            {
                persistSystem = new TSPersistSystem();
            }
            return persistSystem;
        }

        public static TSSimulator Simulator()
        {
            if (simulator == null)
            {
                simulator = new TSSimulator();
            }
            return simulator;
        }
    }
}