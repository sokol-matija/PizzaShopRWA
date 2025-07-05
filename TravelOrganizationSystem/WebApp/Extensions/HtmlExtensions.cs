using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using WebApp.Services;

namespace WebApp.Extensions
{
    public static class HtmlExtensions
    {
        /// <summary>
        /// Renders an optimized image with lazy loading and responsive sizing
        /// </summary>
        /// <param name="htmlHelper">The HTML helper instance</param>
        /// <param name="imageUrl">The image URL or search query</param>
        /// <param name="alt">Alt text for the image</param>
        /// <param name="cssClass">CSS classes for the image</param>
        /// <param name="style">Inline styles for the image</param>
        /// <param name="size">Image size optimization (thumb, small, regular, full)</param>
        /// <param name="width">Custom width (optional)</param>
        /// <param name="height">Custom height (optional)</param>
        /// <returns>HTML string for the optimized image</returns>
        public static IHtmlContent OptimizedImage(
            this IHtmlHelper htmlHelper,
            string imageUrl,
            string alt = "",
            string cssClass = "img-fluid",
            string style = "object-fit: cover; width: 100%;",
            string size = "regular",
            int? width = null,
            int? height = null)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return new HtmlString(GeneratePlaceholder(alt, cssClass, style, width, height));
            }

            var optimizedUrl = OptimizeImageUrl(imageUrl, size, width, height);
            
            var img = new StringBuilder();
            img.Append("<img");
            img.Append($" src=\"{optimizedUrl}\"");
            img.Append($" alt=\"{alt}\"");
            img.Append($" class=\"{cssClass}\"");
            img.Append($" style=\"{style}\"");
            img.Append(" loading=\"lazy\"");
            img.Append(" decoding=\"async\"");
            
            // Add responsive srcset for better performance
            if (imageUrl.Contains("images.unsplash.com"))
            {
                var srcset = GenerateSrcSet(imageUrl);
                if (!string.IsNullOrEmpty(srcset))
                {
                    img.Append($" srcset=\"{srcset}\"");
                    img.Append(" sizes=\"(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 33vw\"");
                }
            }
            
            img.Append(" />");
            
            return new HtmlString(img.ToString());
        }

        /// <summary>
        /// Renders an optimized image with a loading placeholder
        /// </summary>
        public static IHtmlContent OptimizedImageWithPlaceholder(
            this IHtmlHelper htmlHelper,
            string imageUrl,
            string alt = "",
            string cssClass = "img-fluid",
            string style = "object-fit: cover; width: 100%;",
            string size = "regular",
            int? width = null,
            int? height = null)
        {
            var containerStyle = width.HasValue && height.HasValue 
                ? $"width: {width}px; height: {height}px; position: relative; overflow: hidden;"
                : "position: relative; overflow: hidden;";

            var html = new StringBuilder();
            html.Append($"<div class=\"optimized-image-container\" style=\"{containerStyle}\">");
            
            // Loading placeholder
            html.Append("<div class=\"image-placeholder d-flex align-items-center justify-content-center bg-light\" ");
            html.Append($"style=\"{style} position: absolute; top: 0; left: 0; width: 100%; height: 100%; z-index: 1;\">");
            html.Append("<div class=\"spinner-border text-primary\" role=\"status\">");
            html.Append("<span class=\"visually-hidden\">Loading...</span>");
            html.Append("</div></div>");
            
            // Actual image
            html.Append(OptimizedImage(htmlHelper, imageUrl, alt, cssClass, style, size, width, height));
            
            html.Append("</div>");
            
            // Add JavaScript to hide placeholder when image loads
            html.Append("<script>");
            html.Append("document.addEventListener('DOMContentLoaded', function() {");
            html.Append("  const containers = document.querySelectorAll('.optimized-image-container');");
            html.Append("  containers.forEach(container => {");
            html.Append("    const img = container.querySelector('img');");
            html.Append("    const placeholder = container.querySelector('.image-placeholder');");
            html.Append("    if (img && placeholder) {");
            html.Append("      img.onload = () => placeholder.style.display = 'none';");
            html.Append("      img.onerror = () => placeholder.innerHTML = '<i class=\"fas fa-image fa-2x text-muted\"></i>';");
            html.Append("    }");
            html.Append("  });");
            html.Append("});");
            html.Append("</script>");
            
            return new HtmlString(html.ToString());
        }

        private static string OptimizeImageUrl(string imageUrl, string size, int? width, int? height)
        {
            if (string.IsNullOrEmpty(imageUrl) || !imageUrl.Contains("images.unsplash.com"))
            {
                return imageUrl;
            }

            var separator = imageUrl.Contains('?') ? "&" : "?";
            var optimizedUrl = $"{imageUrl}{separator}auto=format&fit=crop&q=80";

            // Add dimensions based on size
            switch (size.ToLower())
            {
                case "thumb":
                    optimizedUrl += "&w=200&h=150";
                    break;
                case "small":
                    optimizedUrl += "&w=400&h=300";
                    break;
                case "regular":
                    optimizedUrl += "&w=800&h=600";
                    break;
                // "full" doesn't add dimension constraints
            }

            // Override with custom dimensions if provided
            if (width.HasValue && height.HasValue)
            {
                optimizedUrl += $"&w={width}&h={height}";
            }

            return optimizedUrl;
        }

        private static string GenerateSrcSet(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || !imageUrl.Contains("images.unsplash.com"))
            {
                return string.Empty;
            }

            var baseUrl = imageUrl.Split('?')[0];
            var srcset = new StringBuilder();
            
            // Generate different sizes for responsive images
            var sizes = new[] { 
                (400, "400w"), 
                (800, "800w"), 
                (1200, "1200w"), 
                (1600, "1600w") 
            };

            foreach (var (width, descriptor) in sizes)
            {
                if (srcset.Length > 0) srcset.Append(", ");
                srcset.Append($"{baseUrl}?auto=format&fit=crop&q=80&w={width} {descriptor}");
            }

            return srcset.ToString();
        }

        private static string GeneratePlaceholder(string alt, string cssClass, string style, int? width, int? height)
        {
            var placeholderStyle = style;
            if (width.HasValue && height.HasValue)
            {
                placeholderStyle += $" width: {width}px; height: {height}px;";
            }
            placeholderStyle += " background: #f8f9fa; display: flex; align-items: center; justify-content: center;";

            return $"<div class=\"{cssClass}\" style=\"{placeholderStyle}\" title=\"{alt}\">" +
                   "<i class=\"fas fa-image fa-2x text-muted\"></i>" +
                   "</div>";
        }
    }
} 