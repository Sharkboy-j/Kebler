using System;

namespace Kebler.Core.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class SetIgnoreAttribute : Attribute
    {
    }
}