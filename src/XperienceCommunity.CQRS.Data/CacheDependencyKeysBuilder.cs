using CMS.Base;
using CMS.DataEngine;

namespace XperienceCommunity.CQRS.Data;

/// <summary>
/// Generates and stores cache dependency key strings according to https://docs.xperience.io/configuring-xperience/configuring-caching/setting-cache-dependencies
/// If any provided values are null, whitespace, empty, default, or invalid (negative int identifiers), no cache key will be generated for that value
/// </summary>
public interface ICacheDependencyKeysBuilder
{
    /// <summary>
    /// nodeid|&lt;node id&gt;
    /// </summary>
    /// <param name="nodeId"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder Node(int nodeId);
    /// <summary>
    /// nodeid|&lt;node id&gt;
    /// </summary>
    /// <param name="nodeId"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder Node(Maybe<int> nodeId);
    /// <summary>
    /// nodeguid|&lt;site name&gt;|&lt;node guid&gt;
    /// </summary>
    /// <param name="nodeGUID"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder Node(Guid nodeGUID);
    /// <summary>
    /// nodeguid|&lt;site name&gt;|&lt;node guid&gt;
    /// </summary>
    /// <param name="nodeGUID"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder Node(Maybe<Guid> nodeGUID);
    /// <summary>
    /// nodeid|&lt;node id&gt;|relationships
    /// </summary>
    /// <param name="nodeId"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder NodeRelationships(int nodeId);
    /// <summary>
    /// nodeid|&lt;node id&gt;|relationships
    /// </summary>
    /// <param name="nodeId"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder NodeRelationships(Maybe<int> nodeId);
    /// <summary>
    /// nodeorder
    /// </summary>
    /// <returns></returns>
    ICacheDependencyKeysBuilder PageOrder();
    /// <summary>
    /// documentid|&lt;document id&gt;
    /// </summary>
    /// <param name="documentId"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder Page(int documentId);
    /// <summary>
    /// documentid|&lt;document id&gt;
    /// </summary>
    /// <param name="documentId"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder Page(Maybe<int> documentId);
    /// <summary>
    /// documentguid|&lt;site name&gt;|&lt;document guid&gt;
    /// </summary>
    /// <param name="documentGUID"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder Page(Guid documentGUID);
    /// <summary>
    /// documentguid|&lt;site name&gt;|&lt;document guid&gt;
    /// </summary>
    /// <param name="documentGUID"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder Page(Maybe<Guid> documentGUID);
    /// <summary>
    /// documentid|&lt;document id&gt;
    /// </summary>
    /// <param name="pages"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder Pages(IEnumerable<TreeNode> pages);
    /// <summary>
    /// documentid|&lt;document id&gt;
    /// </summary>
    /// <param name="documentIds"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder Pages(IEnumerable<int> documentIds);
    /// <summary>
    /// documentid|&lt;document id&gt;
    /// </summary>
    /// <param name="documentIds"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder Pages(IEnumerable<Maybe<int>> documentIds);
    /// <summary>
    /// node|&lt;site name&gt;|&lt;alias path&gt;
    /// node|&lt;site name&gt;|&lt;alias path&gt;|childnodes
    /// </summary>
    /// <param name="path"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder PagePath(string path, PathTypeEnum type = PathTypeEnum.Explicit);
    /// <summary>
    /// node|&lt;site name&gt;|&lt;alias path&gt;
    /// node|&lt;site name&gt;|&lt;alias path&gt;|childnodes
    /// </summary>
    /// <param name="path"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder PagePath(Maybe<string> path, PathTypeEnum type = PathTypeEnum.Explicit);
    /// <summary>
    /// cms.documenttype|byname|&lt;page type code name&gt;
    /// </summary>
    /// <param name="className"></param>
    /// <remarks>This generates a cache dependency key for the Page Type definition/structure, not Pages of that Page Type</remarks>
    /// <returns></returns>
    ICacheDependencyKeysBuilder PageTypeDefinition(string className);
    /// <summary>
    /// nodes|&lt;site name&gt;|&lt;page type code name&gt;|all
    /// </summary>
    /// <param name="className"></param>
    /// <remarks>This generates a cache dependencey key for Pages of a given Type</remarks>
    /// <returns></returns>
    ICacheDependencyKeysBuilder SitePageType(string className);
    /// <summary>
    /// &lt;object type&gt;|byid|&lt;id&gt;
    /// </summary>
    /// <param name="objectType"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder Object(string objectType, int id);
    /// <summary>
    /// &lt;object type&gt;|byid|&lt;id&gt;
    /// </summary>
    /// <param name="objectType"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder Object(string objectType, Maybe<int> id);
    /// <summary>
    /// &lt;object type&gt;|byguid|&lt;id&gt;
    /// </summary>
    /// <param name="objectType"></param>
    /// <param name="guid"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder Object(string objectType, Guid guid);
    /// <summary>
    /// &lt;object type&gt;|byguid|&lt;id&gt;
    /// </summary>
    /// <param name="objectType"></param>
    /// <param name="guid"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder Object(string objectType, Maybe<Guid> guid);
    /// <summary>
    /// &lt;object type&gt;|byid|&lt;id&gt;
    /// </summary>
    /// <param name="objects"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder Objects(IEnumerable<BaseInfo> objects);
    /// <summary>
    /// &lt;object type&gt;|all
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder ObjectType(string typeName);
    /// <summary>
    /// &lt;cms.settingskey|site id|key name&gt;
    /// </summary>
    /// <param name="keyName"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder SiteSettingKey(string keyName);
    /// <summary>
    /// &lt;cms.settingskey|key name&gt;
    /// </summary>
    /// <param name="keyName"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder GlobalSettingKey(string keyName);
    /// <summary>
    /// attachment|&lt;guid&gt;
    /// </summary>
    /// <param name="attachmentGUID"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder Attachment(Guid attachmentGUID);
    /// <summary>
    /// attachment|&lt;guid&gt;
    /// </summary>
    /// <param name="attachmentGUID"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder Attachment(Maybe<Guid> attachmentGUID);
    /// <summary>
    /// mediafile|&lt;guid&gt;
    /// </summary>
    /// <param name="mediaFileGUID"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder Media(Guid mediaFileGUID);
    /// <summary>
    /// mediafile|&lt;guid&gt;
    /// </summary>
    /// <param name="mediaFileGUID"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder Media(Maybe<Guid> mediaFileGUID);
    /// <summary>
    /// customtableitem.&lt;custom table code name&gt;|all
    /// </summary>
    /// <param name="codeName"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder CustomTableItems(string codeName);
    /// <summary>
    /// customtableitem.&lt;custom table code name&gt;|byid|&lt;id&gt;
    /// </summary>
    /// <param name="codeName"></param>
    /// <param name="customTableItemId"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder CustomTableItem(string codeName, int customTableItemId);
    /// <summary>
    /// customtableitem.&lt;custom table code name&gt;|byid|&lt;id&gt;
    /// </summary>
    /// <param name="codeName"></param>
    /// <param name="customTableItemId"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder CustomTableItem(string codeName, Maybe<int> customTableItemId);
    /// <summary>
    /// Executes the <paramref name="action"/> on each item in the collectio <paramref name="items"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    /// <example>
    /// var items = new[] { 1, 3, 4 };
    /// 
    /// builder.Collection(items, (i, b) => b.Node(i));
    /// </example>
    ICacheDependencyKeysBuilder Collection<T>(IEnumerable<T> items, Action<T, ICacheDependencyKeysBuilder> action);
    /// <summary>
    /// Executes the <paramref name="action"/> on each item in the collectio <paramref name="items"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    /// <example>
    /// var items = new[] { 1, 3, 4 };
    /// 
    /// builder.Collection(items, (i, b) => b.Node(i));
    /// </example>
    ICacheDependencyKeysBuilder Collection<T>(IEnumerable<Maybe<T>> items, Action<T, ICacheDependencyKeysBuilder> action);
    /// <summary>
    /// Executes the <paramref name="action"/> on each item in the collectio <paramref name="items"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    /// <example>
    /// var items = new[] { 1, 3, 4 };
    /// 
    /// builder.Collection(items, (i) => builder.Node(i));
    /// </example>
    ICacheDependencyKeysBuilder Collection<T>(IEnumerable<T> items, Action<T> action);
    /// <summary>
    /// Executes the <paramref name="action"/> on each item in the collectio <paramref name="items"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    /// <example>
    /// var items = new[] { 1, 3, 4 };
    /// 
    /// builder.Collection(items, (i) => builder.Node(i));
    /// </example>
    ICacheDependencyKeysBuilder Collection<T>(IEnumerable<Maybe<T>> items, Action<T> action);
    /// <summary>
    /// Can be used to add any custom key to the builder
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder CustomKey(string key);
    /// <summary>
    /// Can be used to add any custom keys to the builder
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    ICacheDependencyKeysBuilder CustomKeys(IEnumerable<string> keys);
}

public class CacheDependencyKeysBuilder : ICacheDependencyKeysBuilder
{
    private readonly HashSet<string> cacheKeys = new(StringComparer.InvariantCultureIgnoreCase);
    private readonly ISiteService siteService;

    public CacheDependencyKeysBuilder(ISiteService siteService) =>
        this.siteService = siteService;

    public ISet<string> GetKeys() => cacheKeys;
    public void ClearKeys() => cacheKeys.Clear();

    public ICacheDependencyKeysBuilder CustomKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return this;
        }

        _ = cacheKeys.Add(key);

        return this;
    }
    public ICacheDependencyKeysBuilder CustomKeys(IEnumerable<string> keys)
    {
        cacheKeys.UnionWith(keys);

        return this;
    }

    public ICacheDependencyKeysBuilder PageTypeDefinition(string className)
    {
        if (string.IsNullOrWhiteSpace(className))
        {
            return this;
        }

        _ = cacheKeys.Add($"{DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE}|byname|{className}");

        return this;
    }

    public ICacheDependencyKeysBuilder ObjectType(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return this;
        }

        _ = cacheKeys.Add($"{typeName}|all");

        return this;
    }

    public ICacheDependencyKeysBuilder SiteSettingKey(string keyName)
    {
        if (string.IsNullOrWhiteSpace(keyName))
        {
            return this;
        }

        _ = cacheKeys.Add($"cms.settingskey|{siteService.CurrentSite.SiteID}|{keyName}");

        return this;
    }

    public ICacheDependencyKeysBuilder GlobalSettingKey(string keyName)
    {
        if (string.IsNullOrWhiteSpace(keyName))
        {
            return this;
        }

        _ = cacheKeys.Add($"cms.settingskey|{keyName}");

        return this;
    }

    public ICacheDependencyKeysBuilder PageOrder()
    {
        _ = cacheKeys.Add("nodeorder");

        return this;
    }

    public ICacheDependencyKeysBuilder SitePageType(string className)
    {
        if (string.IsNullOrWhiteSpace(className))
        {
            return this;
        }

        _ = cacheKeys.Add($"nodes|{siteService.CurrentSite.SiteName}|{className}|all");

        return this;
    }

    public ICacheDependencyKeysBuilder Pages(IEnumerable<TreeNode> pages)
    {
        cacheKeys.UnionWith(pages.Select(p => $"documentid|{p.DocumentID}"));

        return this;
    }

    public ICacheDependencyKeysBuilder Object(string objectType, Maybe<int> id) =>
        Object(objectType, id.GetValueOrDefault());
    public ICacheDependencyKeysBuilder Object(string objectType, int id)
    {
        if (string.IsNullOrWhiteSpace(objectType) || id <= 0)
        {
            return this;
        }

        _ = cacheKeys.Add($"{objectType}|byid|{id}");

        return this;
    }

    public ICacheDependencyKeysBuilder Object(string objectType, Maybe<Guid> guid) =>
        Object(objectType, guid.GetValueOrDefault());
    public ICacheDependencyKeysBuilder Object(string objectType, Guid guid)
    {
        if (guid == default)
        {
            return this;
        }

        _ = cacheKeys.Add($"{objectType}|byguid|{guid}");

        return this;
    }

    public ICacheDependencyKeysBuilder Objects(IEnumerable<BaseInfo> objects)
    {
        cacheKeys.UnionWith(objects.Select(o => $"{o.TypeInfo.ObjectType}|byid|{o.Generalized.ObjectID}"));

        return this;
    }

    public ICacheDependencyKeysBuilder PagePath(Maybe<string> path, PathTypeEnum type = PathTypeEnum.Explicit) =>
        PagePath(path.GetValueOrDefault(), type);
    public ICacheDependencyKeysBuilder PagePath(string path, PathTypeEnum type = PathTypeEnum.Explicit)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return this;
        }

        switch (type)
        {
            case PathTypeEnum.Single:
                _ = cacheKeys.Add($"node|{siteService.CurrentSite.SiteName}|{path}");
                break;
            case PathTypeEnum.Children:
                _ = cacheKeys.Add($"node|{siteService.CurrentSite.SiteName}|{path}|childnodes");
                break;
            case PathTypeEnum.Section:
                _ = cacheKeys.Add($"node|{siteService.CurrentSite.SiteName}|{path}");
                _ = cacheKeys.Add($"node|{siteService.CurrentSite.SiteName}|{path}|childnodes");
                break;
            case PathTypeEnum.Explicit:
            default:
                _ = path.EndsWith("/%")
                    ? cacheKeys.Add($"node|{siteService.CurrentSite.SiteName}|{path}|childnodes")
                    : cacheKeys.Add($"node|{siteService.CurrentSite.SiteName}|{path}");
                break;
        }

        return this;
    }

    public ICacheDependencyKeysBuilder Page(Maybe<int> documentId) =>
        Page(documentId.GetValueOrDefault());
    public ICacheDependencyKeysBuilder Page(int documentId)
    {
        if (documentId <= 0)
        {
            return this;
        }

        _ = cacheKeys.Add($"documentid|{documentId}");

        return this;
    }

    public ICacheDependencyKeysBuilder Pages(IEnumerable<Maybe<int>> documentIds) =>
        Pages(documentIds.Choose());
    public ICacheDependencyKeysBuilder Pages(IEnumerable<int> documentIds)
    {
        cacheKeys.UnionWith(documentIds.Select(id => $"documentid|{id}"));

        return this;
    }

    public ICacheDependencyKeysBuilder Node(Maybe<int> nodeId) =>
        Node(nodeId.GetValueOrDefault());
    public ICacheDependencyKeysBuilder Node(int nodeId)
    {
        if (nodeId <= 0)
        {
            return this;
        }

        _ = cacheKeys.Add($"nodeid|{nodeId}");

        return this;
    }

    public ICacheDependencyKeysBuilder Node(Maybe<Guid> nodeGUID) =>
        Node(nodeGUID.GetValueOrDefault());
    public ICacheDependencyKeysBuilder Node(Guid nodeGUID)
    {
        if (nodeGUID == default)
        {
            return this;
        }

        _ = cacheKeys.Add($"nodeguid|{siteService.CurrentSite.SiteName}|{nodeGUID}");

        return this;
    }

    public ICacheDependencyKeysBuilder NodeRelationships(Maybe<int> nodeId) =>
        NodeRelationships(nodeId.GetValueOrDefault());
    public ICacheDependencyKeysBuilder NodeRelationships(int nodeId)
    {
        if (nodeId <= 0)
        {
            return this;
        }

        _ = cacheKeys.Add($"nodeid|{nodeId}|relationships");

        return this;
    }

    public ICacheDependencyKeysBuilder Page(Maybe<Guid> documentGUID) =>
        Page(documentGUID.GetValueOrDefault());
    public ICacheDependencyKeysBuilder Page(Guid documentGUID)
    {
        if (documentGUID == default)
        {
            return this;
        }

        _ = cacheKeys.Add($"documentguid|{siteService.CurrentSite.SiteName}|{documentGUID}");

        return this;
    }

    public ICacheDependencyKeysBuilder Attachment(Maybe<Guid> attachmentGUID) =>
        Attachment(attachmentGUID.GetValueOrDefault());
    public ICacheDependencyKeysBuilder Attachment(Guid attachmentGUID)
    {
        if (attachmentGUID == default)
        {
            return this;
        }

        _ = cacheKeys.Add($"attachment|{attachmentGUID}");

        return this;
    }

    public ICacheDependencyKeysBuilder Media(Maybe<Guid> mediaFileGUID) =>
        Media(mediaFileGUID.GetValueOrDefault());
    public ICacheDependencyKeysBuilder Media(Guid mediaFileGUID)
    {
        if (mediaFileGUID == default)
        {
            return this;
        }

        _ = cacheKeys.Add($"mediafile|{mediaFileGUID}");

        return this;
    }

    public ICacheDependencyKeysBuilder Collection<T>(IEnumerable<Maybe<T>> items, Action<T, ICacheDependencyKeysBuilder> action) =>
        Collection(items.Choose(), action);
    public ICacheDependencyKeysBuilder Collection<T>(IEnumerable<T> items, Action<T, ICacheDependencyKeysBuilder> action)
    {
        foreach (var item in items)
        {
            action(item, this);
        }

        return this;
    }

    public ICacheDependencyKeysBuilder Collection<T>(IEnumerable<Maybe<T>> items, Action<T> action) =>
        Collection(items.Choose(), action);
    public ICacheDependencyKeysBuilder Collection<T>(IEnumerable<T> items, Action<T> action)
    {
        foreach (var item in items)
        {
            action(item);
        }

        return this;
    }

    public ICacheDependencyKeysBuilder CustomTableItems(string codeName)
    {
        if (string.IsNullOrWhiteSpace(codeName))
        {
            return this;
        }

        _ = cacheKeys.Add($"customtableitem.{codeName}|all");

        return this;
    }

    public ICacheDependencyKeysBuilder CustomTableItem(string codeName, Maybe<int> customTableItemId) =>
        CustomTableItem(codeName, customTableItemId.GetValueOrDefault());
    public ICacheDependencyKeysBuilder CustomTableItem(string codeName, int customTableItemId)
    {
        if (string.IsNullOrWhiteSpace(codeName) || customTableItemId <= 0)
        {
            return this;
        }

        _ = cacheKeys.Add($"customtableitem.{codeName}|byid|{customTableItemId}");

        return this;
    }
}
