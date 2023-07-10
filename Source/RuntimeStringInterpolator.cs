using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Merthsoft.DesignatorShapes;

public static class RuntimeStringInterpolator
{
    public const char INTERPOLATION_START = '{';
    public const char INTERPOLATION_END = '}';
    public const char FORMAT_SEPARATOR = ':';

    private enum State
    {
        BuildingString,
        BuildingInterpolation,
        BuildingFormat
    }


    [Serializable]
    public class InterpolationException : Exception
    {
        public InterpolationException()
        {
        }
        public InterpolationException(string message) : base(message)
        {
        }
        public InterpolationException(string message, Exception inner) : base(message, inner)
        {
        }
        protected InterpolationException(
          SerializationInfo info,
          StreamingContext context) : base(info, context)
        {
        }
    }

    public static string Interpolate(this string s, Dictionary<string, object> args)
    {
        var builder = new StringBuilder(s.Length + args.Count() * 8);

        var formatBuffer = new StringBuilder();
        var interpolationBuffer = new StringBuilder();

        var state = State.BuildingString;
        foreach (var c in s)
            switch (state)
            {
                case State.BuildingString:
                    switch (c)
                    {
                        case INTERPOLATION_START:
                            state = State.BuildingInterpolation;
                            break;
                        default:
                            builder.Append(c);
                            break;
                    }
                    break;
                case State.BuildingInterpolation:
                    switch (c)
                    {
                        case FORMAT_SEPARATOR:
                            state = State.BuildingFormat;
                            break;
                        case INTERPOLATION_START:
                            builder.Append(c);
                            state = State.BuildingString;
                            break;
                        case INTERPOLATION_END:
                            var obj = args[interpolationBuffer.ToString()];
                            builder.Append(obj);
                            interpolationBuffer.Length = 0;
                            state = State.BuildingString;
                            break;
                        default:
                            interpolationBuffer.Append(c);
                            break;
                    }
                    break;
                case State.BuildingFormat:
                    switch (c)
                    {
                        case INTERPOLATION_END:
                            var key = interpolationBuffer.ToString();
                            var obj = args[key];
                            var formattable = obj as IFormattable;
                            var format = formatBuffer.ToString();
                            Console.WriteLine($"{key}:{format}");
                            builder.Append(formattable?.ToString(format, null) ?? obj);
                            interpolationBuffer.Length = 0;
                            formatBuffer.Length = 0;
                            state = State.BuildingString;
                            break;
                        default:
                            formatBuffer.Append(c);
                            break;
                    }
                    break;
            }

        return builder.ToString();
    }
}
