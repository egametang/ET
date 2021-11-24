namespace ILRuntime.Mono.Cecil
{
    internal enum TypeComparisonMode {
        Exact,
        SignatureOnly,

        /// <summary>
        /// Types can be in different assemblies, as long as the module, assembly, and type names match they will be considered equal
        /// </summary>
        SignatureOnlyLoose
    }
}
