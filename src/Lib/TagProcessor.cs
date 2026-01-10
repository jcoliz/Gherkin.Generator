using System.Collections.Generic;
using Gherkin.Ast;

namespace Gherkin.Generator.Lib;

/// <summary>
/// Processes Gherkin feature tags to extract metadata like namespace, base class, and using directives.
/// </summary>
internal static class TagProcessor
{
    /// <summary>
    /// Processes feature-level tags for namespace, base class, and using directives.
    /// </summary>
    /// <param name="tags">Collection of feature tags.</param>
    /// <param name="crif">The CRIF object to populate.</param>
    public static void ProcessFeatureTags(IEnumerable<Tag> tags, FeatureCrif crif)
    {
        foreach (var tag in tags)
        {
            if (tag.Name.StartsWith("@namespace:"))
            {
                ProcessNamespaceTag(tag, crif);
            }
            else if (tag.Name.StartsWith("@baseclass:"))
            {
                ProcessBaseClassTag(tag, crif);
            }
            else if (tag.Name.StartsWith("@using:"))
            {
                ProcessUsingTag(tag, crif);
            }
        }
    }

    /// <summary>
    /// Processes a namespace tag.
    /// </summary>
    /// <param name="tag">The namespace tag.</param>
    /// <param name="crif">The CRIF object to populate.</param>
    private static void ProcessNamespaceTag(Tag tag, FeatureCrif crif)
    {
        crif.Namespace = tag.Name.Substring("@namespace:".Length);
    }

    /// <summary>
    /// Processes a base class tag, extracting namespace if present.
    /// </summary>
    /// <param name="tag">The base class tag.</param>
    /// <param name="crif">The CRIF object to populate.</param>
    private static void ProcessBaseClassTag(Tag tag, FeatureCrif crif)
    {
        var baseClassValue = tag.Name.Substring("@baseclass:".Length);
        var lastDotIndex = baseClassValue.LastIndexOf('.');

        if (lastDotIndex >= 0)
        {
            var ns = baseClassValue.Substring(0, lastDotIndex);
            crif.BaseClass = baseClassValue.Substring(lastDotIndex + 1);
            if (!crif.Usings.Contains(ns))
            {
                crif.Usings.Add(ns);
            }
        }
        else
        {
            crif.BaseClass = baseClassValue;
        }
    }

    /// <summary>
    /// Processes a using directive tag.
    /// </summary>
    /// <param name="tag">The using tag.</param>
    /// <param name="crif">The CRIF object to populate.</param>
    private static void ProcessUsingTag(Tag tag, FeatureCrif crif)
    {
        var usingValue = tag.Name.Substring("@using:".Length);
        if (!crif.Usings.Contains(usingValue))
        {
            crif.Usings.Add(usingValue);
        }
    }

    /// <summary>
    /// Applies project-level defaults from project metadata when feature tags don't override them.
    /// </summary>
    /// <param name="crif">The CRIF object to populate.</param>
    /// <param name="projectMetadata">Project metadata containing defaults.</param>
    public static void ApplyProjectDefaults(FeatureCrif crif, ProjectMetadata? projectMetadata)
    {
        if (projectMetadata == null)
            return;

        // Apply namespace default if not explicitly set by @namespace tag
        if (string.IsNullOrEmpty(crif.Namespace) && !string.IsNullOrEmpty(projectMetadata.GeneratedNamespace))
        {
            crif.Namespace = projectMetadata.GeneratedNamespace;
        }

        // Apply base class default if not explicitly set by @baseclass tag
        if (string.IsNullOrEmpty(crif.BaseClass) && projectMetadata.DefaultTestBase != null)
        {
            crif.BaseClass = projectMetadata.DefaultTestBase.ClassName;
            
            // Add base class namespace to usings
            if (!string.IsNullOrEmpty(projectMetadata.DefaultTestBase.Namespace) &&
                !crif.Usings.Contains(projectMetadata.DefaultTestBase.Namespace))
            {
                crif.Usings.Add(projectMetadata.DefaultTestBase.Namespace);
            }
        }
    }
}
