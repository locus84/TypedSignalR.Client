using System.Collections.Generic;
using System.Text;
using TypedSignalR.Client.CodeAnalysis;

namespace TypedSignalR.Client.Templates;

public sealed class HubConnectionExtensionsBinderTemplate
{
    private readonly IReadOnlyList<TypeMetadata> _receiverTypes;
    private readonly SpecialSymbols _specialSymbols;

    public HubConnectionExtensionsBinderTemplate(IReadOnlyList<TypeMetadata> receiverTypes, SpecialSymbols specialSymbols)
    {
        _receiverTypes = receiverTypes;
        _specialSymbols = specialSymbols;
    }

    public string TransformText()
    {
        var sb = new StringBuilder();

        sb.AppendLine("""
// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY TypedSignalR.Client
// </auto-generated>
#nullable enable
#pragma warning disable CS1591
#pragma warning disable CS8767
#pragma warning disable CS8613
namespace TypedSignalR.Client
{
    internal static partial class HubConnectionExtensions
    {
""");
        foreach (var receiverType in _receiverTypes)
        {
            sb.AppendLine($$"""
        private sealed class BinderFor_{{receiverType.CollisionFreeName}} : IReceiverBinder<{{receiverType.FullyQualifiedInterfaceName}}>
        {
            public global::System.IDisposable Bind(global::Microsoft.AspNetCore.SignalR.Client.HubConnection connection, {{receiverType.FullyQualifiedInterfaceName}} receiver)
            {
                var compositeDisposable = new CompositeDisposable({{receiverType.Methods.Count}});

{{CreateRegistrationString(receiverType)}}

                return compositeDisposable;
            }
        }

""");
        }

        sb.AppendLine("""
        private static partial global::System.Collections.Generic.Dictionary<global::System.Type, IReceiverBinder> CreateBinders()
        {
            var binders = new global::System.Collections.Generic.Dictionary<global::System.Type, IReceiverBinder>();

""");

        foreach (var receiverType in _receiverTypes)
        {
            sb.AppendLine($$"""
            binders.Add(typeof({{receiverType.FullyQualifiedInterfaceName}}), new BinderFor_{{receiverType.CollisionFreeName}}());
""");
        }

        sb.AppendLine("""

            return binders;
        }
    }
}
#pragma warning restore CS8613
#pragma warning restore CS8767
#pragma warning restore CS1591
""");

        return sb.ToString();
    }

    private string CreateRegistrationString(TypeMetadata receiverType)
    {
        if (receiverType.Methods.Count == 0)
        {
            return string.Empty;
        }

        if (receiverType.Methods.Count == 1)
        {
            return CreateRegistrationStringCore(receiverType.Methods[0]);
        }

        var sb = new StringBuilder();

        sb.Append(CreateRegistrationStringCore(receiverType.Methods[0]));

        for (int i = 1; i < receiverType.Methods.Count; i++)
        {
            sb.AppendLine();
            sb.Append(CreateRegistrationStringCore(receiverType.Methods[i]));
        }

        return sb.ToString();
    }

    private string CreateRegistrationStringCore(MethodMetadata method)
    {
        return $$"""
                compositeDisposable.Add(global::Microsoft.AspNetCore.SignalR.Client.HubConnectionExtensions.On(connection, nameof(receiver.{{method.MethodName}}), {{method.CreateParameterTypeArrayString(_specialSymbols)}}, HandlerConverter.Convert{{method.CreateTypeArgumentsStringForHandlerConverter(_specialSymbols)}}(receiver.{{method.MethodName}})));
""";
    }
}
