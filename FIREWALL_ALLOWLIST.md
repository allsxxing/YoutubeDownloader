# Firewall Allowlist Requirements

This document lists the network requirements for YoutubeDownloader to function properly in corporate or restricted environments.

## Critical URLs (Required)

These URLs must be accessible for core functionality:

- **`https://www.youtube.com`** - Main YouTube website
- **`https://youtube.com`** - Alternate YouTube URL
- **`https://accounts.google.com`** - Google account authentication (for private content access)
- **`https://api.github.com`** - GitHub API (for application updates)

## Optional URLs (Recommended)

These URLs enhance functionality but are not strictly required:

- **`https://www.googleapis.com`** - YouTube Data API (used by YoutubeExplode library)
- **`https://ffmpeg.org`** - FFmpeg information (app bundles FFmpeg, so this is informational)

## Network Requirements

- **HTTPS (port 443)** outbound access required
- **DNS resolution** for the above domains
- **User-Agent** requests (application identifies itself as YoutubeDownloader)

## Testing

A GitHub Actions workflow is available to test firewall allowlist requirements:

```bash
# Manually trigger the workflow
gh workflow run "Firewall Allowlist URL Test"
```

The workflow tests connectivity to all required URLs and generates a report in the workflow summary.

## Troubleshooting

If YoutubeDownloader fails to work in your environment:

1. Run the firewall allowlist test workflow
2. Check that all critical URLs are accessible from your network
3. Ensure HTTPS outbound traffic is not blocked
4. Verify DNS resolution works for YouTube/Google domains

## Corporate Network Considerations

- Some networks may require proxy configuration
- Content filtering may block YouTube domains
- API rate limiting may apply
- Consider using the "bare" version if FFmpeg downloads are blocked