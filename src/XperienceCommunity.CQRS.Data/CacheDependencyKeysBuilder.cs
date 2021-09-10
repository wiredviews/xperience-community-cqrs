using System;
using System.Collections.Generic;
using System.Linq;
using CMS.DataEngine;
using CMS.DocumentEngine;
using XperienceCommunity.CQRS.Core;

namespace XperienceCommunity.CQRS.Data
{
    public interface ICacheDependencyKeysBuilder
    {
        ICacheDependencyKeysBuilder Node(int nodeId);
        ICacheDependencyKeysBuilder Node(Guid nodeGUID);
        ICacheDependencyKeysBuilder NodeRelationships(int nodeId);
        ICacheDependencyKeysBuilder PageOrder();
        ICacheDependencyKeysBuilder Page(int documentId);
        ICacheDependencyKeysBuilder Page(Guid documentGUID);
        ICacheDependencyKeysBuilder Pages(IEnumerable<TreeNode> pages);
        ICacheDependencyKeysBuilder Pages(IEnumerable<int> documentIds);
        ICacheDependencyKeysBuilder PagePath(string path, PathTypeEnum type = PathTypeEnum.Explicit);
        ICacheDependencyKeysBuilder PageType(string className);
        ICacheDependencyKeysBuilder SitePageType(string className);
        ICacheDependencyKeysBuilder Objects(IEnumerable<BaseInfo> objects);
        ICacheDependencyKeysBuilder ObjectType(string typeName);
        ICacheDependencyKeysBuilder Attachment(Guid attachmentGUID);
        ICacheDependencyKeysBuilder Media(Guid mediaFileGUID);
        ICacheDependencyKeysBuilder CustomTableItems(string codeName);
        ICacheDependencyKeysBuilder CustomTableItem(string codeName, int customTableItemId);
        ICacheDependencyKeysBuilder CustomKey(string key);
        ICacheDependencyKeysBuilder CustomKeys(IEnumerable<string> keys);
    }

    public class CacheDependencyKeysBuilder : ICacheDependencyKeysBuilder
    {
        private readonly HashSet<string> cacheKeys = new(StringComparer.InvariantCultureIgnoreCase);
        private readonly ISiteContext context;

        public CacheDependencyKeysBuilder(ISiteContext context) => this.context = context;

        public ISet<string> GetKeys() => cacheKeys;

        public ICacheDependencyKeysBuilder CustomKey(string key)
        {
            cacheKeys.Add(key);

            return this;
        }
        public ICacheDependencyKeysBuilder CustomKeys(IEnumerable<string> keys)
        {
            cacheKeys.UnionWith(keys);

            return this;
        }

        public ICacheDependencyKeysBuilder PageType(string className)
        {
            cacheKeys.Add($"{DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE}|byname|{className}");

            return this;
        }

        public ICacheDependencyKeysBuilder ObjectType(string typeName)
        {
            cacheKeys.Add($"{typeName}|all");

            return this;
        }

        public ICacheDependencyKeysBuilder PageOrder()
        {
            cacheKeys.Add("nodeorder");

            return this;
        }

        public ICacheDependencyKeysBuilder SitePageType(string className)
        {
            cacheKeys.Add($"nodes|{context.SiteName}|{className}|all");

            return this;
        }

        public ICacheDependencyKeysBuilder Pages(IEnumerable<TreeNode> pages)
        {
            cacheKeys.UnionWith(pages.Select(p => $"documentid|{p.DocumentID}"));

            return this;
        }

        public ICacheDependencyKeysBuilder Objects(IEnumerable<BaseInfo> objects)
        {
            cacheKeys.UnionWith(objects.Select(o => $"{o.TypeInfo.ObjectType}|byid|{o.Generalized.ObjectID}"));

            return this;
        }

        public ICacheDependencyKeysBuilder PagePath(string path, PathTypeEnum type = PathTypeEnum.Explicit)
        {
            switch (type)
            {
                case PathTypeEnum.Single:
                    cacheKeys.Add($"node|{context.SiteName}|{path}");
                    break;
                case PathTypeEnum.Children:
                    cacheKeys.Add($"node|{context.SiteName}|{path}|childnodes");
                    break;
                case PathTypeEnum.Section:
                    cacheKeys.Add($"node|{context.SiteName}|{path}");
                    cacheKeys.Add($"node|{context.SiteName}|{path}|childnodes");
                    break;
                case PathTypeEnum.Explicit:
                default:
                    if (path.EndsWith("/%"))
                    {
                        cacheKeys.Add($"node|{context.SiteName}|{path}|childnodes");
                    }
                    else
                    {
                        cacheKeys.Add($"node|{context.SiteName}|{path}");
                    }
                    break;
            }

            return this;
        }

        public ICacheDependencyKeysBuilder Page(int documentId)
        {
            cacheKeys.Add($"documentid|{documentId}");

            return this;
        }

        public ICacheDependencyKeysBuilder Pages(IEnumerable<int> documentIds)
        {
            cacheKeys.UnionWith(documentIds.Select(id => $"documentid|{id}"));

            return this;
        }

        public ICacheDependencyKeysBuilder Node(int nodeId)
        {
            cacheKeys.Add($"nodeid|{nodeId}");

            return this;
        }

        public ICacheDependencyKeysBuilder Node(Guid nodeGUID)
        {
            cacheKeys.Add($"nodeguid|{context.SiteName}|{nodeGUID}");

            return this;
        }

        public ICacheDependencyKeysBuilder NodeRelationships(int nodeId)
        {
            cacheKeys.Add($"nodeid|{nodeId}|relationships");

            return this;
        }

        public ICacheDependencyKeysBuilder Page(Guid documentGUID)
        {
            cacheKeys.Add($"documentguid|{context.SiteName}|{documentGUID}");

            return this;
        }

        public ICacheDependencyKeysBuilder Attachment(Guid attachmentGUID)
        {
            cacheKeys.Add($"attachment|{attachmentGUID}");

            return this;
        }

        public ICacheDependencyKeysBuilder Media(Guid mediaFileGUID)
        {
            cacheKeys.Add($"mediafile|{mediaFileGUID}");

            return this;
        }

        public ICacheDependencyKeysBuilder CustomTableItems(string codeName)
        {
            cacheKeys.Add($"customtableitem.{codeName}|all");

            return this;
        }

        public ICacheDependencyKeysBuilder CustomTableItem(string codeName, int customTableItemId)
        {
            cacheKeys.Add($"customtableitem.{codeName}|byid|{customTableItemId}");

            return this;
        }
    }
}