using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HotChocolate.Internal;
using Microsoft.FSharp.Collections;

#nullable enable

namespace HotChocolate.Utilities;

internal sealed class FSharpListTypeConverter : IChangeTypeProvider
{
    private static readonly MethodInfo _listConvert =
        typeof(FSharpListTypeConverter).GetMethod(
            "FSharpListConverter",
            BindingFlags.Static | BindingFlags.NonPublic)!;

    public bool TryCreateConverter(
        Type source,
        Type target,
        ChangeTypeProvider root,
        [NotNullWhen(true)] out ChangeType? converter)
    {
        var sourceElement = ExtendedType.Tools.GetElementType(source);
        var targetElement = ExtendedType.Tools.GetElementType(target);

        if (sourceElement is not null
            && targetElement is not null
            && root(sourceElement, targetElement, out var elementConverter))
        {
            if (target.IsGenericType
                && target.GetGenericTypeDefinition() == typeof(FSharpList<>))
            {
                var converterMethod =
                    _listConvert.MakeGenericMethod(targetElement);
                converter = source => converterMethod.Invoke(
                    null, new[] { source, elementConverter });
                return true;
            }
        }

        converter = null;
        return false;
    }

    private static object? FSharpListConverter<ElementType>(
        ICollection? input,
        ChangeType elementConverter)
    {
        if (input is null)
        {
            return null;
        }

        var list = (IList)Activator.CreateInstance(typeof(List<ElementType>))!;
        foreach (var item in input)
        {
            list.Add(elementConverter(item));
        }

        var fSharpList = ListModule.OfSeq((IEnumerable<ElementType>)list);
        return fSharpList;
    }
}
