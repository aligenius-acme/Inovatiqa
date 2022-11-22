namespace Inovatiqa.Web.UI
{
    public partial interface IPageHeadBuilder
    {
        void AddScriptParts(ResourceLocation location, string src, string debugSrc, bool excludeFromBundle, bool isAsync);

        string GenerateInlineScripts(ResourceLocation location);

        void AddInlineScriptParts(ResourceLocation location, string script);

        string GetActiveMenuItemSystemName();
    }
}
