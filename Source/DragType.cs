using System;
namespace SpartansLib
{
    [Flags]
    public enum DragType
    {
        None = 0b0,
        Move = 0b1,
        ResizeTop = 0b10,
        ResizeRight = 0b100,
        ResizeBottom = 0b1000,
        ResizeLeft = 0b10000
    }
}
