using System.Text.Json;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FlightPlan.Services;

public interface IViteAssetService
{
    IHtmlContent RenderAsset(string entryPoint, bool isStyle = false);
}

public class ViteAssetService : IViteAssetService
{
    private readonly IWebHostEnvironment _env;
    private Dictionary<string, ViteManifestEntry>? _manifest;
    private readonly string _manifestPath;

    public ViteAssetService(IWebHostEnvironment env)
    {
        _env = env;
        var webRoot = _env.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
        }
        _manifestPath = Path.Combine(webRoot, "assets", ".vite", "manifest.json");
    }

    private void LoadManifest()
    {
        if (_manifest != null) return;

        if (!string.IsNullOrEmpty(_manifestPath) && File.Exists(_manifestPath))
        {
            var json = File.ReadAllText(_manifestPath);
            _manifest = JsonSerializer.Deserialize<Dictionary<string, ViteManifestEntry>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        else
        {
            _manifest = new Dictionary<string, ViteManifestEntry>();
        }
    }

    public IHtmlContent RenderAsset(string entryPoint, bool isStyle = false)
    {
        LoadManifest();

        if (_manifest == null || !_manifest.TryGetValue(entryPoint, out var entry))
        {
            return HtmlString.Empty;
        }

        if (isStyle)
        {
            var cssFiles = new HashSet<string>();
            CollectCss(entry, cssFiles);

            if (cssFiles.Count > 0)
            {
                var cssLinks = cssFiles.Select(cssFile => $"<link rel=\"stylesheet\" href=\"/assets/{cssFile}\">");
                return new HtmlString(string.Join("\n", cssLinks));
            }
            return HtmlString.Empty;
        }
        else
        {
            return new HtmlString($"<script type=\"module\" src=\"/assets/{entry.File}\"></script>");
        }
    }

    private void CollectCss(ViteManifestEntry entry, HashSet<string> cssFiles)
    {
        if (entry.Css != null)
        {
            foreach (var cssFile in entry.Css)
            {
                cssFiles.Add(cssFile);
            }
        }

        if (entry.Imports != null)
        {
            foreach (var importName in entry.Imports)
            {
                if (_manifest != null && _manifest.TryGetValue(importName, out var importEntry))
                {
                    CollectCss(importEntry, cssFiles);
                }
            }
        }
    }

    private class ViteManifestEntry
    {
        public string File { get; set; } = string.Empty;
        public string[]? Css { get; set; }
        public string[]? Imports { get; set; }
    }
}

public static class ViteHtmlHelpers
{
    public static IHtmlContent ViteAsset(this IHtmlHelper htmlHelper, string entryPoint)
    {
        var service = htmlHelper.ViewContext.HttpContext.RequestServices.GetService<IViteAssetService>();
        return service?.RenderAsset(entryPoint) ?? HtmlString.Empty;
    }

    public static IHtmlContent ViteStyle(this IHtmlHelper htmlHelper, string entryPoint)
    {
        var service = htmlHelper.ViewContext.HttpContext.RequestServices.GetService<IViteAssetService>();
        return service?.RenderAsset(entryPoint, true) ?? HtmlString.Empty;
    }
}
