public static class GetPlatfromType
{
    public static bool IsEditorMode()
    {
#if UNITY_EDITOR
        return true;
#else
        return false;
#endif
        return false;
    }
}
