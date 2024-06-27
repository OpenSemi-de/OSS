﻿using ACDCs.Interfaces.Drawing;
using Point = ACDCs.Interfaces.Point;

namespace ACDCs.Renderer.Drawings.Composite;

/// <summary>
/// Drawing for a capacitor element.
/// </summary>
/// <seealso cref="ACDCs.Interfaces.Drawing.IDrawing" />
/// <seealso cref="ACDCs.Interfaces.Drawing.ICompositeDrawing" />
/// <seealso cref="ACDCs.Interfaces.Drawing.IDrawingWithSize" />
public class CapacitorDrawing : IDrawing, ICompositeDrawing, IDrawingWithSize
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CapacitorDrawing"/> class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="value">The value.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="rotation">The rotation.</param>
    /// <param name="isPolar">if set to <c>true</c> [is polar].</param>
    public CapacitorDrawing(string id, float value, float x, float y, float rotation, bool isPolar = true)
    {
        Id = id;
        Rotation = rotation;
        IsPolar = isPolar;
        Value = value;
        X = x;
        Y = y;
    }

    /// <summary>
    /// Gets or sets the color of the background.
    /// </summary>
    /// <value>
    /// The color of the background.
    /// </value>
    public Color? BackgroundColor { get; set; } = Colors.CadetBlue;

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    /// <value>
    /// The height.
    /// </value>
    public float Height { get; set; } = 1;

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    /// <value>
    /// The identifier.
    /// </value>
    public string Id { get; set; }

    /// <summary>
    /// Gets a value indicating whether this instance is polar.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is polar; otherwise, <c>false</c>.
    /// </value>
    public bool IsPolar { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is relative scale.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is relative scale; otherwise, <c>false</c>.
    /// </value>
    public bool IsRelativeScale { get; set; }

    /// <summary>
    /// Gets or sets the color of the line.
    /// </summary>
    /// <value>
    /// The color of the line.
    /// </value>
    public Color? LineColor { get; set; }

    /// <summary>
    /// Gets or sets the offset.
    /// </summary>
    /// <value>
    /// The offset.
    /// </value>
    public Point Offset { get; set; } = new Point(0, -0.5);

    /// <summary>
    /// Gets or sets the parent drawing.
    /// </summary>
    /// <value>
    /// The parent drawing.
    /// </value>
    public IDrawing? ParentDrawing { get; set; }

    /// <summary>
    /// Gets or sets the rotation.
    /// </summary>
    /// <value>
    /// The rotation.
    /// </value>
    public float Rotation { get; set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>
    /// The value.
    /// </value>
    public float Value { get; set; }

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    /// <value>
    /// The width.
    /// </value>
    public float Width { get; set; } = 2;

    /// <summary>
    /// Gets or sets the x.
    /// </summary>
    /// <value>
    /// The x.
    /// </value>
    public float X { get; set; }

    /// <summary>
    /// Gets or sets the y.
    /// </summary>
    /// <value>
    /// The y.
    /// </value>
    public float Y { get; set; }

    /// <summary>
    /// Gets the drawings.
    /// </summary>
    /// <returns></returns>
    public List<IDrawing> GetDrawings()
    {
        List<IDrawing> drawings =
        [
            new PointDrawing(Id + "_Pin1", 0f, 0.45f, 0.1f, 0.55f, true),
            new LineDrawing(Id + "_Line1", 0.1f, 0.5f, 0.4f, 0.5f, true),
            new LineDrawing(Id + "_Line2", 0.4f, 0.2f, 0.4f, 0.8f, true),
            new ArcDrawing(Id + "_Arc1", 0.5f, 0.2f, 0.2f, 0.6f, 120, 240, true),
            new LineDrawing(Id + "_Line3", 0.7f, 0.5f, 0.9f, 0.5f, true),
            new PointDrawing(Id + "_Pin2", 0.9f, 0.45f, 1f, 0.55f, true),
            new TextDrawing(Id + "_Text", Value.ToString(), 0f, 0.8f, 1f, 0.4f, 0, true)
        ];

        foreach (IDrawing drawing in drawings)
        {
            drawing.ParentDrawing = this;
        }

        return drawings;
    }
}