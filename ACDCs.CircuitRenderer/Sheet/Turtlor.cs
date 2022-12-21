﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using ACDCs.CircuitRenderer.Definitions;
using ACDCs.CircuitRenderer.Drawables;
using ACDCs.CircuitRenderer.Interfaces;
using ACDCs.CircuitRenderer.Items;
using Microsoft.Maui.Graphics;

namespace ACDCs.CircuitRenderer.Sheet;

public class Turtlor
{
    private readonly List<RectFr> _collisionRectangles = new();

    private readonly WorksheetItemList _items;

    private readonly WorksheetItemList _nets;

    private readonly Coordinate _sheetSize;

    private readonly Worksheet _worksheet;

    public Turtlor(WorksheetItemList? items, WorksheetItemList? nets, Coordinate? sheetSize, Worksheet worksheet)
    {
        _worksheet = worksheet;
        _items = items ?? new WorksheetItemList(_worksheet);
        _nets = nets ?? new WorksheetItemList(_worksheet);
        _sheetSize = sheetSize ?? new Coordinate();
    }

    public static Coordinate GetAbsolutePinPosition(PinDrawable pin)
    {
        Coordinate center = pin.ParentItem.DrawableComponent.Size.Multiply(0.5f)
            .Add(pin.ParentItem.DrawableComponent.Position);
        
        Coordinate rotatedPinPos = new(pin.ParentItem.DrawableComponent.Position.X, pin.ParentItem.DrawableComponent.Position.Y);
        rotatedPinPos = rotatedPinPos.Add(pin.Position.Multiply(pin.ParentItem.DrawableComponent.Size));
        rotatedPinPos = rotatedPinPos.RotateCoordinate(center.X, center.Y, pin.ParentItem.Rotation);

        return rotatedPinPos;
    }

    public static bool LineIntersectsLine(Point line1Point1, Point line1Point2, Point line2Point1, Point line2Point2)
    {
        float q = Convert.ToSingle((line1Point1.Y - line2Point1.Y) * (line2Point2.X - line2Point1.X) -
                                   (line1Point1.X - line2Point1.X) * (line2Point2.Y - line2Point1.Y));
        float d = Convert.ToSingle((line1Point2.X - line1Point1.X) * (line2Point2.Y - line2Point1.Y) -
                                   (line1Point2.Y - line1Point1.Y) * (line2Point2.X - line2Point1.X));

        if (d == 0)
        {
            return false;
        }

        float r = q / d;

        q = Convert.ToSingle((line1Point1.Y - line2Point1.Y) * (line1Point2.X - line1Point1.X) -
                             (line1Point1.X - line2Point1.X) * (line1Point2.Y - line1Point1.Y));
        float s = q / d;

        if (r < 0 || r > 1 || s < 0 || s > 1)
        {
            return false;
        }

        return true;
    }

    public static Direction LineIntersectsRect(Point p1, Point p2, RectFr r)
    {
        if (LineIntersectsLine(
                p1,
                p2,
                new Point(r.X1, r.Y1),
                new Point(r.X2, r.Y2)))
            return Direction.Top;

        if (LineIntersectsLine(
                p1,
                p2,
                new Point(r.X2, r.Y2),
                new Point(r.X3, r.Y3)))
            return Direction.Right;

        if (LineIntersectsLine(
                p1,
                p2,
                new Point(r.X3, r.Y3),
                new Point(r.X4, r.Y4)))
            return Direction.Bottom;

        if (LineIntersectsLine(
                p1,
                p2,
                new Point(r.X4, r.Y4),
                new Point(r.X1, r.Y1)))
            return Direction.Left;

        int hitContains = 0;
        if (PointInTriangle(p1, new Point(r.X1, r.Y1), new Point(r.X2, r.Y2), new Point(r.X4, r.Y4)))
            hitContains++;
        if (PointInTriangle(p2, new Point(r.X1, r.Y1), new Point(r.X2, r.Y2), new Point(r.X4, r.Y4)))
            hitContains++;
        if (PointInTriangle(p1, new Point(r.X1, r.Y1), new Point(r.X2, r.Y2), new Point(r.X3, r.Y3)))
            hitContains++;
        if (PointInTriangle(p2, new Point(r.X1, r.Y1), new Point(r.X2, r.Y2), new Point(r.X3, r.Y3)))
            hitContains++;

        if (hitContains > 1)
        {
            return Direction.Contains;
        }

        return Direction.None;
    }

    public static bool PointInTriangle(Point pt, Point v1, Point v2, Point v3)
    {
        float d1 = Sign(pt, v1, v2);
        float d2 = Sign(pt, v2, v3);
        float d3 = Sign(pt, v3, v1);

        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNeg && hasPos);
    }

    public static void Rotate(IDrawableComponent pindrawable, ref float positionX, ref float positionY, ref float pinX,
        ref float pinY)
    {
        if (pindrawable.Rotation != 0)
        {
            float centerX = pindrawable.Position.X + pindrawable.Size.X / 2;
            float centerY = pindrawable.Position.Y + pindrawable.Size.Y / 2;
            Coordinate rotatedPinPos = new(positionX, positionY);
            rotatedPinPos = rotatedPinPos.RotateCoordinate(centerX, centerY, pindrawable.Rotation);
            positionX = rotatedPinPos.X;
            positionY = rotatedPinPos.Y;
            Coordinate rotatedPinRelPos = new(pinX, pinY);
            rotatedPinRelPos.RotateCoordinate(0.5f, 0.5f, pindrawable.Rotation);
            pinX = rotatedPinRelPos.X;
            pinY = rotatedPinRelPos.Y;
        }
    }


    public static float Sign(Point p1, Point p2, Point p3)
    {
        return Convert.ToSingle((p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y));
    }

    public Dictionary<RectFr, IWorksheetItem> GetCollisionRects()
    {
        Dictionary<RectFr, IWorksheetItem> collList = new();
        foreach (IWorksheetItem item in _items)
        {
            RectFr rect = new()
            {
                X1 = item.X,
                Y1 = item.Y,
                X2 = item.X + item.Width,
                Y2 = item.Y,
                X3 = item.X + item.Width,
                Y3 = item.Y + item.Height,
                X4 = item.X,
                Y4 = item.Y + item.Height
            };

            if (item.Rotation != 0)
            {
                float rotation = item.Rotation;
                float centerX = rect.X1 + (rect.X3 - rect.X1) / 2;
                float centerY = rect.Y1 + (rect.Y3 - rect.Y1) / 2;

                Coordinate rotatedItemPos1 = new(rect.X1, rect.Y1);
                rotatedItemPos1 = rotatedItemPos1.RotateCoordinate(centerX, centerY, rotation);
                Coordinate rotatedItemPos2 = new(rect.X2, rect.Y2);
                rotatedItemPos2 = rotatedItemPos2.RotateCoordinate(centerX, centerY, rotation);
                Coordinate rotatedItemPos3 = new(rect.X3, rect.Y3);
                rotatedItemPos3 = rotatedItemPos3.RotateCoordinate(centerX, centerY, rotation);
                Coordinate rotatedItemPos4 = new(rect.X4, rect.Y4);
                rotatedItemPos4 = rotatedItemPos4.RotateCoordinate(centerX, centerY, rotation);

                List<Coordinate> itemPositions = new()
                {
                    rotatedItemPos1, rotatedItemPos2, rotatedItemPos3, rotatedItemPos4
                };

                rect.X1 = itemPositions[0].X;
                rect.Y1 = itemPositions[0].Y;
                rect.X2 = itemPositions[1].X;
                rect.Y2 = itemPositions[1].Y;
                rect.X3 = itemPositions[2].X;
                rect.Y3 = itemPositions[2].Y;
                rect.X4 = itemPositions[3].X;
                rect.Y4 = itemPositions[3].Y;
            }

            collList.Add(rect, item);
        }

        return collList;
    }

    public DirectionNine GetPinStartDirection(PinDrawable pin, PinDrawable targetPin)
    {
        int pinX = Convert.ToInt32(Math.Round(pin.Position.X * 2));
        int pinY = Convert.ToInt32(Math.Round(pin.Position.Y * 2));

        DirectionNine[,] position = new DirectionNine[3, 3]
        {
            {
                DirectionNine.UpLeft,
                DirectionNine.Up,
                DirectionNine.UpRight
            },
            {
                DirectionNine.Left,
                DirectionNine.Middle,
                DirectionNine.Right
            },
            {
                DirectionNine.DownLeft,
                DirectionNine.Down,
                DirectionNine.DownRight
            }
        };

        DirectionNine direction = position[pinY, pinX];

        if (direction == DirectionNine.Middle)
        {
            direction = DirectionNine.Up;
        }
        else
        {
            if ((int)direction % 2 == 1)
            {
                direction = pin.Position.Y > targetPin.Position.Y
                    ? (DirectionNine)((int)direction + 1)
                    : (DirectionNine)((int)direction - 1);
            }
        }

        return direction;
    }

    public Coordinate GetStepCoordinate(Coordinate position, DirectionNine direction)
    {
        Dictionary<DirectionNine, Coordinate> directionCoordinates = new()
        {
            { DirectionNine.Up , new Coordinate(0,-1,0)},
            { DirectionNine.Down , new Coordinate(0,1,0)},
            { DirectionNine.Right , new Coordinate(1,0,0)},
            { DirectionNine.Left , new Coordinate(-1,0,0)}
        };

        if (directionCoordinates.ContainsKey(direction))
        {
            return directionCoordinates[direction].Add(position);
        }

        Debug.Write(direction);
        return new Coordinate(-100, -100, 0);
    }

    public WorksheetItemList GetTraces()
    {
        Dictionary<RectFr, IWorksheetItem> collisionRects = GetCollisionRects();
        WorksheetItemList traces = new(_worksheet);

        foreach (IWorksheetItem net in _nets)
        {
            TraceItem trace = new();
            var pins = net.Pins
                .OrderBy(pin => pin.ParentItem.X)
                .ThenBy(pin => pin.ParentItem.Y)
                .ToList();

            PinDrawable? lastPin = null;

            foreach (PinDrawable pin in pins)
            {
                if (lastPin != null)
                {
                    trace = GetTrace(trace, lastPin, pin);
                }

                lastPin = pin;
            }

            traces.AddItem(trace);
        }

        return traces;
    }

    private TraceItem GetTrace(TraceItem trace, PinDrawable fromPin, PinDrawable toPin)
    {
        DirectionNine startDirectionPinFrom = GetPinStartDirection(fromPin, toPin);
        DirectionNine startDirectionPinTo = GetPinStartDirection(toPin, fromPin);

        Coordinate pinAbsoluteCoordinateFrom = GetAbsolutePinPosition(fromPin);
        Coordinate pinAbsoluteCoordinateTo = GetAbsolutePinPosition(toPin);

        Coordinate firstStepCoordinateFrom = GetStepCoordinate(pinAbsoluteCoordinateFrom, startDirectionPinFrom);
        Coordinate firstStepCoordinateTo = GetStepCoordinate(pinAbsoluteCoordinateTo, startDirectionPinTo);

        trace.AddPart(pinAbsoluteCoordinateFrom, firstStepCoordinateFrom);

        Coordinate currentPositionCoordinate = firstStepCoordinateFrom;

        Turtle turtle = new(currentPositionCoordinate, firstStepCoordinateTo, pinAbsoluteCoordinateFrom);
        turtle.Run();

        Coordinate loopPos = firstStepCoordinateFrom;
        foreach (Coordinate pathCoordinate in turtle.PathCoordinates)
        {
            trace.AddPart(loopPos, pathCoordinate);
            loopPos = pathCoordinate;
        }
        trace.AddPart(pinAbsoluteCoordinateTo, firstStepCoordinateTo);

        return trace;
    }

    private int R(float number)
    {
        return Convert.ToInt32(Math.Round(number));
    }
}
