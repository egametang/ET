namespace ProtoBuf
{
    /// <summary>
    /// Specifies the method used to infer field tags for members of the type
    /// under consideration. Tags are deduced using the invariant alphabetic
    /// sequence of the members' names; this makes implicit field tags very brittle,
    /// and susceptible to changes such as field names (normally an isolated
    /// change).
    /// </summary>
    public enum ImplicitFields
    {
        /// <summary>
        /// No members are serialized implicitly; all members require a suitable
        /// attribute such as [ProtoMember]. This is the recmomended mode for
        /// most scenarios.
        /// </summary>
        None = 0,
        /// <summary>
        /// Public properties and fields are eligible for implicit serialization;
        /// this treats the public API as a contract. Ordering beings from ImplicitFirstTag.
        /// </summary>
        AllPublic= 1,
        /// <summary>
        /// Public and non-public fields are eligible for implicit serialization;
        /// this acts as a state/implementation serializer. Ordering beings from ImplicitFirstTag.
        /// </summary>
        AllFields = 2
    }
}
