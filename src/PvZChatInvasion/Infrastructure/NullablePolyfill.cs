// Polyfill de los atributos nullable.
// Al consumir librerías compiladas con nullable enable (TwitchLib), el compilador
// necesita emitir System.Runtime.CompilerServices.NullableAttribute. En este
// proyecto, con el set de referencias IL2CPP, el ref pack no se lo provee de forma
// fiable, así que lo declaramos nosotros. Es un patrón estándar y benigno.
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    internal sealed class NullableAttribute : Attribute
    {
        public NullableAttribute(byte b) { }
        public NullableAttribute(byte[] b) { }
    }

    [AttributeUsage(AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Method
        | AttributeTargets.Interface | AttributeTargets.Delegate | AttributeTargets.Struct,
        AllowMultiple = false, Inherited = false)]
    internal sealed class NullableContextAttribute : Attribute
    {
        public NullableContextAttribute(byte b) { }
    }
}
