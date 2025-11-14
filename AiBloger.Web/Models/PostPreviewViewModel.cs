using System.Collections.Generic;
using AiBloger.Core.Entities;

namespace AiBloger.Web.Models
{
	public sealed class PostPreviewViewModel
	{
		public string Url { get; set; } = string.Empty;
		public string Model { get; set; } = string.Empty;
		public IReadOnlyList<string> AvailableModels { get; set; } = new List<string>();
		public PostInfo? Result { get; set; }
		public string? Error { get; set; }
	}
}


