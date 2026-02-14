# YouTube Clip Extraction Skill

A parameterized CLI tool for extracting time-bounded clips from YouTube videos using `yt-dlp` and `ffmpeg`.

## Prerequisites

- [yt-dlp](https://github.com/yt-dlp/yt-dlp) - `brew install yt-dlp` / `pip install yt-dlp`
- [ffmpeg](https://ffmpeg.org) - `brew install ffmpeg` / `apt install ffmpeg`

## Quick Start

```bash
# Basic clip extraction
./extract_clip.sh --url "https://youtu.be/VIDEO_ID" --start "11:25" --end "11:40"

# Dry run (validate without downloading)
./extract_clip.sh --url "https://youtu.be/VIDEO_ID" --start "11:25" --end "11:40" --dry-run

# Custom output name and quality
./extract_clip.sh --url "https://youtu.be/VIDEO_ID" --start "1:02:30" --end "1:03:00" \
    --name "my_clip" --quality 720 --format mkv
```

## Options

| Flag | Description | Default |
|------|-------------|---------|
| `--url URL` | YouTube video URL | (required) |
| `--start TIME` | Start timestamp (MM:SS or HH:MM:SS) | (required) |
| `--end TIME` | End timestamp | (required) |
| `--output DIR` | Output directory | `./clips` |
| `--format FMT` | Output format: mp4, mkv, webm | `mp4` |
| `--quality QUAL` | Video quality: best, 1080, 720, 480 | `best` |
| `--name NAME` | Custom filename (without extension) | auto-generated |
| `--dry-run` | Validate inputs only | off |
| `--verbose` | Show yt-dlp output | off |

## Claude Code Prompt Templates

### A: Single Clip

```
Extract a clip from VIDEO_URL starting at MM:SS and ending at MM:SS.
Run: ./extract_clip.sh --url "URL" --start "MM:SS" --end "MM:SS"
```

### B: Dry Run First

```
Validate then extract a clip from VIDEO_URL (MM:SS to MM:SS):
1. ./extract_clip.sh --url "URL" --start "MM:SS" --end "MM:SS" --dry-run
2. If validation passes, run without --dry-run
```

### C: Custom Quality

```
Extract a 720p MP4 clip from VIDEO_URL (MM:SS to MM:SS):
./extract_clip.sh --url "URL" --start "MM:SS" --end "MM:SS" --quality 720 --format mp4 --name "clip_name"
```

### D: Multiple Clips from Same Video

```
Extract these clips from VIDEO_URL:
1. ./extract_clip.sh --url "URL" --start "05:00" --end "05:30" --name "intro"
2. ./extract_clip.sh --url "URL" --start "15:20" --end "16:00" --name "main_point"
3. ./extract_clip.sh --url "URL" --start "45:10" --end "45:45" --name "conclusion"
```

## Output

The script produces:
- **Clip file** in `./clips/` (auto-named from video title + timestamps)
- **Log file** in `./logs/` (timestamped, contains yt-dlp and ffmpeg output)
- **JSON summary** printed to stdout with status, file path, size, and codec info

### JSON Output Example

```json
{
  "status": "success",
  "video_title": "Example Video Title",
  "video_id": "x2R2cwDCxoY",
  "start": "00:11:25",
  "end": "00:11:40",
  "duration_seconds": 15,
  "output_path": "./clips/Example_Video_Title_001125-001140.mp4",
  "file_size": "2.4M",
  "codecs": "h264,aac,",
  "log_file": "./logs/extract_20260214_143022.log"
}
```

## Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success (or dry-run passed) |
| 1 | Invalid arguments or missing dependencies |
| 2 | Download failed |
| 3 | FFmpeg extraction failed |

## Directory Structure

```
YoutubeDownloader/
├── extract_clip.sh      # Main extraction script
├── clips/               # Output clips (git-ignored)
│   └── .gitkeep
├── logs/                # Extraction logs (git-ignored)
│   └── .gitkeep
└── CLIP_EXTRACTION.md   # This file
```
