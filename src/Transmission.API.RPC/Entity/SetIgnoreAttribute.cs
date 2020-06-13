using System;

namespace Transmission.API.RPC.Entity
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class SetIgnoreAttribute : Attribute
    {
    }
}