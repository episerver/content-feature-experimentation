using EPiServer.Core;
using EPiServer.Data;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Shell;
using System;
using System.Collections.Specialized;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using EPiServer.Web;
using EPiServer.Framework.Internal;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    /// <summary>
    /// Helper class to provide utilities to resolve Url to the module
    /// </summary>
    public static class ModuleHelper
    {
        private static readonly ILogger _log = LogManager.GetLogger(typeof(ModuleHelper));

        public const string MODULENAME = "EPiServer.Marketing.Testing.Web";

        /// <summary>
        /// Readonly propery for getting the module path.
        /// </summary>
        public static string MyModulePath
        {
            get
            {
                return Paths.ToResource(typeof(ModuleHelper), string.Empty);
            }
        }

        /// <summary>
        /// Return resource path from type and relative path, Ex: "/se/EPiServer.Marketing.Testing.Web/virtualPath"
        /// </summary>
        /// <param name="type">The type for finding in</param>
        /// <param name="virtualPath">Resource's relative path</param>
        public static string ToResource(Type type, string virtualPath)
        {
            return Paths.ToResource(type, virtualPath);
        }

        /// <summary>
        /// Return path to physical VPP folder, ex: "C:\\EPiServer\\CMS9\\wwwroot\\modules\\_protected\\virtualPath"
        /// </summary>
        /// <param name="virtualpath">The virtualpath.</param>
        public static string ToPhysicalVPP(string virtualpath)
        {
            // now we have vpp.LocalPath point to C:\EPiServer\VPP\MyEPiServerSite\Modules\
            return Path.Combine(GetProtectedAddonsLocalPath(), virtualpath);
        }

        /// <summary>
        /// This will return string.Empty while site is working in ReadOnly mode.
        /// </summary>
        /// <returns></returns>
        private static string GetProtectedAddonsLocalPath()
        {
            return PhysicalPathResolver.Instance.Rebase(Path.Combine("modules", "_protected"));
        }

        /// <summary>
        /// <example>Output could be "C:\\EPiServer\\VPP\\MyEPiServerSite\\Modules\\EPiServer.MODULENAME\\virtualpath" or "C:\\EPiServer\\80\\wwwroot\\modules\\_protected\\EPiServer.MODULENAME\\0.1.0.8000\\\\ClientResources\\ViewMode" for converted addon to new (nuget addon) folder-style of CMS8</example>
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        private static string ToPhysicalResource(string relativePath, string virtualPath)
        {
            // Remove protected root path of relative path first
            // We have to call Paths.ProtectedRootPath.TrimStart('~') because when install Find the Paths.ProtectedRootPath starts with "~" (ex: "~/EPiServer/")
            var path = Path.Combine(relativePath.Replace(Paths.ProtectedRootPath.TrimStart('~'), ""), virtualPath);

            // Convert to physical path
            return ToPhysicalVPP(path);
        }


        /// <summary>
        /// <example>Output could be "C:\\EPiServer\\CMS9\\wwwroot\\modules\\_protected\\EPiServer.MODULENAME\\\\virtualPath"</example>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="virtualpath"></param>
        /// <returns></returns>
        public static string ToPhysicalVPPResource(Type type, string virtualpath)
        {
            // pathToModuleResource will be "EPiServer.MODULENAME\", we use this technique to get correct MODULENAME and VersionString in VPP physical path
            return ToPhysicalResource(ToResource(type, ""), virtualpath);
        }

        /// <summary>
        /// Get current SiteUrl.
        /// </summary>
        /// <returns></returns>
        public static System.Uri GetSiteUrl()
        {
            var httpContextAccessor = ServiceLocator.Current.GetInstance<IHttpContextAccessor>();
            if (httpContextAccessor.HttpContext != null)
            {
                var requestUri = new UriBuilder(httpContextAccessor.HttpContext.Request.GetEncodedUrl()).Uri;
                var siteUrl = requestUri.GetLeftPart(UriPartial.Authority);
                return new Uri(siteUrl);
            }

            return EPiServer.Web.SiteDefinition.Current.SiteUrl; // this API is for CMS7.5++
        }

        /// <summary>
        /// Check if the translation folder is not existing, create it.
        /// Return path to the translation folder.
        /// </summary>
        public static string CreateTranslationFolderIfNotExisted(string folderName)
        {
            var translationFolder = ToPhysicalVPPResource(typeof(ModuleHelper), folderName);
            if (!Directory.Exists(translationFolder))
            {
                Directory.CreateDirectory(translationFolder);
            }

            return translationFolder;
        }
    }
}