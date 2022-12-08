﻿using ACDCs.CircuitRenderer.Drawables;
using Newtonsoft.Json.Linq;

namespace ACDCs.CircuitRenderer.Items
{
    public class SourceItem: WorksheetItem
    {
        public SourceItem(SourceDrawableType sourceDrawableType)
        {
            DrawableComponent = new SourceDrawable(this, DefaultValue, SourceDrawableType.Voltage, 1, 1);
            Value = DefaultValue;
        }

        public SourceItem(string value, SourceDrawableType type, float x, float y)
        {
            DrawableComponent = new SourceDrawable(this, value, type, x, y);
            Value = value;
        }

    }
}
