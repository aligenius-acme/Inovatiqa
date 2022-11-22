using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inovatiqa.Web.UI
{
    public partial class PageHeadBuilder:IPageHeadBuilder
    {
        #region Fields

        private readonly Dictionary<ResourceLocation, List<ScriptReferenceMeta>> _scriptParts;
        private readonly Dictionary<ResourceLocation, List<string>> _inlineScriptParts;

        private string _activeAdminMenuSystemName;

        #endregion

        #region Ctor

        public PageHeadBuilder()
        {
            _scriptParts = new Dictionary<ResourceLocation, List<ScriptReferenceMeta>>();
            _inlineScriptParts = new Dictionary<ResourceLocation, List<string>>();
        }

        #endregion


        #region Methods

        public virtual void AddScriptParts(ResourceLocation location, string src, string debugSrc, bool excludeFromBundle, bool isAsync)
        {
            if (!_scriptParts.ContainsKey(location))
                _scriptParts.Add(location, new List<ScriptReferenceMeta>());

            if (string.IsNullOrEmpty(src))
                return;

            if (string.IsNullOrEmpty(debugSrc))
                debugSrc = src;

            _scriptParts[location].Add(new ScriptReferenceMeta
            {
                ExcludeFromBundle = excludeFromBundle,
                IsAsync = isAsync,
                Src = src,
                DebugSrc = debugSrc
            });
        }

        public virtual string GenerateInlineScripts(ResourceLocation location)
        {
            if (!_inlineScriptParts.ContainsKey(location) || _inlineScriptParts[location] == null)
                return "";

            if (!_inlineScriptParts.Any())
                return "";

            var result = new StringBuilder();
            foreach (var item in _inlineScriptParts[location])
            {
                result.Append(item);
                result.Append(Environment.NewLine);
            }
            return result.ToString();
        }

        public virtual void AddInlineScriptParts(ResourceLocation location, string script)
        {
            if (!_inlineScriptParts.ContainsKey(location))
                _inlineScriptParts.Add(location, new List<string>());

            if (string.IsNullOrEmpty(script))
                return;

            if (_inlineScriptParts[location].Contains(script))
                return;

            _inlineScriptParts[location].Add(script);
        }

        public virtual string GetActiveMenuItemSystemName()
        {
            return _activeAdminMenuSystemName;
        }

        #endregion

        #region Nested Classes

        private class ScriptReferenceMeta : IEquatable<ScriptReferenceMeta>
        {
            public bool ExcludeFromBundle { get; set; }

            public bool IsAsync { get; set; }

            public string Src { get; set; }

            public string DebugSrc { get; set; }

            public bool Equals(ScriptReferenceMeta item)
            {
                if (item == null)
                    return false;
                return Src.Equals(item.Src) && DebugSrc.Equals(item.DebugSrc);
            }
            public override int GetHashCode()
            {
                return Src == null ? 0 : Src.GetHashCode();
            }
        }

        #endregion

    }
}
