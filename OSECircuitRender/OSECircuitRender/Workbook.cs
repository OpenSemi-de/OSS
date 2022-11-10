﻿using Newtonsoft.Json;
using System;
using System.IO;
using Microsoft.Maui.Graphics.Skia;
using OSECircuitRender.Sheet;

namespace OSECircuitRender
{
    public sealed class Workbook
    {
        public WorksheetsList Sheets = new();
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public Workbook()
        {
            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
            };
        }

        public static string BasePath { get; set; }
        public static SkiaBitmapExportContext DebugContext { get; set; }

        public Worksheet AddNewSheet()
        {
            Worksheet ws = new();
            Log.L("Adding sheet");
            Sheets.AddSheet(ws);
            return ws;
        }

        public Worksheet LoadSheet(string fileName)
        {
            var json = File.ReadAllText(fileName);
            Worksheet ws = JsonConvert.DeserializeObject<Worksheet>(json, _jsonSerializerSettings);

            Sheets.AddSheet(ws);
            return ws;
        }

        public void SaveSheet(Worksheet ws, string fileName)
        {
            var json = JsonConvert.SerializeObject(ws, _jsonSerializerSettings);
            File.WriteAllText(fileName, json);
        }
    }

    public static class Log
    {
        public static void L(string text)
        {
            Method?.Invoke(text);
        }

        public static Action<string> Method;
    }
}