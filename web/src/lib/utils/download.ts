/**
 * Browser-based file download utilities.
 * SSR-safe - checks for browser environment.
 */

/**
 * Triggers a browser download of text content as a file.
 * Creates a temporary blob URL and triggers download via anchor element.
 *
 * @param content - Text content to download
 * @param filename - Name of the downloaded file
 * @param mimeType - MIME type (default: application/json)
 */
export function downloadTextAsFile(
  content: string,
  filename: string,
  mimeType = "application/json"
): void {
  // SSR safety check
  if (typeof window === "undefined") {
    console.warn("downloadTextAsFile called in SSR context");
    return;
  }

  const blob = new Blob([content], { type: mimeType });
  const url = URL.createObjectURL(blob);

  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = filename;
  anchor.style.display = "none";

  document.body.appendChild(anchor);
  anchor.click();

  // Cleanup
  document.body.removeChild(anchor);
  URL.revokeObjectURL(url);
}

/**
 * Triggers a browser download of JSON data as a formatted .json file.
 *
 * @param data - Data to serialize as JSON
 * @param filename - Name of the downloaded file (without extension)
 */
export function downloadJSON(data: unknown, filename: string): void {
  const json = JSON.stringify(data, null, 2);
  const filenameWithExt = filename.endsWith(".json") ? filename : `${filename}.json`;
  downloadTextAsFile(json, filenameWithExt, "application/json");
}
