using System;
namespace Merthsoft.DesignatorShapes;
// The standalone native Fill route owns its worker regardless of Harmony prefix order.
public static class InputOwnership
{
    public static bool IsStandaloneFill(Type workerType) => workerType?.FullName == "Merthsoft.DesignatorShapes.FloodFill.Fill";
    public static bool CustomOwnsRoute(bool showControls, Type workerType) => showControls && !IsStandaloneFill(workerType);
}
